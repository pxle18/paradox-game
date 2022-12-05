using System;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tasks;

namespace VMP_CNR.Module.Vehicles.Garages
{
    public class GaragePlayerVehicleTakeOutTask : SqlTask
    {
        private readonly Garage garage;
        private readonly uint vehicleId;
        private readonly DbPlayer dbPlayer;
        private readonly GarageSpawn spawnPosition;

        public GaragePlayerVehicleTakeOutTask(Garage garage, uint vehicleId, DbPlayer dbPlayer, GarageSpawn spawnPosition)
        {
            this.garage = garage;
            this.vehicleId = vehicleId;
            this.dbPlayer = dbPlayer;
            this.spawnPosition = spawnPosition;
        }

        public override string GetQuery()
        {
            return
                $"UPDATE `vehicles` SET `inGarage` = '0' WHERE `inGarage` = '1' AND `garage_id` = '{garage.Id}' AND `id` = '{vehicleId}';";
        }

        public override void OnFinished(int result)
        {
            if (result != 1) return;

            var spawn = garage.GetFreeSpawnPosition();

            if (spawn == null)
            {
                dbPlayer.SendNewNotification("Kein freier Ausparkpunkt.");
                string query = $"UPDATE `vehicles` SET `inGarage` = '1' WHERE `inGarage` = '0' AND `garage_id` = '{garage.Id}' AND `id` = '{vehicleId}';";
                MySQLHandler.ExecuteAsync(query, Sync.MySqlSyncThread.MysqlQueueTypes.Vehicles);
                return;
            }

            spawnPosition.LastUsed = DateTime.Now;
            VehiclesModule.LoadServerVehicle(vehicleId, garage, dbPlayer, spawn);
            Logging.Logger.LogVehicleTakeout(dbPlayer.Id, vehicleId);
        }
    }
}