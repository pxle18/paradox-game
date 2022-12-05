using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Keys;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Handler
{
    public class VehicleKeyHandler
    {
        public static VehicleKeyHandler Instance { get; } = new VehicleKeyHandler();

        private VehicleKeyHandler()
        {
        }

        public void DeletePlayerKey(DbPlayer dbPlayer, uint vehicleId)
        {
            if (!dbPlayer.VehicleKeys.ContainsKey(vehicleId)) return;
            dbPlayer.VehicleKeys.Remove(vehicleId);
            MySQLHandler.ExecuteAsync(
                $"DELETE FROM `player_to_vehicle` WHERE `playerID` = '{dbPlayer.Id}' AND `vehicleID` = '{vehicleId}';");
        }

        public void DeleteAllVehicleKeys(uint vehicleId)
        {
            foreach (DbPlayer dbPlayer in Players.Instance.GetValidPlayers())
            {
                if (dbPlayer?.VehicleKeys == null) continue;
                if (dbPlayer.VehicleKeys.ContainsKey(vehicleId))
                {
                    dbPlayer.VehicleKeys.Remove(vehicleId);
                }
            }
            MySQLHandler.ExecuteAsync($"DELETE FROM `player_to_vehicle` WHERE `vehicleID` = '{vehicleId}';");
        }

        public int GetVehicleKeyCount(uint vehicleId)
        {
            if (vehicleId == 0) return 0;
            int keyCount = 0;

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
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

            return keyCount;
        }

        public void AddPlayerKey(DbPlayer dbPlayer, uint vehicleId, string vehicleName)
        {
            if (vehicleId == 0) return;
            if (dbPlayer.VehicleKeys.ContainsKey(vehicleId)) return;
            dbPlayer.VehicleKeys.Add(vehicleId, vehicleName);
            MySQLHandler.ExecuteAsync(
                $"INSERT INTO `player_to_vehicle` (`playerID`, `vehicleID`) VALUES ('{dbPlayer.Id}', '{vehicleId}');");
        }

        public async Task LoadPlayerVehicleKeys(DbPlayer dbPlayer)
        {
            
                using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var keyCmd = keyConn.CreateCommand())
                {
                    await keyConn.OpenAsync();
                    keyCmd.CommandText =
                        $"SELECT player_to_vehicle.vehicleID, vehicles.vehiclehash FROM player_to_vehicle INNER JOIN vehicles ON player_to_vehicle.vehicleID = vehicles.id WHERE playerID = '{dbPlayer.Id}';";
                    using (var keyReader = keyCmd.ExecuteReader())
                    {
                        if (keyReader.HasRows)
                        {
                            while (keyReader.Read())
                            {
                                var keyId = keyReader.GetUInt32(0);
                                var keyName = keyReader.GetString(1);
                                if (!dbPlayer.VehicleKeys.ContainsKey(keyId))
                                {
                                    dbPlayer.VehicleKeys.Add(keyId, keyName);
                                }
                            }
                        }
                    }

                    keyCmd.CommandText = $"SELECT id, vehiclehash FROM `vehicles` WHERE owner = '{dbPlayer.Id}';";
                    using (var keyReader = keyCmd.ExecuteReader())
                    {
                        if (keyReader.HasRows)
                        {
                            while (keyReader.Read())
                            {
                                var keyId = keyReader.GetUInt32(0);
                                var keyName = keyReader.GetString(1);
                                if (!dbPlayer.OwnVehicles.ContainsKey(keyId))
                                {
                                    dbPlayer.OwnVehicles.Add(keyId, keyName);
                                }
                            }
                        }
                    }
                    await keyConn.CloseAsync();
                }
            
        }

        public List<VHKey> GetAllKeysPlayerHas(DbPlayer dbPlayer)
        {
            List<VHKey> vehicles = new List<VHKey>();
            foreach (var item in dbPlayer.VehicleKeys)
            {
                vehicles.Add(new VHKey(item.Value, item.Key));
            }
            foreach (var item in dbPlayer.OwnVehicles)
            {
                vehicles.Add(new VHKey(item.Value, item.Key));
            }

            return vehicles;
        }

        public List<VHKey> GetOwnVehicleKeys(DbPlayer dbPlayer)
        {
            List<VHKey> vehicles = new List<VHKey>();
            foreach (var item in dbPlayer.OwnVehicles)
            {
                vehicles.Add(new VHKey(item.Value, item.Key));
            }

            return vehicles;
        }
    }
}