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
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.Carsell.Menu
{
    public class CarsellCustomerMenuBuilder : MenuBuilder
    {
        public CarsellCustomerMenuBuilder() : base(PlayerMenu.CarsellCustomerMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.RageExtension.IsInVehicle) return null;

            var l_Menu = new Module.Menu.Menu(Menu, "Fahrzeug Bereitstellen");

            l_Menu.Add($"Schließen");

            l_Menu.Add($"Kundenauftrag erstellen");
            l_Menu.Add($"Farbe lackieren");
            //l_Menu.Add($"Reifenfelgen wechseln"); //04.08.2021 Felgenmenü Espenhai Deaktivierung
            l_Menu.Add($"Preisschild ändern");

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
                if (!dbPlayer.RageExtension.IsInVehicle) return true;

                SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVehicle == null || !sxVehicle.IsValid()) return true;

                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else if (index == 1) // Kundenauftrag Erstellen
                {
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Auftrag erstellen", Callback = "JobCarsellCreateOrder", Message = "Geben Sie den Namen des Kunden an:" });
                    return true;
                }
                else if (index == 2) // Farbe lackieren
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Fahrzeugfarbe ändern", Callback = "JobCarsellSetCarColor", Message = "Geben Sie die Farben an BSP: (101 1):" });
                    return true;
                }
                   /* else if (index == 3) // Reifenfelgen wechseln
                    {
                        Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.CarsellTuneWheelMenu, dbPlayer).Show(dbPlayer);
                        return false;
                    }
                    */
                else if (index == 3) // Preisschild
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Preisschild ändern", Callback = "JobCarsellSetPriceAttach", Message = $"Geben Sie einen Preis zwischen ${Convert.ToInt32(sxVehicle.Data.Price*0.8)} und {Convert.ToInt32(sxVehicle.Data.Price * 1.1)}:" });
                    return true;
                }
                return true;
            }
        }
    }
}
