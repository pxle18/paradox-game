using System;
using System.Linq;
using GTANetworkAPI;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Vehicles.Garages
{
    public class GarageSpawnModule : SqlModule<GarageSpawnModule, GarageSpawn, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `garages_spawns`;";
        }

        public override Type[] RequiredModules()
        {
            return new[] {typeof(GarageModule)};
        }

        protected override void OnItemLoaded(GarageSpawn garageSpawn)
        {
            var garage = GarageModule.Instance[garageSpawn.GarageId];
            if (garage == null)
            {
                Logger.Print($"Unknown GarageId {garageSpawn.GarageId}");
            }
            else
            {
                garage.Spawns.Add(garageSpawn);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandqueueusage(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanAccessMethod()) return;

            int Auslastung1 = Sync.MySqlSyncThread.Instance.queue.ToList().Count;
            int Auslastung2 = Sync.MySqlSyncThread.Instance.queue2.ToList().Count;
            int Auslastung3 = Sync.MySqlSyncThread.Instance.queue3.ToList().Count;
            int Inventory = Sync.MySqlSyncThread.Instance.InventoryQueue.ToList().Count;
            int Vehicles = Sync.MySqlSyncThread.Instance.VehiclesQueue.ToList().Count;

            dbPlayer.SendNewNotification($"Queue Auslastung: (1) {Auslastung1}, (2) {Auslastung2}, (3) {Auslastung3}, (Inventory) {Inventory}, Vehicles {Vehicles}");
        }
    }
}