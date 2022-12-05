using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;

namespace VMP_CNR.Module.Teams.Blacklist.Menu
{
    public class BlacklistMenuBuilder : MenuBuilder
    {
        public BlacklistMenuBuilder() : base(PlayerMenu.BlacklistMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (p_DbPlayer == null || !p_DbPlayer.IsValid() || !p_DbPlayer.IsAGangster()) return null;

            var l_Menu = new Module.Menu.Menu(Menu, "Blacklist Einträge");
            l_Menu.Add($"Schließen");

            foreach(BlacklistEntry entry in p_DbPlayer.Team.blacklistEntries)
            {
                if (!BlacklistModule.Instance.blacklistTypes.ContainsKey(entry.TypeId)) continue;
                DbPlayer target = Players.Players.Instance.FindPlayerById((uint)entry.BlacklistPlayerId);
                if (target == null || !target.IsValid()) continue;
                l_Menu.Add($"{PlayerNameModule.Instance.Get((uint)entry.BlacklistPlayerId).Name} | ${BlacklistModule.Instance.blacklistTypes[entry.TypeId].Costs} | {BlacklistModule.Instance.blacklistTypes[entry.TypeId].ShortDesc}");
            }

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
