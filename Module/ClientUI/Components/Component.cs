using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using Extension = VMP_CNR.Module.Players.BigDataSender.BigDataSenderPlayerExtension;

namespace VMP_CNR.Module.ClientUI.Components
{
    public abstract class Component : Script
    {
        public string Name { get; }

        public Component(string name)
        {
            Name = name;
            ComponentManager.Instance.Register(this);
        }

        public void TriggerNewClient(Player player, string eventName, params object[] args)
        {
            var eventArgs = new object[2 + args.Length];
            eventArgs[0] = Name;
            eventArgs[1] = eventName;

            for (var i = 0; i < args.Length; i++)
            {
                eventArgs[i + 2] = args[i];
            }

            player.TriggerNewClient("componentServerEvent", eventArgs);
        }

        public void TriggerNewClientBig(Player client, string eventName, params object[] args)
        {
            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (!dbPlayer.HasData(Extension.PlayerDataName))
                {
                    dbPlayer.SetData(Extension.PlayerDataName, new List<Extension.PlayerBigData>());
                }

                var dataArray = Extension.ChunkString(NAPI.Util.ToJson(args), Extension.ChunkSize);
                var dataId = Extension.MakeId(32);

                if (!(dbPlayer.GetData(Extension.PlayerDataName) is List<Extension.PlayerBigData> bigDataToSend))
                {
                    Logging.Logger.Debug("Could not get data object from player: " + Extension.PlayerDataName);

                    return;
                }

                // Don't send data for an event twice.
                if (bigDataToSend.Count(val =>
                    val.EventName == eventName
                    && val.Status != Extension.PlayerBigDataStatus.Finished
                    && val.Status != Extension.PlayerBigDataStatus.Failed
                ) > 0) return;

                var playerBigDataObject = new Extension.PlayerBigData(dataId, dataArray, eventName);
                bigDataToSend.Add(playerBigDataObject);

                dbPlayer.SetData(Extension.PlayerDataName, bigDataToSend);

                // Tell client to make data receiver ready for us.
                client.TriggerNewClient(
                    "cDataReceiverComponent-init",
                    dataId,
                    eventName,
                    Name,
                    playerBigDataObject.GetChunkSize()
                );
            }
            catch (Exception e)
            {
                Logging.Logger.Debug("Could not send big data to client.");
                Logging.Logger.Crash(e);
            }
        }
    }
}