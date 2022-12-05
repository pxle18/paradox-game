using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.Carsell.Menu
{
    public class CarsellMenuBuilder : MenuBuilder
    {
        public CarsellMenuBuilder() : base(PlayerMenu.CarsellMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "Verwaltung");

            l_Menu.Add($"Fahrzeug bestellen");
            l_Menu.Add($"Fahrzeug entfernen");
            l_Menu.Add($"Lieferfahrzeuge");

            l_Menu.Add($"Schließen");
            
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
                    Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.CarsellBuyMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.CarsellDeleteMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
                else if (index == 2)
                {
                    Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.CarsellDeliverCustomerMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
