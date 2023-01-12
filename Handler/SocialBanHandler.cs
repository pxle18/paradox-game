using MySql.Data.MySqlClient;
using System;
using GTANetworkAPI;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players;
using System.Threading.Tasks;

namespace VMP_CNR
{
    public sealed class SocialBanHandler
    {
        public static SocialBanHandler Instance { get; } = new SocialBanHandler();

        private SocialBanHandler()
        {
        }

        public void AddEntry(Player player)
        {
            MySQLHandler.ExecuteAsync(
                $"INSERT INTO socialbans (Name) VALUES ('{player.SocialClubName}');");
        }

        public async Task<bool> IsPlayerSocialBanned(Player player)
        {
            if (player == null) return false;

            // Main Thread, durch PlayerWrapper
            string scname = await new PlayerWrapper(player).getProperty("SocialClubName");
            if (scname == "") return false;

            // Wechsel in einen Background Thread
            await Task.Delay(1);
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = $"SELECT * FROM socialbans WHERE Name = '{MySqlHelper.EscapeString(scname)}';";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                }
                await conn.CloseAsync();
            }

            return false;
        }

        public async Task<bool> IsHwidBanned(Player player)
        {
            if (player == null) return false;

            // Main Thread, durch PlayerWrapper
            string scname = await new PlayerWrapper(player).getProperty("Serial");
            if (scname == "") return false;

            // Wechsel in einen Background Thread
            await Task.Delay(1);
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = $"SELECT * FROM player WHERE Hwid = '{MySqlHelper.EscapeString(scname)}' AND warns >= 3;";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                }
                await conn.CloseAsync();
            }

            return false;
        }

        public async Task<int> GetSocialClubCount(Player player, int PlayerId)
        {
            if (player == null) return 0;

            // Main Thread, durch PlayerWrapper
            string scname = await new PlayerWrapper(player).getProperty("SocialClubName");

            // Wechsel in einen Background Thread
            await Task.Delay(1);
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = $"SELECT Count(Id) As SocialCount FROM player WHERE Id = '{PlayerId}';";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            return reader.GetInt32("SocialCount");
                        }
                    }
                    else return 0;
                }
                await conn.CloseAsync();
            }

            return 0;
        }

        public void DeleteEntry(Player player)
        {
            var query =
                $"DELETE FROM socialbans WHERE Name = '{player.SocialClubName}';";
            MySQLHandler.ExecuteAsync(query);
        }
    }
}