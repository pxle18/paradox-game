using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Email;
using VMP_CNR.Module.FIB;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Events;
using VMP_CNR.Module.Staatskasse;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Weapons.Component;

namespace VMP_CNR.Module.Crime
{
    public static class CrimePlayerExtension
    {

        public static void AddCrime(this DbPlayer dbPlayer, DbPlayer Cop, CrimeReason crime, string notice = "")
        {
            if (!Cop.IsValid() || !dbPlayer.IsValid() || crime == null) return;


            string reporter = Cop.GetName();

            if (crime.Jailtime <= 0)
            {
                EmailModule.Instance.SendPlayerEmail(dbPlayer, "Ticket erhalten", EmailTemplates.GetTicketTemplate(CrimeModule.Instance.GetCrimeCosts(new CrimePlayerReason(crime, notice), dbPlayer.EconomyIndex), crime.Name));
            }

            dbPlayer.AddCrimeLogical(reporter, new CrimePlayerReason(crime, notice));
        }

        public static void AddCrime(this DbPlayer dbPlayer, string reporter, CrimeReason crime, string notice = "")
        {
            if (!dbPlayer.IsValid() || crime == null) return;

            dbPlayer.AddCrimeLogical(reporter, new CrimePlayerReason(crime, notice));

            if (crime.Jailtime <= 0)
            {
                EmailModule.Instance.SendPlayerEmail(dbPlayer, "Ticket erhalten", EmailTemplates.GetTicketTemplate(CrimeModule.Instance.GetCrimeCosts(new CrimePlayerReason(crime, notice), dbPlayer.EconomyIndex), crime.Name));
            }
        }

        private static void AddCrimeLogical(this DbPlayer dbPlayer, string reporter, CrimePlayerReason crime, string notice = "")
        {
            if (notice == "")
            {
                notice = $"Beamter {reporter} am {DateTime.Now.ToString("dd/MM/yyyy")} um {DateTime.Now.ToString("HH:mm")} Uhr";
            }
            crime.Notice = notice;
            
            dbPlayer.Crimes.Add(crime);
            dbPlayer.AddDbCrime(crime);
        }

        public static void RemoveCrime(this DbPlayer dbPlayer, CrimePlayerReason crime, string officer = "", bool showMessage = true)
        {
            if (dbPlayer.Crimes.Contains(crime))
                dbPlayer.Crimes.Remove(crime);

            // dbPlayer.SendNewNotification($"Dir wurde das Verbrechen {crime.Name} erlassen.");
            if (showMessage)
                Teams.TeamModule.Instance.SendChatMessageToDepartments($"{dbPlayer.GetName()} wurde {crime.Name} {(officer != "" ? "von " + officer : "")} erlassen!");

            dbPlayer.RemoveSingleDBCrime(crime);
        }

        public static void RemoveAllCrimes(this DbPlayer dbPlayer, string officer = "", bool showMessage = true)
        {
            dbPlayer.Crimes.Clear();
            dbPlayer.ResetDbCrimes();
            dbPlayer.UHaftTime = 0;

            if (showMessage)
                Teams.TeamModule.Instance.SendChatMessageToDepartments($"{dbPlayer.GetName()} wurde die Akte {(officer != "" ? "von " + officer : "")} erlassen!");
        }
        
        private static void AddDbCrime(this DbPlayer dbPlayer, CrimePlayerReason crime)
        {
            // Insert into DB
            string query =
                $"INSERT INTO `player_crime` (`player_id`, `crime_reason_id`, `notice`) VALUES ('{dbPlayer.Id}', '{crime.Id}', '{crime.Notice}');";
            MySQLHandler.ExecuteAsync(query);
        }

        private static void RemoveSingleDBCrime(this DbPlayer dbPlayer, CrimePlayerReason crime)
        {
            // Insert into DB
            string query =
                $"DELETE FROM `player_crime` WHERE player_id = '{dbPlayer.Id}' AND crime_reason_id = '{crime.Id}' AND notice LIKE '%{crime.Notice}%';";
            MySQLHandler.ExecuteAsync(query);
        }

        public static async Task LoadCrimes(this DbPlayer dbPlayer)
        {
            dbPlayer.Crimes.Clear();

            // Loading Wanted for Player
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText =
                    $"SELECT crime_reason_id, notice FROM `player_crime` WHERE player_id = '{dbPlayer.Id}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var reason = CrimeReasonModule.Instance.Get(reader.GetUInt32("crime_reason_id"));
                            if (reason == null) continue;

                            string notice = reader.GetString("notice");
                            dbPlayer.Crimes.Add(new CrimePlayerReason(reason, notice));
                        }
                    }
                }
                await conn.CloseAsync();
            }

            // Crime History
            dbPlayer.CrimeHistories.Clear();

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText =
                    $"SELECT crimes, date FROM `player_crime_history` WHERE player_id = '{dbPlayer.Id}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dbPlayer.CrimeHistories.Add(new CrimePlayerHistory(reader.GetString("crimes"), reader.GetDateTime("date")));
                        }
                    }
                }
                await conn.CloseAsync();
            }
        }

        public static void AddToCrimeHistory(this DbPlayer dbPlayer, string crimeString)
        {
            dbPlayer.CrimeHistories.Add(new CrimePlayerHistory(crimeString, DateTime.Now));
            MySQLHandler.ExecuteAsync($"INSERT INTO player_crime_history (`player_id`, `crimes`, `date`) VALUES ('{dbPlayer.Id}', '{crimeString}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}');");
        }
        
        private static void ResetDbCrimes(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"DELETE FROM `player_crime` WHERE `player_id` = '{dbPlayer.Id}'");
        }
        
        public static void ArrestPlayer(this DbPlayer dbPlayer, DbPlayer dbPlayerCop, bool SpawnPlayer = true)
        {
            if (dbPlayer.IsACop()  && dbPlayer.IsInDuty()) return;

            int jailtime = CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes);
            int jailcosts = CrimeModule.Instance.CalcJailCosts(dbPlayer.Crimes, dbPlayer.EconomyIndex);

            dbPlayer.JailTime[0] = jailtime;

            // Checke auf Jailtime
            if (dbPlayerCop != null && dbPlayer.JailTime[0] == 0)
            {
                dbPlayerCop.SendNewNotification(dbPlayer.GetName() + " hat keine Haftzeit offen!");
                return;
            }

            // Uhaft
            if(dbPlayer.UHaftTime > 0)
            {
                //Limit auf 60min
                int uHaftJailTimeReverse = dbPlayer.UHaftTime > 60 ? 60 : dbPlayer.UHaftTime;

                // Maximal bis auf 10 min runter
                dbPlayer.JailTime[0] = dbPlayer.JailTime[0] - uHaftJailTimeReverse <= 10 ? 10 : dbPlayer.JailTime[0] - dbPlayer.UHaftTime;
            }

            EmailModule.Instance.SendPlayerEmail(dbPlayer, "Inhaftierung", EmailTemplates.GetArrestTemplate(dbPlayer.Crimes, jailcosts, jailtime));

            string JailStringHistroy = $"Inhaftierung ({jailtime} | $ {jailcosts}):";

            string ListCrimes = "Sie wurden wegen folgenden Verbrechen Inhaftiert: ";

            foreach(CrimePlayerReason crime in dbPlayer.Crimes)
            {
                ListCrimes += crime.Name + ",";

                if(crime.Jailtime > 0)
                {
                    JailStringHistroy += crime.Name + ",";
                }
            }
            
            dbPlayer.RemoveAllCrimes();
            dbPlayer.TempWanteds = 0;

            dbPlayer.ResetData("follow");

            if (dbPlayerCop != null)
            {
                dbPlayerCop.ResetData("follow");
                dbPlayerCop.SendNewNotification(GlobalMessages.General.hasArrested(dbPlayer.GetName(), dbPlayer.JailTime[0] - 1));
                dbPlayer.SendNewNotification(GlobalMessages.General.isArrested(dbPlayerCop.GetName(), dbPlayer.JailTime[0] - 1));
            }

            dbPlayer.TakeAnyMoney((int)jailcosts, true);
            KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, jailcosts);

            dbPlayer.SendNewNotification(
                "Durch Ihre Inhaftierung wurde Ihnen eine Strafzahlung von $" + jailcosts + " in Rechnung gestellt!");

            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, " + dbPlayer.GetName() +
                " sitzt nun hinter Gittern!");

            dbPlayer.jailtimeReducing[0] = Convert.ToInt32(dbPlayer.JailTime[0] / 3);

            dbPlayer.SendNewNotification(ListCrimes);
            dbPlayer.AddToCrimeHistory(JailStringHistroy);

            // Set Voice To Normal
            dbPlayer.Player.SetSharedData("voiceRange", (int)VoiceRangeTypes.Whisper);
            dbPlayer.SetData("voiceType", 3);
            dbPlayer.Player.TriggerNewClient("setVoiceType", 3);

            if (SpawnPlayer)
            {
                dbPlayer.RemoveWeapons();
                dbPlayer.ResetAllWeaponComponents();

                dbPlayer.Player.SetPosition(new Vector3());
                PlayerSpawn.OnPlayerSpawn(dbPlayer.Player);
            }

            dbPlayer.Save();
        }
    }
}
