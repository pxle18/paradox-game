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
    public class AntiCheatTeleportDetailMenu : MenuBuilder
    {
        public AntiCheatTeleportDetailMenu() : base(PlayerMenu.AntiCheatTeleportDetailMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.HasData("acUserId")) return null;

            uint targetPlayerId = p_DbPlayer.GetData("acUserId");

            if (!AntiCheatModule.Instance.ACTeleportReports.ContainsKey(targetPlayerId)) return null;

            p_DbPlayer.Player.TriggerNewClient("removeAcMark");

            var l_Menu = new Module.Menu.NativeMenu(Menu, $"AC Meldungen {PlayerName.PlayerNameModule.Instance.Get(targetPlayerId).Name}");

            l_Menu.Add($"Schließen");

            foreach (ACTeleportReportObject kvp in AntiCheatModule.Instance.ACTeleportReports[targetPlayerId])
            {
                l_Menu.Add($"{kvp.ReportDateTime.ToString("yyyy-MM-dd H:mm:ss")} - Distanz: {kvp.Distance}");
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
                if (!dbPlayer.HasData("acUserId")) return true;

                uint targetPlayerId = dbPlayer.GetData("acUserId");

                if (!AntiCheatModule.Instance.ACTeleportReports.ContainsKey(targetPlayerId)) return true;


                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else
                {
                    int idx = 1;

                    foreach (ACTeleportReportObject kvp in AntiCheatModule.Instance.ACTeleportReports[targetPlayerId])
                    {
                        if (idx == index)
                        {
                            dbPlayer.Player.TriggerNewClient("setAcMark", kvp.SourcePos, kvp.DestinationPos);
                            dbPlayer.SendNewNotification("Positionen von TeleportAC Meldung auf Karte markiert!");
                            return true;
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