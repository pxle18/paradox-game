using GTANetworkAPI;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Voice
{
    public class VoiceEventHandler : Script
    {
        [RemoteEvent]
        public static void ChangeVoicRange(Player client, uint garageId, string state, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            client.SetSharedData("VOICE_RANGE", state);
        }

        [RemoteEvent]
        public static void changeFrequenz(Player client, string frequenz, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            if (frequenz == "" || frequenz.Length < 1 || frequenz.Length > 9) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            // Check Cuff Die Death
            if (!dbPlayer.CanInteract()) return;

            if (!double.TryParse(frequenz, NumberStyles.Any, CultureInfo.InvariantCulture, out double fq)) return;

            VoiceModule.Instance.ChangeFrequenz(dbPlayer, fq);
        }

        [RemoteEvent]
        public static void changeSettings(Player client, int state, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;
            if (dbPlayer.ActionsBlocked) return;
            FunkStatus old = dbPlayer.funkStatus;
            dbPlayer.funkStatus = (FunkStatus)state;
            VoiceModule.Instance.refreshFQVoiceForPlayerFrequenz(dbPlayer);

            try
            {
                double frequenz = VoiceModule.Instance.getPlayerFrequenz(dbPlayer);
                // Send sounds
                if (old == FunkStatus.Hearing && dbPlayer.funkStatus == FunkStatus.Active)
                {
                    if (dbPlayer.HasData("lastFunkStartSound2"))
                    {
                        DateTime lastSound = dbPlayer.GetData("lastFunkStartSound2");
                        if (lastSound.AddSeconds(3) < DateTime.Now)
                        {
                            dbPlayer.SetData("lastFunkStartSound2", DateTime.Now);
                            VoiceModule.Instance.sendSoundToFrequenz(frequenz, "Start_Squelch", "CB_RADIO_SFX");
                        }
                    }
                    else
                    {
                        dbPlayer.SetData("lastFunkStartSound2", DateTime.Now);
                        VoiceModule.Instance.sendSoundToFrequenz(frequenz, "Start_Squelch", "CB_RADIO_SFX");
                    }
                }
                else if (old == FunkStatus.Active && dbPlayer.funkStatus == FunkStatus.Hearing)
                {
                    if (dbPlayer.HasData("lastFunkEndSound2"))
                    {
                        DateTime lastSound = dbPlayer.GetData("lastFunkEndSound2");
                        if (lastSound.AddSeconds(3) < DateTime.Now)
                        {
                            dbPlayer.SetData("lastFunkEndSound2", DateTime.Now);
                            VoiceModule.Instance.sendSoundToFrequenz(frequenz, "End_Squelch", "CB_RADIO_SFX");
                        }
                    }
                    else
                    {
                        dbPlayer.SetData("lastFunkEndSound2", DateTime.Now);
                        VoiceModule.Instance.sendSoundToFrequenz(frequenz, "End_Squelch", "CB_RADIO_SFX");
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Crash(e);
            }


            switch (dbPlayer.funkStatus)
            {
                case FunkStatus.Active:
                    if (!dbPlayer.RageExtension.IsInVehicle && !dbPlayer.IsCuffed && !dbPlayer.IsTied)
                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "random@arrests", "generic_radio_chatter");
                        
                    break;
                case FunkStatus.Hearing:
                case FunkStatus.Deactive:
                    if (dbPlayer.RageExtension.IsInVehicle)
                        break;

                    dbPlayer.StopAnimation();
                    break;
                default:
                    break;
            }

            client.TriggerNewClient("updateVoiceState", state);
        }


        public static void Connect(Player player, string characterName)
        {
            player.TriggerNewClient("ConnectTeamspeak");
        }
    }
}