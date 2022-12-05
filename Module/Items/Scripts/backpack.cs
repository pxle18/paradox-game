using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool backpack(DbPlayer dbPlayer, ItemModel ItemData, bool drop = false)
        {
            var selectedModelConfiguration = BackpackList.backpackList.Find(model => model.ItemModel == ItemData);
            if (selectedModelConfiguration == null) return false;

            // Resort for Managing all
            dbPlayer.Container.ResortInventory();

            // Aktuelle Slots
            var currentPlayerMaxSlots = dbPlayer.Container.MaxSlots;
            var currentPlayerMaxWeight= dbPlayer.Container.MaxWeight;
                        
            // overall Change backpack
            var currentUsedSlots = ContainerManager.GetUsedSlots(dbPlayer.Container);
            var currentUsedWeight = currentPlayerMaxWeight - ContainerManager.GetInventoryFreeSpace(dbPlayer.Container);

            // New Slots/Weights...
            int newSlots = 0;
            int newWeight = 0;
            
            // Bei drop
            if(drop)
            {
                // Set Slots on defaultones...
                newSlots = ContainerManager.defaultSlots;
                newWeight = ContainerManager.defaultWeight;

                // Wenn spieler mehr dabei hat als ohne rucksack tragbar
                if (currentUsedWeight > newWeight || currentUsedSlots > newSlots)
                {
                    dbPlayer.SendNewNotification($"Du traegst zu viel bei dir!");
                    return false;
                }
            }
            else
            {
                // mehr drin als neuer rucksack tragen kann...
                if (currentUsedWeight > selectedModelConfiguration.MaxWeight ||
                    currentUsedSlots > selectedModelConfiguration.MaxSlots)
                {
                    dbPlayer.SendNewNotification($"Du traegst zu viel bei dir!");
                    dbPlayer.SendNewNotification(
                        $"{currentUsedWeight}kg/{selectedModelConfiguration.MaxWeight.ToString().Substring(0, selectedModelConfiguration.MaxWeight.ToString().Length - 3)}kg:{currentUsedSlots}/{selectedModelConfiguration.MaxSlots} Slots");
                    return false;
                }

                newSlots = selectedModelConfiguration.MaxSlots;
                newWeight = selectedModelConfiguration.MaxWeight;
            }
            
            // Wenn neue Slots nicht alten entsprechen
            if (newSlots != dbPlayer.Container.MaxSlots || newWeight != dbPlayer.Container.MaxWeight)
            {
                if (newSlots != 0 || newWeight != 0)
                {
                    dbPlayer.Container.ChangeSlots(newSlots);
                    dbPlayer.Container.ChangeWeight(newWeight);
                    dbPlayer.SendNewNotification(
                        $"Inventar wurde auf {newSlots} Slots sowie {newWeight.ToString().Substring(0, newWeight.ToString().Length - 3)}kg veraendert!",
                        title: "Inventar", notificationType: PlayerNotification.NotificationType.SUCCESS);
                }
                return true;
            }

            dbPlayer.SendNewNotification($"Inventar wurde nicht veraendert!");
            return false;
        }
    }

    public static class BackpackList
    {
        public static bool IsBackPack(ItemModel itemModel)
        {
            return (itemModel.Script.StartsWith("bp_"));
        }

        public static List<BackpackConfiguration> backpackList;

        static BackpackList()
        {
            backpackList = new List<BackpackConfiguration>();
            backpackList.Add(new BackpackConfiguration(ItemModelModule.Instance.GetItemByNameOrTag("bp_tasche"), 100, 30000, 8));
            backpackList.Add(new BackpackConfiguration(ItemModelModule.Instance.GetItemByNameOrTag("bp_rucksack"), 3000, 35000, 10));
            backpackList.Add(new BackpackConfiguration(ItemModelModule.Instance.GetItemByNameOrTag("bp_alicepack"), 6000,  40000, 12));
            backpackList.Add(new BackpackConfiguration(ItemModelModule.Instance.GetItemByNameOrTag("bp_louisbeton"), 6000, 40000, 12));
            backpackList.Add(new BackpackConfiguration(ItemModelModule.Instance.GetItemByNameOrTag("bp_bauchtasche"), 6000, 40000, 12));
            backpackList.Add(new BackpackConfiguration(ItemModelModule.Instance.GetItemByNameOrTag("bp_armypack"), 10000, 50000, 14));
            backpackList.Add(new BackpackConfiguration(ItemModelModule.Instance.GetItemByNameOrTag("bp_bropack"), 0, 100000000, 48));
        }

        public class BackpackConfiguration
        {
            public ItemModel ItemModel { get; }
            public int Preis { get; }
            public int MaxWeight { get; }
            public int MaxSlots { get; }

            public BackpackConfiguration(ItemModel itemModel, int preis, int maxWeight, int maxSlots)
            {
                ItemModel = itemModel;
                Preis = preis;
                MaxWeight = maxWeight;
                MaxSlots = maxSlots;
            }
        }
    }
}