using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Shops;
using VMP_CNR.Module.Clothes.Slots;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.RemoteEvents;

namespace VMP_CNR.Module.Players.Windows
{
    public class InventoryWindow : Window<Func<DbPlayer, List<ClientContainerObject>, bool>>
    {
        private class ShowEvent : Event
        {
            //private string InventoryContent { get; } // --- appears to be empty if used?
            [JsonProperty(PropertyName = "inventory")] private List<ClientContainerObject> InventoryContent { get; } // --- Produces incorrect json

            public ShowEvent(DbPlayer dbPlayer, List<ClientContainerObject> inventoryContent) : base(dbPlayer)
            {
                InventoryContent = inventoryContent;
            }
        }
        public override Func<DbPlayer, List<ClientContainerObject>, bool> Show()
        {
            return (player, inventoryContent) => OnShow(new ShowEvent(player, inventoryContent));
        }

        public InventoryWindow() : base("Inventory")
        {
        }

        [RemoteEvent]
        public async void inventoryChooseCloth(Player client, uint clothId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
                    return;

                if (dbPlayer.RageExtension.IsInVehicle) return;

                Logger.Debug("inventoryChooseCloth " + clothId);
                Cloth cloth = ClothModule.Instance.Get(clothId);
                if (cloth != null)
                {
                    // Keine Berechtigung
                    if (!dbPlayer.Character.Wardrobe.Contains(cloth.Id)
                        && !cloth.Teams.Contains(dbPlayer.TeamId) && !cloth.IsDefault) return;

                    if (cloth.Gender != dbPlayer.Customization.Gender) return;

                    dbPlayer.SetCannotInteract(true);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                    Chats.sendProgressBar(dbPlayer, 3000);
                    await Task.Delay(3000);

                    dbPlayer.SetCannotInteract(false);
                    if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
                        return;

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.StopAnimation();


                    if (dbPlayer.Character.Clothes.ContainsKey(cloth.Slot))
                    {
                        // save old clothes
                        Cloth clothOld = ClothModule.Instance.Get(dbPlayer.Character.Clothes[cloth.Slot]);
                        if (clothOld != null && !dbPlayer.InventoryClothesBag.Clothes.ToList().Contains(clothOld.Id))
                        {
                            dbPlayer.InventoryClothesBag.Clothes.Add(clothOld.Id);
                            Logger.Debug("add to clothesbag " + clothOld.Id);
                        }

                        // actualize Wearing
                        dbPlayer.Character.Clothes[cloth.Slot] = cloth.Id;
                    }
                    else
                    {
                        // actualize Wearing
                        dbPlayer.Character.Clothes.Add(cloth.Slot, cloth.Id);
                    }
                    ClothModule.Instance.RefreshPlayerClothes(dbPlayer);
                    ClothModule.SaveCharacter(dbPlayer);
                }
            }
            catch(Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public async void inventoryChooseProp(Player client, uint clothId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
                    return;

                if (dbPlayer.RageExtension.IsInVehicle) return;

                Logger.Debug("inventoryChooseProp " + clothId);
                Clothes.Props.Prop cloth = Clothes.Props.PropModule.Instance.Get(clothId);
                if (cloth != null)
                {
                    // Keine Berechtigung
                    if (!dbPlayer.Character.Props.Contains(cloth.Id)
                        && cloth.TeamId != dbPlayer.TeamId && !cloth.IsDefault) return;

                    if (cloth.Gender != dbPlayer.Customization.Gender) return;

                    dbPlayer.SetCannotInteract(true);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                    Chats.sendProgressBar(dbPlayer, 3000);
                    await Task.Delay(3000);

                    dbPlayer.SetCannotInteract(false);
                    if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
                        return;

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.StopAnimation();

                    if (dbPlayer.Character.EquipedProps.ContainsKey(cloth.Slot))
                    {
                        Clothes.Props.Prop clothOld = Clothes.Props.PropModule.Instance.Get(dbPlayer.Character.EquipedProps[cloth.Slot]);
                        if (clothOld != null && !dbPlayer.InventoryClothesBag.Props.ToList().Contains(clothOld.Id))
                        {
                            dbPlayer.InventoryClothesBag.Props.Add(clothOld.Id);
                            Logger.Debug("add to clothesbag " + clothOld.Id);
                        }
                        dbPlayer.Character.EquipedProps[cloth.Slot] = cloth.Id;
                    }
                    else
                    {
                        dbPlayer.Character.EquipedProps.Add(cloth.Slot, cloth.Id);
                    }
                    ClothModule.Instance.RefreshPlayerClothes(dbPlayer);
                    ClothModule.SaveCharacter(dbPlayer);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
        [RemoteEvent]
        public void packClothesBag(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
                return;

            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.ClothesPacked))
                return;

            dbPlayer.InventoryClothesBag.Clothes.Clear();
            dbPlayer.InventoryClothesBag.Props.Clear();

            // Add Clothes from actual wearing
            foreach (KeyValuePair<int, uint> kvp in dbPlayer.Character.Clothes.ToList())
            {
                Clothes.Cloth cloth = Clothes.ClothModule.Instance.Get(kvp.Value);
                if (cloth == null || cloth.Gender != dbPlayer.Customization.Gender) continue;

                if (dbPlayer.InventoryClothesBag.Clothes.Contains(kvp.Value)) continue;
                dbPlayer.InventoryClothesBag.Clothes.Add(kvp.Value);
            }

            // Add Clothes from actual wearing
            foreach (KeyValuePair<int, uint> kvp in dbPlayer.Character.EquipedProps.ToList())
            {
                Clothes.Props.Prop cloth = Clothes.Props.PropModule.Instance.Get(kvp.Value);
                if (cloth == null || cloth.Gender != dbPlayer.Customization.Gender) continue;

                if (dbPlayer.InventoryClothesBag.Props.Contains(kvp.Value)) continue;
                dbPlayer.InventoryClothesBag.Props.Add(kvp.Value);
            }

            dbPlayer.SendNewNotification("Du hast deine aktuelle Kleidung in deinen Kleiderbeutel gepackt...");
        }

        [RemoteEvent]
        public void requestPlayerClothes(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            try 
            {
                Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
                {
                    DbPlayer dbPlayer = client.GetPlayer();
                    if (dbPlayer == null || !dbPlayer.IsValid())
                        return;

                    List<InventoryPlayerClothesCategory> playerInvCats = new List<InventoryPlayerClothesCategory>();

                    // Clothes
                    foreach (ClothesSlot slot in ClothesShopModule.Instance.clothesSlots.Values.ToList())
                    {
                        InventoryPlayerClothesCategory slotInvCat = new InventoryPlayerClothesCategory()
                        {
                            Slot = slot.Id,
                            Name = slot.Name,
                            Items = new List<InventoryPlayerClothesItem>()
                        };

                        if (slot.Id == 3) // Körper
                        {
                            foreach (uint clothId in dbPlayer.Character.Wardrobe.ToList())
                            {
                                Clothes.Cloth cloth = Clothes.ClothModule.Instance.Get(clothId);
                                if (cloth == null || cloth.Slot != 3 || cloth.Gender != dbPlayer.Customization.Gender) continue;

                                if (slotInvCat.Items.Where(i => i.Id == cloth.Id).Count() > 0) continue;
                                slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = cloth.Id, Name = cloth.Name });
                            }
                        }
                        else
                        {
                            // First add Default
                            Clothes.Cloth defaultCloth = Clothes.ClothModule.Instance.GetAll().Values.Where(c => c.Slot == slot.Id && c.IsDefault && c.Gender == dbPlayer.Customization.Gender).FirstOrDefault();

                            if (defaultCloth != null)
                            {
                                if (slotInvCat.Items.Where(i => i.Id == defaultCloth.Id).Count() > 0) continue;
                                slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = defaultCloth.Id, Name = defaultCloth.Name });
                            }

                            // Add Clothes from actual wearing
                            foreach (KeyValuePair<int, uint> kvp in dbPlayer.Character.Clothes.ToList().Where(c => c.Key == slot.Id))
                            {
                                Clothes.Cloth cloth = Clothes.ClothModule.Instance.Get(kvp.Value);
                                if (cloth == null || cloth.Gender != dbPlayer.Customization.Gender) continue;

                                if (slotInvCat.Items.Where(i => i.Id == cloth.Id).Count() > 0) continue;
                                slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = cloth.Id, Name = cloth.Name });
                            }

                            // Add Clothes from clothes bag
                            foreach (uint clothId in dbPlayer.InventoryClothesBag.Clothes.ToList())
                            {
                                Clothes.Cloth cloth = Clothes.ClothModule.Instance.Get(clothId);
                                if (cloth == null || cloth.Gender != dbPlayer.Customization.Gender || cloth.Slot != slot.Id) continue;

                                if (slotInvCat.Items.Where(i => i.Id == cloth.Id).Count() > 0) continue;
                                slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = cloth.Id, Name = cloth.Name });
                            }
                        }
                        playerInvCats.Add(slotInvCat);
                    }

                    // Props
                    foreach (PropsSlot slot in ClothesShopModule.Instance.propsSlots.Values.ToList())
                    {
                        InventoryPlayerClothesCategory slotInvCat = new InventoryPlayerClothesCategory()
                        {
                            Slot = slot.Id,
                            Name = slot.Name,
                            Items = new List<InventoryPlayerClothesItem>()
                        };

                        // First add Default
                        Clothes.Props.Prop defaultCloth = Clothes.Props.PropModule.Instance.GetAll().Values.Where(c => c.Slot == slot.Id && c.IsDefault && c.Gender == dbPlayer.Customization.Gender).FirstOrDefault();

                        if (defaultCloth != null)
                        {
                            if (slotInvCat.Items.Where(i => i.Id == defaultCloth.Id).Count() > 0) continue;
                            slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = defaultCloth.Id, Name = defaultCloth.Name, Prop = true });
                        }

                        // Add Props from actual wearing
                        foreach (KeyValuePair<int, uint> kvp in dbPlayer.Character.EquipedProps.ToList().Where(c => c.Key == slot.Id))
                        {
                            Clothes.Props.Prop cloth = Clothes.Props.PropModule.Instance.Get(kvp.Value);
                            if (cloth == null || cloth.Gender != dbPlayer.Customization.Gender) continue;

                            if (slotInvCat.Items.Where(i => i.Id == cloth.Id).Count() > 0) continue;
                            slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = cloth.Id, Name = cloth.Name, Prop = true });
                        }

                        // Add Props from clothes bag
                        foreach (uint clothId in dbPlayer.InventoryClothesBag.Props.ToList())
                        {
                            Clothes.Props.Prop cloth = Clothes.Props.PropModule.Instance.Get(clothId);
                            if (cloth == null || cloth.Gender != dbPlayer.Customization.Gender || cloth.Slot != slot.Id) continue;

                            if (slotInvCat.Items.Where(i => i.Id == cloth.Id).Count() > 0) continue;
                            slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = cloth.Id, Name = cloth.Name, Prop = true });
                        }

                        playerInvCats.Add(slotInvCat);
                    }

                    Logger.Debug(NAPI.Util.ToJson(playerInvCats));
                    dbPlayer.Player.TriggerNewClient("componentServerEvent", "Inventory", "responseInventoryClothes", NAPI.Util.ToJson(playerInvCats));
                }));
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
    }

    public class InventoryPlayerClothesCategory
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Slot")]
        public int Slot { get; set; }

        [JsonProperty(PropertyName = "Items")]
        public List<InventoryPlayerClothesItem> Items { get; set; }
    }

    public class InventoryPlayerClothesItem
    {
        [JsonProperty(PropertyName = "Id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Prop")]
        public bool Prop { get; set; }

        public InventoryPlayerClothesItem()
        {
            Prop = false;
        }
    }
}
