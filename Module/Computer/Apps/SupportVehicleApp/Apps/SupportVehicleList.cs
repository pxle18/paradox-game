using GTANetworkAPI;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using static VMP_CNR.Module.Computer.Apps.SupportVehicleApp.SupportVehicleFunctions;

namespace VMP_CNR.Module.Computer.Apps.SupportVehicleApp.Apps
{
    class SupportVehicleList : SimpleApp
    {
        public SupportVehicleList() : base("SupportVehicleList") { }

        [RemoteEvent]
        public async void requestSupportVehicleList(Player client, int owner, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            VehicleCategory category = VehicleCategory.ALL;
            var vehicleData = await SupportVehicleFunctions.GetVehicleData(category, owner);

            if (vehicleData == null) return;

            var vehicleDataJson = NAPI.Util.ToJson(vehicleData);
            TriggerNewClient(client, "responseVehicleList", vehicleDataJson);
        }
    }
}
