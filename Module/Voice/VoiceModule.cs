using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Teams;
using GTANetworkAPI;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.FIB;

namespace VMP_CNR.Module.Voice
{
    public enum FunkStatus
    {
        Deactive = 0,
        Hearing = 1,
        Active = 2
    }

    public class VoiceSettings
    {
        public double Room { get; set; }
        public int Active { get; set; }

        public VoiceSettings(double room, int active)
        {
            Room = room;
            Active = active;
        }
    }

    public sealed class VoiceModule : Module<VoiceModule>
    {

        public Dictionary<double, List<DbPlayer>> voiceFQ;
        public Dictionary<double, string> voiceFQDataStrings;

        public List<DbPlayer> airFunkTalkingPlayers;

        public static int ZivilFunkCountLimit = 10;
        public static int LSPDFunkCountLimit = 30;

        public static int AdminFunkFrequenz = 9999;
        public static int EventFunkFrequenz = 8888;
        protected override bool OnLoad()
        {
            voiceFQ = new Dictionary<double, List<DbPlayer>>();
            voiceFQDataStrings = new Dictionary<double, string>();
            airFunkTalkingPlayers = new List<DbPlayer>();
            return true;
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            RemoveFromVoice(dbPlayer);
        }

        public override void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
            if (dbPlayer.IsInjured())
                Instance.turnOffFunk(dbPlayer);
        }

        public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            InitPlayerVoice(dbPlayer);
            AddToVoice(dbPlayer);
        }

        public void ChangeVoiceRange(Player player, float range = 15.0f)
        {
            player.SetSharedData("VOICE_RANGE", range);
        }

        public void Connect(Player player, string characterName)
        {
            player.TriggerNewClient("ConnectTeamspeak");
        }

        public void refreshFQVoiceForPlayerFrequenz(DbPlayer dbPlayer)
        {
            if (dbPlayer == null)
                return;

            if (hasPlayerRadio(dbPlayer))
            {
                double frequenz = getPlayerFrequenz(dbPlayer);
                refreshFQVoiceForFrequenz(frequenz);
            }
            else if (voiceFQDataStrings.ToList().Where(f => f.Value.Contains(dbPlayer.VoiceHash)).Count() > 0)
            {
                double frequenz = voiceFQDataStrings.FirstOrDefault(f => f.Value.Contains(dbPlayer.VoiceHash)).Key;
                refreshFQVoiceForFrequenz(frequenz);
            }
            else
            {
                dbPlayer.Player.TriggerNewClient("setRadioChatPlayers", "");
            }
        }

        public void refreshAirFunk()
        {
            foreach (DbPlayer xx in Players.Players.Instance.GetValidPlayers().Where(p => p.funkAirStatus == FunkStatus.Hearing || p.funkAirStatus == FunkStatus.Active))
            {
                string frequenzString = GetAirFunkTalkingString(xx); 
                xx.Player.TriggerNewClient("setAirFunkPlayers", frequenzString);
            }
        }

        public string GetAirFunkTalkingString(DbPlayer exceptPlayer)
        {
            string s = "";

            foreach (DbPlayer xx in airFunkTalkingPlayers.ToList().Where(p => p != null && p.IsValid() && p.funkAirStatus == FunkStatus.Active))
            {
                if (xx == exceptPlayer) continue;
                s += ";" + xx.VoiceHash + "~-6~0~0~0";
            }

            return s;
        }

        public void RemoveFromAirFunk(DbPlayer dbPlayer)
        {

            dbPlayer.Player.TriggerNewClient("setAirFunkPlayers", "");

            if (airFunkTalkingPlayers.ToList().Contains(dbPlayer))
            {
                airFunkTalkingPlayers.Remove(dbPlayer);
            }
        }

        public void RemoveFromVoice(DbPlayer dbPlayer)
        {
            // Remove From Radio Frequenz
            if(hasPlayerRadio(dbPlayer))
            {
                double frequenz = getPlayerFrequenz(dbPlayer);
                CheckFrequenz(frequenz);
                voiceFQ[frequenz].Remove(dbPlayer);
                refreshFQVoiceForFrequenz(frequenz);
            }

            if(airFunkTalkingPlayers.ToList().Contains(dbPlayer))
            {
                airFunkTalkingPlayers.Remove(dbPlayer);
            }

            // Remove from DeathVoice
            VoiceListHandler.RemoveFromDeath(dbPlayer);
        }

        public void refreshFQVoiceForFrequenz(double frequenz)
        {
            actualizeFrequenzDataString(frequenz);

            if (voiceFQ.ContainsKey(frequenz))
            {
                List<DbPlayer> PlayersInFrequences = voiceFQ[frequenz].ToList();
                foreach (DbPlayer xx in PlayersInFrequences)
                {
                    if (hasPlayerRadio(xx) && getPlayerFrequenz(xx) == frequenz)
                    {
                        string frequenzString = voiceFQDataStrings[frequenz];
                        xx.Player.TriggerNewClient("setRadioChatPlayers", frequenzString);
                    }
                }
            }
        }

        public void sendSoundToFrequenz(double frequenz, string sound1, string sound2)
        {
            foreach (DbPlayer xx in voiceFQ[frequenz].ToList())
            {
                if (hasPlayerRadio(xx) && getPlayerFrequenz(xx) == frequenz && xx.funkStatus != FunkStatus.Deactive)
                {
                    xx.Player.TriggerNewClient("startsoundplay", sound1, sound2);
                }
            }
        }


        public void sendSoundToAirFunk(string sound1, string sound2)
        {
            foreach (DbPlayer xx in Players.Players.Instance.GetValidPlayers().Where(p => p.funkAirStatus == FunkStatus.Hearing || p.funkAirStatus == FunkStatus.Active))
            {
                xx.Player.TriggerNewClient("startsoundplay", sound1, sound2);
            }
        }

        private void actualizeFrequenzDataString(double frequenz)
        {
            try
            {
                if (frequenz < 1)
                {
                    voiceFQDataStrings[frequenz] = "";
                    return;
                }

                if (voiceFQ.ContainsKey(frequenz))
                {
                    List<DbPlayer> PlayerFrequenzen = voiceFQ[frequenz].ToList();

                    string s = "";
                    foreach (DbPlayer xx in PlayerFrequenzen.Where(p => p != null && p.IsValid() && p.funkStatus == FunkStatus.Active))
                    {
                        s += ";" + xx.VoiceHash + "~-6~0~0~2";
                    }

                    if (!voiceFQDataStrings.ContainsKey(frequenz)) voiceFQDataStrings.Add(frequenz, "");
                    voiceFQDataStrings[frequenz] = s + ";";
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }


        public void turnOffFunk(DbPlayer dbPlayer, bool fullReset = false)
        {
            if (!hasPlayerRadio(dbPlayer)) return;

            dbPlayer.funkStatus = FunkStatus.Deactive;

            dbPlayer.Player.TriggerNewClient("updateVoiceState", (int)dbPlayer.funkStatus);

            if (fullReset)
            {
                double frequenz = getPlayerFrequenz(dbPlayer);
                voiceFQ[frequenz].Remove(dbPlayer);
                refreshFQVoiceForFrequenz(frequenz);
                return;
            }

            VoiceModule.Instance.refreshFQVoiceForPlayerFrequenz(dbPlayer);
        }

        private bool isFrequenzLocked(double frequenz)
        {
            foreach (DbTeam dbTeam in TeamModule.Instance.GetAll().Values.ToList())
            {
                if (dbTeam.Frequenzen.Contains(frequenz)) return true;
            }
            return false;
        }

        private bool isFrequenzLockedForTeam(DbTeam team, double frequenz)
        {
            return (!team.Frequenzen.Contains(frequenz) && isFrequenzLocked(frequenz));
        }

        private int hfreq(double freq) => (int)Math.Floor(freq);

        private bool isFrequenzAvailableByCount(double frequenz)
        {
            if (!voiceFQ.ContainsKey(frequenz) || isFrequenzLocked(frequenz)) return true;
            if (frequenz == 0) return true;
            if (hfreq(frequenz) == AdminFunkFrequenz) return true;
            if (hfreq(frequenz) == EventFunkFrequenz) return true;
            if (voiceFQ[frequenz].Count >= ZivilFunkCountLimit) return false;
            else return true;
        }

        private void InitPlayerVoice(DbPlayer dbPlayer)
        {
            VoiceListHandler.Instance.InitPlayerVoice(dbPlayer);
            dbPlayer.Player.TriggerNewClient("setVoiceType", 1);

            NAPI.Task.Run(() =>
            {
                dbPlayer.Player.SetSharedData("voiceRange", 12);
                dbPlayer.Player.SetSharedData("VOICE_RANGE_TYPE", 1);
            });

            dbPlayer.SetData("voiceType", 1);
        }

        private bool CheckNSATerm(double frequenz)
        {
            int l_Frequenz = (int)frequenz;

            if (l_Frequenz == 1000 || l_Frequenz == 1004 || l_Frequenz == 1005 || l_Frequenz == 1001 || l_Frequenz == 9000 || l_Frequenz == 1008 || l_Frequenz == 1010)
                return true;

            return false;
        }

        private void CheckFrequenz(double frequenz)
        {
            if (!voiceFQ.ContainsKey(frequenz)) voiceFQ.Add(frequenz, new List<DbPlayer>());
            if (!voiceFQDataStrings.ContainsKey(frequenz)) voiceFQDataStrings.Add(frequenz, "");
        }

        private void AddToVoice(DbPlayer dbPlayer)
        {
            if (hasPlayerRadio(dbPlayer))
            {
                double frequenz = getPlayerFrequenz(dbPlayer);
                CheckFrequenz(frequenz);
                voiceFQ[frequenz].Add(dbPlayer);
                actualizeFrequenzDataString(frequenz);
                refreshFQVoiceForPlayerFrequenz(dbPlayer);
            }
        }

        public void ChangeFrequenz(DbPlayer dbPlayer, double frequenz, bool ignoreLocked = false)
        {
            if (hasPlayerRadio(dbPlayer))
            {
                if (isFrequenzLockedForTeam(dbPlayer.Team, frequenz) && !ignoreLocked && !dbPlayer.IsNSADuty)
                {
                    if (voiceFQ.ContainsKey(frequenz)) voiceFQ[frequenz].Remove(dbPlayer);
                    dbPlayer.SendNewNotification("Ihre Sicherheitsstufe reicht fuer diese Frequenz nicht aus!");
                    return;
                }

                CheckFrequenz(frequenz);
                                
                if(!isFrequenzAvailableByCount(frequenz) && getPlayerFrequenz(dbPlayer) != frequenz)
                {
                    dbPlayer.SendNewNotification("Diese Frequenz ist bereits voll!");
                    return;
                }

                if (dbPlayer.IsNSADuty && !CheckNSATerm(frequenz) && isFrequenzLockedForTeam(dbPlayer.Team, frequenz))
                {
                    if (voiceFQ.ContainsKey(frequenz)) voiceFQ[frequenz].Remove(dbPlayer);
                    dbPlayer.SendNewNotification("Ihre Sicherheitsstufe reicht fuer diese Frequenz nicht aus!");
                    return;
                }

                if (dbPlayer.IsACop() && dbPlayer.IsInDuty() && !isFrequenzLocked(frequenz) && !dbPlayer.IsUndercover() && hfreq(frequenz) != AdminFunkFrequenz && hfreq(frequenz) != EventFunkFrequenz)
                {
                    if (voiceFQ.ContainsKey(frequenz)) voiceFQ[frequenz].Remove(dbPlayer);
                    dbPlayer.SendNewNotification("Im Dienst können Sie keine oeffentlichen Kanäle nutzen!");
                    return;
                }

                if(hfreq(frequenz) == AdminFunkFrequenz)
                {
                    if(!dbPlayer.Rank.Features.Contains("adminfunk"))
                    {
                        dbPlayer.SendNewNotification("Warum du auch immer hier rein willst, du darfst das aber nicht!");
                        return;
                    }
                }

                if (hfreq(frequenz) == EventFunkFrequenz)
                {
                    if (!Configuration.Instance.EventActive)
                    {
                        dbPlayer.SendNewNotification("Es findet derzeit kein Admin-Event statt! Funkkanal entsprechend nicht verfügbar.", PlayerNotification.NotificationType.ERROR, "Fehler");
                        return;
                    }
                }

                double oldFQ = getPlayerFrequenz(dbPlayer);

                if (voiceFQ.ContainsKey(oldFQ)) voiceFQ[oldFQ].Remove(dbPlayer);

                // Change Item to new
                dbPlayer.Container.EditFirstItemData(ItemModelModule.Instance.GetByType(ItemModelTypes.Radio), "Fq", frequenz);

                // Remove From Old Frequenz
                refreshFQVoiceForFrequenz(oldFQ);

                // Check new fq if exist
                CheckFrequenz(frequenz);

                // Add Playert to FQ and refresh
                if (!voiceFQ[frequenz].Contains(dbPlayer)) voiceFQ[frequenz].Add(dbPlayer);
                refreshFQVoiceForFrequenz(frequenz);
            }
        }

        public bool hasPlayerRadio(DbPlayer dbPlayer)
        {
            if(dbPlayer.Container.GetItemAmount(ItemModelModule.Instance.GetByType(ItemModelTypes.Radio)) > 0) return true;
            else
            {
                dbPlayer.Player.TriggerNewClient("setRadioChatPlayers", "");
                dbPlayer.funkStatus = FunkStatus.Deactive;
                return false;
            }
        }

        public double getPlayerFrequenz(DbPlayer dbPlayer)
        {
            double fq = 0.0;

            if(hasPlayerRadio(dbPlayer))
            {
                try
                {
                    // Get Frequenz from Funkgerat Item
                    Item item = dbPlayer.Container.GetItemById((int)ItemModelModule.Instance.GetByType(ItemModelTypes.Radio).Id);
                    if (item.Data == null)
                    {
                        item.Data = new Dictionary<string, dynamic>();
                    }
                    if (!item.Data.ContainsKey("Fq"))
                    {
                        item.Data.TryAdd("Fq", 0.0);
                    }
                    fq = (double)item.Data["Fq"];
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            return fq;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.funkStatus = FunkStatus.Deactive;
        }
    }
}