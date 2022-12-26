using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.Carsell.Menu
{
    public class CarsellDeleteMenuBuilder : MenuBuilder
    {
        public CarsellDeleteMenuBuilder() : base(PlayerMenu.CarsellDeleteMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer DbPlayer)
        {
            var l_Menu = new Module.Menu.NativeMenu(Menu, "Fahrzeug entfernen");
            l_Menu.Add($"Schließen");

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM fvehicles WHERE team = '{DbPlayer.TeamId}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            VehicleData vehData = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));
                            if (vehData == null) continue;
                            
                            l_Menu.Add($"{(vehData.mod_car_name.Length <= 0 ? vehData.Model : vehData.mod_car_name)}");
                        }
                    }
                }
                conn.Close();
            }

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if(index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }

                int idx = 1;

                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT * FROM fvehicles WHERE team = '{dbPlayer.TeamId}';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                VehicleData vehData = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));
                                if (vehData == null) continue;
                                
                                if (index == idx)
                                {
                                    if (reader.GetInt32("inGarage") == 0)
                                    {
                                        dbPlayer.SendNewNotification($"Fahrzeug ist derzeit ausgeparkt!");
                                        return true;
                                    }

                                    MySQLHandler.ExecuteAsync($"DELETE FROM `fvehicles` WHERE id = '{reader.GetUInt32("id")}';");
                                    dbPlayer.SendNewNotification($"Fahrzeug {(vehData.mod_car_name.Length <= 0 ? vehData.Model : vehData.mod_car_name)} wurde entfernt!");
                                    return true;
                                }
                                idx++;
                            }
                        }
                    }
                    conn.Close();
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
