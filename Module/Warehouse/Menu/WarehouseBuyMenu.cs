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

namespace VMP_CNR.Module.Warehouse
{
    public class WarehouseBuyMenuBuilder : MenuBuilder
    {
        public WarehouseBuyMenuBuilder() : base(PlayerMenu.WarehouseBuyMenu)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            Warehouse warehouse = WarehouseModule.Instance.GetThis(dbPlayer.Player.Position);
            if (warehouse == null) return null;

            var menu = new Menu.NativeMenu(Menu, "Warenlager Ankauf");

            menu.Add($"Schließen");

            foreach(WarehouseItem warehouseItem in warehouse.WarehouseItems)
            {
                menu.Add($"{ItemModelModule.Instance.Get(warehouseItem.RequiredItemId).Name} ${warehouseItem.RequiredItemPrice}");
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
                Warehouse warehouse = WarehouseModule.Instance.GetThis(dbPlayer.Player.Position);
                if (warehouse == null) return false;
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else
                {
                    int idx = 1;
                    foreach (WarehouseItem warehouseItem in warehouse.WarehouseItems)
                    {
                        if (idx == index)
                        {
                            // Get Item Amount Player Has
                            int amount = dbPlayer.Container.GetItemAmount(warehouseItem.RequiredItemId);
                            int buyamount = (amount / warehouseItem.RequiredToResultItemAmount); // zb nur 5er stacks... x.x

                            if(buyamount > 0 && amount >= warehouseItem.RequiredToResultItemAmount)
                            {
                                int realamount = buyamount * warehouseItem.RequiredToResultItemAmount;
                                int playerResultPrice = realamount * warehouseItem.RequiredItemPrice;

                                // Remove Players Items...
                                dbPlayer.Container.RemoveItem(warehouseItem.RequiredItemId, realamount);

                                // Add To bestand..
                                warehouseItem.ResultItemBestand += buyamount;
                                warehouseItem.UpdateBestand();

                                dbPlayer.GiveMoney(playerResultPrice);
                                dbPlayer.SendNewNotification($"Sie haben {realamount} {ItemModelModule.Instance.Get(warehouseItem.RequiredItemId).Name} für ${playerResultPrice} verkauft!");
                            }
                            return true;
                        }
                        idx++;
                    }
                    return true;
                }
            }
        }
    }
}