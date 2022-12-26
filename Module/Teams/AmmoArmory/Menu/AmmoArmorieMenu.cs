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
using VMP_CNR.Module.Teams.AmmoPackageOrder;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Teams.AmmoArmory
{
    public class AmmoArmorieMenuBuilder : MenuBuilder
    {
        public AmmoArmorieMenuBuilder() : base(PlayerMenu.AmmoArmorieMenu)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            AmmoArmorie ammoArmorie = AmmoArmoryModule.Instance.GetByPosition(dbPlayer.Player.Position);
            if (ammoArmorie == null || !dbPlayer.Team.IsGangsters() || dbPlayer.Team.Id != ammoArmorie.TeamId) return null;

            var menu = new Menu.NativeMenu(Menu, "Munitionskammer (" + ammoArmorie.Packets + "P | " + ammoArmorie.Powder +" SP)");

            menu.Add($"Schließen");

            menu.Add("Pakete abgeben");
            menu.Add("Preise einstellen");

            foreach (AmmoArmorieItem ammoArmorieItem in ammoArmorie.ArmorieItems)
            {
                menu.Add(ItemModelModule.Instance.Get(ammoArmorieItem.ItemId).Name + " $" + ammoArmorieItem.TeamPrice, "(P:" + ammoArmorieItem.GetRequiredPacketsForTeam(dbPlayer.Team) + ")");
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
                else if (index == 1) // Packete abgeben
                {
                    AmmoArmorie ammoArmorie = AmmoArmoryModule.Instance.GetByPosition(dbPlayer.Player.Position);
                    if (ammoArmorie == null || !dbPlayer.Team.IsGangsters() || dbPlayer.Team.Id != ammoArmorie.TeamId) return false;

                    int hasChests = dbPlayer.Container.GetItemAmount(AmmoArmoryModule.AmmoChestItem);
                    int hasPowder = dbPlayer.Container.GetItemAmount(AmmoArmoryModule.BlackPowderItem);
                    if (hasChests > 0)
                    {
                        if(ammoArmorie.Packets+(hasChests * AmmoPackageOrderModule.AmmoChestToPackageMultipliert) >= AmmoArmoryModule.MaxLagerBestand)
                        {
                            dbPlayer.SendNewNotification("Maximale Anzahl erreicht! (" + AmmoArmoryModule.MaxLagerBestand + ")");
                            return false;
                        }
                        else
                        {
                            ammoArmorie.ChangePackets(hasChests * AmmoPackageOrderModule.AmmoChestToPackageMultipliert);
                            dbPlayer.Container.RemoveItem(AmmoArmoryModule.AmmoChestItem, hasChests);
                            dbPlayer.SendNewNotification($"Sie haben {hasChests} Kisten ({hasChests* AmmoPackageOrderModule.AmmoChestToPackageMultipliert} Pakete) eingelagert!");
                            return true;
                        }
                    }

                    if (hasPowder > 0)
                    {
                        if (ammoArmorie.Powder + (hasPowder * AmmoPackageOrderModule.BlackPowderToPackageMultiplier) >= AmmoArmoryModule.MaxLagerBestand)
                        {
                            dbPlayer.SendNewNotification("Maximale Anzahl erreicht! (" + AmmoArmoryModule.MaxLagerBestand + ")");
                            return false;
                        }
                        else
                        {
                            ammoArmorie.ChangePowder(hasPowder * AmmoPackageOrderModule.BlackPowderToPackageMultiplier);
                            dbPlayer.Container.RemoveItem(AmmoArmoryModule.BlackPowderItem, hasPowder);
                            dbPlayer.SendNewNotification($"Sie haben {hasPowder} Schwarzpulver ({hasPowder * AmmoPackageOrderModule.BlackPowderToPackageMultiplier}) eingelagert!");
                            return true;
                        }
                    }

                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 2) // Preis einstellen
                {
                    AmmoArmorie ammoArmorie = AmmoArmoryModule.Instance.GetByPosition(dbPlayer.Player.Position);
                    if (ammoArmorie == null || !dbPlayer.Team.IsGangsters() || dbPlayer.Team.Id != ammoArmorie.TeamId) return false;

                    if (dbPlayer.TeamRank < 11) return false;
                    
                    MenuManager.Instance.Build(PlayerMenu.AmmoArmoriePriceMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
                else // choose x.x
                {
                    AmmoArmorie ammoArmorie = AmmoArmoryModule.Instance.GetByPosition(dbPlayer.Player.Position);
                    if (ammoArmorie == null || !dbPlayer.Team.IsGangsters() || dbPlayer.Team.Id != ammoArmorie.TeamId) return false;

                    int idx = 3;
                    foreach (AmmoArmorieItem ammoArmorieItem in ammoArmorie.ArmorieItems)
                    {
                        if (idx == index)
                        {
                            int RequiredPackets = ammoArmorieItem.GetRequiredPacketsForTeam(dbPlayer.Team);

                            if(ammoArmorie.Packets < RequiredPackets)
                            {
                                dbPlayer.SendNewNotification("Nicht genug Waffenpakete!");
                                return false;
                            }

                            if (ammoArmorie.Powder < RequiredPackets)
                            {
                                dbPlayer.SendNewNotification("Nicht genug Schwarzpulver!");
                                return false;
                            }

                            ItemModel resultItem = ItemModelModule.Instance.Get(ammoArmorieItem.ItemId);
                            if (resultItem == null) return false;

                            if(dbPlayer.Container.CanInventoryItemAdded(resultItem))
                            {
                                if (ammoArmorieItem.TeamPrice > 0)
                                {
                                    if (!dbPlayer.TakeBankMoney(ammoArmorieItem.TeamPrice))
                                    {
                                        dbPlayer.SendNewNotification("Nicht genug Geld (Bank)!");
                                        return false;
                                    }

                                    TeamShelter teamShelter = TeamShelterModule.Instance.GetByTeam(dbPlayer.Team.Id);
                                    if (teamShelter == null) return false;

                                    teamShelter.GiveMoney(ammoArmorieItem.TeamPrice);
                                }

                                ammoArmorie.ChangePackets(-RequiredPackets);
                                ammoArmorie.ChangePowder(-RequiredPackets);
                                dbPlayer.Container.AddItem(resultItem);
                                dbPlayer.SendNewNotification($"Sie haben {resultItem.Name} für ${ammoArmorieItem.TeamPrice} (P: {RequiredPackets} | SP: {RequiredPackets}) entnommen!");
                                
                                return false;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Zu wenig Platz!");
                                return false;
                            }
                        }
                        else idx++;
                    }
                    return true;
                }
            }
        }
    }
}