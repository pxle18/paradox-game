using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Ranks;

namespace VMP_CNR.Module.ACP
{
    public sealed class ACPModule : Module<ACPModule>
    {
        enum ActionType
        {
            KICK,
            SUSPEND
        }

        public override async Task OnTenSecUpdateAsync()
        {
            if (!ServerFeatures.IsActive("acpupdate"))
                return;

            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                await keyConn.OpenAsync();
                keyCmd.CommandText = "SELECT * FROM acp_action";
                using (var reader = await keyCmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = reader.GetInt32("id");

                            MySQLHandler.ExecuteAsync($"DELETE FROM `acp_action` WHERE `id` = '{id}'");
                        }
                    }
                }
                await keyConn.CloseAsync();
            }
        }


    }
}
