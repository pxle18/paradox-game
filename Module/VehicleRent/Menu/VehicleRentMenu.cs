using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Tattoo;

namespace VMP_CNR.Module.VehicleRent
{
    public class VehicleRentMenuBuilder : MenuBuilder
    {
        public VehicleRentMenuBuilder() : base(PlayerMenu.VehicleRentMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.Menu(Menu, "Fahrzeug vermieten");

            menu.Add($"Schließen");

            foreach(SxVehicle SxVeh in VehicleHandler.Instance.GetPlayerVehicles(dbPlayer.Id))
            {
                menu.Add($"{SxVeh.databaseId} {(SxVeh.Data.mod_car_name == "" ? SxVeh.Data.Model : SxVeh.Data.mod_car_name)}");
            }
            return menu;
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
                    return false;
                }
                else
                {
                    int idx = 1;

                    foreach (SxVehicle SxVeh in VehicleHandler.Instance.GetPlayerVehicles(dbPlayer.Id))
                    {
                        if(idx == index)
                        {
                            MenuManager.DismissCurrent(dbPlayer);
                            dbPlayer.SetData("vehicleRentId", SxVeh.databaseId);

                            // Open User Input
                            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Fahrzeug vermieten", Callback = "PlayerRentDays", Message = "Wie lange soll das Fahrzeug vermietet werden?" });
                            return false;
                        }
                        idx++;
                    }
                }
                return false;
            }
        }
    }
}