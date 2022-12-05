using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Teams.Blacklist
{
    public class BlacklistEvents : Script
    {
        [RemoteEvent]
        public void SetBlacklist(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            
            DbPlayer target = Players.Players.Instance.FindPlayer(returnstring);
            if (target != null && target.IsValid() && target.IsInjured() && target.Level >= 5)
            {
                dbPlayer.SetData("blsetplayer", target.Id);
                
                Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.BlacklistTypeMenu, dbPlayer).Show(dbPlayer);
            }
            return;
        }
    }
}
