using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Computer.Apps.FahrzeuguebersichtApp;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles.Garages;
using static VMP_CNR.Module.Computer.Apps.FahrzeuguebersichtApp.Apps.FahrzeugUebersichtApp;

namespace VMP_CNR.Module.Computer.Apps.FahrzeugUebersichtApp
{
    public class FahrzeugUebersichtFunctions
    {
        public static async Task<List<OverviewVehicle>> GetOverviewVehiclesForPlayerByCategory(DbPlayer dbPlayer, OverviewCategory overviewCategory)
        {
            List<OverviewVehicle> overviewVehicles = new List<OverviewVehicle>();

            if(dbPlayer.LastQueryBreak.AddSeconds(5) > DateTime.Now)
            {
                dbPlayer.SendNewNotification("Antispam: Bitte 5 Sekunden warten!");
                return overviewVehicles;
            }
            //82227
            switch (overviewCategory)
            {
                case OverviewCategory.OWN:
                    overviewVehicles = await GetOverviewVehiclesFromDb($"SELECT vehicles.id, color1, color2, fuel, inGarage, km, vehicles.garage_id, vehiclehash, gps_tracker, garages_spawns.pos_x, garages_spawns.pos_y, garages_spawns.pos_z FROM vehicles LEFT JOIN garages_spawns ON vehicles.garage_id = garages_spawns.garage_id WHERE vehicles.owner = {dbPlayer.Id} GROUP BY vehicles.id;");
                    break;
                case OverviewCategory.KEY:
                    overviewVehicles = await GetOverviewVehiclesFromDb($"SELECT vehicles.id, color1, color2, fuel, inGarage, km, vehicles.garage_id, vehiclehash, gps_tracker, garages_spawns.pos_x, garages_spawns.pos_y, garages_spawns.pos_z FROM vehicles LEFT JOIN player_to_vehicle ON player_to_vehicle.vehicleId = vehicles.id LEFT JOIN garages_spawns ON vehicles.garage_id = garages_spawns.garage_id WHERE player_to_vehicle.playerId = { dbPlayer.Id } GROUP BY vehicles.id;");
                    break;
                case OverviewCategory.RENT:
                    overviewVehicles = await GetOverviewVehiclesFromDb($"SELECT vehicles.id, color1, color2, fuel, inGarage, km, vehicles.garage_id, vehiclehash, gps_tracker, garages_spawns.pos_x, garages_spawns.pos_y, garages_spawns.pos_z FROM vehicles LEFT JOIN player_vehicle_rent ON player_vehicle_rent.vehicle_id = vehicles.id LEFT JOIN garages_spawns ON vehicles.garage_id = garages_spawns.garage_id WHERE player_vehicle_rent.player_id = { dbPlayer.Id } GROUP BY vehicles.id;");
                    break;
                case OverviewCategory.BUSINESS:
                    if (dbPlayer.GetActiveBusiness() != null)
                    {
                        overviewVehicles = await GetOverviewVehiclesFromDb($"SELECT vehicles.id, color1, color2, fuel, inGarage, km, vehicles.garage_id, vehiclehash, gps_tracker, garages_spawns.pos_x, garages_spawns.pos_y, garages_spawns.pos_z FROM vehicles INNER JOIN business_vehicles ON business_vehicles.vehicle_id = vehicles.id LEFT JOIN garages_spawns ON vehicles.garage_id = garages_spawns.garage_id WHERE business_vehicles.business_id = { dbPlayer.GetActiveBusiness().Id} GROUP BY vehicles.id;");
                    }
                    break;
            }

            dbPlayer.LastQueryBreak = DateTime.Now;

            return overviewVehicles;
        }

        private static async Task<List<OverviewVehicle>> GetOverviewVehiclesFromDb(string statement)
        {
            List<OverviewVehicle> ownVehicles = new List<OverviewVehicle>();

            try
            {
                using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var keyCmd = keyConn.CreateCommand())
                {
                    await keyConn.OpenAsync();
                    keyCmd.CommandText = statement;
                    using (var reader = await keyCmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                CarCoorinate ccor = new CarCoorinate
                                {
                                    position_x = reader.GetFloat("pos_x"),
                                    position_y = reader.GetFloat("pos_y"),
                                    position_z = reader.GetFloat("pos_z")
                                };

                                OverviewVehicle vehicle = new OverviewVehicle
                                {
                                    Id = reader.GetUInt32("id"),
                                    Color1 = reader.GetUInt32("color1"),
                                    Color2 = reader.GetUInt32("color2"),
                                    Fuel = reader.GetDouble("fuel"),
                                    InGarage = reader.GetInt32("inGarage") == 1,
                                    Km = reader.GetFloat("km"),
                                    GarageName = reader.GetInt32("gps_tracker") == 1 && GarageModule.Instance.Contains(reader.GetUInt32("garage_id")) ? GarageModule.Instance.Get(reader.GetUInt32("garage_id")).Name : "kein GPS Signal...",
                                    Vehiclehash = reader.GetString("vehiclehash"),
                                    Besitzer = "",
                                    CarCor = ccor
                                };

                                if (vehicle != null) ownVehicles.Add(vehicle);
                            }
                        }
                    }
                    await keyConn.CloseAsync();
                }
            }
            catch(Exception e)
            {
                // Weil wegen iwas mit SQL Field Value zeug...
                Logging.Logger.Crash(e);
            }

            return ownVehicles;
        }
    }
}
