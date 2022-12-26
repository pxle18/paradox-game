using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Schwarzgeld;

namespace VMP_CNR
{
    public class ItemOrderItemsMenuBuilder : MenuBuilder
    {
        public ItemOrderItemsMenuBuilder() : base(PlayerMenu.ItemOrderItemsMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            ItemOrderNpc itemOrderNpc = ItemOrderNpcModule.Instance.GetByPlayerPosition(dbPlayer);
            if (itemOrderNpc == null) return null;

            var menu = new NativeMenu(Menu, "Verarbeitung");

            if (itemOrderNpc.RequiredTeams.Count == 0 || itemOrderNpc.RequiredTeams.Contains((int)dbPlayer.TeamId))
            {
                foreach (ItemOrderNpcItem npcItem in itemOrderNpc.NpcItems.Where(i => dbPlayer.TeamRank >= i.RangRestricted))
                {
                    menu.Add($"{npcItem.RewardItemAmount} {npcItem.RewardItem.Name} {npcItem.Hours}h");
                }
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
                foreach (ItemOrderNpcItem npcItem in itemOrderNpc.NpcItems.Where(i => dbPlayer.TeamRank >= i.RangRestricted))
                {
                    if (index == idx)
                    {
                        if (itemOrderNpc.RequiredTeams.Count > 0 && !itemOrderNpc.RequiredTeams.Contains((int)dbPlayer.TeamId)) return false;
                        //if (!dbPlayer.CanInteractAntiFlood()) return false;
                        if(npcItem.RangRestricted > dbPlayer.TeamRank)
                        {
                            dbPlayer.SendNewNotification($"Sie benötigen mindestens Rang {dbPlayer.TeamRank}!");
                            return false;
                        }
                        if(npcItem.Limited != 0 && ItemOrderModule.Instance.GetItemOrderCountByItem(dbPlayer, npcItem) >= npcItem.Limited)
                        {
                            dbPlayer.SendNewNotification($"Maximal {npcItem.Limited} {npcItem.RewardItem.Name} herstellbar!");
                            return true;
                        }

                        // Discount 4 Gangwar Gebiet

                        int totalhours = npcItem.Hours;
                        int totalPrice = npcItem.RequiredMoney;
                        if (dbPlayer.Team != null && dbPlayer.Team.IsGangsters())
                        {
                            int owned = GangwarTownModule.Instance.GetAll().Where(gt => gt.Value.OwnerTeam == dbPlayer.Team).Count();

                            if (owned > 0)
                            {
                                totalPrice -= (totalPrice / 10); // 10% nachlass bei GW Gebiet
                                if (totalhours > 4)
                                {
                                    totalhours = totalhours - 1; // Stunden bei Besitz von Gebieten -1h
                                }
                            }
                        }

                        // Check Required Items
                        foreach (KeyValuePair<ItemModel, int> kvp in npcItem.RequiredItems)
                        {
                            if (dbPlayer.Container.GetItemAmount(kvp.Key) < kvp.Value)
                            {
                                dbPlayer.SendNewNotification($"Sie benoetigen {kvp.Value} {kvp.Key.Name}!");
                                return true;
                            }
                        }

                        // Check if user got schwarzgeld in inventory
                        if(dbPlayer.Container.GetItemAmount(SchwarzgeldModule.SchwarzgeldId) <= totalPrice)
                        {
                            if (!dbPlayer.TakeMoney(totalPrice))
                            {
                                dbPlayer.SendNewNotification($"Sie benoetigen {totalPrice}$!");
                                return true;
                            }
                        }
                        else
                        {
                            dbPlayer.Container.RemoveItem(SchwarzgeldModule.SchwarzgeldId, totalPrice);
                        }

                        //RemoveRequiredItems
                        foreach (KeyValuePair<ItemModel, int> kvp in npcItem.RequiredItems)
                        {
                            dbPlayer.Container.RemoveItem(kvp.Key, kvp.Value);
                        }
                        
                        ItemOrderModule.Instance.AddDbOrder(npcItem.RewardItemId, npcItem.RewardItemAmount, (int)dbPlayer.Id, totalhours, (int)itemOrderNpc.Id);

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
                            dbPlayer.SendNewNotification($"Herstellung von {npcItem.RewardItem.Name} begonnen, Dauer {totalhours}h!");
                            if (itemOrderNpc.Id == 12)
                            {
                                Logger.AddWeaponFactoryLog(dbPlayer.Id, npcItem.RewardItemId);
                            }
                        });

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