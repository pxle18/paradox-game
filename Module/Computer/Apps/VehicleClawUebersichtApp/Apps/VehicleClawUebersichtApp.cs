using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.KennzeichenUebersichtApp;
using VMP_CNR.Module.LeitstellenPhone;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.VehicleClawUebersichtApp.Apps
{
    public class VehicleClawUebersichtApp : SimpleApp
    {
        public VehicleClawUebersichtApp() : base("VehicleClawUebersichtApp") { }
        public enum SearchType
        {
            PLAYERNAME = 0,
            VEHICLEID = 1
        }


        [RemoteEvent]
        public async Task requestVehicleClawOverviewByPlayerName(Player client, String playerName, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            if (!MySQLHandler.IsValidNoSQLi(client, playerName)) return;
            await HandleVehicleClawOverview(client, playerName, SearchType.PLAYERNAME);
        }

        [RemoteEvent]
        public async Task requestVehicleClawOverviewByVehicleId(Player client, int vehicleId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            await HandleVehicleClawOverview(client, vehicleId.ToString(), SearchType.VEHICLEID);

        }


        private async Task HandleVehicleClawOverview(Player p_Client, String information, SearchType type)
        {
            DbPlayer p_DbPlayer = p_Client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            await NAPI.Task.WaitForMainThread(0);

            if (LeitstellenPhoneModule.Instance.GetByAcceptor(p_DbPlayer) == null)
            {
                p_DbPlayer.SendNewNotification("Sie müssen als Leitstelle angemeldet sein", PlayerNotification.NotificationType.ERROR);
                return;
            }

            var l_Overview = VehicleClawUebersichtFunctions.GetVehicleClawByIdOrName(p_DbPlayer, type, information);
            TriggerNewClient(p_Client, "responseVehicleClawOverview", NAPI.Util.ToJson(l_Overview));
        }

    }
}
