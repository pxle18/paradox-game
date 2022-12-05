using System.Collections.Generic;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Boerse
{
    public class PlayerAktie
    {
        public uint PlayerID { get; private set; }
        public uint AktienID { get; private set; }
        public uint Amount { get; private set; }

        public PlayerAktie(uint playerid, uint aktienId, uint amount)
        {
            PlayerID = playerid;
            AktienID = aktienId;
            Amount = amount;
        }

        public void Add(uint amount)
        {
            Refresh();
            Amount += amount;
            Save();
        }

        public void Subtract(uint amount)
        {
            Refresh();
            
            if (Amount - amount <= 0)
                Amount = 0;
            else
            {
                Amount -= amount;
            }

            Save();
        }

        private void Save()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnectionBoerse()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"UPDATE `player_aktien` SET amount = {Amount} WHERE player_id = '{PlayerID}' AND aktien_id = '{AktienID}';";

                cmd.ExecuteNonQuery();
            }
        }
        
        /// <summary>
        /// Holt sich den aktuellen Datenbank-Stand um Glitches/Dupes zu vermeiden
        /// </summary>
        private void Refresh()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnectionBoerse()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT `amount` FROM `player_aktien` WHERE player_id = '{PlayerID}' AND `aktien_id` = '{AktienID}';";
                
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Amount = reader.GetUInt32("aktien_id");
                        }
                    }
                }
            }
        }
    }

    public static class PlayerAktienExtensionClass
    {
        /// <summary>
        /// Lädt die vom Spieler gekauften Aktien
        /// </summary>
        /// <param name="dbPlayer"></param>
        public static void LoadPlayerAktien(this DbPlayer dbPlayer)
        {
            dbPlayer.Aktien = new List<PlayerAktie>();
            
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnectionBoerse()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT * FROM `player_aktien` WHERE player_id = '{dbPlayer.Id}';";
                
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            uint AktienID = reader.GetUInt32("aktien_id");
                            uint PlayerID = reader.GetUInt32("player_id");
                            uint Amount = reader.GetUInt32("amount");
                            
                            PlayerAktie aktie = new PlayerAktie(PlayerID, AktienID, Amount);
                            dbPlayer.Aktien.Add(aktie);
                        }
                    }
                }
            }
        }
    }
}