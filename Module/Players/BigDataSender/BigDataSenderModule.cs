using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using Extension = VMP_CNR.Module.Players.BigDataSender.BigDataSenderPlayerExtension;

namespace VMP_CNR.Module.Players.BigDataSender
{
    public class BigDataSenderModule : Module<BigDataSenderModule>
    {
        /// <summary>
        /// After how many minutes does big data to send expire?
        /// </summary>
        private const int ExpiredTimeMinutes = 5;

        /// <summary>
        /// Clear none finished data send attempts.
        /// </summary>
        public override void OnFiveMinuteUpdate()
        {
            try
            {
                foreach (DbPlayer dbPlayer in Players.Instance.GetValidPlayers())
                {
                    if (!dbPlayer.HasData(Extension.PlayerDataName)) return;
                    if (!(dbPlayer.GetData(Extension.PlayerDataName) is List<Extension.PlayerBigData> bigDataToSend))
                    {
                        Logger.Debug("Could not get data object from player: " + Extension.PlayerDataName);

                        return;
                    }

                    DateTime time = DateTime.Now;
                    foreach (Extension.PlayerBigData bigData in bigDataToSend)
                    {
                        // Delete expired, finished & failed
                        bigDataToSend = bigDataToSend
                            .Where(
                                val =>
                                    val.Status == Extension.PlayerBigDataStatus.Failed
                                    || val.Status == Extension.PlayerBigDataStatus.Finished
                                    || (time - bigData.UpdatedAt).TotalMinutes > ExpiredTimeMinutes
                            )
                            .ToList();
                    }

                    dbPlayer.SetData(Extension.PlayerDataName, bigDataToSend);
                }
            }
            catch (Exception e)
            {
                Logger.Debug("Could not reset big data on player instances.");
                Logger.Crash(e);
            }
        }
    }

    public class BigDataSenderEvents : Script
    {
        /// <summary>
        /// 
        /// </summary>
        private const int PauseTimeBetweenChunks = 100;

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <param name="key"></param>
        [RemoteEvent("sDataSender-initSuccess")]
        public void BigDataSenderInitSuccess(Player client, string id, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (!dbPlayer.HasData(Extension.PlayerDataName)) return;

                if (!(dbPlayer.GetData(Extension.PlayerDataName) is List<Extension.PlayerBigData> bigDataToSend))
                {
                    Logger.Debug("Could not get data object from player: " + Extension.PlayerDataName);

                    return;
                }

                // Find data to send by id.
                Extension.PlayerBigData dataToSend = bigDataToSend.First(val => val.Id == id);

                // Is already sending
                if (dataToSend.Status == Extension.PlayerBigDataStatus.Sending) return;

                dataToSend.Status = Extension.PlayerBigDataStatus.Sending;
                dbPlayer.SetData(Extension.PlayerDataName, bigDataToSend);

                Task.Run(async () =>
                {
                    // Send each chunk to the client.
                    for (var i = 0; i < dataToSend.DataArray.Count(); i++)
                    {
                        var data = dataToSend.DataArray.ToArray()[i];

                        client.TriggerNewClient(
                            "cDataReceiver-receive",
                            dataToSend.Id,
                            data,
                            i == (dataToSend.DataArray.Count() - 1) ? 1 : 0,
                            i
                        );

                        await Task.Delay(PauseTimeBetweenChunks);
                    }
                });
            }
            catch (InvalidOperationException)
            {
                // id not found.
            }
            catch (Exception e)
            {
                Logger.Debug("Could not send big data to client after init success.");
                Logger.Crash(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <param name="key"></param>
        [RemoteEvent("sDataSender-end")]
        public void BigDataSenderEnd(Player client, string id, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (!dbPlayer.HasData(Extension.PlayerDataName)) return;

                if (!(dbPlayer.GetData(Extension.PlayerDataName) is List<Extension.PlayerBigData> bigDataToSend))
                {
                    Logger.Debug("Could not get data object from player: " + Extension.PlayerDataName);

                    return;
                }

                // Find data to send by id.
                Extension.PlayerBigData dataToSend = bigDataToSend.First(val => val.Id == id);

                // Not sending data can't end.
                if (dataToSend.Status != Extension.PlayerBigDataStatus.Sending) return;

                dataToSend.Status = Extension.PlayerBigDataStatus.Finished;

                dbPlayer.SetData(Extension.PlayerDataName, bigDataToSend);
            }
            catch (InvalidOperationException)
            {
                // id not found.
            }
            catch (Exception e)
            {
                Logger.Debug("Could not send handle big data end.");
                Logger.Crash(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <param name="key"></param>
        [RemoteEvent("sDataSender-failed")]
        public void BigDataSenderFailed(Player client, string id, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (!dbPlayer.HasData(Extension.PlayerDataName)) return;
                if (!(dbPlayer.GetData(Extension.PlayerDataName) is List<Extension.PlayerBigData> bigDataToSend))
                {
                    Logger.Debug("Could not get data object from player: " + Extension.PlayerDataName);

                    return;
                }

                // Find data to send by id.
                Extension.PlayerBigData dataToSend = bigDataToSend.First(val => val.Id == id);

                // Not sending data can't fail.
                if (dataToSend.Status != Extension.PlayerBigDataStatus.Sending) return;

                dataToSend.Status = Extension.PlayerBigDataStatus.Failed;

                dbPlayer.SetData(Extension.PlayerDataName, bigDataToSend);
            }
            catch (InvalidOperationException)
            {
                // id not found.
            }
            catch (Exception e)
            {
                Logger.Debug("Could not handle failed big data.");
                Logger.Crash(e);
            }
        }
    }
}