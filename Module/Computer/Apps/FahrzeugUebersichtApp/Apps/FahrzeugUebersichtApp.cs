using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.FahrzeugUebersichtApp;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.FahrzeuguebersichtApp.Apps
{
    public class FahrzeugUebersichtApp : SimpleApp
    {
        public FahrzeugUebersichtApp() : base("FahrzeugUebersichtApp") { }


        public enum OverviewCategory
        {
            OWN=0,
            KEY=1,
            BUSINESS=2,
            RENT = 3
        }



        [RemoteEvent]
        public async void requestVehicleOverviewByCategory(Player client, int id, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer p_DbPlayer = client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            OverviewCategory l_Category = (OverviewCategory)id;
            var l_Overview = await FahrzeugUebersichtFunctions.GetOverviewVehiclesForPlayerByCategory(p_DbPlayer, l_Category);
            TriggerNewClientBig(client, "responseVehicleOverview", NAPI.Util.ToJson(l_Overview));
        }
    }
}
