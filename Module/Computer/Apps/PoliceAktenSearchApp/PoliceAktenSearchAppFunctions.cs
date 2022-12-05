using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.PoliceAktenSearchApp
{
    public class PoliceAktenSearchAppFunctions
    {
        public static List<DbPlayer> GetPlayersFromSearchQuery(DbPlayer dbPlayer, string searchQuery)
        {
            List<DbPlayer> returnPlayers = new List<DbPlayer>();

            if (dbPlayer.LastQueryBreak.AddSeconds(5) > DateTime.Now)
            {
                dbPlayer.SendNewNotification("Antispam: Bitte 5 Sekunden warten!");
                return returnPlayers;
            }

            returnPlayers = GetSearchPlayersFromDb(searchQuery);

            dbPlayer.LastQueryBreak = DateTime.Now;

            return returnPlayers;
        }

        private static List<DbPlayer> GetSearchPlayersFromDb(string statement)
        {
            List<DbPlayer> resultPlayers = new List<DbPlayer>();

            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                keyConn.Open();
                keyCmd.CommandText = statement;
                using (var reader = keyCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            
                        }
                    }
                }
                keyConn.Close();
            }

            return resultPlayers;
        }
    }
}
