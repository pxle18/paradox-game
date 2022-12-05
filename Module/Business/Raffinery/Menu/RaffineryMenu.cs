using GTANetworkMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Business.Raffinery;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Tattoo;

namespace VMP_CNR.Module.Business.Raffinery
{
    public class RaffineryMenuBuilder : MenuBuilder
    {
        public RaffineryMenuBuilder() : base(PlayerMenu.RaffineryMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.TryData("raffineryId", out uint raffineryId)) return null;
            var raffinery = RaffineryModule.Instance.Get(raffineryId);
            if (raffinery == null) return null;
            
            var menu = new Menu.Menu(Menu, "Oelfoerderpumpe");

            menu.Add($"Schließen");

            if (raffinery.IsOwnedByBusines())
            {
                try
                {
                    if (raffinery.GetOwnedBusiness() == dbPlayer.GetActiveBusiness() && dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Raffinery) // Member of business and has rights
                    {
                        Console.WriteLine($"---RAFFINERY {raffinery.Id}---");
                        Console.WriteLine($"RaffineryBusiness: {raffinery.GetOwnedBusiness().Id}");
                        Console.WriteLine($"Name: {dbPlayer.GetName()}");
                        Console.WriteLine($"BusinessId: {dbPlayer.ActiveBusinessId}");
                        Console.WriteLine($"IsMemberOfABusiness: {dbPlayer.IsMemberOfBusiness()}");
                        Console.WriteLine($"Rights?: {dbPlayer.GetActiveBusinessMember().Raffinery}");

                        SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position, 12.0f);
                        if (sxVehicle != null)
                        {
                            menu.Add($"{sxVehicle.GetName()} ({sxVehicle.databaseId}) beladen");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Crash(ex);
                }
            }
            else if (dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Owner && dbPlayer.GetActiveBusiness().BusinessBranch.RaffinerieId == 0 && dbPlayer.GetActiveBusiness().BusinessBranch.CanBuyBranch())
            {
                menu.Add($"Oelfoerderpumpe kaufen {raffinery.BuyPrice}$");
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
                if (index == 1)
                {
                    if (!dbPlayer.TryData("raffineryId", out uint raffineryId)) return false;
                    var raffinery = RaffineryModule.Instance.Get(raffineryId);
                    if (raffinery == null) return false;

                    if (raffinery.IsOwnedByBusines())
                    {
                        if (raffinery.GetOwnedBusiness() == dbPlayer.GetActiveBusiness() && dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Raffinery) // Member of business and has rights     
                        {
                            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position, 6.0f);
                            if (sxVehicle != null)
                            {
                                // Fahrzeug beladen
                                if(sxVehicle.SyncExtension.Locked)
                                {
                                    dbPlayer.SendNewNotification("Fahrzeug muss aufgeschlossen sein!");
                                    return true;
                                }
                                else
                                {
                                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = sxVehicle.GetName() + " beladen" , Callback = "LoadIntoVehicle", Message = "Geben Sie die Lademenge an" });

                                }
                            }
                        }
                    }
                    else if (dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Owner && dbPlayer.GetActiveBusiness().BusinessBranch.RaffinerieId == 0 && dbPlayer.GetActiveBusiness().BusinessBranch.CanBuyBranch())
                    {
                        // Kaufen
                        if (dbPlayer.GetActiveBusiness().TakeMoney(raffinery.BuyPrice))
                        {
                            dbPlayer.GetActiveBusiness().BusinessBranch.SetRaffinerie(raffinery.Id);
                            dbPlayer.SendNewNotification($"Oelfoerderpumpe erfolgreich fuer ${raffinery.BuyPrice} erworben!");
                            raffinery.OwnerBusiness = dbPlayer.GetActiveBusiness();
                        }
                        else {
                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(raffinery.BuyPrice));
                        }
                    }
                    return true;
                }
                MenuManager.DismissCurrent(dbPlayer);
                return false;
            }
        }
    }
}