using System.Collections.Generic;
using System.Linq;
using System;
using GTANetworkAPI;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Configurations;
using System.Collections.Concurrent;
using System.Threading;
using VMP_CNR.Module.Players;

namespace VMP_CNR
{
    public sealed class VoiceListHandler
    {
        public static VoiceListHandler Instance { get; } = new VoiceListHandler();
        
        public ConcurrentDictionary<int, DbPlayer> voiceHashList { get; set; }

        // Voice Ranges
        public const float VoiceRangeNormal = 12.0f;
        public const float VoiceRangeWhisper = 5.0f;
        public const float VoiceRangeShout = 45.0f;

        public readonly ConcurrentDictionary<uint, string> PlayerVoiceList;
        
        public static readonly Random Random = new Random();

        private int CurrentVoiceHash = 0;


        private VoiceListHandler()
        {
            PlayerVoiceList = new ConcurrentDictionary<uint, string>();
            voiceHashList = new ConcurrentDictionary<int, DbPlayer>();
        }
        
        public static void AddToDeath(DbPlayer dbPlayer)
        {
            /*NAPI.Task.Run(() =>
            {
                dbPlayer.Player.SetSharedData("isDead", true);
            });*/
        }

        public static void RemoveFromDeath(DbPlayer dbPlayer)
        {
            /*NAPI.Task.Run(() =>
            {
                if (dbPlayer.Player.HasSharedData("isDead"))
                {
                    dbPlayer.Player.ResetSharedData("isDead");
                }
            });*/
        }

        public void RemoveFromVoiceList(DbPlayer dbPlayer)
        {
            if(PlayerVoiceList.ContainsKey(dbPlayer.Id)) 
            {
                PlayerVoiceList.TryRemove(dbPlayer.Id, out string val);
            }

            var itemsToRemove = voiceHashList.Where(k => k.Value == dbPlayer);
            foreach(var item in itemsToRemove)
            {
                voiceHashList.TryRemove(item.Key, out DbPlayer empty);
            }
        }

        public void InitPlayerVoice(DbPlayer iPlayer)
        {
            iPlayer.Player.TriggerNewClient("setVoiceData", 1, Configuration.Instance.VoiceChannel, Configuration.Instance.VoiceChannelPassword);
            if (!PlayerVoiceList.ContainsKey(iPlayer.Id))
            {
                // Aufgrund Asynchronität muss es Thread-Safe sein um einen Crash zu vermeiden. Interlocked.Increment arbeitet auf atmorarer Ebene und ist daher hierfür gut geeignet
                while (voiceHashList.ContainsKey(CurrentVoiceHash))
                    Interlocked.Increment(ref CurrentVoiceHash);

                voiceHashList.TryAdd(CurrentVoiceHash, iPlayer);

                if (Configuration.Instance.DevMode)
                {
                    iPlayer.VoiceHash = "DEVMODE-" + CurrentVoiceHash;
                    PlayerVoiceList.TryAdd(iPlayer.Id, "DEVMODE-" +((CurrentVoiceHash < 100) ? "00" + CurrentVoiceHash : "" + CurrentVoiceHash));
                }
                else
                {
                    iPlayer.VoiceHash = "" + ((CurrentVoiceHash < 100) ? "00" + CurrentVoiceHash : "" + CurrentVoiceHash);
                    PlayerVoiceList.TryAdd(iPlayer.Id, "" + CurrentVoiceHash);
                }
            }

            iPlayer.Player.SetSharedData("voiceHash", iPlayer.VoiceHash);
            iPlayer.Player.TriggerNewClient("setVoiceHash", iPlayer.VoiceHash);
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}
