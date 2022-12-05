using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Voice;

namespace VMP_CNR.Module.Telefon.App
{
    public class SettingsApp : SimpleApp
    {
        public SettingsApp() : base("SettingsApp")
        {
        }

        [RemoteEvent]
        public void requestPhoneSettings(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            TriggerNewClient(player, "responsePhoneSettings", dbPlayer.phoneSetting.flugmodus, dbPlayer.phoneSetting.lautlos, dbPlayer.phoneSetting.blockCalls);
        }

        [RemoteEvent]
        public void savePhoneSettings(Player player, bool flugmodus, bool lautlos, bool blockCalls, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            dbPlayer.phoneSetting.flugmodus = flugmodus;
            dbPlayer.phoneSetting.lautlos = lautlos;
            dbPlayer.phoneSetting.blockCalls = blockCalls;

            if (flugmodus)
            {
                dbPlayer.CancelPhoneCall();
                VoiceModule.Instance.ChangeFrequenz(dbPlayer, 0, true);
                VoiceModule.Instance.turnOffFunk(dbPlayer);
            }
        }
    }
}
