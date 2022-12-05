using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.CayoPerico
{
    public class CayoPericoModule : Module<CayoPericoModule>
    {
        public static uint FaehreLSGarageId = 1016;
        public static uint FaehreCPGarageId = 1015;
        public static uint FaehreStandbyLSzuCP = 1018;
        public static uint FaehreStandbyCPzuLS = 1017;

        public static Vector3 FaehreCPPosition = new Vector3(4519.72, -4536.85, 4.24648);
        public static float FaehreCPHeading = 18.3487f;

        public static Vector3 FaehreLSPosition = new Vector3(1291.42, -3210.73, 5.90512);
        public static float FaehreLSHeading = 180.036f;

        public ColShape CayoPerico;

        protected override bool OnLoad()
        {
            CayoPerico = Spawners.ColShapes.Create(new Vector3(4840, -5174, 2), 1800.0f);
            CayoPerico.SetData("cayoPerico", true);

            CayoPerico = Spawners.ColShapes.Create(new Vector3(3846.81, -4723.1, 2), 1800.0f);
            CayoPerico.SetData("cayoPerico2", true);

            new Npc(PedHash.Andreas, FaehreCPPosition, FaehreCPHeading, 0);
            new Npc(PedHash.Andreas, FaehreLSPosition, FaehreLSHeading, 0);

            PlayerNotifications.Instance.Add(FaehreCPPosition,
                "Fähre nach Los Santos",
                "Benutze 'E' um dein Fahrzeug zu verschiffen!");


            PlayerNotifications.Instance.Add(FaehreLSPosition,
                "Fähre nach Cayo Perico",
                "Benutze 'E' um dein Fahrzeug zu verschiffen!");

            return base.OnLoad();
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShape.HasData("cayoPerico"))
            {
                if (colShapeState == ColShapeState.Enter)
                {
                    dbPlayer.SetData("cayoPerico", 1);

                    if(!dbPlayer.HasData("cayoPerico2"))
                    {
                        dbPlayer.Player.TriggerNewClient("loadisland", true);
                    }
                }
                else
                {
                    dbPlayer.ResetData("cayoPerico");

                    if (!dbPlayer.HasData("cayoPerico2"))
                    {
                        dbPlayer.Player.TriggerNewClient("loadisland", false);
                    }
                }
            }
            if (colShape.HasData("cayoPerico2"))
            {
                if (colShapeState == ColShapeState.Enter)
                {
                    dbPlayer.SetData("cayoPerico2", 1);

                    if (!dbPlayer.HasData("cayoPerico"))
                    {
                        dbPlayer.Player.TriggerNewClient("loadisland", true);
                    }
                }
                else
                {
                    dbPlayer.ResetData("cayoPerico2");

                    if (!dbPlayer.HasData("cayoPerico"))
                    {
                        dbPlayer.Player.TriggerNewClient("loadisland", false);
                    }
                }
            }
            return false;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if(dbPlayer.IsValid() && dbPlayer.CanInteract() && key == Key.E)
            {
                if(dbPlayer.Player.Position.DistanceTo(FaehreCPPosition) < 2.5f)
                {
                    // Get Vehicles Count
                    int count = 0;
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = $"SELECT COUNT(*) FROM `vehicles` WHERE garage_id = '{FaehreCPGarageId}' AND inGarage = '1' AND owner = '{dbPlayer.Id}';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    count = reader.GetInt32(0);
                                    break;
                                }
                            }
                        }
                    }

                    if(count == 0)
                    {
                        dbPlayer.SendNewNotification("Sie haben keine Fahrzeuge in der Garage zum verschiffen!");
                        return true;
                    }

                    dbPlayer.SetData("shipment_vehicle_count", count);
                    ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Fahrzeuge verschiffen", $"Wollen Sie Ihre ({count}) Fahrzeuge für ${count * 1000} nach Los Santos verschiffen? Die Fähre braucht ca 10 Minuten.", "shipment_cp_ls_confirm", "", ""));
                    return true;
                }
                if (dbPlayer.Player.Position.DistanceTo(FaehreLSPosition) < 2.5f)
                {

                    // Get Vehicles Count
                    int count = 0;
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = $"SELECT COUNT(*) FROM `vehicles` WHERE garage_id = '{FaehreLSGarageId}' AND inGarage = '1' AND owner = '{dbPlayer.Id}';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    count = reader.GetInt32(0);
                                    break;
                                }
                            }
                        }
                    }

                    if (count == 0)
                    {
                        dbPlayer.SendNewNotification("Sie haben keine Fahrzeuge in der Garage zum verschiffen!");
                        return true;
                    }

                    dbPlayer.SetData("shipment_vehicle_count", count);
                    ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Fahrzeuge verschiffen", $"Wollen Sie Ihre ({count}) Fahrzeuge für ${count*1000} nach Cayo Perico verschiffen? Die Fähre braucht ca 10 Minuten. (Verschiffen von Fahrzeugen welche nicht ihnen gehören ist NICHT möglich!)", "shipment_ls_cp_confirm", "", ""));
                    return true;
                }
            }

            return false;
        }

        /*
         * using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT COUNT(*) FROM `player_to_vehicle` WHERE vehicleID = '{vehicleId}'";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            keyCount = reader.GetInt32(0);
                            break;
                        }
                    }
                }
            }
         * */

        public override void OnMinuteUpdate()
        {
            // Set to LS
            MySQLHandler.ExecuteAsync($"UPDATE vehicles SET garage_id = '{FaehreLSGarageId}' WHERE inGarage = '1' AND garage_id = '{FaehreStandbyCPzuLS}' AND UNIX_TIMESTAMP(lastUpdate) < UNIX_TIMESTAMP(DATE_SUB(NOW(), INTERVAL 10 MINUTE));");

            // Set to CP
            MySQLHandler.ExecuteAsync($"UPDATE vehicles SET garage_id = '{FaehreCPGarageId}' WHERE inGarage = '1' AND garage_id = '{FaehreStandbyLSzuCP}' AND UNIX_TIMESTAMP(lastUpdate) < UNIX_TIMESTAMP(DATE_SUB(NOW(), INTERVAL 10 MINUTE));");
            base.OnMinuteUpdate();
        }
    }
    public class CayoPericoEventHandler : Script
    {
        [RemoteEvent]
        public void shipment_ls_cp_confirm(Player p_Player, string pb_map, string none, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!dbPlayer.HasData("shipment_vehicle_count")) return;

            int count = dbPlayer.GetData("shipment_vehicle_count");
            if (count <= 0 || count >= 10) return;

            int price = 1000 * count;

            if (!dbPlayer.TakeMoney(price))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                return;
            }

            MySQLHandler.ExecuteAsync($"UPDATE vehicles SET garage_id = '{CayoPericoModule.FaehreStandbyLSzuCP}' WHERE inGarage = '1' AND owner = '{dbPlayer.Id}' AND garage_id = '{CayoPericoModule.FaehreLSGarageId}' LIMIT {count};");

            dbPlayer.SendNewNotification("Deine Fahrzeuge werden nun nach Cayo Perico verschifft! (Fährendauer ca 10 Minuten)");
            return;
        }

        [RemoteEvent]
        public void shipment_cp_ls_confirm(Player p_Player, string pb_map, string none, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!dbPlayer.HasData("shipment_vehicle_count")) return;

            int count = dbPlayer.GetData("shipment_vehicle_count");
            if (count <= 0 || count >= 10) return;

            int price = 1000 * count;

            if (!dbPlayer.TakeMoney(price))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                return;
            }

            MySQLHandler.ExecuteAsync($"UPDATE vehicles SET garage_id = '{CayoPericoModule.FaehreStandbyCPzuLS}' WHERE inGarage = '1' AND owner = '{dbPlayer.Id}' AND garage_id = '{CayoPericoModule.FaehreCPGarageId}' LIMIT {count};");

            dbPlayer.SendNewNotification("Deine Fahrzeuge werden nun nach Cayo Perico verschifft! (Fährendauer ca 10 Minuten)");
            return;
        }
    }
}
