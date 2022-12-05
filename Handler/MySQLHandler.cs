using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Sync;
using static VMP_CNR.Module.Sync.MySqlSyncThread;

namespace VMP_CNR
{
    public static class GVMPDataReader 
    {
        public static UInt32 GetUInt32(this DbDataReader reader, string ordinal)
        {
            return Convert.ToUInt32(reader.GetInt32(reader.GetOrdinal(ordinal)));
        }
        public static Double GetDouble(this DbDataReader reader, string ordinal)
        {
            return Convert.ToDouble(reader.GetOrdinal(ordinal));
        }

        public static Int32 GetInt32(this DbDataReader reader, string ordinal)
        {
            return reader.GetInt32(reader.GetOrdinal(ordinal));
        }

        public static string GetString(this DbDataReader reader, string ordinal)
        {
            return reader.GetString(reader.GetOrdinal(ordinal));
        }

        public static float GetFloat(this DbDataReader reader, string ordinal)
        {
            return reader.GetFloat(reader.GetOrdinal(ordinal));
        }

        public static DateTime GetDateTime(this DbDataReader reader, string ordinal)
        {
            return reader.GetDateTime(reader.GetOrdinal(ordinal));
        }
    }

    public static class MySQLHandler
    {
        public static void Execute(string query)
        {
            if (query == "") return;
            try
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static void ExecuteAsync(string query, MysqlQueueTypes queType = MysqlQueueTypes.Default )
        {
            if (query == "") return;

            MySqlSyncThread.Instance.Add(query, queType);
        }

        public static async void InsertAsync(string tableName, params object[] data)
        {
            if (data.Length == 0) return;

            string columns = string.Join(",", data.Where((value, index) => index % 2 == 0));
            string values = string.Join(",", data.Where((value, index) => index % 2 == 1));

            string query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

            try
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    cmd.CommandText = query;
                    await cmd.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static async void UpdateAsync(string tableName, string condition, params object[] data)
        {
            if (data.Length == 0) return;

            int i = 0;
            string str = "";

            foreach (var item in data)
            {
                str += item;
                if (i < data.Length - 1)
                {
                    if (i % 2 == 0)
                        str += " = ";
                    else
                        str += ", ";
                }

                i++;
            }

            string query = $"UPDATE {tableName} SET {str} WHERE {condition}";

            try
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    cmd.CommandText = query;
                    await cmd.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static void QueryFailed(Task task)
        {
            Exception ex = task.Exception;
            if (ex != null) Logger.Crash(ex);
        }

        public static void ExecuteForum(string query)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (Configuration.Instance.DevMode) return;
                    using (var connection = new MySqlConnection(Configuration.Instance.GetMySqlConnectionForum()))
                    using (var command = connection.CreateCommand())
                    {
                        connection.Open();
                        command.CommandText = @query;
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            });
        }

        public static bool IsValidNoSQLi(DbPlayer dbPlayer, string query)
        {
            bool injectionMaybe = false;
            if(query.ToLower().Contains("select") ||  query.ToLower().Contains("update") || query.ToLower().Contains("insert") || query.ToLower().Contains("delete"))
            {
                injectionMaybe = true;
            }

            if (query.ToLower().Contains("'") || query.ToLower().Contains(";"))
            {
                injectionMaybe = true;
            }

            if (injectionMaybe)
            {
                Players.Instance.SendMessageToAuthorizedUsers("highteamchat", $"DRINGEND Anticheat-Verdacht: {dbPlayer.GetName()} (SQLi Verdacht: {query}).");
                Logger.LogToAcDetections(dbPlayer.Id, ACTypes.Injection, $"{dbPlayer.GetName()} A");
                return false;
            }

            return true;
        }

        public static bool IsValidNoSQLi(Player player, string query)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            bool injectionMaybe = false;
            if (query.ToLower().Contains("update") || query.ToLower().Contains("insert") || query.ToLower().Contains("delete"))
            {
                injectionMaybe = true;
            }

            if (query.ToLower().Contains("'") || query.ToLower().Contains(";"))
            {
                injectionMaybe = true;
            }

            if (injectionMaybe)
            {
                Players.Instance.SendMessageToAuthorizedUsers("highteamchat", $"DRINGEND Anticheat-Verdacht: {dbPlayer.GetName()} (SQLi Verdacht: {query}).");
                Logger.LogToAcDetections(dbPlayer.Id, ACTypes.Injection, $"{dbPlayer.GetName()}:: {query}");
                return false;
            }

            return true;
        }
    }
}