using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerLoginDataValidationModule
    {
        public static int FreigeschaltetGroupId = 63;

        public static void SyncUserBanToForum(int forumId)
        {
            // Remove Freigeschaltet Gruppe
            if (!Configuration.Instance.DevMode && ServerFeatures.IsActive("forumsync"))
                MySQLHandler.ExecuteForum($"DELETE FROM wcf1_user_to_group WHERE userID = '{forumId}' AND groupID = '63'");
        }

        public static bool HasValidForumAccount(int forumid)
        {
            if (Configuration.Instance.DevMode)
                return true;

            if (!ServerFeatures.IsActive("forumsync"))
                return true;

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnectionForum()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM wcf1_user WHERE userID = '{forumid}' AND banned = '0' AND userID IN (SELECT userID FROM wcf1_user_to_group WHERE groupID = '63');";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                }
                conn.Close();
            }
            return false;
        }
    }
}
