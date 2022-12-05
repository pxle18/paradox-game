using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.FahrzeugUebersichtApp;
using VMP_CNR.Module.LeitstellenPhone;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.KennzeichenUebersichtApp.Apps
{
    public class KennzeichenUebersichtApp : SimpleApp
    {
        public KennzeichenUebersichtApp() : base("KennzeichenUebersichtApp") { }

        public enum SearchType
        {
            PLATE = 0,
            VEHICLEID = 1
        }

        [RemoteEvent]
        public async Task requestVehicleOverviewByPlate(Player client, String plate, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            if (!MySQLHandler.IsValidNoSQLi(client, plate)) return;

            await HandleVehicleOverview(client, plate, SearchType.PLATE);
        }

        [RemoteEvent]
        public async Task requestVehicleOverviewByVehicleId(Player client, int vehicleId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            await HandleVehicleOverview(client, vehicleId.ToString(), SearchType.VEHICLEID);
            
        }


        private async Task HandleVehicleOverview(Player p_Client, String information, SearchType type)
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

            var l_Overview = KennzeichenUebersichtFunctions.GetVehicleInfoByPlateOrId(p_DbPlayer, type, information);
            TriggerNewClient(p_Client, "responsePlateOverview", NAPI.Util.ToJson(l_Overview));
        }


    }
}
