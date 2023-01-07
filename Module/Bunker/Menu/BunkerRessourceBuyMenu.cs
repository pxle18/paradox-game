using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using VMP_CNR.Module.Bunker;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Bunker.Menu
{
    public class BunkerRessourceBuyMenu : MenuBuilder
    {
        public BunkerRessourceBuyMenu() : base(PlayerMenu.BunkerRessourceBuyMenu)
        {
        }

        public override Module.Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
           
            var l_Menu = new Module.Menu.NativeMenu(Menu, "Bunker Import", "");
            l_Menu.Add("Schließen", "");

            l_Menu.Add("100x Aluminiumbarren 80000$", "");
            l_Menu.Add("100x Eisenbarren 120500$", "");
            l_Menu.Add("100x Batterien 18000$", "");
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
                Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
                if (bunker == null || !bunker.IsControlledByTeam(dbPlayer.TeamId)) return false;

                switch (index)
                {
                    case 0: // 100 Alu
                        int price = 80000;
                        uint ItemId = 462;
                        int amount = 100;

                        if (BunkerModule.Instance.LimitRessourceAlu >= 1000)
                        {
                            dbPlayer.SendNewNotification($"Davon hab ich nichts mehr da!");
                            return false;
                        }
                        else BunkerModule.Instance.LimitRessourceAlu += amount;

                        if (!dbPlayer.TakeBlackMoney(price))
                        {
                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughSWMoney(price));
                            return false;
                        }
                        else
                        {
                            dbPlayer.SendNewNotification($"Bestellung von {amount} {ItemModelModule.Instance.Get(ItemId).Name} für ${price} Schwarzgeld!");

                            BunkerModule.Instance.RessourceOrders.Add(new BunkerOrder(ItemId, amount));
                            return true;
                        }
                    case 1: // 100 Eisen
                        price = 120500;
                        ItemId = 300;
                        amount = 100;

                        if (BunkerModule.Instance.LimitRessourceIron >= 1000)
                        {
                            dbPlayer.SendNewNotification($"Davon hab ich nichts mehr da!");
                            return false;
                        }
                        else BunkerModule.Instance.LimitRessourceIron += amount;

                        if (!dbPlayer.TakeBlackMoney(price))
                        {
                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughSWMoney(price));
                            return false;
                        }
                        else
                        {
                            dbPlayer.SendNewNotification($"Bestellung von {amount} {ItemModelModule.Instance.Get(ItemId).Name} für ${price} Schwarzgeld!");

                            BunkerModule.Instance.RessourceOrders.Add(new BunkerOrder(ItemId, amount));
                            return true;
                        }
                    case 2: // 100 Batterien
                        price = 18000;
                        ItemId = 15;
                        amount = 100;

                        if (BunkerModule.Instance.LimitRessourceBatteries >= 1000)
                        {
                            dbPlayer.SendNewNotification($"Davon hab ich nichts mehr da!");
                            return false;
                        }
                        else BunkerModule.Instance.LimitRessourceBatteries += amount;

                        if (!dbPlayer.TakeBlackMoney(price))
                        {
                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughSWMoney(price));
                            return false;
                        }
                        else
                        {
                            dbPlayer.SendNewNotification($"Bestellung von {amount} {ItemModelModule.Instance.Get(ItemId).Name} für ${price} Schwarzgeld!");

                            BunkerModule.Instance.RessourceOrders.Add(new BunkerOrder(ItemId, amount));
                            return true;
                        }

                    default:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                }
                return false;
            }
        }
    }
}
