using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Computer.Apps.VehicleTaxApp;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.HouseApp
{
    public class HouseAppFunctions
    {
        public static List<Tenant> GetTenantsForHouseByPlayer(DbPlayer dbPlayer)
        {
            try
            {
                List<Tenant> tenants = new List<Tenant>();
                if (dbPlayer.OwnHouse[0] == 0) return tenants;

                foreach (HouseRent houseRent in HouseRentModule.Instance.houseRents.ToList().Where(hr => hr.HouseId == dbPlayer.OwnHouse[0]))
                {
                    if (houseRent == null || houseRent.PlayerId == 0 || houseRent.SlotId == 0) continue;
                    tenants.Add(new Tenant()
                    {
                        SlotId = houseRent.SlotId,
                        PlayerId = houseRent.PlayerId,
                        Name = houseRent.PlayerId != 0 ? PlayerNameModule.Instance.Get(houseRent.PlayerId).Name : "Freier Mietplatz",
                        RentPrice = houseRent.RentPrice,
                        Handy = houseRent.PlayerId != 0 ? PlayerNameModule.Instance.Get(houseRent.PlayerId).HandyNr : 0,

                    });
                }

                return tenants;
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
            return new List<Tenant>();
        }

        public static List<HouseVehicle> GetVehiclesForHouseByPlayer(DbPlayer dbPlayer, House house)
        {
            List<HouseVehicle> vehicles = new List<HouseVehicle>();
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = $"SELECT id, vehiclehash, owner FROM vehicles WHERE garage_id = @garageId";
                cmd.Parameters.AddWithValue("@garageId", $"{house.GarageId}");
                cmd.Prepare();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string name = "";
                            // Get PlayerName
                            if (PlayerNameModule.Instance.Contains(reader.GetUInt32("owner"))) name = PlayerNameModule.Instance.Get(reader.GetUInt32("owner")).Name;

                            HouseVehicle houseVehicle = new HouseVehicle()
                            {
                                Id = reader.GetUInt32("id"),
                                Name = reader.GetString("vehiclehash"),
                                Owner = name
                            };
                            vehicles.Add(houseVehicle);
                        }
                    }
                }
                conn.Close();
            }
            return vehicles;
        }
    }
}
