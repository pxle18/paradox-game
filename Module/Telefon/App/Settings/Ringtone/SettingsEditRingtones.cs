using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Telefon.App.Settings.Ringtone;

namespace VMP_CNR.Module.Telefon.App.Settings
{
    public class RingtoneResponseObject
    {
        public List<Ringtone.Ringtone> ringtones { get; set; }

        public string volume { get; set; }
    }

    public class SettingsEditRingtones : SimpleApp
    {
        public SettingsEditRingtones() : base("SettingsEditRingtonesApp") { }


        [RemoteEvent]
        public void requestRingtoneList(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            RingtoneResponseObject ringtoneResponseObject = new RingtoneResponseObject() { ringtones = RingtoneModule.Instance.getRingtonesForPlayer(dbPlayer) };

            TriggerNewClient(player, "responseRingtoneList", NAPI.Util.ToJson(ringtoneResponseObject));

        }

        [RemoteEvent]
        public void saveRingtone(Player player, int ringtoneId, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;


            dbPlayer.ringtone = RingtoneModule.Instance.Get((uint)ringtoneId);

            dbPlayer.SaveRingtone();

            dbPlayer.Player.TriggerNewClient("setActiveRingtone", ringtoneId);
        }

    }

}
