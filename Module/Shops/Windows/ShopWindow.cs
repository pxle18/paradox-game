using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Business.NightClubs;
using System.Threading.Tasks;
using VMP_CNR.Module.Schwarzgeld;
using VMP_CNR.Module.NSA;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Teams.Shelter;
using VMP_CNR.Module.ShopTakeover;

namespace VMP_CNR.Module.Shops.Windows
{
    public class ShopItemX
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }
        [JsonProperty(PropertyName = "price")]
        public int Price { get; }

        [JsonProperty(PropertyName = "img")]
        public string Img { get; }

        public ShopItemX(uint id, string name, int price, string img)
        {
            Id = id;
            Name = name;
            Price = price;
            Img = img;
        }
    }

    public class ShopWindow : Window<Func<DbPlayer, string, int, List<ShopItemX>, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "title")] private string Title { get; }
            [JsonProperty(PropertyName = "id")] private int ShopId { get; }
            [JsonProperty(PropertyName = "items")] private List<ShopItemX> Items { get; }

            public ShowEvent(DbPlayer dbPlayer, string title, int id, List<ShopItemX> items) : base(dbPlayer)
            {
                Title = title;
                ShopId = id;
                Items = items;
            }
        }

        public ShopWindow() : base("Shop")
        {
        }

        public override Func<DbPlayer, string, int, List<ShopItemX>, bool> Show()
        {
            return (player, title, shopId, items) => OnShow(new ShowEvent(player, title, shopId, items));
        }

        [RemoteEvent]
        public void shopBuy(Player client, string shopBuyEventJson, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                try
                {
                    var shopBuyEvent = JsonConvert.DeserializeObject<ShopBuyEvent>(shopBuyEventJson);

                    DbPlayer dbPlayer = client.GetPlayer();
                    if (dbPlayer == null || !dbPlayer.IsValid()) return;

                    var accessedShop = ShopModule.Instance.Get((uint)shopBuyEvent.shopId);
                    int price = 0;
                    float weight = 0.0f;
                    int requiredSlots = 0;

                    NightClub nightClub = null;

                    if (dbPlayer.HasData("nightClubShopId"))
                    {
                        nightClub = NightClubModule.Instance.Get(dbPlayer.GetData("nightClubShopId"));
                        dbPlayer.ResetData("nightClubShopId");
                        if (nightClub == null) return;
                    }

                    // Check Weight Items and Calculate Price
                    foreach (ShopBuyEvent.BasketItem item in shopBuyEvent.basket)
                    {
                        if (item == null) continue;
                        ItemModel itemData = ItemModelModule.Instance.Get((uint)item.itemId);

                        if (item.count < 1)
                        {
                            dbPlayer.SendNewNotification("Du kannst keine negative Anzahl kaufen!");
                            return;
                        }

                        if (nightClub != null)
                        {
                            if (nightClub.Container.GetItemAmount(itemData.Id) < item.count)
                            {
                                dbPlayer.SendNewNotification($"{itemData.Name} aktuell nicht vorraetig!");
                                return;
                            }

                            // Add to Price
                            price += nightClub.NightClubShopItems.First(x => x.ItemId == item.itemId).Price * item.count; ;
                        }
                        else
                        {
                            // Stored Items Checks..
                            ShopItem shopItem = accessedShop.ShopItems.Where(x => x.ItemId == item.itemId).FirstOrDefault();
                            if (shopItem == null) return; // BUG?!

                            if (shopItem.IsStoredItem && shopItem.Stored < item.count)
                            {
                                dbPlayer.SendNewNotification($"{itemData.Name} aktuell nicht vorraetig!");
                                return;
                            }

                            price += accessedShop.ShopItems.First(x => x.ItemId == item.itemId).Price * item.count;
                        }
                        weight += itemData.Weight * item.count;

                        //Item ist ein Rucksack
                        if (item.itemId >= 3 && item.itemId <= 5)
                        {
                            foreach (var backpackConfiguration in Module.Items.Scripts.BackpackList.backpackList)
                            {
                                if (dbPlayer.Container.GetItemAmount(backpackConfiguration.ItemModel) >= 1)
                                {
                                    dbPlayer.SendNewNotification("Du besitzt bereits einen Rucksack! Einkauf Fehlgeschlagen");
                                    return;
                                }
                            }

                        }

                        var similarStack = ContainerManager.GetSlotOfSimilairSingleItemsToStack(dbPlayer.Container, itemData);
                        var stackRequiredSlots = 99;

                        if (similarStack == -1)
                        { //Es gibt bisher keinen Stack mit diesem Itemtyp
                            stackRequiredSlots = item.count / itemData.MaximumStacksize < 1 ? 1 : (int)Math.Ceiling((decimal)item.count / (decimal)itemData.MaximumStacksize);
                        }
                        else
                        { //Es wurde ein Stack mit dem Itemtyp gefunden
                            var spaceLeftOnSlot = itemData.MaximumStacksize - ContainerManager.GetAmountOfItemsOnSlot(dbPlayer.Container, similarStack);
                            stackRequiredSlots = item.count <= spaceLeftOnSlot ? 0 : (int)Math.Ceiling((decimal)(item.count - spaceLeftOnSlot) / (decimal)itemData.MaximumStacksize);
                        }

                        requiredSlots += itemData.MaximumStacksize > 1 ? stackRequiredSlots : 1;
                    }

                    if (dbPlayer.Container.GetInventoryUsedSpace() + weight > dbPlayer.Container.MaxWeight)
                    {
                        dbPlayer.SendNewNotification("So viel kannst du nicht tragen");
                        return;
                    }

                    if (price <= 0)
                    {
                        dbPlayer.SendNewNotification("You should not read this.");
                        return;
                    }

                    if (dbPlayer.Container.GetUsedSlots() + requiredSlots > dbPlayer.Container.MaxSlots)
                    {
                        dbPlayer.SendNewNotification("Dein Inventar hat zu wenige Slots!");
                        return;
                    }

                    if (accessedShop.SchwarzgeldUse)
                    {
                        if (!dbPlayer.TakeBlackMoney(price))
                        {
                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughSWMoney(price));
                            return;
                        }
                    }
                    else if (accessedShop.CWSId > 0)
                    {
                        if (!dbPlayer.TakeCWS((CWSTypes)accessedShop.CWSId, price))
                        {
                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughCW(price, (CWSTypes)accessedShop.CWSId));
                            return;
                        }
                    }
                    else
                    {
                        Payment.PaymentStatus pStatus = await Payment.PaymentModule.Instance.AskForPayment(dbPlayer, price);

                        if (pStatus == Payment.PaymentStatus.Wallet)
                        {
                            if (!dbPlayer.TakeMoney(price))
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                                return;
                            }
                        }
                        else if (pStatus == Payment.PaymentStatus.Bank)
                        {
                            string storename = accessedShop.Name;
                            if (nightClub != null && nightClub.IsOwnedByBusines())
                            {
                                storename = nightClub.Name;
                            }

                            if (!dbPlayer.TakeBankMoney(price, "Ihre Zahlung im " + storename + " ($" + price + ")"))
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                                return;
                            }
                        }
                        else return;

                        /*
                         * if (accessedShop.ShopCanOwned && accessedShop.OwnerTeam != 0)
                         * {
                         *    TeamShelterModule.Instance.Get(accessedShop.OwnerTeam).GiveMoney(Convert.ToInt32(price * 0.10));
                         * }
                         */
                    }

                    // Nightclub Module Give Money To NightClub
                    if (nightClub != null && nightClub.IsOwnedByBusines())
                    {
                        nightClub.GetOwnedBusiness().GiveMoney(price);
                    }

                    // Add Items & Remove if Nightclub
                    foreach (ShopBuyEvent.BasketItem item in shopBuyEvent.basket)
                    {
                        if (item == null) continue;
                        ItemModel itemData = ItemModelModule.Instance.Get((uint)item.itemId);

                        // Remove From NightClub
                        if (nightClub != null) nightClub.Container.RemoveItem(itemData.Id, item.count);
                        else
                        {
                            ShopItem shopItem = accessedShop.ShopItems.Where(x => x.ItemId == item.itemId).FirstOrDefault();
                            if (shopItem == null) return; // BUG?!
                            if (shopItem.IsStoredItem)
                            {
                                shopItem.Stored -= item.count;
                                shopItem.SaveStoreds();
                            }
                            Logger.AddShopBuyLog(dbPlayer.Id, item.itemId, item.count, shopItem.Price * item.count);
                        }

                        if (!dbPlayer.Container.AddItem(itemData.Id, item.count))
                        {
                            dbPlayer.SendNewNotification("Deine Taschen sind nicht groß genug!");
                        }

                        dbPlayer.SyncAttachmentOnlyItems();

                        dbPlayer.SendNewNotification($"{item.count} {itemData.Name} gekauft!");

                        if (dbPlayer.HasMoneyTransferWantedStatus() && !dbPlayer.IsMasked())
                        {
                            TeamModule.Instance.SendMessageToTeam($"Finanz-Detection: Die Gesuchte Person {dbPlayer.GetName()} hat einen Einkauf getätigt! (Standort: {accessedShop.Name})", TeamTypes.TEAM_FIB, 10000, 3);
                            NSAPlayerExtension.AddTransferHistory($"{dbPlayer.GetName()} Shop {accessedShop.Name}", accessedShop.Position);
                        }

                        var shopTakeoverModel = ShopTakeoverModule.Instance.GetAll().Values.FirstOrDefault(shopTakeover => shopTakeover.Shop.Id == accessedShop.Id);
                        if (shopTakeoverModel == null) return;

                        // TODO: Change money amount for ShopTakeover
                        shopTakeoverModel.AddMoney((int)(price * 0.75f));
                    }
                }
                catch { }
            }));
        }
    }
}
