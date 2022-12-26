using GTANetworkAPI;
using System.Data;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teamfight;
using VMP_CNR.Module.Teamfight.Windows;

namespace VMP_CNR.Module.Teams.Shelter
{
    public class PistoleCreateMenuBuilder : MenuBuilder
    {
        public static uint PistolSetItemId = 1078;
        public static uint MuniPackItemId = 1;

        public PistoleCreateMenuBuilder() : base(PlayerMenu.PistoleCreateMenu)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.NativeMenu(Menu, "Pistolen Herstellung", "Pistolen Herstellung");

            menu.Add($"Schließen");
            menu.Add($"Heavypistole ($5500)");
            menu.Add($"Pistole ($4000)");
            menu.Add($"Pistole 50 ($7000)");
            //menu.Add($"----Munition----");
            //menu.Add($"Heavypistol Munition");
            //menu.Add($"Pistole Munition");
            //menu.Add($"Pistole 50 Munition");


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
                else if (index == 1)
                {
                    uint weaponId = 1291;
                    int price = 5500;

                    if (dbPlayer.Container.GetItemAmount(PistolSetItemId) < 1) return false;

                    if (!dbPlayer.Container.CanInventoryItemAdded(weaponId))
                    {
                        dbPlayer.SendNewNotification($"Nicht genug platz im Rucksack!");
                        return false;
                    }

                    if (!dbPlayer.TakeBlackMoney(price))
                    {
                        dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughSWMoney(price));
                        return false;
                    }

                    dbPlayer.Container.RemoveItem(PistolSetItemId);

                    dbPlayer.Container.AddItem(weaponId, 1);

                    dbPlayer.SendNewNotification($"Du hast eine Heavypistol für ${price} Schwarzgeld hergestellt!");
                    return true;
                }
                else if (index == 2)
                {
                    uint weaponId = 1292;
                    int price = 4000;

                    if (dbPlayer.Container.GetItemAmount(PistolSetItemId) < 1) return false;

                    if (!dbPlayer.Container.CanInventoryItemAdded(weaponId))
                    {
                        dbPlayer.SendNewNotification($"Nicht genug platz im Rucksack!");
                        return false;
                    }

                    if (!dbPlayer.TakeBlackMoney(price))
                    {
                        dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughSWMoney(price));
                        return false;
                    }

                    dbPlayer.Container.RemoveItem(PistolSetItemId);

                    dbPlayer.Container.AddItem(weaponId, 1);

                    dbPlayer.SendNewNotification($"Du hast eine Pistol für ${price} Schwarzgeld hergestellt!");
                    return true;
                }
                else if (index == 3)
                {
                    uint weaponId = 1316;
                    int price = 7000;

                    if (dbPlayer.Container.GetItemAmount(PistolSetItemId) < 1) return false;

                    if (!dbPlayer.Container.CanInventoryItemAdded(weaponId))
                    {
                        dbPlayer.SendNewNotification($"Nicht genug platz im Rucksack!");
                        return false;
                    }

                    if (!dbPlayer.TakeBlackMoney(price))
                    {
                        dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughSWMoney(price));
                        return false;
                    }

                    dbPlayer.Container.RemoveItem(PistolSetItemId);

                    dbPlayer.Container.AddItem(weaponId, 1);

                    dbPlayer.SendNewNotification($"Du hast eine Pistol für ${price} Schwarzgeld hergestellt!");
                    return true;
                }

                return false;
            }
        }
    }
}
