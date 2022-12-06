using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Weapons;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR.Module.Blitzer
{
    public class BlitzerModule : SqlModule<BlitzerModule, Blitzer, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `blitzer`;";
        }

        public Dictionary<int, int> BlitzerLoaded = new Dictionary<int, int>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcreateblitzer(Player player, string arguments)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !Configurations.Configuration.Instance.DevMode) return;

            if (!Int32.TryParse(arguments, out int range)) return;

            dbPlayer.SetData("blitzercreate_pos", dbPlayer.Player.Position);
            dbPlayer.SetData("blitzercreate_range", range);

            dbPlayer.SendNewNotification("Blitzer Shape angelegt, nutze nun /saveblitzer [kmh] [Gruppe] (An der Objektposition)");

            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsaveblitzer(Player player, string arguments)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !Configurations.Configuration.Instance.DevMode) return;


            string[] argsSplit = arguments.Split(' ');
            if (argsSplit.Length < 2) return;

            if (!Int32.TryParse(argsSplit[0], out int kmh)) return;
            if (!Int32.TryParse(argsSplit[1], out int group)) return;

            if (!dbPlayer.HasData("blitzercreate_pos") || !dbPlayer.HasData("blitzercreate_range")) return;

            Vector3 colShapePos = dbPlayer.GetData("blitzercreate_pos");
            int colShapeRange = dbPlayer.GetData("blitzercreate_range");

            string x = colShapePos.X.ToString().Replace(",", ".");
            string y = colShapePos.Y.ToString().Replace(",", ".");
            string z = colShapePos.Z.ToString().Replace(",", ".");


            string objx = player.Position.X.ToString().Replace(",", ".");
            string objy = player.Position.Y.ToString().Replace(",", ".");
            string objz = player.Position.Z.ToString().Replace(",", ".");
            string objrotz = player.Heading.ToString().Replace(",", ".");

            MySQLHandler.ExecuteAsync($"INSERT INTO `blitzer` (`pos_x`, `pos_y`, `pos_z`, `speed_limit`, `obj_pos_x`, `obj_pos_y`, `obj_pos_z`, `obj_heading`, `group`, `range`) " +
                $"VALUES ('{x}', '{y}', '{z}', '{kmh}', '{objx}', '{objy}', '{objz}', '{objrotz}', '{group}', '{colShapeRange}');");

            dbPlayer.SendNewNotification("Blitzer angelegt!");
            return;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if(colShapeState == ColShapeState.Exit)
            {
                if(colShape.HasData("inBlitzerRange"))
                {
                    dbPlayer.ResetData("inBlitzerRange");
                }
            }

            if(colShapeState == ColShapeState.Enter)
            {
                if (colShape.HasData("blitzer"))
                {
                    if (dbPlayer.HasData("inBlitzerRange"))
                    {
                        dbPlayer.ResetData("inBlitzerRange");
                        return false;
                    }
                    dbPlayer.SetData("inBlitzerRange", colShape.GetData<uint>("blitzer"));

                    Blitzer xBlitzer = BlitzerModule.Instance.GetAll().Values.Where(b => b.Id == colShape.GetData<uint>("blitzer")).FirstOrDefault();
                    if (xBlitzer == null || !xBlitzer.Active)
                    {
                        dbPlayer.ResetData("inBlitzerRange");
                        return false;
                    }

                    // in Fahrzeug, kein cop medic oder regierung
                    NAPI.Task.Run(() =>
                    {
                        if (dbPlayer.RageExtension.IsInVehicle)
                        {
                            if (dbPlayer.IsInGuideDuty() || dbPlayer.IsInGameDesignDuty() || dbPlayer.Dimension[0] != 0
                                || dbPlayer.IsInAdminDuty() || dbPlayer.DimensionType[0] != DimensionType.World)
                            {
                                dbPlayer.ResetData("inBlitzerRange");
                                return;
                            }

                            // Nur wenn fahrer
                            if (dbPlayer.Player.VehicleSeat == 0)
                            {
                                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();

                                // Z Koordinate < -3Blitzer (wegen Tunnel etc) oder > +10 (flugzeug bla)
                                if (sxVeh == null || !sxVeh.IsValid() ||
                                    sxVeh.entity.Position.Z < (xBlitzer.Position.Z - 5.0f) || sxVeh.entity.Position.Z > (xBlitzer.Position.Z + 5.0f))
                                {

                                    dbPlayer.ResetData("inBlitzerRange");
                                    return;
                                }

                                // wenn FIB Karre & ID
                                if (dbPlayer.TeamId == (uint)TeamTypes.TEAM_FIB && dbPlayer.IsInDuty() && sxVeh.teamid == (uint)TeamTypes.TEAM_FIB)
                                {
                                    dbPlayer.ResetData("inBlitzerRange");
                                    return;
                                }

                                // Govlevel zwecks immunität
                                if (dbPlayer.GovLevel.ToLower() == "a" || dbPlayer.GovLevel.ToLower() == "b" || dbPlayer.GovLevel.ToLower() == "c")
                                {
                                    dbPlayer.ResetData("inBlitzerRange");
                                    return;
                                }

                                // SWAT generell
                                if (dbPlayer.TeamId == (uint)TeamTypes.TEAM_SWAT && dbPlayer.IsInDuty())
                                {
                                    dbPlayer.ResetData("inBlitzerRange");
                                    return;
                                }

                                if (sxVeh.SirensActive)
                                {
                                    dbPlayer.ResetData("inBlitzerRange");
                                    return;
                                }

                                if (dbPlayer.HasData("BlitzerTimestamp"))
                                {
                                    DateTime date = (DateTime)dbPlayer.GetData("BlitzerTimestamp");
                                    if (date.AddMinutes(1) >= DateTime.Now)
                                    {
                                        dbPlayer.ResetData("inBlitzerRange");
                                        dbPlayer.ResetData("BlitzerTimestamp");
                                        return;
                                    }
                                }

                                int speed = sxVeh.GetSpeed();
                                if ((speed - xBlitzer.Tolleranz) > xBlitzer.SpeedLimit)
                                {
                                    int differenz = speed - xBlitzer.SpeedLimit;
                                    int wantedReasonId = 110; // Standard-Fall (0 - 20)

                                    // 20-50 Überschreitung Strafe
                                    if (differenz > 20 && differenz < 50)
                                    {
                                        wantedReasonId = 111;
                                    }
                                    else if (differenz >= 50 && differenz < 100) // 50-100 Überschreitung Strafe
                                    {
                                        wantedReasonId = 112;
                                    }
                                    else if (differenz > 100) // 100+ Überschreitung Strafe
                                    {
                                        wantedReasonId = 113;
                                    }

                                    try
                                    {
                                        dbPlayer.SetData("BlitzerTimestamp", DateTime.Now);
                                        string wantedstring = $"{sxVeh.Data.Model} ({sxVeh.databaseId}) mit {speed}/{xBlitzer.SpeedLimit} geblitzt - { DateTime.Now.Hour}:{ DateTime.Now.Minute} { DateTime.Now.Day}/{ DateTime.Now.Month}/{ DateTime.Now.Year}";
                                        dbPlayer.AddCrime("Leitstelle", CrimeReasonModule.Instance.Get((uint)wantedReasonId), wantedstring);
                                        dbPlayer.SendNewNotification($"Fahrzeug {sxVeh.Data.Model} ({sxVeh.databaseId}) wurde mit {speed}/{xBlitzer.SpeedLimit} km/h geblitzt! (Toleranz: {xBlitzer.Tolleranz} km/h einberechnet)", PlayerNotification.NotificationType.ERROR, title: "Blitzer", duration: 10000);
                                        dbPlayer.Player.TriggerNewClient("startScreenEffect", "MP_SmugglerCheckpoint", 3000, false);
                                        dbPlayer.Player.TriggerNewClient("startsoundplay", "Camera_Shoot", "Phone_Soundset_Franklin");
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Crash(e);
                                        dbPlayer.ResetData("inBlitzerRange");
                                        return;
                                    }
                                }

                                dbPlayer.ResetData("inBlitzerRange");
                                return;
                            }
                        }
                        else
                        {
                            dbPlayer.ResetData("BlitzerTimestamp");
                            dbPlayer.ResetData("inBlitzerRange");
                        }
                    });

                    return true;
                }
            }
            return false;
        }
    }
}