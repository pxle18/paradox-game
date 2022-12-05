using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Events;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Shops.Windows;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR.Module.Shops
{
    public sealed class ShopsModule : Module<ShopsModule>
    {
        public static int TimeToGetShopOwned = 15*12; // 15 min
        public static int RewardShopOwning = 50000;

        public override Type[] RequiredModules()
        {
            return new[] { typeof(TeamModule)};
        }

        public Shop GetThisShop(Vector3 pos, float range = 2.5f)
        {
            return (from kvp in ShopModule.Instance.GetAll() where kvp.Value.Position.DistanceTo(pos) <= range select kvp.Value)
                .FirstOrDefault();
        }

        public Shop GetRobableShopAtPos(Vector3 pos, float range = 2.5f)
        {
            return (from kvp in ShopModule.Instance.GetAll() where kvp.Value.RobPosition.X != 0 && kvp.Value.RobPosition.DistanceTo(pos) <= range select kvp.Value)
                .FirstOrDefault();
        }

        public Shop GetDeliveryShop(Vector3 pos, float range = 2.5f)
        {
            return (from kvp in ShopModule.Instance.GetAll() where kvp.Value.DeliveryPosition.DistanceTo(pos) <= range select kvp.Value)
                .FirstOrDefault();
        }

        public void ResetAllRobStatus()
        {
            foreach (var kvp in ShopModule.Instance.GetAll())
            {
                kvp.Value.Robbed = false;
            }
        }

        public Shop GetShop(int id)
        {
            return ShopModule.Instance.GetAll().ContainsKey((uint)id) ? ShopModule.Instance.GetAll()[(uint)id] : null;
        }

        public void SetShopRobbed(int id, bool robbed = true)
        {
            var shop = GetShop(id);
            if (shop == null) return;
            shop.Robbed = robbed;
        }

        public bool CanShopOwned()
        {
            if (Configurations.Configuration.Instance.DevMode) return true;

            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 30)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 20)
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }

        public override void OnFiveSecUpdate()
        {
            if (!CanShopOwned()) return;

            foreach(Shop shop in ShopModule.Instance.GetAll().Values.Where(s => s.ShopCanOwned))
            {
                if (shop == null) continue;

                try
                {
                    shop.ShoppingRangePlayers.RemoveAll(s => s == null || !s.IsValid());

                }
                catch(Exception e)
                {
                    Logging.Logger.Crash(e);
                }

                Dictionary<uint, uint> actingPlayersGrouped = new Dictionary<uint, uint>();

                foreach(DbPlayer dbPlayer in shop.ShoppingRangePlayers.ToList())
                {
                    if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsAGangster() || dbPlayer.RageExtension.IsInVehicle) continue;

                    if (dbPlayer.TeamId == shop.OwnerTeam) continue;

                    var currWeapon = dbPlayer.Player.CurrentWeapon;
                    if (currWeapon != 0)
                    {
                        var l_WeaponDatas = WeaponDataModule.Instance.GetAll();

                        var l_Weapon = l_WeaponDatas.Values.FirstOrDefault(data => data.Hash == (int)currWeapon);
                        if (l_Weapon == null) continue;

                        if(l_Weapon.Weight > 4)
                        {
                            // Add zwecks einschüchtern
                            if (actingPlayersGrouped.ContainsKey(dbPlayer.TeamId))
                            {
                                actingPlayersGrouped[dbPlayer.TeamId] += 1;
                            }
                            else
                            {
                                actingPlayersGrouped.Add(dbPlayer.TeamId, 1);
                            }
                        }
                    }
                }

                int minFraklerToOwn = 5;
                if(Configurations.Configuration.Instance.DevMode)
                {
                    minFraklerToOwn = 3;
                }

                if (actingPlayersGrouped.Count() == 1 && actingPlayersGrouped.First().Value >= minFraklerToOwn) // weil wenn nur eine Fraktion u know
                {
                    if (shop.LastOwned.AddHours(1) > DateTime.Now)
                    {
                        foreach(DbPlayer dbPlayer in shop.ShoppingRangePlayers.ToList())
                        {
                            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.TeamId != actingPlayersGrouped.First().Key) continue;
                            dbPlayer.SendNewNotification($"Sie können diesen Shop noch nicht einschüchtern! (Wurde vor kurzem erst)");

                        }
                        continue;
                    }
                    if(ShopModule.Instance.GetAll().Values.Where(s => s.ShopCanOwned && s.OwnerTeam == actingPlayersGrouped.First().Key).Count() >= 3)
                    {
                        foreach (DbPlayer dbPlayer in shop.ShoppingRangePlayers.ToList())
                        {
                            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.TeamId != actingPlayersGrouped.First().Key) continue;
                            dbPlayer.SendNewNotification($"Ihre Fraktion hat bereits 3 Shops eingeschüchtert!");

                        }
                        continue;
                    }
                    shop.ActiveOwningState(actingPlayersGrouped.First().Key);
                }
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if(colShape.HasData("shopRobbingShape"))
            {
                uint shopId = colShape.GetData<uint>("shopRobbingShape");

                Shop shop = ShopModule.Instance.GetAll().Values.Where(s => s.Id == shopId).FirstOrDefault();

                if(shop != null)
                {
                    if(colShapeState == ColShapeState.Enter)
                    {
                        if(!shop.ShoppingRangePlayers.ToList().Contains(dbPlayer))
                        {
                            shop.ShoppingRangePlayers.Add(dbPlayer);
                        }
                        return true;
                    }
                    else
                    {
                        if (shop.ShoppingRangePlayers.ToList().Contains(dbPlayer))
                        {
                            shop.ShoppingRangePlayers.Remove(dbPlayer);
                        }
                        return true;
                    }
                }
            }

            return false;
        }
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key == Key.E)
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) return false;


                Shop shop = Instance.GetThisShop(dbPlayer.Player.Position);
                if (shop != null) {

                    if (shop.Teams.Count > 0 && !shop.Teams.Contains(dbPlayer.Team)) return false;
                    if (dbPlayer.RageExtension.IsInVehicle && shop.DriveIn != 1) return false;

                    if (shop.EventId > 0)
                    {
                        if (!EventModule.Instance.IsEventActive(shop.EventId))
                            return false;
                    }

                    if (shop.CWSId > 0)
                    {
                        CWS cws = CWSModule.Instance.Get(shop.CWSId);

                        if (cws != null)
                        {
                            dbPlayer.SendNewNotification($"An diesem Geschäft zahlen Sie mit {cws.Name}-Punkten!");
                        }
                    }

                    List<ShopItemX> Items = new List<ShopItemX>();
                    foreach (var item in shop.ShopItems)
                    {
                        Items.Add(new ShopItemX(item.ItemId, item.Name, item.Price, ItemModelModule.Instance.Get(item.ItemId).ImagePath)); // TODO
                    }
                    ComponentManager.Get<ShopWindow>().Show()(dbPlayer, shop.Name, (int)shop.Id, Items);
                    return true;
                }
                // try set shop for delivery
                shop = Instance.GetDeliveryShop(dbPlayer.Player.Position);
                if(shop != null)
                {
                    if (shop.Teams.Count > 0 && !shop.Teams.Contains(dbPlayer.Team)) return false;

                    foreach (ShopItem shopItem in shop.ShopItems.Where(si => si.IsStoredItem))
                    {
                        int neededAmount = shopItem.GetRequiredAmount();
                        int neededChestAmount = neededAmount / 5;
                        if(neededChestAmount > 0) // Shop braucht items...
                        {
                            int playerHasItemAmount = dbPlayer.Container.GetItemAmount((uint)shopItem.RequiredChestItemId);
                            if (playerHasItemAmount > 0)
                            {
                                int resultprice = playerHasItemAmount * 5 * shopItem.EKPrice;
                                dbPlayer.GiveMoney(resultprice);
                                dbPlayer.Container.RemoveItem((uint)shopItem.RequiredChestItemId, playerHasItemAmount);

                                dbPlayer.SendNewNotification($"{playerHasItemAmount} {ItemModelModule.Instance.Get((uint)shopItem.RequiredChestItemId).Name} für ${resultprice} verkauft!");

                                shopItem.Stored += playerHasItemAmount * 5;
                                shopItem.SaveStoreds();
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}