using System.Collections.Generic;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using Newtonsoft.Json;
using System;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Messenger.App
{
    public class MessengerApp : SimpleApp
    {
        public MessengerApp() : base("MessengerApp")
        {
        }

        [RemoteEvent]
        public void sendMessage(Player client, uint number, string messageContent, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
        }

        [RemoteEvent]
        public void forwardMessage(Player client, uint number, uint messageId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            // Forwars selected message in "original" and fake-proof. TBD later.
        }
    }
}