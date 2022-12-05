using System;
using System.Collections.Generic;
using System.Linq;
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

namespace VMP_CNR.Module.Business.FuelStations
{
    public class FuelStationMenuBuilder : MenuBuilder
    {
        public FuelStationMenuBuilder() : base(PlayerMenu.FuelStationMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.TryData("fuelstationId", out uint fuelStationId)) return null;
            var fuelstation = FuelStationModule.Instance.Get(fuelStationId);
            if (fuelstation == null) return null;
            
            var menu = new Menu.Menu(Menu, fuelstation.Name);

            menu.Add($"Schließen");

            if(fuelstation.IsOwnedByBusines())
            {
                if(fuelstation.GetOwnedBusiness() == dbPlayer.GetActiveBusiness() && dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Fuelstation) // Member of business and has rights
                {
                    // Preis einstellen...
                    menu.Add($"Literpreis einstellen");
                    menu.Add($"Namen einstellen");
                }
            }
            else if(dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Owner && dbPlayer.GetActiveBusiness().BusinessBranch.FuelstationId == 0 && dbPlayer.GetActiveBusiness().BusinessBranch.CanBuyBranch())
            {
                menu.Add($"Tankstelle kaufen {fuelstation.BuyPrice}$");
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else
                {
                    if (!dbPlayer.TryData("fuelstationId", out uint fuelStationId)) return false;
                    var fuelstation = FuelStationModule.Instance.Get(fuelStationId);
                    if (fuelstation == null) return false;
                    
                    if (fuelstation.IsOwnedByBusines())
                    {
                        if (fuelstation.GetOwnedBusiness() == dbPlayer.GetActiveBusiness() && dbPlayer.GetActiveBusinessMember().Fuelstation) // Member of business and has rights
                        {
                            MenuManager.DismissCurrent(dbPlayer);

                            if (index == 1)
                            {
                                // Preis einstellen...
                                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Preis pro Liter", Callback = "SetFuelPrice", Message = "Stelle den Preis pro Liter ein" });
                                return true;
                            }
                            else if(index == 2)
                            {
                                // Name
                                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Tankstellen Name", Callback = "SetFuelName", Message = "Gib einen neuen Namen ein (max 32 Stellen)." });
                                return true;
                            }
                        }
                    }
                    else if (dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Owner && dbPlayer.GetActiveBusiness().BusinessBranch.FuelstationId == 0 && dbPlayer.GetActiveBusiness().BusinessBranch.CanBuyBranch())
                    {
                        // Kaufen
                        if(dbPlayer.GetActiveBusiness().TakeMoney(fuelstation.BuyPrice))
                        {
                            dbPlayer.GetActiveBusiness().BusinessBranch.SetFuelstation(fuelstation.Id);
                            dbPlayer.SendNewNotification($"{fuelstation.Name} erfolgreich fuer ${fuelstation.BuyPrice} erworben!");
                            fuelstation.OwnerBusiness = dbPlayer.GetActiveBusiness();
                        }
                        else {
                            dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(fuelstation.BuyPrice));
                        }
                    }
                    return true;
                }
            }
        }
    }
}