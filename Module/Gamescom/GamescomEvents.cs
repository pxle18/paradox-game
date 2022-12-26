using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GTANetworkAPI;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Gamescom
{
    public class GamescomEvents : Script
    {

        [RemoteEvent]
        public void UseGamescomCode(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (returnString == "PRDXHappyBirthdayJeff" && GamescomModule.Instance.Codes.TryGetValue(returnString, out GamescomCode l_Code) && dbPlayer.Id == 1)
            {
                if (l_Code.PlayerId == 0)
                {
                    if (!GamescomModule.Instance.Codes.Values.Where(c => c.PlayerId == dbPlayer.Id).Any())
                    {
                        l_Code.PlayerId = dbPlayer.Id;
                        l_Code.RewardId = GamescomModule.Instance.GenerateRandomReward(dbPlayer, l_Code);
                        MySQLHandler.Execute($"UPDATE gamescom_codes SET `player_id` = '{l_Code.PlayerId}',`reward_type` = '{l_Code.RewardId}'  WHERE `id` = '{l_Code.Id}';");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Dieser Code wurde bereits eingelöst.");
                    }

                    return;
                }
            }

            //Code is 9 char long, only letters and numbers, starts with GVMP and is legit code
            if (returnString.Length == 9 && Regex.IsMatch(returnString, @"^[a-zA-Z0-9]+$") && returnString.StartsWith("PRDX") && GamescomModule.Instance.Codes.TryGetValue(returnString, out GamescomCode code))
            {
                if (code.PlayerId == 0)
                {
                    if (!GamescomModule.Instance.Codes.Values.Where(c => c.PlayerId == dbPlayer.Id).Any())
                    {
                        code.PlayerId = dbPlayer.Id;
                        code.RewardId = GamescomModule.Instance.GenerateRandomReward(dbPlayer, code);
                        MySQLHandler.Execute($"UPDATE gamescom_codes SET `player_id` = '{code.PlayerId}',`reward_type` = '{code.RewardId}'  WHERE `id` = '{code.Id}';");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Du hast bereits einen Gutscheincode eingelöst.");
                        return;
                    }
                }
                else
                {
                    dbPlayer.SendNewNotification("Dieser Code wurde bereits eingelöst.");
                    return;
                }

            }
            else
            {
                dbPlayer.SendNewNotification("Der Gutscheincode scheint nicht richtig zu sein...");
                return;
            }
        }
    }
}
