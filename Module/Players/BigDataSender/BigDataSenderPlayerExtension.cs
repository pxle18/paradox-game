using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.BigDataSender
{
    public static class BigDataSenderPlayerExtension
    {
        /// <summary>
        /// Size of each chunk which is sent to the player..
        /// </summary>
        public const int ChunkSize = 10024;

        /// <summary>
        /// Simple random generator.
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// Name under which the data to send should be stored in the player object.
        /// </summary>
        public const string PlayerDataName = "bigDataToSend";

        /// <summary>
        /// Send data to client but with big data arrays.
        /// </summary>
        /// 
        /// <param name="client">Client who receives the data.</param>
        /// <param name="eventName">Event name the client listens for.</param>
        /// <param name="args">Big data to sent.</param>
        public static void TriggerNewClientBig(this Player client, string eventName, params object[] args)
        {
            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                // Store chunked data to the player object until we
                // receive a successful init callback from the client.
                if (!dbPlayer.HasData(PlayerDataName))
                {
                    dbPlayer.SetData(PlayerDataName, new List<PlayerBigData>());
                }

                var dataArray = ChunkString(NAPI.Util.ToJson(args), ChunkSize);
                var dataId = MakeId(32);

                if (!(dbPlayer.GetData(PlayerDataName) is List<PlayerBigData> bigDataToSend))
                {
                    Logging.Logger.Debug("Could not get data object from player: " + PlayerDataName);

                    return;
                }

                // Don't send data for an event twice.
                if (bigDataToSend.Count(val =>
                    val.EventName == eventName
                    && val.Status != PlayerBigDataStatus.Finished
                    && val.Status != PlayerBigDataStatus.Failed
                ) > 0) return;

                var playerBigDataObject = new PlayerBigData(dataId, dataArray, eventName);
                bigDataToSend.Add(playerBigDataObject);

                dbPlayer.SetData(PlayerDataName, bigDataToSend);

                // Tell client to make data receiver ready for us.
                client.TriggerNewClient(
                    "cDataReceiver-init",
                    dataId,
                    eventName,
                    playerBigDataObject.GetChunkSize()
                );
            }
            catch (Exception e)
            {
                Logging.Logger.Debug("Could not send big data to client.");
                Logging.Logger.Crash(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <param name="str"></param>
        /// <param name="maxChunkSize"></param>
        /// 
        /// <returns></returns>
        public static IEnumerable<string> ChunkString(string str, int maxChunkSize)
        {
            for (var i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <param name="length"></param>
        /// 
        /// <returns></returns>
        public static string MakeId(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

            return new string(chars.Select(c => chars[Random.Next(chars.Length)]).Take(length).ToArray());
        }

        public class PlayerBigData
        {
            public IEnumerable<string> DataArray;

            public string Id;

            public DateTime CreatedAt;

            public DateTime UpdatedAt;

            public string EventName;

            private PlayerBigDataStatus status;

            public PlayerBigDataStatus Status
            {
                get => status;

                set
                {
                    status = value;

                    UpdatedAt = DateTime.Now;
                }
            }

            public PlayerBigData(string id, IEnumerable<string> dataArray, string eventName)
            {
                Id = id;
                DataArray = dataArray;
                EventName = eventName;

                CreatedAt = DateTime.Now;
                UpdatedAt = DateTime.Now;
                Status = PlayerBigDataStatus.Scheduled;
            }

            public int GetChunkSize()
            {
                return DataArray.Count();
            }
        }

        public enum PlayerBigDataStatus
        {
            Scheduled,
            Sending,
            Failed,
            Finished,
        }
    }
}