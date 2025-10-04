using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Business.FuelStations;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Threading;
using static VMP_CNR.Module.Chat.Chats;

namespace VMP_CNR.Module.Anticheat
{
    class AntiCheatEvents : Script
    {
        [RemoteEvent]
        public void __ragemp_cheat_detected(Player player, int cheatCode)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            string l_Cheat = "Cheat Engine";

            switch (cheatCode)
            {
                case 0:
                case 1:
                    l_Cheat = "Cheat Engine";
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    l_Cheat = "Externer Hack";
                    break;
                case 7:
                    l_Cheat = "Mod-Menü";
                    break;
                case 8:
                case 9:
                    l_Cheat = "Speed Hack";
                    break;
                case 11:
                    l_Cheat = "Nutzung von Sandboxie";
                    break;
                default:
                    break;
            }

            Logger.AddActionLogg(dbPlayer.Id, cheatCode);
            Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Dringender Anticheat-Verdacht: {dbPlayer.GetName()} ({l_Cheat}) gegeben.");
        }

        [RemoteEvent]
        public void __ragemp_cheat_detected_timed(Player player, int cheatCode)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            string l_Cheat = "Cheat Engine [EARLYDETECT]";

            switch (cheatCode)
            {
                case 0:
                case 1:
                    l_Cheat = "Cheat Engine [EARLYDETECT]";
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    l_Cheat = "Externer Hack [EARLYDETECT]";
                    break;
                case 7:
                    l_Cheat = "Mod-Menü [EARLYDETECT]";
                    break;
                case 8:
                case 9:
                    l_Cheat = "Speed Hack [EARLYDETECT]";
                    break;
                case 11:
                    l_Cheat = "Nutzung von Sandboxie [EARLYDETECT]";
                    break;
                default:
                    break;
            }

            Logger.AddActionLogg(dbPlayer.Id, cheatCode);
            Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Dringender Anticheat-Verdacht: {dbPlayer.GetName()} ({l_Cheat}) gegeben.  [EARLYDETECT]");
        }

        [RemoteEvent]
        public void amo(Player player, string secobj, string wpnObj, string amountObj, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!Int32.TryParse(secobj, out Int32 sec)) return;
            if (!Int32.TryParse(wpnObj, out Int32 wpn)) return;
            if (!Int32.TryParse(amountObj, out Int32 amount)) return;

            var query = $"INSERT INTO `log_makro` (`player_id` ,`sec`,`wpn`,`amount`) VALUES ('{dbPlayer.Id}', '{sec}','{wpn}','{amount}');";
            MySQLHandler.ExecuteAsync(query, Sync.MySqlSyncThread.MysqlQueueTypes.Logging);
        }

        [RemoteEvent]
        public void aads(Player player, Player targetPlayer,uint distance,uint damage,uint bone, string wpnhsh)
        {
            if (player == null || targetPlayer == null) return;
            if (!long.TryParse(wpnhsh, out long wpn)) return;

            DamageLogItem dmg = new DamageLogItem(player, targetPlayer, distance, damage, bone, wpn);
            DamageThread.Instance.AddToDamageLogs(dmg);
        }

        [RemoteEvent]
        public void aains(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            if (player == null) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.CheatKeyInsert, $"Pressed at {DateTime.Now}");
        }

        [RemoteEvent]
        public void CheckNameTags(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(0);

                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                    
                if (dbPlayer.RankId > 0) return;

                if (dbPlayer.HasData(""))
                {
                    Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.NameTags, $"(nametags aktiv - Spielerhuds)");
                }

                Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"DRINGEND Anticheat-Verdacht: {dbPlayer.GetName()} (nametags detected).");
            });
        }


        [RemoteEvent]
        public async Task sftptbp(Player player, string key) // Event Shift + Tab anonymized for Steam Overlay
        {
            if (!player.CheckRemoteEventKey(key)) return;

            await Task.Run(() =>
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (dbPlayer.HasData("mediusing"))
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("highteamchat", $"DRINGEND Anticheat-Verdacht: {dbPlayer.GetName()} (Steamoverlay während Verbandkasten).");
                    Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.sftptbp, $"{dbPlayer.GetName()} M");
                }
                if (dbPlayer.HasData("armorusing"))
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("highteamchat", $"DRINGEND Anticheat-Verdacht: {dbPlayer.GetName()} (Steamoverlay während Schutzweste).");
                    Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.sftptbp, $"{dbPlayer.GetName()} A");
                }
            });
        }


        [RemoteEvent]
        public void wrongScreenScale(Player player, float res, string key) // disabled for now, remove clientside event too
        {
            /* if (!player.CheckRemoteEventKey(key)) return;

            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if(dbPlayer.HasData("lastScreenResFailed"))
            {
                int warnings = dbPlayer.GetData("lastScreenResFailed");
                if (warnings > 3)
                {
                    dbPlayer.Kick("Dieses Bildformat wird auf Void nicht unterstützt!");
                    Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.WrondScreenFormat, $"{dbPlayer.GetName()} Format: " + res);
                    return;
                }
                else
                {
                    dbPlayer.SetData("lastScreenResFailed", warnings + 1);
                    dbPlayer.SendNewNotification("Dieses Bildformat wird auf Void nicht unterstützt, bitte änder deine Grafikeinstellung! (Warning " + warnings +"/3");
                    return;
                }
            }
            else
            {
                dbPlayer.SetData("lastScreenResFailed", 1);
                dbPlayer.SendNewNotification("Dieses Bildformat wird auf Void nicht unterstützt, bitte änder deine Grafikeinstellung! (Warning 1/3");
            } */
        }
    }
}
