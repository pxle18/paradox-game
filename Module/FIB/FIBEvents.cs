using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.FIB
{
    public class FIBEvents : Script
    {
        [RemoteEvent]
        public void FIBSetUnderCoverName(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamId != (int)teams.TEAM_FIB) return;
            
            if(!returnstring.Contains("_") || returnstring.Length < 3)
            {
                dbPlayer.SendNewNotification("Bitte Format einhalten: Max_Mustermann!");
                return;
            }

            string[] ucName = returnstring.Split("_");

            if(ucName.Length < 2 || ucName[0].Length < 3 || ucName[1].Length < 3)
            {
                dbPlayer.SendNewNotification("Bitte Format einhalten: Max_Mustermann!");
                return;
            }
            
            dbPlayer.SendNewNotification($"Sie sind nun als {ucName[0]}_{ucName[1]} im Undercover dienst!");
            dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} ist nun als {ucName[0]}_{ucName[1]} im Undercover dienst!", 5000, 10);

            dbPlayer.SetUndercover(ucName[0], ucName[1]);
            return;
        }

        [RemoteEvent]
        public void FIBEditMemberCallback(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamId != (int)teams.TEAM_FIB) return;
            if (dbPlayer.TeamRank < 11) return;

            if (!returnstring.Contains("_") || returnstring.Length < 3)
            {
                dbPlayer.SendNewNotification("Bitte Format einhalten: Max_Mustermann!");
                return;
            }

            DbPlayer targetAgent = Players.Players.Instance.FindPlayer(returnstring);
            if (targetAgent == null || !targetAgent.IsValid())
            {
                dbPlayer.SendNewNotification("Nicht gefunden!");
                return;
            }

            if (targetAgent.TeamId != (int)teams.TEAM_FIB)
            {
                dbPlayer.SendNewNotification("Der Bürger ist nicht beim FIB angestellt!");
                return;
            }

            if (targetAgent.TeamRank == 0)
            {
                dbPlayer.SendNewNotification("Nur vollwertige Agents haben die Möglichkeit eine Freigabe zu erlangen!");
                return;
            }

            dbPlayer.SetData("fib_permit_id", targetAgent.Id);
            Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.FIBPermitMenu, dbPlayer).Show(dbPlayer);
            return;
        }
    }
}
