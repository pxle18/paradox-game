using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Anticheat.Menu
{
    public class AntiCheatTeleportMenu : MenuBuilder
    {
        public AntiCheatTeleportMenu() : base(PlayerMenu.AntiCheatTeleportMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (p_DbPlayer == null) return null;

            p_DbPlayer.Player.TriggerNewClient("removeAcMark");

            var l_Menu = new Module.Menu.Menu(Menu, "AC Meldungen");

            l_Menu.Add($"Schließen");

            foreach (KeyValuePair<uint, List<ACTeleportReportObject>> kvp in AntiCheatModule.Instance.ACTeleportReports)
            {
                l_Menu.Add($"{PlayerName.PlayerNameModule.Instance.Get(kvp.Key).Name} - {kvp.Value.Count()} Meldungen");
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else
                {
                    int idx = 1;

                    foreach (KeyValuePair<uint, List<ACTeleportReportObject>> kvp in AntiCheatModule.Instance.ACTeleportReports)
                    {
                        if (idx == index)
                        {
                            dbPlayer.SetData("acUserId", kvp.Key);
                            Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.AntiCheatTeleportDetailMenu, dbPlayer).Show(dbPlayer);
                            return false;
                        }
                        else idx++;
                    }

                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
            }
        }
    }
}