using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.VehicleTaxApp.Apps
{
    public class VehicleTaxApp : SimpleApp
    {
        public VehicleTaxApp() : base("VehicleTaxApp") { }

        [RemoteEvent]
        public void requestVehicleTaxByModel(Player client, String searchString, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, searchString)) return;

            var overview = VehicleTaxFunctions.GetVehicleTaxOverviews(dbPlayer, searchString);
            TriggerNewClient(client, "responseVehicleTax", NAPI.Util.ToJson(overview));
        }
    }
}
