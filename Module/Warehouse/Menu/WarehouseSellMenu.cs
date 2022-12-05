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
    public class WarehouseSellMenuBuilder : MenuBuilder
    {
        public WarehouseSellMenuBuilder() : base(PlayerMenu.WarehouseSellMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            Warehouse warehouse = WarehouseModule.Instance.GetThis(dbPlayer.Player.Position);
            if (warehouse == null) return null;

            var menu = new Menu.Menu(Menu, "Warenlager Verkauf");

            menu.Add($"Schließen");

            foreach (WarehouseItem warehouseItem in warehouse.WarehouseItems)
            {
                menu.Add($"{ItemModelModule.Instance.Get(warehouseItem.ResultItemId).Name} ${warehouseItem.ResultItemPrice}");
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
                            // Check is enough there...
                            
                            if (warehouseItem.ResultItemBestand > 0)
                            {
                                if(!dbPlayer.Container.CanInventoryItemAdded(warehouseItem.ResultItemId))
                                {
                                    dbPlayer.SendNewNotification("Sie haben nicht genug platz im Inventar!");
                                    return false;
                                }

                                if (dbPlayer.TakeMoney(warehouseItem.ResultItemPrice))
                                {
                                    // Add Players Items...
                                    dbPlayer.Container.AddItem(warehouseItem.ResultItemId);

                                    // Add To bestand..
                                    warehouseItem.ResultItemBestand -= 1;
                                    warehouseItem.UpdateBestand();

                                    dbPlayer.SendNewNotification($"Sie haben {ItemModelModule.Instance.Get(warehouseItem.ResultItemId).Name} für ${warehouseItem.ResultItemPrice} verkauft!");
                                }
                                else
                                {
                                    dbPlayer.SendNewNotification($"Sie haben nicht genug Geld dabei!");
                                    return false;
                                }
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