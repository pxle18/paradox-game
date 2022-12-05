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
        public override bool Load(bool reload = false)
        {
            return true;
        }


        enum ActionType
        {
            KICK=0,
            WHISPER=1,
            SETMONEY = 2,
        }

        public override async Task OnTenSecUpdateAsync()
        {
            if (!ServerFeatures.IsActive("acpupdate"))
                return;

            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = $"SELECT * FROM acp_action";
                using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    int lastId = 0;
                    while (await reader.ReadAsync())
                    {
                        lastId = await reader.GetFieldValueAsync<int>(0);
                        int playerId = await reader.GetFieldValueAsync<int>(1);
                        PlayerName.PlayerName admin = PlayerNameModule.Instance.GetAll().Values.ToList().Where(pn => pn.Id == (uint) playerId).FirstOrDefault();
                        ActionType actionType = (ActionType)await reader.GetFieldValueAsync<int>(2);
                        var actionInfo = (await reader.GetFieldValueAsync<string>(3)).Split(new string[] { "###" }, StringSplitOptions.None);

                        var findPlayer = Players.Players.Instance.FindPlayer(actionInfo[0]);
                        if (findPlayer == null || !findPlayer.IsValid())
                            continue;

                        switch (actionType)
                        {
                            //TAGETPLAYERID###REASON
                            case ActionType.KICK:
                                findPlayer.SendNewNotification($"Du wirst in 60 Sekunden vom Server gekickt! Grund: {actionInfo[1]}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 25000);
                                await Task.Delay(30000);
                                findPlayer.SendNewNotification($"Du wirst in 30 Sekunden vom Server gekickt! Grund: {actionInfo[1]}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);
                                await Task.Delay(30000);
                                await Chats.SendGlobalMessage(RankModule.Instance.Get(admin.RankId).Name + " " + admin.Name + " hat " +
                                                              findPlayer.GetName() + " vom Server gekickt! (Grund: " + actionInfo[1] + ")", Chats.COLOR.RED, Chats.ICON.GLOB);
                                DatabaseLogging.Instance.LogAcpAdminAction(admin, findPlayer.GetName(), adminLogTypes.kick, actionInfo[1]);
                                findPlayer.Save();
                                findPlayer.SendNewNotification($"Sie wurden gekickt. Grund {actionInfo[1]}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);
                                findPlayer.Player.Kick();
                                break;
                            case ActionType.WHISPER:
                                findPlayer.SendNewNotification($"{actionInfo[1]}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);
                                break;
                            case ActionType.SETMONEY:
                                if (!int.TryParse(actionInfo[1], out int amount))
                                    break; ;

                                if (amount > 0)
                                {
                                    findPlayer.GiveBankMoney(amount);

                                    findPlayer.SendNewNotification("ERSTATTUNG: Ihnen wurde$" +
                                                               amount + " auf Ihr Konto gutgeschrieben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);

                                    DatabaseLogging.Instance.LogAcpAdminAction(admin.Name, findPlayer.GetName(), adminLogTypes.log, $"{amount}$ Givemoney");
                                }

                                break;
                            default:
                                break;
                        }

                        MySQLHandler.ExecuteAsync($"DELETE FROM `acp_action` WHERE `id` = '{lastId}'");
                    }
                }

                await conn.CloseAsync();
            }
        }
    }
}
