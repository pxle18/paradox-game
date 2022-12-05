using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.PlayerLoginSecure
{
    public class PlayerLoginSecureModule : Module<PlayerLoginSecureModule>
    {
        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            // TODO: Edit before release
            //if (Configuration.Instance.DevMode) return;
            //try
            //{
            //    if (!Configuration.Instance.DevMode)
            //    {
            //        using (var conn2 = new MySqlConnection(Configuration.Instance.GetMySqlConnectionForum()))
            //        using (var cmd2 = conn2.CreateCommand())
            //        {
            //            conn2.Open();
            //            // Check Player Forum ACC Last 24h
            //            cmd2.CommandText = $"SELECT lastActivityTime FROM `wcf1_user` WHERE userID = '{dbPlayer.ForumId}' LIMIT 1;";
            //            using (var reader2 = cmd2.ExecuteReader())
            //            {
            //                if (reader2.HasRows)
            //                {
            //                    if (reader2.Read())
            //                    {
            //                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(reader2.GetInt64("lastActivityTime"));
            //                        DateTime dateTime = dateTimeOffset.UtcDateTime;

            //                        if (DateTime.Now > dateTime.AddHours(24))
            //                        {
            //                            // kick user because forum 24h...
            //                            Logging.Logger.Print("Player " + dbPlayer.GetName() + " 24h Forum login kick!");
            //                            dbPlayer.Player.SendNotification("Letzter Forum Login muss weniger als 24h her sein!");
            //                            dbPlayer.Player.Kick("Letzter Forum Login muss weniger als 24h her sein!");
            //                            return;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //catch(Exception e)
            //{
            //    Logging.Logger.SaveToDbLog(e.ToString());
            //}
        }
    }
}
