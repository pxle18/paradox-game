using System.Collections.Generic;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using Newtonsoft.Json;
using System;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Voice;

namespace VMP_CNR.Module.Funk.App
{
    public class FunkApp : SimpleApp
    {
        public FunkApp() : base("FunkApp")
        {
        }

        [RemoteEvent]
        public void requestVoiceSettings(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            TriggerNewClient(
                client,
                "responseVoiceSettings",
                NAPI.Util.ToJson(
                    new VoiceSettings(
                        VoiceModule.Instance.getPlayerFrequenz(dbPlayer),
                        (int)dbPlayer.funkStatus)
                )
            );
        }
    }
}