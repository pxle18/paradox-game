using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Email
{
    public class EmailModule : Module<EmailModule>
    {
        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.Emails = new Dictionary<uint, DbEmail>();

            string query = $"SELECT * FROM `player_emails` WHERE `player_id` = '{dbPlayer.Id}' ORDER BY date DESC LIMIT 15;";
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var emailReader = cmd.ExecuteReader())
                {
                    if (emailReader.HasRows)
                    {
                        while (emailReader.Read())
                        {
                            dbPlayer.Emails.Add(emailReader.GetUInt32("id"), new DbEmail(emailReader));
                        }
                    }
                }
            }
        }

        public void SendPlayerEmail(DbPlayer dbPlayer, string subject, string template)
        {
            string query = $"INSERT INTO player_emails(player_id, subject, body, readed, date)  VALUES('{dbPlayer.Id}', '{subject}', '{template}', '0', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'); SELECT LAST_INSERT_ID();";

            uint newId = 0;
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var emailReader = cmd.ExecuteReader())
                {
                    if (emailReader.HasRows)
                    {
                        while (emailReader.Read())
                        {
                            newId = emailReader.GetUInt32(0);
                        }
                    }
                }
            }

            dbPlayer.Emails.Add(newId, new DbEmail(newId, dbPlayer.Id, subject, template, false, DateTime.Now));
            dbPlayer.SendNewNotification("Sie haben eine Email erhalten!");
        }        
    }

    public class EMailScript : Script
    {
        [RemoteEvent]
        public void deleteMail(Player client, int eMailID, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null && !dbPlayer.IsValid()) return;
            dbPlayer.Emails.Remove((uint)eMailID);
            dbPlayer.SendNewNotification("Die Email wurde erfolgreich gelöscht!");

            string query = $"DELETE FROM `player_emails` WHERE `id` = '{eMailID}' AND player_id = '{dbPlayer.Id}';";
            MySQLHandler.ExecuteAsync(query);
        }
    }
}
