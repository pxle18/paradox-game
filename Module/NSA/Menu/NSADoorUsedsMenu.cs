using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.NSA.Menu
{
    public class NSADoorUsedsMenuBuilder : MenuBuilder
    {
        public NSADoorUsedsMenuBuilder() : base(PlayerMenu.NSADoorUsedsMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "NSA Keycard Nutzungen");
            l_Menu.Add($"Schließen");

            if (p_DbPlayer.TryData("doorId", out uint doorId))
            {
                Door door = DoorModule.Instance.Get(doorId);
                if (door == null) return null;
                if (door.LastUseds.Count > 0)
                {
                    foreach (LastUsedFrom lastUsed in door.LastUseds)
                    {
                        l_Menu.Add($"{lastUsed.Name} - {lastUsed.DateTime.ToShortTimeString()} - {(lastUsed.Opened ? "geöffnet" : "geschlossen")}");
                    }
                }
            }

            if (p_DbPlayer.TryData("jumpPointId", out int jpid))
            {
                JumpPoint jumpPoint = JumpPointModule.Instance.Get(jpid);
                if (jumpPoint == null) return null;
                if (jumpPoint.LastUseds.Count > 0)
                {
                    foreach (LastUsedFrom lastUsed in jumpPoint.LastUseds)
                    {
                        l_Menu.Add($"{lastUsed.Name} - {lastUsed.DateTime.ToShortTimeString()} - {(lastUsed.Opened ? "geöffnet" : "geschlossen")}");
                    }
                }
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
