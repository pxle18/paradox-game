using System;
using System.Collections.Generic;
using System.IO;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Paintball.Menu;
using System.Xml;
using System.Linq;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using System.Threading.Tasks;

namespace VMP_CNR.Module.Paintball
{
    /*
     * 
     * 
      - KnownBugs
      -- 
      -- Weapons after log NOT FIXED
      --
      PAINTBALL v0.9 Stand 11.06.2020
      - Table `paintball` - Einstellungen
      -- Name
      -- Area (Gebietsbegrenzung)
      -- Dimension
      -- Weapons / Ammo
      -- MaxPlayerLobby
      -- EnterLobbyPrice
      -- MaxLife 
      -- RespawnTime (Millisekunden)
      -- SpawnProtection (Millisekunden)
      -- Active (1/0)
      ---
      - Table `paintball_spawns`
      -- Spawns x,y,z
      -- Active (1/0)
      ---
      - Bemerkung:
      -- Jedes Gebiet sollte eine eigene Dimension haben
      -- RespawnTime nicht über 2 Minuten
      -- Richtige Paintball WPNZ kommen mit Rage 1.1
      ---
      - Regeln:
      -- Befehle Deaktiviert: /packgun 
    */

    public class PaintballModule : Module<PaintballModule>
    {

        public static bool PaintballDeactivated = false;
        public static Random Rand = new Random();
        public static Dictionary<string, dynamic> pbLobbies = new Dictionary<string, dynamic>();
        public static Vector3 PaintballMenuPosition = new Vector3(568.955, 2796.59, 42.0183);

        protected override bool OnLoad()
        {
            if (!PaintballDeactivated)
            {
                MenuManager.Instance.AddBuilder(new PaintballEnterMenuBuilder());
                new Npc(PedHash.Marine03SMY, new Vector3(568.955, 2796.59, 42.0183), 270, 0);
            }
            return base.OnLoad();
        }

        public override void OnPlayerFirstSpawnAfterSync(DbPlayer dbPlayer)
        {
            if(dbPlayer.Paintball == 1)
            {
                dbPlayer.Dimension[0] = 0;
                dbPlayer.DimensionType[0] = DimensionType.World;
                dbPlayer.Player.SetPosition(PaintballMenuPosition);
                dbPlayer.SetPaintball(0);
            }
        }

        public void StartPaintball(DbPlayer dbPlayer, uint id)
        {
            if (PaintballDeactivated) return;
            if (dbPlayer != null)
            {
                dbPlayer.SetData("ac-ignorews", 4);

                PaintballArea pba = PaintballAreaModule.Instance.Get(id);
                pba.pbPlayers.TryAdd(dbPlayer, new vars { life = pba.MaxLife, kills = 0, deaths = 0, killstreak = 0 });
                dbPlayer.SetData("paintball_map", pba.Id);
                //REMOVE & LOAD WEAPONZ
                //SAVE WESTE
                dbPlayer.SetData("paintball_armor", dbPlayer.Player.Armor);
                dbPlayer.RemoveAllServerWeapons();
                // SAVE Player 
                dbPlayer.SetPaintball(1);

                Spawn(dbPlayer);

                dbPlayer.Player.TriggerNewClient("initializePaintball");
            }
        }



        public void Spawn(DbPlayer dbPlayer,bool quit=false,bool colshapeSpawn=false)
        {
            if (dbPlayer != null && dbPlayer.HasData("paintball_map"))
            {
                var playerMap = dbPlayer.GetData("paintball_map");
                PaintballArea pba = PaintballAreaModule.Instance.Get(playerMap);

                if (pba.pbPlayers.ContainsKey(dbPlayer)|| quit)
                {

                    if (pba.pbPlayers[dbPlayer].life <= 0|| quit)
                    {
                        NAPI.Task.Run(async () =>
                        {
                            await NAPI.Task.WaitForMainThread(1000);
                            //FINISH PAINTBALL
                            pba.pbPlayers.TryRemove(dbPlayer, out vars value);
                            dbPlayer.ResetData("paintball_map");
                            dbPlayer.ResetData("paintball_death");

                            //WORKAROUND PREVENT ALL ACTIONS?
                            //GIVE OLD ARMOR
                            if (dbPlayer.HasData("paintball_armor"))
                            {
                                dbPlayer.SetArmorPlayer(dbPlayer.GetData("paintball_armor"));
                            }
                            //REMOVE WEAPONS
                            dbPlayer.RemoveAllServerWeapons();

                            //dbPlayer.Player.TriggerNewClient("emptyWeaponAmmo");
                            //REVIVE IF INJURED
                            if (dbPlayer.Injury.Id != 0)
                            {
                                dbPlayer.Revive();
                            }

                            dbPlayer.Player.TriggerNewClient("finishPaintball");

                            await NAPI.Task.WaitForMainThread(100);
                            //GiveOldWeaponz
                            dbPlayer.LoadPlayerWeapons();

                            await NAPI.Task.WaitForMainThread(500);
                            // Just do crazy stuff bra
                            dbPlayer.SetTied(false);
                            dbPlayer.SetMedicCuffed(false);
                            dbPlayer.SetCuffed(false);

                            await NAPI.Task.WaitForMainThread(3000);

                            dbPlayer.SetPaintball(0);
                            dbPlayer.SetDimension(0);
                            dbPlayer.Dimension[0] = 0;
                            dbPlayer.DimensionType[0] = DimensionType.World;
                            dbPlayer.Player.SetPosition(PaintballMenuPosition);

                        });
                    }
                    else
                    {
                        NAPI.Task.Run(async () =>
                        {
                            await NAPI.Task.WaitForMainThread(0);
                            //GET/SET NEW SPAWN 
                            var spawn = PaintballSpawnModule.Instance.getSpawn(pba.Id);
                            dbPlayer.SetDimension(pba.PaintBallDimension);
                            dbPlayer.Dimension[0] = pba.PaintBallDimension;
                            dbPlayer.DimensionType[0] = DimensionType.Paintball;
                            dbPlayer.Player.SetPosition(new Vector3(spawn.x, spawn.y, spawn.z));

                            if (!colshapeSpawn)
                            {
                                await NAPI.Task.WaitForMainThread(100);
                                dbPlayer.SetHealth(100);
                                dbPlayer.SetArmor(99, false);
                            }

                            //SPAWNPROTECTION IN MS
                            if (pba.SpawnProtection > 0)
                            {
                                dbPlayer.Player.TriggerNewClient("spawnProtection", pba.SpawnProtection);
                            }

                            //REMOVE WEAPONS
                            dbPlayer.RemoveAllServerWeapons();
                            //dbPlayer.Player.TriggerNewClient("emptyWeaponAmmo");

                            //WEAPONS
                            foreach (var wpz in pba.Weapons)
                            {
                                dbPlayer.GiveServerWeapon(NAPI.Util.WeaponNameToModel(wpz.name), wpz.ammo);
                            }


                            //REVIVE IF INJURED
                            if (dbPlayer.Injury.Id != 0)
                            {
                                dbPlayer.Revive();
                            }

                            if (!colshapeSpawn)
                            {
                                dbPlayer.SendNewNotification($"Du hast noch {pba.pbPlayers[dbPlayer].life} Leben");
                            }

                            dbPlayer.ResetData("paintball_death");

                            await NAPI.Task.WaitForMainThread(500);

                            dbPlayer.SetTied(false);
                            dbPlayer.SetMedicCuffed(false);
                            dbPlayer.SetCuffed(false);

                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        });
                    }
                }
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Dimension[0] == 0 && key == Key.E)
            {
                if (dbPlayer.Player.Position.DistanceTo(PaintballMenuPosition) < 2.0f)
                {

                    if (Crime.CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes) > 0)
                    {
                        dbPlayer.SendNewNotification("Zutritt verweigert: Ihr Steckbrief wurde von der Polizei bei uns ausgelegt.");
                        return true;
                    }

                    Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.PaintballEnterMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }
            
            return false;
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (dbPlayer != null && dbPlayer.HasData("paintball_map"))
            {
                PaintballArea pba = PaintballAreaModule.Instance.Get(dbPlayer.GetData("paintball_map"));
                pba.pbPlayers.Remove(dbPlayer, out vars value);
            }
        }

        public override void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint hash)
        {
            try
            {
                DbPlayer iKiller = killer.ToPlayer().GetPlayer();
                if (dbPlayer == null && !dbPlayer.IsValid()) return;
                if (iKiller == null && !iKiller.IsValid()) return;
                if (!dbPlayer.IsAlive()) return; // Erneuter Tot verhindern


                if (iKiller.HasData("paintball_map") && dbPlayer.HasData("paintball_map"))
                {
                    var playerMap = dbPlayer.GetData("paintball_map");
                    PaintballArea pba = PaintballAreaModule.Instance.Get(playerMap);
                    if (pba.pbPlayers.ContainsKey(dbPlayer) && pba.pbPlayers.ContainsKey(iKiller))
                    {
                        dbPlayer.SetTied(true);

                        if (dbPlayer != iKiller)
                        {
                            dbPlayer.SetData("paintball_death", 1);
                            pba.pbPlayers[dbPlayer] = new vars { life = pba.pbPlayers[dbPlayer].life - 1, kills = pba.pbPlayers[dbPlayer].kills, deaths = pba.pbPlayers[dbPlayer].deaths + 1, killstreak = 0 };

                            if (pba.pbPlayers.ContainsKey(iKiller))
                            {
                                pba.pbPlayers[iKiller] = new vars { life = pba.pbPlayers[iKiller].life, kills = pba.pbPlayers[iKiller].kills + 1, deaths = pba.pbPlayers[iKiller].deaths, killstreak = pba.pbPlayers[iKiller].killstreak + 1 };

                                dbPlayer.SendNewNotification($"Du wurdest umgebracht von {iKiller.GetName()}");
                                iKiller.SendNewNotification($"Du hast {dbPlayer.GetName()} umgebracht");

                                //HP - ARMOR - BOOST
                                iKiller.SetHealth(Math.Min(100, NAPI.Player.GetPlayerHealth(iKiller.Player) + 25));
                                iKiller.SetArmor(Math.Min(99, NAPI.Player.GetPlayerArmor(iKiller.Player) + 25));

                                if (pba.pbPlayers[iKiller].killstreak == 3)
                                {
                                    foreach (var Players in pba.pbPlayers)
                                    {
                                        Players.Key.Player.TriggerNewClient("sendGlobalNotification", $"Bei {iKiller.GetName()} läuft!", 5000, "white", "glob");
                                    }
                                }
                                if (pba.pbPlayers[iKiller].killstreak == 6)
                                {
                                    foreach (var Players in pba.pbPlayers)
                                    {
                                        Players.Key.Player.TriggerNewClient("sendGlobalNotification", $"{iKiller.GetName()} scheppert richtig!", 5000, "white", "glob");
                                    }
                                }
                                if (pba.pbPlayers[iKiller].killstreak == 9)
                                {
                                    foreach (var Players in pba.pbPlayers)
                                    {
                                        Players.Key.Player.TriggerNewClient("sendGlobalNotification", $"{iKiller.GetName()} ist GODLIKE", 5000, "white", "glob");
                                    }
                                }
                                if (pba.pbPlayers[iKiller].killstreak == 12)
                                {
                                    foreach (var Players in pba.pbPlayers)
                                    {
                                        Players.Key.Player.TriggerNewClient("sendGlobalNotification", $"{iKiller.GetName()} - Dragan, bist du es?", 5000, "white", "glob");
                                    }
                                }

                                dbPlayer.Player.TriggerNewClient("updatePaintballScore", pba.pbPlayers[dbPlayer].kills, pba.pbPlayers[dbPlayer].deaths, pba.pbPlayers[dbPlayer].killstreak);
                                iKiller.Player.TriggerNewClient("updatePaintballScore", pba.pbPlayers[iKiller].kills, pba.pbPlayers[iKiller].deaths, pba.pbPlayers[iKiller].killstreak);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return;
            }
            return;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape p_ColShape, ColShapeState p_ColShapeState)
        {
            if (p_ColShape.HasData("paintballId") && dbPlayer.DimensionType[0] == DimensionType.Paintball)
            {
                if (dbPlayer.IsInjured()) return false;

                switch (p_ColShapeState)
                {
                    case ColShapeState.Exit:
                        if (dbPlayer.HasData("paintball_map"))
                        {
                            Spawn(dbPlayer, false, true);
                        }
                        return true;
                    default:
                        return true;
                }
            }

            return false;

        }


        [CommandPermission]
        [Command]
        public void Commandquit(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod() || dbPlayer.IsInjured()) return;
            if (dbPlayer.HasData("paintball_map"))
            {
                if (!dbPlayer.HasData("paintball_death"))
                {
                    PaintballModule.Instance.Spawn(dbPlayer, true);
                }
                else
                {
                    dbPlayer.SendNewNotification($"/quit erst nach dem Spawn.");
                }
            }
        }
    }



    public class PaintballConfirm: Script
    {
        [RemoteEvent]
        public void PbaConfirm(Player p_Player, string pb_map, string none, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            PaintballArea pba = PaintballAreaModule.Instance.Get(Convert.ToUInt32(pb_map));
            if (dbPlayer == null || !dbPlayer.IsValid() || pba == null)
            {
                return;
            }

            if (!dbPlayer.TakeMoney(pba.LobbyEnterPrice))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(pba.LobbyEnterPrice));
                return;
            }

            PaintballModule.Instance.StartPaintball(dbPlayer, pba.Id);
        }

        [RemoteEvent]
        public void PbaConfirmPassword(Player p_Player, string returnstring, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (!dbPlayer.HasData("pba_choose")) return;
            
            PaintballArea pba = PaintballAreaModule.Instance.Get(dbPlayer.GetData("pba_choose"));
            if (pba.Password == returnstring)
            {
                PaintballModule.Instance.StartPaintball(dbPlayer, pba.Id);
                dbPlayer.ResetData("pba_choose");
            }
            else
            {
                dbPlayer.SendNewNotification($"Passwort ist falsch!");
            }
        }
    }
}
