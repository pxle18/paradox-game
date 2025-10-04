using GTANetworkAPI;
using System;
using System.Linq;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Gangwar;
using System.Threading.Tasks;
using VMP_CNR.Module.Einreiseamt;
using VMP_CNR.Module.AnimationMenu;
using VMP_CNR.Module.Swat;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Paintball;
using Newtonsoft.Json;
using VMP_CNR.Handler;
using VMP_CNR.Module.Anticheat;
using Google.Protobuf.WellKnownTypes;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Players.Windows;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Chat;
using static VMP_CNR.Module.Chat.Chats;

namespace VMP_CNR.Module.Players.Events
{
    public class PlayerSpawn : Script
    {
        private static async Task<string> GetRandomColor()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = $"SELECT value FROM `namegeneration_color` ORDER BY RAND() LIMIT 1;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            return reader.GetString("value");
                        }
                    }
                }
                await conn.CloseAsync();
            }
            return "";
        }
        private static async Task<string> GetRandomAnimal()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = $"SELECT value FROM `namegeneration_animals` ORDER BY RAND() LIMIT 1;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            return reader.GetString("value");
                        }
                    }
                }
                await conn.CloseAsync();
            }
            return "";
        }

        public static async void GeneratePlayerRandomizedName(DbPlayer dbPlayer)
        {
            Random rnd = new Random();

            string xx = await GetRandomColor() + await GetRandomAnimal() + rnd.Next(0, 100);

            while (NAPI.Pools.GetAllPlayers().Where(p => p.Name == xx).Count() > 0)
            {
                xx = await GetRandomColor() + await GetRandomAnimal() + rnd.Next(0, 100);
            }

            Logger.InsertGeneratedName(dbPlayer.Id, dbPlayer.Player.Name, xx);
            dbPlayer.Player.Name = xx;
            return;
        }

        public static void InitPlayerSpawnData(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            Task.Run(async () =>
            {
                await Task.Delay(3000);
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (dbPlayer.IsInjured())
                {
                    dbPlayer.Player.TriggerNewClient("startScreenEffect", "DeathFailMPIn", 5000, true);
                }
                else
                {
                    dbPlayer.Player.TriggerNewClient("stopScreenEffect", "DeathFailMPIn");
                    dbPlayer.StopAnimation();
                }
            });

            dbPlayer.Player.TriggerNewClient("updateInjured", dbPlayer.IsInjured());

            NAPI.Task.Run(() =>
            {
                //dbPlayer.Player.SetSharedData("deathStatus", dbPlayer.isInjured());
                player.Transparency = 255;
            });

            player.TriggerNewClient("setPlayerHealthRechargeMultiplier");

            // Workaround for freeze fails
            if (dbPlayer.IsFreezed == false)
            {
                player.TriggerNewClient("freezePlayer", false);
            }
        }


        [ServerEvent(Event.PlayerSpawn)]
        public static void OnPlayerSpawnEvent(Player player)
        {
            if (player == null) return;
            OnPlayerSpawn(player);
        }

        public static void OnPlayerSpawn(Player player)
        {
            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(500);

                while (player == null)
                {
                    await NAPI.Task.WaitForMainThread(50);
                }

                try
                {
                    var firstName = "";
                    var lastName = "";

                    if (player == null) return;

                    DbPlayer dbPlayer = player.GetPlayer();

                    string l_EventKey = Helper.Helper.GenerateAuthKey();
                    if (player.HasData("auth_key"))
                        player.ResetData("auth_key");

                    player.SetData<string>("auth_key", l_EventKey);

                    if (!player.HasData("connectedAt") && (dbPlayer == null || !dbPlayer.IsValid()))
                    {
                        player.Health = 99;
                        
                        // da isn anderer Spieler?
                        Player olderPlayer = NAPI.Pools.GetAllPlayers().ToList().Where(p => p != null && p.Name == player.Name && p.HasData("connectedAt")).FirstOrDefault();

                        if (olderPlayer != null && olderPlayer != player && (!olderPlayer.HasData("Connected") || olderPlayer.GetData<bool>("Connected") != true))
                        {
                            olderPlayer.SendNotification("Duplicate Entry 1");
                            olderPlayer.ResetData("Duplicate Entry!");
                            olderPlayer.Kick("Duplicate Entry!");

                            return;
                        }

                        player.SetData<DateTime>("connectedAt", DateTime.Now);
                        PlayerConnect.OnPlayerConnected(player);
                        return;
                    }
                    else if (dbPlayer == null || !dbPlayer.IsValid())
                    {
                        await NAPI.Task.WaitForMainThread(2000);
                        // Re-fetch dbPlayer and guard against null
                        dbPlayer = player.GetPlayer();
                        if (dbPlayer == null || !dbPlayer.IsValid())
                        {
                            return;
                        }
                        dbPlayer.SetAcPlayerSpawnDeath();
                        if (dbPlayer.IsFirstSpawn)
                        {
                            Modules.Instance.OnPlayerLoggedIn(dbPlayer);
                        }
                    }
                    else
                    {
                        dbPlayer.SetAcPlayerSpawnDeath();
                        if (dbPlayer.IsFirstSpawn)
                        {
                            Modules.Instance.OnPlayerLoggedIn(dbPlayer);
                        }
                    }

                    // Interrupt wrong Spawn saving
                    dbPlayer.ResetData("lastPosition");

                    Modules.Instance.OnPlayerSpawn(dbPlayer);

                    // init Spawn details
                    var pos = new Vector3();
                    float heading = 0.0f;

                    uint dimension = 0;
                    DimensionTypes dimensionType = DimensionTypes.World;

                    // Default Data required for Spawn
                    bool FreezedNoAnim = false;

                    if (dbPlayer.IsNewbie())
                    {
                        if (dbPlayer.IsInjured()) dbPlayer.Revive();

                        dbPlayer.JailTime[0] = 0;
                        dbPlayer.ApplyCharacter();

                        pos = new GTANetworkAPI.Vector3(-1144.26, -2792.27, 27.708);
                        heading = 237.428f;
                        dimension = 0;

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.Player.SetPosition(pos);
                        dbPlayer.SetDimension(dimension);

                    }
                    // SMG Kill (Gummigeschosse)
                    else if (dbPlayer.HasData("SMGkilledPos") && dbPlayer.HasData("SMGkilledDim"))
                    {
                        pos = (Vector3)dbPlayer.GetData("SMGkilledPos");
                        heading = 0.0f;
                        dimension = dbPlayer.GetData("SMGkilledDim");

                        dbPlayer.SetStunned(true);
                        FreezedNoAnim = true;
                    }
                    // Verletzt
                    else if (dbPlayer.IsInjured())
                    {
                        pos.X = dbPlayer.dead_x[0];
                        pos.Y = dbPlayer.dead_y[0];
                        pos.Z = dbPlayer.dead_z[0];
                        FreezedNoAnim = true;

                        if (dbPlayer.HasData("tmpDeathDimension"))
                        {
                            dimension = dbPlayer.GetData("tmpDeathDimension");
                            dbPlayer.ResetData("tmpDeathDimension");
                        }

                        if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team) && dbPlayer.DimensionType[0] == DimensionTypes.Gangwar)
                        {
                            dimension = GangwarModule.Instance.DefaultDimension;
                        }

                        if (dbPlayer.Injury.StabilizedInjuryId != 0 && dbPlayer.Injury.Id != InjuryModule.Instance.InjuryKrankentransport)
                        {
                            VoiceListHandler.AddToDeath(dbPlayer);
                        }
                        else VoiceListHandler.RemoveFromDeath(dbPlayer);
                    }
                    // Koma after
                    else if (dbPlayer.HasData("komaSpawn"))
                    {
                        dbPlayer.ResetData("komaSpawn");

                        // Guard dead position arrays
                        float dx = (dbPlayer.dead_x != null && dbPlayer.dead_x.Length > 0) ? dbPlayer.dead_x[0] : player.Position.X;
                        float dy = (dbPlayer.dead_y != null && dbPlayer.dead_y.Length > 0) ? dbPlayer.dead_y[0] : player.Position.Y;
                        float dz = (dbPlayer.dead_z != null && dbPlayer.dead_z.Length > 0) ? dbPlayer.dead_z[0] : player.Position.Z;
                        Vector3 spawnPos = InjuryModule.Instance.GetClosestHospital(new Vector3(dx, dy, dz));
                        dbPlayer.SetPlayerKomaSpawn();

                        pos.X = spawnPos.X;
                        pos.Y = spawnPos.Y;
                        pos.Z = spawnPos.Z;
                        dimension = 0;
                    }
                    else if (HalloweenModule.isActive && dbPlayer.IsZombie())
                    {

                        pos = HalloweenModule.Instance.GetClosestSpawn(dbPlayer.Player.Position);
                        heading = 0.0f;
                        dimension = 0;

                        dbPlayer.SetStunned(false);
                        FreezedNoAnim = true;
                    }
                    else if (dbPlayer.JailTime[0] > 1 && !dbPlayer.IsFirstSpawn)
                    {
                        //Jail Spawn
                        if (dbPlayer.JailTime[0] > 1)
                        {
                            pos.X = 1691.28f;
                            pos.Y = 2565.91f;
                            pos.Z = 45.5648f;
                            heading = 177.876f;
                            dbPlayer.Player.TriggerNewClient("setInvincible", false);
                        }
                    }
                    else
                    {
                        if (dbPlayer.spawnchange[0] == 1 && (dbPlayer.OwnHouse[0] > 0 || dbPlayer.IsTenant())) //Haus
                        {
                            House iHouse;
                            if ((iHouse = HouseModule.Instance.Get(dbPlayer.OwnHouse[0])) != null)
                            {
                                pos = iHouse.Position;
                                heading = iHouse.Heading;
                            }
                            else if ((iHouse = HouseModule.Instance.Get(dbPlayer.GetTenant().HouseId)) != null)
                            {
                                pos = iHouse.Position;
                                heading = iHouse.Heading;
                            }
                        }
                        else
                        {
                            if (dbPlayer.Team != null && dbPlayer.Team.TeamSpawns != null && dbPlayer.Team.TeamSpawns.TryGetValue(dbPlayer.fspawn != null && dbPlayer.fspawn.Length > 0 ? dbPlayer.fspawn[0] : 0, out var spawn))
                            {
                                pos = spawn.Position;
                                heading = spawn.Heading;
                            }
                            else
                            {
                                spawn = (dbPlayer.Team != null && dbPlayer.Team.TeamSpawns != null) ? dbPlayer.Team.TeamSpawns.FirstOrDefault().Value : null;
                                if (spawn != null)
                                {
                                    pos = spawn.Position;
                                    heading = spawn.Heading;
                                }
                            }
                        }
                    }

                    // Setting Pos
                    if (dbPlayer.IsFirstSpawn)
                    {
                        if (dbPlayer.pos_x != null && dbPlayer.pos_x.Length > 0 && dbPlayer.pos_x[0] != 0f && !dbPlayer.IsNewbie())
                        {
                            dbPlayer.spawnProtection = DateTime.Now;

                            pos = new Vector3(dbPlayer.pos_x[0], dbPlayer.pos_y[0], dbPlayer.pos_z[0] + 0.1f);

                            if (dbPlayer.HasData("cayoPerico"))
                            {                                
                                pos = new Vector3(pos.X, pos.Y, pos.Z + 3.0f);
                            }

                            heading = (dbPlayer.pos_heading != null && dbPlayer.pos_heading.Length > 0) ? dbPlayer.pos_heading[0] : heading;

                        }

                        NAPI.Task.Run(async () =>
                        {
                            await NAPI.Task.WaitForMainThread(9000);
                            Modules.Instance.OnPlayerFirstSpawnAfterSync(dbPlayer);
                            // mark first spawn lifecycle completed for safe armor/health application
                            dbPlayer.FirstSpawnCompleted = true;

                            // resync cuffstate

                            // Cuff & Tie
                            if (dbPlayer.IsCuffed)
                            {
                                dbPlayer.SetCuffed(true);
                                FreezedNoAnim = true;
                            }

                            if (dbPlayer.IsTied)
                            {
                                dbPlayer.SetTied(true);
                                FreezedNoAnim = true;
                            }
                        });

                        Main.OnPlayerFirstSpawn(player);

                        // Fallback ...
                        if (dbPlayer.DimensionType != null && dbPlayer.DimensionType.Length > 0 && dbPlayer.DimensionType[0] == DimensionTypes.Gangwar)
                        {
                            dbPlayer.Dimension[0] = 0;
                            dbPlayer.DimensionType[0] = DimensionTypes.World;
                        }

                        // Load player Dimension from DB
                        dimension = (dbPlayer.Dimension != null && dbPlayer.Dimension.Length > 0) ? dbPlayer.Dimension[0] : 0;
                        dimensionType = (dbPlayer.DimensionType != null && dbPlayer.DimensionType.Length > 0) ? dbPlayer.DimensionType[0] : DimensionTypes.World;

                        DialogMigrator.CloseUserDialog(player, Dialogs.menu_info);

                        // Connect to TS
                        VoiceModule.Instance.Connect(player, dbPlayer.GetName());

                        var crumbs = player.Name.Split('_');
                        if (crumbs.Length > 1)
                        {
                            firstName = crumbs[0].ToString();
                            lastName = crumbs[1].ToString();
                            // Support multiple lastNames
                            for (int i = 2; i < crumbs.Length; i++)
                            {
                                lastName += "_" + crumbs[i];
                            }

                            string insurance = "keine";
                            if (dbPlayer.InsuranceType == 1)
                            {
                                insurance = "vorhanden";
                            }
                            else if (dbPlayer.InsuranceType == 2)
                            {
                                insurance = "privat";
                            }

                            player.TriggerNewClient("SetOwnAnimData", JsonConvert.SerializeObject(new AnimationSyncItem(dbPlayer)));
                            player.TriggerNewClient("onPlayerLoaded", firstName, lastName, dbPlayer.Id, 
                                (dbPlayer.RP != null && dbPlayer.RP.Length > 0) ? dbPlayer.RP[0] : 0,
                                dbPlayer.GetActiveBusiness()?.Id ?? 0, 
                                (dbPlayer.grade != null && dbPlayer.grade.Length > 0) ? dbPlayer.grade[0] : 0, 
                                (dbPlayer.Money != null && dbPlayer.Money.Length > 0) ? dbPlayer.Money[0] : 0, 
                                0,
                                (dbPlayer.OwnHouse != null && dbPlayer.OwnHouse.Length > 0) ? dbPlayer.OwnHouse[0] : 0, 
                                dbPlayer.TeamId, 
                                dbPlayer.TeamRank, 
                                dbPlayer.Level, 
                                dbPlayer.IsInjured(), 
                                dbPlayer.IsInDuty(),
                                dbPlayer.IsTied, 
                                dbPlayer.IsCuffed, 
                                dbPlayer.VoiceHash ?? "", 
                                dbPlayer.funkStatus, 
                                (dbPlayer.handy != null && dbPlayer.handy.Length > 0) ? dbPlayer.handy[0] : 0, 
                                (dbPlayer.job != null && dbPlayer.job.Length > 0) ? dbPlayer.job[0] : 0,
                                (dbPlayer.JobSkill != null && dbPlayer.JobSkill.Length > 0) ? dbPlayer.JobSkill[0] : 0, 
                                dbPlayer.GetJsonAnimationsShortcuts() ?? "[]", 
                                dbPlayer.RankId >= (uint)AdminLevelTypes.FirstLevelTeam,
                                Configuration.Instance.WeaponDamageMultipier, 
                                Configuration.Instance.PlayerSync,
                                Configuration.Instance.VehicleSync, 
                                (dbPlayer.BlackMoney != null && dbPlayer.BlackMoney.Length > 0) ? dbPlayer.BlackMoney[0] : 0, 
                                dbPlayer.Ringtone?.Id ?? 0, 
                                insurance, 
                                (dbPlayer.zwd != null && dbPlayer.zwd.Length > 0) ? dbPlayer.zwd[0] : 0,
                                Configuration.Instance.MeleeDamageMultipier,
                                Configuration.Instance.DamageLog
                                );
                        }
                        else dbPlayer.Kick();

                        // Cuff & Tie
                        if (dbPlayer.IsCuffed)
                        {
                            dbPlayer.SetCuffed(true);
                            FreezedNoAnim = true;
                        }

                        if (dbPlayer.IsTied)
                        {
                            dbPlayer.SetTied(true);
                            FreezedNoAnim = true;
                        }

                        // Anonymize Rage names (anti friendly list in cheats..)
                        if(ServerFeatures.IsActive("ac-randomizedNames")) GeneratePlayerRandomizedName(dbPlayer);
                    }
                    else
                    {
                        dbPlayer.SetHealth(100);
                    }

                    if (dbPlayer.JailTime[0] > 0) dbPlayer.ApplyCharacter();
                    

                    InitPlayerSpawnData(player);

                    dbPlayer.LoadPlayerWeapons();

                    if (dbPlayer.Paintball == 1)
                    {
                        PaintballModule.Instance.Spawn(dbPlayer, false, false);
                        dbPlayer.StopAnimation();
                        return;
                    }
                    else
                    {
                        dbPlayer.Player.SetPosition(pos);
                        dbPlayer.Player.SetRotation(heading);
                        dbPlayer.SetDimension(dimension);

                        // Cayo Special
                        if (dbPlayer.HasData("cayoPerico"))
                        {
                            NAPI.Task.Run(async () =>
                            {
                                dbPlayer.Player.TriggerNewClient("spawnProtection", 3000, 255, false);
                                await NAPI.Task.WaitForMainThread(2000);

                                dbPlayer.Player.SetPosition(pos);
                                dbPlayer.Player.SetRotation(heading);
                                dbPlayer.SetDimension(dimension);
                            });
                        }

                        if (!FreezedNoAnim)
                        {
                            await NAPI.Task.WaitForMainThread(1000);
                            if (dbPlayer == null || !dbPlayer.IsValid()) return;

                            // uncuff....
                            dbPlayer.SetTied(false);
                            dbPlayer.SetMedicCuffed(false);
                            dbPlayer.SetCuffed(false);
                            player.TriggerNewClient("freezePlayer", false);
                        }

                        if (dbPlayer.IsFirstSpawn)
                        {
                            NAPI.Task.Run(async () =>
                            {
                                if (dbPlayer.HasData("firstCharacter"))
                                {
                                    if (dbPlayer.IsInjured()) dbPlayer.Revive();

                                    dbPlayer.JailTime[0] = 0;
                                    dbPlayer.ApplyCharacter(true);

                                    await NAPI.Task.WaitForMainThread(100);

                                    dbPlayer.StartCustomization();
                                }
                                else
                                {
                                    dbPlayer.Player.TriggerNewClient("moveSkyCamera", dbPlayer.Player, "up", 1, false);
                                    await NAPI.Task.WaitForMainThread(4000);

                                    if (dbPlayer == null || !dbPlayer.IsValid()) return;
                                    dbPlayer.Player.TriggerNewClient("moveSkyCamera", dbPlayer.Player, "down", 1, false);

                                    dbPlayer.ApplyCharacter(true);
                                    dbPlayer.ApplyPlayerHealth();

                                    dbPlayer.IsFirstSpawn = false;

                                    if(Configuration.Instance.DevMode) await Chats.SendGlobalMessage($"{dbPlayer.GetName()} hat den Test-Server betreten.", COLOR.RED, ICON.DEV);
                                }
                            });
                        }

                        if (dbPlayer.IsInjured())
                        {
                            dbPlayer.ApplyDeathEffects();
                        }
                        else
                        {
                            dbPlayer.Player.TriggerNewClient("stopScreenEffect", "DeathFailMPIn");
                        }

                        if (dbPlayer.HasData("SMGkilledPos") && dbPlayer.HasData("SMGkilledDim"))
                        {
                            await Task.Run(async () =>
                            {
                                await Task.Delay(1500);
                                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor3_beatup", "guard_beatup_kickidle_dockworker");

                                await Task.Delay(5000);

                                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor3_beatup", "guard_beatup_kickidle_dockworker");

                                await Task.Delay(25000);
                                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                                dbPlayer.SetStunned(false);
                                dbPlayer.ResetData("SMGkilledPos");
                                dbPlayer.Player.TriggerNewClient("setInvincible", false);
                            });
                        }

                        if (HalloweenModule.isActive && dbPlayer.IsZombie())
                        {
                            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                            {
                                await NAPI.Task.WaitForMainThread(3000);
                                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                                dbPlayer.StopAnimation();
                                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                                dbPlayer.IsCuffed = false;
                                dbPlayer.SetCuffed(false);
                                dbPlayer.SetTied(false);

                                await NAPI.Task.WaitForMainThread(3000);
                                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                                dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                            }));
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            });
        }
    }
}
