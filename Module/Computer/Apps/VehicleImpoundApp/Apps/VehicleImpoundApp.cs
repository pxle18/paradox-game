using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.VehicleImpoundApp.Apps
{
    public class VehicleImpoundApp : SimpleApp
    {
        public VehicleImpoundApp() : base ("VehicleImpoundApp") { }


        [RemoteEvent]
        public void requestVehicleConfiscationById(Player client, uint vehicleId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var overview = VehicleImpoundFunctions.GetVehicleImpoundOverviews(dbPlayer, vehicleId);
            TriggerNewClient(client, "responseVehicleImpound", NAPI.Util.ToJson(overview));
        }

        [RemoteEvent]
        public void requestVehicleImpoundMember(Player player, string member, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, member)) return;
            if (member == null) return;

            var overview = VehicleImpoundFunctions.GetVehicleImpoundOverviewsByMember(dbPlayer, member);
            TriggerNewClient(player, "responseVehicleImpound", NAPI.Util.ToJson(overview));
        }
    }
}