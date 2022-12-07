using GTANetworkAPI;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;
using static VMP_CNR.Module.Computer.Apps.SupportVehicleApp.SupportVehicleFunctions;

namespace VMP_CNR.Module.Computer.Apps.SupportVehicleApp.Apps
{
    class SupportVehicleProfile : SimpleApp
    {
        public SupportVehicleProfile() : base("SupportVehicleProfile") { }

        [RemoteEvent]
        public async void requestVehicleData(Player client, int id, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            VehicleCategory category = VehicleCategory.ID;
            var vehicleData = await SupportVehicleFunctions.GetVehicleData(category, id);

            if (vehicleData == null) return;

            var vehicleDataJson = NAPI.Util.ToJson(vehicleData);
            TriggerNewClient(client, "responseVehicleData", vehicleDataJson);
        }

        [RemoteEvent]
        public void SupportSetGarage(Player client, uint vehicleId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CanAccessMethod("removeveh")) return;

            SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(vehicleId);
            if (Vehicle == null) return;

            if (Vehicle.IsPlayerVehicle())
            {
                Vehicle.SetPrivateCarGarage(1);
                dbPlayer.SendNewNotification("Fahrzeug wurde in die Garage gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else
            {
                dbPlayer.SendNewNotification("Fahrzeug ist kein privat Fahrzeug!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }        }

        [RemoteEvent]
        public void SupportGoToVehicle(Player client, uint vehicleId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            NAPI.Task.Run(async () =>
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (!dbPlayer.CanAccessMethod("removeveh")) return;

                SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(vehicleId);
                if (Vehicle == null) return;

                await NAPI.Task.WaitForMainThread(0);

                Vector3 pos = Vehicle.Entity.Position;

                if (dbPlayer.RageExtension.IsInVehicle)
                {
                    client.Vehicle.Position = pos;
                }
                else
                {
                    client.SetPosition(pos);
                }

                dbPlayer.SendNewNotification($"Sie haben sich zu Fahrzeug {Vehicle.databaseId} teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            });
        }
    }
}
