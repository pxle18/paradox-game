using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class ItemOrderOrdersMenuBuilder : MenuBuilder
    {
        public ItemOrderOrdersMenuBuilder() : base(PlayerMenu.ItemOrderOrdersMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            ItemOrderNpc itemOrderNpc = ItemOrderNpcModule.Instance.GetByPlayerPosition(dbPlayer);
            if (itemOrderNpc == null) return null;
            
            var menu = new NativeMenu(Menu, "Fertiggestellt");
            
            foreach(ItemOrder itemOrder in ItemOrderModule.Instance.GetPlayerFinishedListByNpc(dbPlayer, itemOrderNpc))
            {
                menu.Add($"{itemOrder.ItemAmount} {itemOrder.Item.Name}");
            }

            menu.Add(GlobalMessages.General.Close());
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

                ItemOrderNpc itemOrderNpc = ItemOrderNpcModule.Instance.GetByPlayerPosition(dbPlayer);
                if (itemOrderNpc == null) return false;

                int idx = 0;
                foreach (ItemOrder itemOrder in ItemOrderModule.Instance.GetPlayerFinishedListByNpc(dbPlayer, itemOrderNpc))
                {
                    if (idx == index)
                    {
                        // Entnehme Item
                        if (!dbPlayer.Container.CanInventoryItemAdded(itemOrder.Item, itemOrder.ItemAmount))
                        {
                            dbPlayer.SendNewNotification($"Sie koennen so viel nicht tragen!");
                            return false;
                        }

                        if(ItemOrderModule.Instance.DeleteOrder(itemOrder))
                        {
                            dbPlayer.Container.AddItem(itemOrder.Item, itemOrder.ItemAmount);

                            Task.Run(async () =>
                            {
                                dbPlayer.SetData("Itemorderflood", true);
                                Chats.sendProgressBar(dbPlayer, 2000);
                                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                                await GTANetworkAPI.NAPI.Task.WaitForMainThread(2000);
                                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                                if (dbPlayer.HasData("Itemorderflood"))
                                {
                                    dbPlayer.ResetData("Itemorderflood");
                                }

                                dbPlayer.SendNewNotification($"Sie haben {itemOrder.ItemAmount} {itemOrder.Item.Name} entnommen!");
                            });

                        }

                        return true;
                    }
                    idx++;
                }
                MenuManager.DismissCurrent(dbPlayer);
                return false;
            }
        }
    }
}