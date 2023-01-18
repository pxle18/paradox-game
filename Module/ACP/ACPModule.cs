using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Anticheat;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Ranks;
using static VMP_CNR.Module.Chat.Chats;

namespace VMP_CNR.Module.ACP
{
    public sealed class ACPModule : Module<ACPModule>
    {
        enum ActionType: int
        {
            KICK = 1,
            WARN,
            SUSPEND,
            CALL_TO_SUPPORT,
            RELOAD_CONTAINER_PLAYER,
            RELOAD_CONTAINER_VEHICLE,
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

                            uint adminId = reader.GetUInt32("admin_id");
                            uint playerId = reader.GetUInt32("player_id");

                            ActionType actionType = (ActionType)reader.GetInt32("action_type");

                            var action = reader.GetString("action");
                            var actionArgument = action.Split("###");

                            PlayerNameModel adminPlayer = PlayerNameModule.Instance.Get(adminId);
                            if (adminPlayer == null) continue;

                            var targetPlayer = Players.Players.Instance.FindPlayerById(playerId);
                            if (targetPlayer == null) continue;

                            switch (actionType)
                            {
                                case ActionType.KICK:
                                    KickPlayer(targetPlayer, adminPlayer, actionArgument[0]);
                                    break;

                                case ActionType.WARN:
                                    WarnPlayer(targetPlayer, adminPlayer, actionArgument[0]);
                                    break;

                                case ActionType.SUSPEND:
                                    SuspendPlayer(targetPlayer, adminPlayer);
                                    break;

                                case ActionType.CALL_TO_SUPPORT:
                                    CallToSupport(targetPlayer, adminPlayer);
                                    break;

                                case ActionType.RELOAD_CONTAINER_PLAYER:
                                    ReloadContainerPlayer(targetPlayer, adminPlayer);
                                    break;
                            }

                            MySQLHandler.ExecuteAsync($"DELETE FROM `acp_action` WHERE `id` = '{id}'");
                        }
                    }
                }
                await keyConn.CloseAsync();
            }
        }

        private async void KickPlayer(DbPlayer player, PlayerNameModel admin, string reason)
        {
            if (player == null || !player.IsValid()) return;

            await SendGlobalMessage("ACP: " + admin.Name + " hat " + player.GetName() + " vom Server gekickt! (Grund: " + reason + ")", COLOR.RED, ICON.GLOB);

            player.Save();
            player.SendNewNotification($"Sie wurden gekickt. Grund {reason}", PlayerNotification.NotificationType.ADMIN);
            player.Player.TriggerNewClient("freezePlayer", true);

            await Task.Delay(5000);

            player.SendNewNotification("Kicked.");
            player.Player.Kick();
        }

        private void WarnPlayer(DbPlayer player, PlayerNameModel admin, string reason)
        {
            if (player == null || !player.IsValid()) return;

            player.SendNewNotification($"Sie haben eine Verwarnung von {admin.Name} wegen {reason} erhalten. Für mehr Informationen kontaktieren Sie denn Support.", PlayerNotification.NotificationType.ADMIN, "ADMIN", 30 * 1000);
        }

        private async void SuspendPlayer(DbPlayer player, PlayerNameModel admin)
        {
            if (player == null || !player.IsValid()) return;

            await Chats.SendGlobalMessage("ACP: " + admin.Name + " hat " +
                                            player.GetName() + " von der Community ausgeschlossen!", COLOR.RED, ICON.GLOB);

            AntiCheatModule.Instance.ACBanPlayer(player, "Community-Ausschluss von " + admin.Name);
        }

        private void ReloadContainerPlayer(DbPlayer player, PlayerNameModel admin)
        {
            Players.Players.Instance.SendMessageToHighTeam("highteam-log", $"(ACP) {admin.Name} hat das Inventar von {player.GetName()} verändert.");

            player.Container = ContainerManager.LoadContainer(player.Id, ContainerTypes.PLAYER, 0);
        }

        private void CallToSupport(DbPlayer player, PlayerNameModel admin)
        {
            if (player == null || !player.IsValid()) return;

            player.SendNewNotification("Sobald die aktive RP-Situation abgeschlossen, bitte im TeamSpeak-Support einfinden. @" + admin.Name, PlayerNotification.NotificationType.ADMIN, "ADMIN - " + admin.Name, 30 * 1000);
        }
    }
}
 