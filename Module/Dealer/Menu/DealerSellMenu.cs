using System;
using System.Collections.Generic;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Dealer.Menu
{
    public class DealerSellMenu : MenuBuilder
    {
        public const int MaxGangwarTownBonus = 4;

        public DealerSellMenu() : base(PlayerMenu.DealerSellMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasData("current_dealer")) return null;

            Dealer l_Dealer = DealerModule.Instance.Get(dbPlayer.GetData("current_dealer"));
            if (l_Dealer == null)
                return null;

            var l_Menu = new Module.Menu.Menu(Menu, "Dealer", l_Dealer.Note);
            l_Menu.Add("Schließen", "");
            if(dbPlayer.Team.IsHeroinTeam())
            {
                l_Menu.Add($"V | Heroinampulle ({l_Dealer.HeroinampullenResource.Price.ToString()} $)", "");
                l_Menu.Add($"V | Kiste veredeltes Heroin (~ {l_Dealer.HeroinKisteResource.Price.ToString()} $)", "");
            }
            l_Menu.Add($"V | Waffenset (~ {(l_Dealer.WeaponResource.Price * 50).ToString()} $)", "");
            l_Menu.Add($"V | Goldbarren ({l_Dealer.GoldResource.Price.ToString()} $)", "");
            l_Menu.Add($"V | Juwelen ({l_Dealer.DiamondResource.Price.ToString()} $)", "");
            l_Menu.Add($"V | Kokain ({l_Dealer.CocainResource.Price.ToString()} $ pro Paket)", "");
            l_Menu.Add($"K | Wegfahrsperre ({l_Dealer.VehicleClawPrice.ToString()} $)", "");

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
                if (!dbPlayer.HasData("current_dealer")) return false;

                Dealer l_Dealer = DealerModule.Instance.Get(dbPlayer.GetData("current_dealer"));
                if (l_Dealer == null) return false;

                bool soldSmth = false;

                switch (index)
                {
                    case 0:
                        break;
                    case 1: // Heroinampulle // Waffenset
                        if(dbPlayer.Team.IsHeroinTeam())
                        {
                            soldSmth = SellHeroinAmpulle(dbPlayer, l_Dealer);
                        }
                        else
                        {
                            soldSmth = SellWaffenset(dbPlayer, l_Dealer);
                        }

                        break;
                    case 2: // Kiste Heroin // Gold
                        if(dbPlayer.Team.IsHeroinTeam())
                        {
                            soldSmth = SellHeroinAmpullenKiste(dbPlayer, l_Dealer);
                        }
                        else
                        {
                            soldSmth = SellGold(dbPlayer, l_Dealer);
                        }

                        break;
                    case 3: // Waffenset // Juwelen
                        if(dbPlayer.Team.IsHeroinTeam())
                        {
                            soldSmth = SellWaffenset(dbPlayer, l_Dealer);
                        }
                        else
                        {
                            soldSmth = SellJuwelen(dbPlayer, l_Dealer);
                        }

                        break;
                    case 4: //Gold // Kokain
                        if(dbPlayer.Team.IsHeroinTeam())
                        {
                            soldSmth = SellGold(dbPlayer, l_Dealer);
                        }
                        else
                        {
                            soldSmth = SellKokain(dbPlayer, l_Dealer);
                        }

                        break;
                    case 5: // Juwelen // Wegfahrsperre
                        if(dbPlayer.Team.IsHeroinTeam())
                        {
                            soldSmth = SellJuwelen(dbPlayer, l_Dealer);
                        }
                        else
                        {
                            BuyWegfahrsperre(dbPlayer, l_Dealer);
                        }

                        break;
                    case 6: // Paket Kokain // Nicht vorhanden
                        if(dbPlayer.Team.IsHeroinTeam())
                        {
                            soldSmth = SellKokain(dbPlayer, l_Dealer);
                        }

                        break;
                    case 7: //Wegfahrsperre // Nicht vorhanden
                        if(dbPlayer.Team.IsHeroinTeam())
                        {
                            BuyWegfahrsperre(dbPlayer, l_Dealer);
                        }

                        break;
                }

                if (soldSmth)
                {
                    double rnd = Utils.RandomDoubleNumber(0, 100);
                    if (rnd <= DealerModule.Instance.MaulwurfAlarmChance || Configuration.Instance.DevMode)
                    {
                        if (!l_Dealer.Alert)
                            TeamModule.Instance.SendMessageToTeam("Ein neuer Tipp von einem Maulwurf ist eingegangen... (/finddealer)", teams.TEAM_FIB, 10000);
                        string messageToDB = $"FindDealer: Neuer Tipp - Id - {l_Dealer.Id}, Heroinpreis - {l_Dealer.HeroinampullenResource.Price}";
                        MySQLHandler.ExecuteAsync($"INSERT INTO `log_bucket` (`player_id`, `message`) VALUES ('{dbPlayer.Id}', '{messageToDB}');");
                        l_Dealer.Alert = true;
                        l_Dealer.LastSeller = dbPlayer;
                    }
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }

            public bool SellHeroinAmpulle(DbPlayer dbPlayer, Dealer l_Dealer)
            {
                bool soldSmth = false;

                uint l_PricePerHeroin = l_Dealer.HeroinampullenResource.Price;
                uint l_HeroinAmount = (uint)dbPlayer.Container.GetItemAmount(DealerModule.Instance.HeroinAmpulleItemId);

                if (l_HeroinAmount <= 0)
                {
                    dbPlayer.SendNewNotification("Du hast kein Heroin dabei, welches du mir verkaufen könntest!");
                    return false;
                }

                int l_HeroinPrice = Convert.ToInt32(l_HeroinAmount * l_PricePerHeroin);
                int l_HeroinFBank = Convert.ToInt32(l_HeroinPrice * 0.05f);
                int l_PlayerHeroinPrice = Convert.ToInt32(l_HeroinPrice * 0.95f);

                if ((l_Dealer.DealerSoldAmount + (l_HeroinPrice)) > DealerModule.Dealer5MinSellCap)
                {
                    dbPlayer.SendNewNotification($"Ich kann aktuell nicht so viel ankaufen...!");
                    return false;
                }

                dbPlayer.Container.RemoveItem(DealerModule.Instance.HeroinAmpulleItemId, (int)l_HeroinPrice);

                dbPlayer.GiveBlackMoney(l_PlayerHeroinPrice);
                TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_HeroinFBank);
                dbPlayer.SendNewNotification($"Du hast {l_HeroinAmount.ToString()} Heroinampullen für {l_HeroinPrice.ToString()}$ verkauft." + $"Es gingen 5% an die Fraktion. ({l_HeroinFBank.ToString()}$)");
                l_Dealer.HeroinampullenResource.Sold += l_HeroinAmount;
                soldSmth = true;
                Logger.AddGangwarSellToDB(dbPlayer.Id, DealerModule.Instance.HeroinAmpulleItemId, (int)l_HeroinAmount, l_HeroinPrice);

                l_Dealer.DealerSoldAmount += Convert.ToInt32(l_HeroinPrice);

                if (l_Dealer.HeroinampullenResource.IsFull())
                    l_Dealer.HeroinampullenResource.TimeSinceFull = DateTime.Now;

                return soldSmth;
            }

            public bool SellHeroinAmpullenKiste(DbPlayer dbPlayer, Dealer l_Dealer)
            {
                bool soldSmth = false;

                uint l_PricePerKiste = l_Dealer.HeroinKisteResource.Price;

                Item item = dbPlayer.Container.GetItemById(1443);

                if (item == null)
                {
                    dbPlayer.SendNewNotification("Du hast keine Kiste mit Heroinampullen dabei!");
                    return false;
                }

                int l_KisteFBank = Convert.ToInt32(l_PricePerKiste * 0.05f);
                int l_PlayerKistePrice = Convert.ToInt32(l_PricePerKiste * 0.95f);


                if ((l_Dealer.DealerSoldAmount + (l_PricePerKiste)) > DealerModule.Dealer5MinSellCap)
                {
                    dbPlayer.SendNewNotification($"Ich kann aktuell nicht so viel ankaufen...!");
                    return false;
                }

                l_Dealer.DealerSoldAmount += Convert.ToInt32(l_PricePerKiste);

                dbPlayer.Container.RemoveItem(item.Model);


                dbPlayer.GiveBlackMoney(l_PlayerKistePrice);
                TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_KisteFBank);
                dbPlayer.SendNewNotification($"Du hast eine Kiste mit veredelten Heroinampullen für {l_PricePerKiste.ToString()}$ verkauft." + $"Es gingen 5% an die Fraktion. ({l_KisteFBank.ToString()}$)");
                l_Dealer.HeroinKisteResource.Sold += 50;
                soldSmth = true;
                Logger.AddGangwarSellToDB(dbPlayer.Id, item.Id, (int)1, l_PlayerKistePrice);

                if (l_Dealer.HeroinKisteResource.IsFull())
                    l_Dealer.HeroinKisteResource.TimeSinceFull = DateTime.Now;

                return soldSmth;
            }

            public bool SellWaffenset(DbPlayer dbPlayer, Dealer l_Dealer)
            {
                bool soldSmth = false;

                uint priceWeaponSet = l_Dealer.WeaponResource.Price * 50;

                var item = dbPlayer.Container.GetItemById(976);
                if (item == null)
                {
                    dbPlayer.SendNewNotification("Du hast keine Waffensets dabei!");
                    return false;
                }


                int fBankRevard = Convert.ToInt32(priceWeaponSet * 0.05);
                int playerReward = Convert.ToInt32(priceWeaponSet - fBankRevard);


                if ((l_Dealer.DealerSoldAmount + (priceWeaponSet)) > DealerModule.Dealer5MinSellCap)
                {
                    dbPlayer.SendNewNotification($"Ich kann aktuell nicht so viel ankaufen...!");
                    return false;
                }

                l_Dealer.DealerSoldAmount += Convert.ToInt32(playerReward);

                dbPlayer.Container.RemoveItem(item.Model);

                dbPlayer.GiveBlackMoney((int)playerReward);
                TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(fBankRevard);
                dbPlayer.SendNewNotification($"Du hast ein Waffenset für {playerReward}$ verkauft." + $"Es gingen 5% an die Fraktion. ({fBankRevard}$)");
                l_Dealer.WeaponResource.Sold++;
                soldSmth = true;
                Logger.AddGangwarSellToDB(dbPlayer.Id, item.Id, 1, (int)priceWeaponSet);

                if (l_Dealer.WeaponResource.IsFull())
                    l_Dealer.WeaponResource.TimeSinceFull = DateTime.Now;

                return soldSmth;
            }

            public bool SellGold(DbPlayer dbPlayer, Dealer l_Dealer)
            {
                bool soldSmth = false;

                uint l_PricePerGold = l_Dealer.GoldResource.Price;
                uint l_GoldAmount = (uint)dbPlayer.Container.GetItemAmount(DealerModule.Instance.GoldBarrenItemId);

                if (l_GoldAmount <= 0)
                {
                    dbPlayer.SendNewNotification("Du hast keine Goldbarren dabei, welche du mir verkaufen könntest!");
                    return false;
                }

                if (l_Dealer.GoldResource.IsFull())
                {
                    dbPlayer.SendNewNotification($"Ich nimm erstmal nix an von dem Zeug. Komm später wieder.");
                    return false;
                }

                if (l_Dealer.GoldResource.GetAvailableAmountToSell() < l_GoldAmount)
                {
                    dbPlayer.SendNewNotification($"Alter... Das is zu viel. Ich kann nur noch {l_Dealer.GoldResource.GetAvailableAmountToSell().ToString()} annehmen...");
                    return false;
                }

                int l_GoldPrice = Convert.ToInt32(l_GoldAmount * l_PricePerGold);
                int l_GoldFBank = Convert.ToInt32(l_GoldPrice * 0.05f);
                int l_PlayerGoldPrice = Convert.ToInt32(l_GoldPrice * 0.95f);

                dbPlayer.Container.RemoveItem(DealerModule.Instance.GoldBarrenItemId, (int)l_GoldAmount);

                dbPlayer.GiveBlackMoney(l_PlayerGoldPrice);
                TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_GoldFBank);
                dbPlayer.SendNewNotification($"Du hast {l_GoldAmount.ToString()} Goldbarren für {l_GoldPrice.ToString()}$ verkauft. Es gingen 5% an die Fraktion. ({l_GoldFBank.ToString()}$)");
                l_Dealer.GoldResource.Sold += l_GoldAmount;
                soldSmth = true;
                Logger.AddGangwarSellToDB(dbPlayer.Id, DealerModule.Instance.GoldBarrenItemId, (int)l_GoldAmount, l_PlayerGoldPrice);

                if (l_Dealer.GoldResource.IsFull())
                    l_Dealer.GoldResource.TimeSinceFull = DateTime.Now;

                return soldSmth;
            }

            public bool SellJuwelen(DbPlayer dbPlayer, Dealer l_Dealer)
            {
                bool soldSmth = false;

                uint l_PricePerDiamond = l_Dealer.DiamondResource.Price;
                uint l_DiamondAmount = (uint)dbPlayer.Container.GetItemAmount(DealerModule.Instance.DiamondItemId);

                if (l_DiamondAmount <= 0)
                {
                    dbPlayer.SendNewNotification("Du hast keine Diamanten dabei, welche du mir verkaufen könntest!");
                    return false;
                }

                if (l_Dealer.DiamondResource.IsFull())
                {
                    dbPlayer.SendNewNotification($"Ich nimm erstmal nix an von dem Zeug. Komm später wieder.");
                    return false;
                }

                if (l_Dealer.DiamondResource.GetAvailableAmountToSell() < l_DiamondAmount)
                {
                    dbPlayer.SendNewNotification($"Alter... Das is zu viel. Ich kann nur noch {l_Dealer.DiamondResource.GetAvailableAmountToSell().ToString()} annehmen...");
                    return false;
                }

                int l_DiamondPrice = Convert.ToInt32(l_DiamondAmount * l_PricePerDiamond);
                int l_DiamondFBank = Convert.ToInt32(l_DiamondPrice * 0.05f);
                int l_PlayerDiamondPrice = Convert.ToInt32(l_DiamondPrice * 0.95f);

                dbPlayer.Container.RemoveItem(DealerModule.Instance.DiamondItemId, (int)l_DiamondAmount);

                dbPlayer.GiveBlackMoney(l_PlayerDiamondPrice);
                TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_DiamondFBank);
                dbPlayer.SendNewNotification($"Du hast {l_DiamondAmount.ToString()} Juwelen für {l_DiamondPrice.ToString()}$ verkauft. Es gingen 5% an die Fraktion. ({l_DiamondFBank.ToString()}$)");
                l_Dealer.DiamondResource.Sold += l_DiamondAmount;
                soldSmth = true;
                Logger.AddGangwarSellToDB(dbPlayer.Id, DealerModule.Instance.DiamondItemId, (int)l_DiamondAmount, l_PlayerDiamondPrice);

                if (l_Dealer.DiamondResource.IsFull())
                    l_Dealer.DiamondResource.TimeSinceFull = DateTime.Now;

                return soldSmth;
            }

            public bool SellKokain(DbPlayer dbPlayer, Dealer l_Dealer)
            {
                bool soldSmth = false;

                var l_PricePerPureMeth = l_Dealer.CocainResource.Price;
                Item xitem = dbPlayer.Container.GetItemById(557);

                if (xitem == null)
                {
                    dbPlayer.SendNewNotification("Du hast keine Paket mit Kokain dabei!");
                    return false;
                }

                int cocainAmount = dbPlayer.Container.GetItemAmount(557);

                if (cocainAmount <= 0)
                {
                    dbPlayer.SendNewNotification("Du hast keine Kokain dabei, welche du mir verkaufen könntest!");
                    return false;
                }

                var l_PureMethPrice = Convert.ToInt32(l_PricePerPureMeth * cocainAmount);
                var l_PureMethFBank = Convert.ToInt32(l_PureMethPrice * 0.05f);
                var l_PlayerPureMethPrice = Convert.ToInt32(l_PureMethPrice * 0.95f);

                dbPlayer.Container.RemoveItem(xitem.Model, cocainAmount);

                dbPlayer.GiveBlackMoney(l_PlayerPureMethPrice);
                TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_PureMethFBank);
                dbPlayer.SendNewNotification($"Du hast {cocainAmount} Kokainpakete für {l_PureMethPrice.ToString()}$ verkauft." + $"Es gingen 5% an die Fraktion. ({l_PureMethFBank.ToString()}$)");

                Logger.AddGangwarSellToDB(dbPlayer.Id, xitem.Id, (int)cocainAmount, l_PlayerPureMethPrice);

                return soldSmth;
            }

            public void BuyWegfahrsperre(DbPlayer dbPlayer, Dealer l_Dealer)
            {
                if (l_Dealer.VehicleClaw)
                {
                    if (l_Dealer.VehicleClawBought < DealerModule.Instance.MaxVehicleClawAmount)
                    {
                        if (dbPlayer.TakeMoney(100000))
                        {
                            dbPlayer.Container.AddItem(732, 1);
                            dbPlayer.SendNewNotification("Du hast eine Wegfahrsperre gekauft.. Lass dich nicht erwischen!");
                            l_Dealer.VehicleClawBought++;
                            return;
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei.");
                            return;
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Ich habe bereits alle verkauft...");
                        return;
                    }
                }
                else
                {
                    dbPlayer.SendNewNotification("Ich habe leider aktuell keine auf Vorrat...");
                }
            }
        }
    }
}
