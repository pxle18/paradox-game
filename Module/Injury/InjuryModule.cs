using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Events;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Teamfight;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Teams.Blacklist;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Racing;
using VMP_CNR.Module.Einreiseamt;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Injury.Windows;
using VMP_CNR.Module.Nutrition;
using VMP_CNR.Module.Teams.Spawn;
using System.Threading.Tasks;

namespace VMP_CNR.Module.Injury
{
    public sealed class InjuryModule : Module<InjuryModule>
    {
        public static List<Vector3> HospitalPositions = new List<Vector3>();

        public uint InjuryDeathScreenId = 100;
        public uint InjuryKrankentransport = 101;
        public uint InjuryBruise = 44;
        public uint InjuryBleeding = 36;
        public uint InjuryGangwar = 102;
        public uint InjuryTeamfight = 105;

        public DateTime lastInjuryDelivered = DateTime.Now;

        public static Vector3 InsurancePos = new Vector3(312.529, -592.932, 43.284);

        protected override bool OnLoad()
        {
            HospitalPositions = new List<Vector3>();
            HospitalPositions.Add(new Vector3(-243.489, 6325.19, 32.4262));
            HospitalPositions.Add(new Vector3(1816.03, 3678.88, 34.2764));
            HospitalPositions.Add(new Vector3(391.001, -1432.46, 29.4338));
            HospitalPositions.Add(new Vector3(355.637, -596.391, 28.7746));
            // KH3 Deactivate
            //HospitalPositions.Add(new Vector3(-498.409, -335.794, 34.5018));

            lastInjuryDelivered = DateTime.Now;

            PlayerNotifications.Instance.Add(InsurancePos,
                "Krankenversicherung",
                "Benutze E um eine Krankenversicherung zu wählen!");


            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key == Key.E && !dbPlayer.RageExtension.IsInVehicle && dbPlayer.CanInteract())
            {
                if (dbPlayer.Player.Position.DistanceTo(InsurancePos) < 1.0f)
                {
                    ComponentManager.Get<InsuranceWindow>().Show()(dbPlayer);
                }
            }

            return false;
        }

        public Vector3 GetClosestHospital(Vector3 Position)
        {
            Vector3 returnPos = HospitalPositions.First();
            foreach (Vector3 pos in HospitalPositions)
            {
                if (Position.DistanceTo(pos) < Position.DistanceTo(returnPos))
                {
                    returnPos = pos;
                }
            }

            return returnPos;
        }

        public override void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint hash)
        {
            dbPlayer.CancelPhoneCall();
            dbPlayer.ClosePhone();

            if (dbPlayer.Paintball == 1) return;

            //SPAWN Player to Trigger all DeathScripts
            NAPI.Task.Run(() => { dbPlayer.Spawn(dbPlayer.Player.Position); });

            var killedByPlayer = killer.ToPlayer() != dbPlayer.Player;
            DbPlayer iKiller = killer.ToPlayer().GetPlayer();

            // avoid some cuff bugs
            dbPlayer.SetCuffed(false);
            if (dbPlayer.HasData("follow")) dbPlayer.ResetData("follow");

            if (hash == 3452007600)
            {
                if (iKiller != null && iKiller.IsValid() && iKiller.RageExtension.IsInVehicle)
                {
                    hash = 133987706; // Run over by car (if other player is involved in Fall)
                }
            }

            string killerweapon = Convert.ToString((WeaponHash)hash) != "" ? Convert.ToString((WeaponHash)hash) : "unbekannt";

            if (HalloweenModule.isActive)
            {
                if (iKiller != null && iKiller.IsValid())
                {
                    if (iKiller.IsZombie() && !dbPlayer.IsZombie())
                    {
                        dbPlayer.GiveCWS(CWSTypes.Halloween, 2);
                    }
                    else if (!iKiller.IsZombie() && dbPlayer.IsZombie())
                    {
                        dbPlayer.GiveCWS(CWSTypes.Halloween, 4);
                    }
                }

                dbPlayer.InfestPlayer();
                return;
            }

            if (iKiller != null && iKiller.IsValid() && (iKiller.IsACop() || iKiller.IsAMedic()) && (WeaponHash)hash == WeaponHash.Smg && !dbPlayer.IsInjured())
            {
                dbPlayer.SetData("SMGkilledPos", dbPlayer.Player.Position);
                dbPlayer.SetData("SMGkilledDim", dbPlayer.Player.Dimension);
                Logger.Debug(dbPlayer.GetName() + " was killed with SMG (Cop)!");
                return;
            }

            //Set Invincible until revive 
            dbPlayer.Player.TriggerNewClient("setInvincible", true);

            if (dbPlayer.DimensionType[0] == DimensionTypes.RacingArea || dbPlayer.HasData("inRacing"))
            {
                dbPlayer.RemoveFromRacing();
                dbPlayer.dead_x[0] = RacingModule.RacingMenuPosition.X;
                dbPlayer.dead_y[0] = RacingModule.RacingMenuPosition.Y;
                dbPlayer.dead_z[0] = RacingModule.RacingMenuPosition.Z;
            }

            if (dbPlayer.IsNewbie())
            {
                dbPlayer.Revive();
                return;
            }

            // SetTMP Dimension
            dbPlayer.SetData("tmpDeathDimension", dbPlayer.Player.Dimension);

            if (!dbPlayer.IsAlive()) return; // Erneuter Tot verhindern

            // if death in jail add jailtime +10
            if (dbPlayer.JailTime[0] > 0)
            {
                dbPlayer.JailTime[0] += 2;
            }

            dbPlayer.Player.TriggerNewClient("startScreenEffect", "DeathFailMPIn", 5000, true);


            var rnd = new Random();
            var injuryCauseOfDeath = InjuryCauseOfDeathModule.Instance.GetAll().Values.ToList().Find(iCoD => iCoD.Hash == hash) ??
                                     InjuryCauseOfDeathModule.Instance.GetAll()[5];

            //if (dbPlayer.GetName().Contains("Walid_Mohammad"))
            //{
            //    dbPlayer.SendNewNotification($"InjuryCauseOfDeath: {injuryCauseOfDeath.Name}");
            //}

            var injuryType = injuryCauseOfDeath.InjuryTypes.OrderBy(x => rnd.Next()).ToList().First() ??
                             injuryCauseOfDeath.InjuryTypes.First(i => i.Id == 1);


            VoiceListHandler.AddToDeath(dbPlayer);

            if (dbPlayer.HasData("injured_by_nutrition"))
            {
                if (dbPlayer.GetData("injured_by_nutrition"))
                {
                    injuryType = InjuryTypeModule.Instance.Get(19); //Herzinfarkt
                    dbPlayer.ResetData("injured_by_nutrition");
                }
            }
            // Player Died in Gangwar
            // TODO: Move to according Module
            if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
            {
                // in GW Gebiet
                if (dbPlayer.DimensionType[0] == DimensionTypes.Gangwar)
                {
                    injuryType = InjuryTypeModule.Instance.Get(InjuryGangwar);

                    if (dbPlayer.HasData("gangwarId"))
                    {
                        GangwarTown gangwarTown = GangwarTownModule.Instance.Get(dbPlayer.GetData("gangwarId"));

                        // Player is in Range

                        if (dbPlayer.Team.Id == gangwarTown.AttackerTeam.Id)
                        {
                            gangwarTown.IncreasePoints(GangwarModule.Instance.KillPoints, 0);
                            gangwarTown.DefenderTeam.SendNotification($"+ {GangwarModule.Instance.KillPoints} Punkte fuer toeten eines Gegners!");
                        }
                        else if (dbPlayer.Team.Id == gangwarTown.DefenderTeam.Id)
                        {
                            gangwarTown.IncreasePoints(0, GangwarModule.Instance.KillPoints);
                            gangwarTown.AttackerTeam.SendNotification($"+ {GangwarModule.Instance.KillPoints} Punkte fuer toeten eines Gegners!");
                        }
                    }
                }
            }

            // Blacklist
            // TODO: Move to according Module
            if (iKiller != null && iKiller.IsValid() && iKiller.IsAGangster())
            {
                if (dbPlayer.IsOnBlacklist((int)iKiller.TeamId))
                {
                    iKiller.Team.IncreaseBlacklist(dbPlayer);

                    int type = dbPlayer.GetBlacklistType((int)iKiller.TeamId);
                    int blCosts = 0;

                    // Costs on Blacklist Death
                    if (type == 1)
                    {
                        blCosts = 5000;
                    }
                    else if (type == 2)
                    {
                        blCosts = 8000;
                    }
                    else
                    {
                        blCosts = 3000;
                    }
                    // multiplier w level  R: (Kosten/25 * Level) + Kosten
                    // BSP: Level 31 bei Type 1(ist eigtl typ 2 da aber mit 0 bla..)  Sind es 5000 + (5000/25 * Level) = 11.200$
                    blCosts = ((blCosts / 25) * dbPlayer.Level) + blCosts;

                    dbPlayer.TakeBankMoney(blCosts, "Blacklisteintrag - " + iKiller.Team.Name);
                    dbPlayer.SendNewNotification($"Durch deinen Blacklisteintrag hast du nun ${blCosts} zusätzlich gezahlt!");
                }
            }

            // Assign Injurys Data to Player (Zeit wird hochgesetzt beim Tot aufzählend... von Daher nicht nötig die Deadtime zu setzen)
            dbPlayer.Injury = injuryType;

            if (dbPlayer.RageExtension.IsInVehicle && dbPlayer.Player.VehicleSeat != 0)
            {
                dbPlayer.dead_x[0] = dbPlayer.Player.Vehicle.Position.X;
                dbPlayer.dead_y[0] = dbPlayer.Player.Vehicle.Position.Y;
                dbPlayer.dead_z[0] = dbPlayer.Player.Vehicle.Position.Z;
            }
            else
            {
                dbPlayer.dead_x[0] = dbPlayer.Player.Position.X;
                dbPlayer.dead_y[0] = dbPlayer.Player.Position.Y;
                dbPlayer.dead_z[0] = dbPlayer.Player.Position.Z;
            }
            dbPlayer.deadtime[0] = 0;

            dbPlayer.ApplyDeathEffects();

            dbPlayer.ResyncWeaponAmmo();

            if (dbPlayer.GovLevel.ToLower() == "a" || dbPlayer.GovLevel.ToLower() == "b" || dbPlayer.GovLevel.ToLower() == "c")
            {
                foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers().Where(m => m.ParamedicLicense && m.TeamId == (uint)TeamTypes.TEAM_ARMY && m.IsInDuty()).ToList())
                {
                    xPlayer.SendNewNotification($"Eine verletzte Person der Sicherheitsfreigabe {dbPlayer.GovLevel} wurde gemeldet!");
                }
            }
        }

        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!dbPlayer.IsInjured()) return;

            if (dbPlayer.Injury.Id != InjuryModule.Instance.InjuryKrankentransport && !dbPlayer.RageExtension.IsInVehicle)
            {
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");
            }

            if (!dbPlayer.HasData("InjuryMovePointID"))
            {
                // isch da son medischiner in der näh? dann machn wa ken timer runna sonst gibbet huddel
                if (TeamModule.Instance.Get((uint)TeamTypes.TEAM_MEDIC).Members.Values.ToList().Where(m => m != null && m.IsValid() &&
                m.Player.Position.DistanceTo(dbPlayer.Player.Position) < 10.0f && !m.IsInjured() && !m.IsCuffed && !m.IsTied && m.IsInDuty()).Count() <= 0)
                {
                    dbPlayer.deadtime[0]++;
                }
            }

            dbPlayer.Player.TriggerNewClient("startScreenEffect", "DeathFailMPIn", 5000, true);

            // Deadtime > max injury Time?
            if (dbPlayer.deadtime[0] <= dbPlayer.Injury.TimeToDeath) return;

            // Self healing
            if (dbPlayer.Injury.Id == InjuryBruise)
            {
                dbPlayer.Revive();
                dbPlayer.SendNewNotification($"Deine Verletzung war nicht ausschlaggebend! Du stehst nun wieder.");

                return;
            }

            if (dbPlayer.Injury.Id == InjuryBleeding)
            {
                var randomInt = Main.Random.Next(1, 100);
                if (randomInt >= 65)
                {
                    dbPlayer.Revive();
                    dbPlayer.SendNewNotification($"Du hattest Glück: Deine Verletzung war nicht ausschlaggebend! Du stehst nun wieder.");

                    return;
                }
            }

            if (dbPlayer.Injury.Id == InjuryGangwar)
            {
                dbPlayer.Revive();
                if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
                {
                    dbPlayer.Player.SetPosition(GangwarTownModule.Instance.GetGangwarTownSpawnByTeam(dbPlayer.Team));
                }
                else
                {
                    TeamSpawn spawn = dbPlayer.Team.TeamSpawns.FirstOrDefault().Value;
                    if (spawn == null)
                    {
                        TeamfightFunctions.RemoveFromGangware(dbPlayer);
                    }
                    else
                    {
                        NAPI.Task.Run(() => { dbPlayer.Player.Position = spawn.Position; });
                        TeamfightFunctions.RemoveFromGangware(dbPlayer);
                    }
                }

                dbPlayer.StopAnimation();

                if (!GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
                {
                    TeamfightFunctions.RemoveFromGangware(dbPlayer);
                }
                else TeamfightFunctions.SetToGangware(dbPlayer);
                return;
            }
            else if (dbPlayer.Injury.Id == InjuryTeamfight)
            {
                dbPlayer.Revive();
                PlayerSpawn.OnPlayerSpawn(dbPlayer.Player);
                return;
            }
            else
            {
                if (dbPlayer.Injury.Id != Instance.InjuryDeathScreenId)
                    dbPlayer.SetDeathScreen();
                else
                    dbPlayer.SetPlayerDied();
                return;
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShapeState == ColShapeState.Enter && colShape.HasData("injuryDeliverId"))
            {
                if (dbPlayer.IsInjured() && dbPlayer.RageExtension.IsInVehicle)
                {
                    SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
                    if (sxVehicle == null || !sxVehicle.IsTeamVehicle()) return false;

                    InjuryDeliver injuryDelivery = InjuryDeliverModule.Instance.Get((uint)colShape.GetData<uint>("injuryDeliverId"));

                    if (injuryDelivery == null) return false;

                    if (dbPlayer.Injury.Id == (int)InjuryModule.Instance.InjuryDeathScreenId) return false; // Koma...

                    // Get Driver of Vehicle
                    DbPlayer medic = sxVehicle.GetOccupants().GetDriver();
                    if (medic == null || !medic.IsValid()) return false;

                    // Check Bad Medic and license stuff
                    if (!medic.IsAMedic() && !medic.ParamedicLicense)
                        return false;

                    // Kann nur eingeliefert werden, wenn der medic im dienst ist
                    if ((medic.IsAGangster() || medic.IsBadOrga()) && !medic.InParamedicDuty)
                        return false;

                    if (!injuryDelivery.BadMedics && medic.IsAGangster()) return false;

                    Task.Run(async () =>
                    {
                        int sleep = 0;
                        if (lastInjuryDelivered.AddMilliseconds(1000) > DateTime.Now)
                        {
                            sleep = 1000;
                        }

                        lastInjuryDelivered = DateTime.Now;

                        await Task.Delay(sleep);

                        InjuryDeliverIntPoint injuryDeliverIntPoint = injuryDelivery.GetFreePoint();
                        if (injuryDeliverIntPoint == null)
                        {
                            medic.SendNewNotification("Keine Liege im Krankenhaus zur Verfügung!");
                            return;
                        }

                        Vector3 positionToDeliver = injuryDeliverIntPoint.Position;
                        positionToDeliver.Z += 0.5f;

                        NAPI.Task.Run(() =>
                        {

                            // Deliver him to Desk
                            dbPlayer.Player.SetPosition(positionToDeliver);
                            dbPlayer.Player.SetRotation(injuryDeliverIntPoint.Heading);
                            dbPlayer.SetDimension((uint)injuryDeliverIntPoint.Dimension);
                        });

                        // up and down to prevent underbed.. maybe
                        await Task.Delay(1000);
                        positionToDeliver.Z -= 0.5f;

                        NAPI.Task.Run(() =>
                        {
                            // Deliver him to Desk
                            dbPlayer.Player.SetPosition(positionToDeliver);
                            dbPlayer.Player.SetRotation(injuryDeliverIntPoint.Heading);
                        });

                        // Resett time in KH and redo pos save
                        dbPlayer.dead_x[0] = dbPlayer.Player.Position.X;
                        dbPlayer.dead_y[0] = dbPlayer.Player.Position.Y;
                        dbPlayer.dead_z[0] = dbPlayer.Player.Position.Z;
                        dbPlayer.deadtime[0] = 0;

                        dbPlayer.Player.TriggerNewClient("noweaponsoninjury", false);
                        dbPlayer.SendNewNotification($"Sie wurden ins Krankenhaus eingewiesen!");

                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");


                        dbPlayer.PhoneSettings.flugmodus = true;
                        VoiceModule.Instance.ChangeFrequenz(dbPlayer, 0, true);
                        VoiceModule.Instance.turnOffFunk(dbPlayer);

                        await Task.Delay(500);
                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");
                    });

                    return true;
                }
            }
            return false;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.Injury = InjuryTypeModule.Instance.Get((uint)reader.GetInt32("deadstatus"));

            dbPlayer.deadtime = new int[2];
            dbPlayer.deadtime[1] = reader.GetInt32("deadtime");
            dbPlayer.deadtime[0] = reader.GetInt32("deadtime");
            dbPlayer.dead_x = new float[2];
            dbPlayer.dead_x[1] = reader.GetFloat("dead_x");
            dbPlayer.dead_x[0] = reader.GetFloat("dead_x");
            dbPlayer.dead_y = new float[2];
            dbPlayer.dead_y[1] = reader.GetFloat("dead_y");
            dbPlayer.dead_y[0] = reader.GetFloat("dead_y");
            dbPlayer.dead_z = new float[2];
            dbPlayer.dead_z[1] = reader.GetFloat("dead_z");
            dbPlayer.dead_z[0] = reader.GetFloat("dead_z");
        }

        public override void OnPlayerWeaponSwitch(DbPlayer dbPlayer, WeaponHash oldgun, WeaponHash newgun)
        {
            if (newgun == WeaponHash.Unarmed)
                return;

            if (dbPlayer.RecentlyInjured)
            {
                if (dbPlayer.TimeSinceTreatment.AddMinutes(5) > DateTime.Now)
                {
                    NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed);
                    dbPlayer.SendNewNotification("Du fühlst dich noch zu schwach um eine Waffe zu bedienen!");
                    return;
                }

                dbPlayer.RecentlyInjured = false;
            }

            if (dbPlayer.drink[0] <= NutritionModule.Instance.Underfed || dbPlayer.food[0] <= NutritionModule.Instance.Underfed
                || dbPlayer.drink[0] >= NutritionModule.Instance.Overfed || dbPlayer.food[0] >= NutritionModule.Instance.Overfed)
            {
                NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed);
                dbPlayer.SendNewNotification("Du fühlst dich nicht gut und kannst keine Waffe halten");
                return;
            }
        }

        [CommandPermission()]
        [Command(GreedyArg = true)]
        public void Commandinstrev(Player player, string name)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || dbPlayer.TeamId != (int)TeamTypes.TEAM_MEDIC) return;


            var findPlayer = Players.Players.Instance.FindPlayer(name);
            if (findPlayer == null || findPlayer.IsAlive()) return;

            findPlayer.Revive();

            dbPlayer.SendNewNotification(
          "Sie haben " + findPlayer.GetName() +
                " per INSTANT revived!");
            findPlayer.SendNewNotification(
          "Medic " + dbPlayer.GetName() +
                " hat Sie per INSTANT revived!");


            Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                "MEDIC " + dbPlayer.GetName() + " hat " + findPlayer.GetName() +
                " per INSTANT revived!");

            PlayerSpawn.OnPlayerSpawn(findPlayer.Player);
        }

        [CommandPermission()]
        [Command(GreedyArg = true)]
        public void Commandgiveparalic(Player player, string name)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || dbPlayer.TeamId != (int)TeamTypes.TEAM_MEDIC) return;


            var findPlayer = Players.Players.Instance.FindPlayer(name);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            // Check Team and Slots
            if (findPlayer.Team.MedicSlots == 0 || findPlayer.Team.MedicSlots <= findPlayer.Team.MedicSlotsUsed)
            {
                dbPlayer.SendNewNotification("Diese Fraktion hat bereits die maximale Anzahl erreicht!");
                return;
            }

            if (findPlayer.ParamedicLicense)
            {
                dbPlayer.SendNewNotification("Spieler hat bereits eine Notfallmedizin Lizenz!");
                return;
            }

            if (!findPlayer.TakeMoney(350000))
            {
                dbPlayer.SendNewNotification("Eine Notfallmedizin Lizenz kostet 350.000$!");
                return;
            }

            findPlayer.SetParamedicLicense();

            dbPlayer.SendNewNotification($"Sie haben {findPlayer.GetName()} eine Notfallmedizin Lizenz für 350.000$ ausgestellt!");
            findPlayer.SendNewNotification($"Arzt {dbPlayer.GetName()} hat ihnen eine Notfallmedizin Lizenz für 350.000$ ausgestellt!");
        }

        [CommandPermission()]
        [Command(GreedyArg = true)]
        public void Commandremoveparalic(Player player, string name)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || dbPlayer.TeamId != (int)TeamTypes.TEAM_MEDIC) return;


            var findPlayer = Players.Players.Instance.FindPlayer(name);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            if (dbPlayer.Player.Position.DistanceTo(findPlayer.Player.Position) > 5.0f)
            {
                dbPlayer.SendNewNotification($"Der Bürger {findPlayer.GetName()} befindet sich nicht in deiner Nähe!", PlayerNotification.NotificationType.ERROR, "Fehler!");
                return;
            }

            if (findPlayer.ParamedicLicense)
            {
                findPlayer.RemoveParamedicLicense();

                dbPlayer.SendNewNotification($"Sie haben {findPlayer.GetName()} die Notfallmedizin Lizenz entzogen!");
                findPlayer.SendNewNotification($"Arzt {dbPlayer.GetName()} hat ihnen die Notfallmedizin Lizenz entzogen!");
                return;
            }

            dbPlayer.SendNewNotification($"Der Bürger {findPlayer.GetName()} besitzt keine Notfallmedizin Lizenz!", PlayerNotification.NotificationType.ERROR, "Fehler!");
        }
    }
}