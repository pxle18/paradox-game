using System;
using System.Collections.Generic;
using VMP_CNR.Module.Bunker;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Dealer;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Bunker.Menu
{
    public class BunkerDealerSellMenu : MenuBuilder
    {
        public BunkerDealerSellMenu() : base(PlayerMenu.BunkerDealerSellMenu)
        {
        }

        public override Module.Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
           
            var l_Menu = new Module.Menu.NativeMenu(Menu, "BunkerExport", "");
            l_Menu.Add("Schließen", "");
            l_Menu.Add($"V | Kiste Meth (~ {BunkerModule.PriceMethChest} $)", "");
            l_Menu.Add($"V | Kiste Waffenset (~ {BunkerModule.PriceWeaponChest} $)", "");
            l_Menu.Add($"V | Paket Cannabis (~ {BunkerModule.PriceCannabisChest} $)", "");
            l_Menu.Add($"V | Goldbarren ({BunkerModule.PriceGoldBars} $)", "");
            l_Menu.Add($"V | Juwelen ({BunkerModule.PriceDiamonds} $)", "");

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
                switch (index)
                {
                    case 0:
                        break;
                    case 1: // Kiste Meth
                        int l_PricePerPureMeth = BunkerModule.PriceMethChest/50;
                        Item item = dbPlayer.Container.GetItemById(726);
                        if (item == null)
                            item = dbPlayer.Container.GetItemById(727);
                        if (item == null)
                            item = dbPlayer.Container.GetItemById(728);
                        if (item == null)
                            item = dbPlayer.Container.GetItemById(729);

                        if (item == null)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Kiste mit Methamphetamin dabei!");
                            break;
                        }

                        uint amount = 0;
                        double quality = 0;
                        foreach (KeyValuePair<string, dynamic> keyValuePair in item.Data)
                        {
                            if (keyValuePair.Key == "amount")
                            {
                                string amount_str = Convert.ToString(keyValuePair.Value);
                                amount = Convert.ToUInt32(amount_str);
                            }
                            else if (keyValuePair.Key == "quality")
                            {
                                string quality_str = Convert.ToString(keyValuePair.Value);
                                quality = Convert.ToDouble(quality_str);
                            }

                        }

                        if (amount < 50 || quality < 0.90)
                        {
                            dbPlayer.SendNewNotification("Mit dieser Kiste scheint etwas nicht korrekt zu sein.");
                            break;
                        }

                        double offset = (quality - 0.90) * 100;
                        l_PricePerPureMeth += (int)offset;

                        int l_PureMethPrice = Convert.ToInt32(amount * l_PricePerPureMeth);
                        int l_PureMethFBank = Convert.ToInt32(l_PureMethPrice * 0.05f);
                        int l_PlayerPureMethPrice = Convert.ToInt32(l_PureMethPrice * 0.95f);

                        dbPlayer.Container.RemoveItem(item.Model);

                        dbPlayer.GiveBlackMoney(l_PlayerPureMethPrice);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_PureMethFBank);
                        dbPlayer.SendNewNotification($"Du hast {amount.ToString()} reines Meth für {l_PureMethPrice.ToString()}$ verkauft." + $"Es gingen 5% an die Fraktion. ({l_PureMethFBank.ToString()}$)");
                        Logger.AddGangwarSellToDB(dbPlayer.Id, item.Id, (int)amount, l_PlayerPureMethPrice);

                        break;
                    case 2: // Waffenset
                        int priceWeaponSet = BunkerModule.PriceWeaponChest;

                        item = dbPlayer.Container.GetItemById(976);
                        if (item == null)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Waffensets dabei!");
                            break;
                        }


                        int fBankRevard = Convert.ToInt32(priceWeaponSet * 0.05);

                        dbPlayer.Container.RemoveItem(item.Model);

                        dbPlayer.GiveBlackMoney((int)priceWeaponSet);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(fBankRevard);
                        dbPlayer.SendNewNotification($"Du hast ein Waffenset für {priceWeaponSet}$ verkauft." + $"Es gingen 5% an die Fraktion. ({fBankRevard}$)");
                        Logger.AddGangwarSellToDB(dbPlayer.Id, item.Id, 1, (int)priceWeaponSet);
                        break;
                    case 3: // Paket Cannabis
                        l_PricePerPureMeth = BunkerModule.PriceCannabisChest/50;
                        Item xitem = dbPlayer.Container.GetItemById(983);
                        if (xitem == null)
                            xitem = dbPlayer.Container.GetItemById(982);
                        if (xitem == null)
                            xitem = dbPlayer.Container.GetItemById(981);
                        if (xitem == null)
                            xitem = dbPlayer.Container.GetItemById(980);

                        if (xitem == null)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Paket mit Cannabis dabei!");
                            break;
                        }

                        amount = 0;
                        quality = 0;

                        foreach (KeyValuePair<string, dynamic> keyValuePair in xitem.Data)
                        {
                            if (keyValuePair.Key == "amount")
                            {
                                string amount_str = Convert.ToString(keyValuePair.Value);
                                amount = Convert.ToUInt32(amount_str);
                            }
                            else if (keyValuePair.Key == "quality")
                            {
                                string quality_str = Convert.ToString(keyValuePair.Value);
                                quality = Convert.ToDouble(quality_str);
                            }

                        }

                        if (amount < 50 || quality < 0.90)
                        {
                            dbPlayer.SendNewNotification("Mit diesem Paket scheint etwas nicht korrekt zu sein.");
                            break;
                        }

                        offset = (quality - 0.90) * 100;
                        l_PricePerPureMeth += (int)offset;

                        l_PureMethPrice = Convert.ToInt32(amount * l_PricePerPureMeth);
                        l_PureMethFBank = Convert.ToInt32(l_PureMethPrice * 0.05f);
                        l_PlayerPureMethPrice = Convert.ToInt32(l_PureMethPrice * 0.95f);

                        dbPlayer.Container.RemoveItem(xitem.Model);

                        dbPlayer.GiveBlackMoney(l_PlayerPureMethPrice);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_PureMethFBank);
                        dbPlayer.SendNewNotification($"Du hast {amount.ToString()} Cannabis für {l_PureMethPrice.ToString()}$ verkauft." + $"Es gingen 5% an die Fraktion. ({l_PureMethFBank.ToString()}$)");
                        Logger.AddGangwarSellToDB(dbPlayer.Id, xitem.Id, (int)amount, l_PlayerPureMethPrice);

                        break;
                    case 4: //Gold
                        int l_PricePerGold = BunkerModule.PriceGoldBars;
                        uint l_GoldAmount = (uint)dbPlayer.Container.GetItemAmount(DealerModule.Instance.GoldBarrenItemId);

                        if (l_GoldAmount <= 0)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Goldbarren dabei, welche du mir verkaufen könntest!");
                            break;
                        }

                        int l_GoldPrice = Convert.ToInt32(l_GoldAmount * l_PricePerGold);
                        int l_GoldFBank = Convert.ToInt32(l_GoldPrice * 0.05f);
                        int l_PlayerGoldPrice = Convert.ToInt32(l_GoldPrice * 0.95f);

                        dbPlayer.Container.RemoveItem(DealerModule.Instance.GoldBarrenItemId, (int)l_GoldAmount);

                        dbPlayer.GiveBlackMoney(l_PlayerGoldPrice);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_GoldFBank);
                        dbPlayer.SendNewNotification($"Du hast {l_GoldAmount.ToString()} Goldbarren für {l_GoldPrice.ToString()}$ verkauft. Es gingen 5% an die Fraktion. ({l_GoldFBank.ToString()}$)");
                        Logger.AddGangwarSellToDB(dbPlayer.Id, DealerModule.Instance.GoldBarrenItemId, (int)l_GoldAmount, l_PlayerGoldPrice);
                        break;
                    case 5: // Juwelen
                        int l_PricePerDiamond = BunkerModule.PriceDiamonds;
                        uint l_DiamondAmount = (uint)dbPlayer.Container.GetItemAmount(DealerModule.Instance.DiamondItemId);

                        if (l_DiamondAmount <= 0)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Diamanten dabei, welche du mir verkaufen könntest!");
                            break;
                        }

                        int l_DiamondPrice = Convert.ToInt32(l_DiamondAmount * l_PricePerDiamond);
                        int l_DiamondFBank = Convert.ToInt32(l_DiamondPrice * 0.05f);
                        int l_PlayerDiamondPrice = Convert.ToInt32(l_DiamondPrice * 0.95f);

                        dbPlayer.Container.RemoveItem(DealerModule.Instance.DiamondItemId, (int)l_DiamondAmount);

                        dbPlayer.GiveBlackMoney(l_PlayerDiamondPrice);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_DiamondFBank);
                        dbPlayer.SendNewNotification($"Du hast {l_DiamondAmount.ToString()} Juwelen für {l_DiamondPrice.ToString()}$ verkauft. Es gingen 5% an die Fraktion. ({l_DiamondFBank.ToString()}$)");
                        Logger.AddGangwarSellToDB(dbPlayer.Id, DealerModule.Instance.DiamondItemId, (int)l_DiamondAmount, l_PlayerDiamondPrice);
                        break;
                    default:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                }
                return false;
            }
        }
    }
}
