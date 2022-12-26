using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.NSA.Menu
{
    public class NSAVehicleModifyMenuBuilder : MenuBuilder
    {
        public NSAVehicleModifyMenuBuilder() : base(PlayerMenu.NSAVehicleModifyMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.HasData("nsa_work_vehicle")) return null;
            var l_Menu = new Module.Menu.NativeMenu(Menu, "NSA Fahrzeugverwaltung");
            l_Menu.Add($"Schließen");
            l_Menu.Add($"Nummernschild ändern");
            l_Menu.Add($"Farbe ändern");
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
                if (!dbPlayer.HasData("nsa_work_vehicle")) return true;
                
                switch (index)
                {
                    case 0:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                    case 1:
                        MenuManager.DismissCurrent(dbPlayer);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Nummernschild ändern", Callback = "SetCarPlateNSA", Message = "Geben Sie ein Nummernschild ein:" });
                        return true;
                    case 2:
                        MenuManager.DismissCurrent(dbPlayer);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Fahrzeugfarbe ändern", Callback = "SetCarColorNSA", Message = "Geben Sie die Farben an BSP: (101 1):" });
                        return true;
                    default:
                        break;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
