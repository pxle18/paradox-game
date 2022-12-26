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
    public class BlacklistTypeMenuBuilder : MenuBuilder
    {
        public BlacklistTypeMenuBuilder() : base(PlayerMenu.BlacklistTypeMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer p_DbPlayer)
        {
            if (p_DbPlayer == null || !p_DbPlayer.IsValid() || !p_DbPlayer.IsAGangster() || !p_DbPlayer.HasData("blsetplayer")) return null;

            var l_Menu = new Module.Menu.NativeMenu(Menu, "Blacklist Hinzufügen");
            l_Menu.Add($"Schließen");
            
            foreach (KeyValuePair<int, BlacklistType> kvp in BlacklistModule.Instance.blacklistTypes)
            {
                l_Menu.Add($"{kvp.Value.Description}");
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
                if(index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else
                {
                    // Get Player
                    DbPlayer target = Players.Players.Instance.GetByDbId(dbPlayer.GetData("blsetplayer"));
                    if (target == null || !target.IsValid() || target.TeamId == dbPlayer.TeamId) return false;

                    if(target.IsOnBlacklist((int)dbPlayer.TeamId))
                    {
                        dbPlayer.SendNewNotification("Person befindet sich bereits auf der Blacklist!");
                        return false;
                    }

                    int idx = 1;
                    foreach (KeyValuePair<int, BlacklistType> kvp in BlacklistModule.Instance.blacklistTypes)
                    {
                        if(idx == index)
                        {
                            if(kvp.Value.RequiredRang > dbPlayer.TeamRank)
                            {
                                dbPlayer.SendNewNotification($"Diesen Eintrag können Sie erst ab Rang {kvp.Value.RequiredRang} setzen!");
                                return false;
                            }

                            dbPlayer.Team.AddBlacklistEntry(dbPlayer, target, kvp.Value.TypeId);
                            dbPlayer.Team.SendNotification($"{target.GetName()} wurde von {dbPlayer.GetName()} auf die Blacklist gesetzt. (Grund: {kvp.Value.Description})");
                            target.SendNewNotification($"Du befindest dich nun auf der Blacklist der {dbPlayer.Team.Name} Grund: {kvp.Value.Description}");
                            return true;
                        }
                        idx++;
                    }
                }
                
                return true;
            }
        }
    }
}
