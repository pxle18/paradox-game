using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Vehicles.Garages;

namespace VMP_CNR.Module.Computer.Apps.SupportVehicleApp
{
    public class SupportVehicleFunctions
    {
        public enum VehicleCategory
        {
            ID = 0,
            ALL = 1
        }

        public static async Task<List<VehicleData>> GetVehicleData(VehicleCategory category, int id)
        {
            List<VehicleData> vehicleData = new List<VehicleData>();

            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();

                string statement;

                if(category == VehicleCategory.ID)
                {
                    statement = $"SELECT id, owner, inGarage, garage_id, vehiclehash FROM vehicles WHERE id = '{ id }'";
                }
                else
                {
                    statement = $"SELECT id, owner, inGarage, garage_id, vehiclehash FROM vehicles WHERE owner = '{ id }'";
                }

                cmd.CommandText = statement;
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            VehicleData data = new VehicleData
                            {
                                Id = reader.GetInt32("id"),
                                InGarage = reader.GetInt32("inGarage"),
                                Garage = reader.GetInt32("garage_id") > 0 && GarageModule.Instance.Contains(reader.GetUInt32("garage_id")) ? GarageModule.Instance.Get(reader.GetUInt32("garage_id")).Name : "Unbekannte Garage!" ,
                                Vehiclehash = reader.GetString("vehiclehash")
                            };

                            vehicleData.Add(data);
                        }

                        reader.Close();
                    }
                }

                conn.Close();
            }

            return vehicleData;
        }
    }
}
