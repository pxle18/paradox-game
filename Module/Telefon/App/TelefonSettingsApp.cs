using System.Collections.Generic;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using Newtonsoft.Json;
using System;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Telefon.App
{
    public class TelefonSettingsApp : SimpleApp
    {
        public TelefonSettingsApp() : base("TelefonSettings") { }

        [RemoteEvent]
        public void requestPhoneData(Player p_Player, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            // JSON struc: guthaben, nummer
            //TriggerNewClient(p_Player, "requestPhoneData", l_Json);
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null)
                return;

            PhoneData l_Data = new PhoneData()
            {
                guthaben = l_DbPlayer.guthaben[0],
                number = (int)l_DbPlayer.handy[0]
            };

            var l_Json = NAPI.Util.ToJson(l_Data);

            TriggerNewClient(p_Player, "responsePhoneData", l_Json);
        }
    }

    public class PhoneData
    {
        [JsonProperty(PropertyName = "guthaben")]
        public int guthaben { get; set; }

        [JsonProperty(PropertyName = "number")]
        public int number { get; set; }
    }
}
