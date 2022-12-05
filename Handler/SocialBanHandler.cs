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
            
            /**
             * TODO: Edit before release 
             * 
            // Main Thread, durch PlayerWrapper
            string scname = await new PlayerWrapper(player).getProperty("SocialClubName");

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
            }*/


            return false;
        }

        public void DeleteEntry(Player player)
        {
            var query =
                $"DELETE FROM socialbans WHERE Name = '{player.SocialClubName}';";
            MySQLHandler.ExecuteAsync(query);
        }
    }
}