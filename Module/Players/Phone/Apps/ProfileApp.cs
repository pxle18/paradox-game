using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.Phone.Apps
{
    public class ProfileApp : SimpleApp
    {
        public ProfileApp() : base("ProfileApp")
        {
        }

        [RemoteEvent]
        public void requestSpecialProfilData(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            Dictionary<string, int> CWSResults = new Dictionary<string, int>();

            foreach(KeyValuePair<uint, PlayerCWS> kvp in dbPlayer.CWS)
            {
                CWS cws = CWSModule.Instance.GetAll().Values.Where(cc => cc.Id == kvp.Key).FirstOrDefault();
                if (cws == null) continue;
                CWSResults.Add(cws.Name, kvp.Value.Value);
            }
            
            TriggerNewClient(player, "responseSpecialProfilData", NAPI.Util.ToJson(CWSResults));
        }
    }
}
