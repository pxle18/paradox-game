using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items.Scripts;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Buffs;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.RemoteEvents;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Guenther;
using VMP_CNR.Module.Asservatenkammer;
using VMP_CNR.Module.Vehicles;
using System.Collections.Concurrent;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Nutrition;
using Newtonsoft.Json;

namespace VMP_CNR.Module.Items
{
    public class ItemsModuleEvents : Script
    {
        public static async Task RequestInventory(DbPlayer dbPlayer)
        {
            try
            {
                // Check Cuff Die Death
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
                {
                    return;
                }

                if (!Configuration.Instance.InventoryActivated)
                {
                    dbPlayer.SendNewNotification("Das Inventarsystem ist aus Performance-Gründen deaktiviert.");
                    dbPlayer.SendNewNotification("Es ist in wenigen Minuten wieder erreichbar!");
                    return;
                }

                if (dbPlayer.HasData("container_refund"))
                {
                    dbPlayer.ResetData("container_refund");
                }

                List<ClientContainerObject> containerList = new List<ClientContainerObject>
                {
                    dbPlayer.Container.ConvertForClient(1, "", dbPlayer.Money[0], dbPlayer.BlackMoney[0], true)
                };

                // Find Now The Inventory
                string clientsending = "[";
                clientsending += JsonConvert.SerializeObject(dbPlayer.Container.ConvertForClient(1));

                Container externContainer = await ItemsModule.Instance.findInventory(dbPlayer);
                if (externContainer != null && !externContainer.Locked)
                {
                    containerList.Add(externContainer.ConvertForClient(2));
                    clientsending += "," + JsonConvert.SerializeObject(externContainer.ConvertForClient(2));
                }

                clientsending += "]";

                Logging.Logger.Debug("clientsending");
                dbPlayer.Player.TriggerNewClient("responseInventory", clientsending);

                ComponentManager.Get<InventoryWindow>().Show()(dbPlayer, containerList);

                if (dbPlayer.IsACop() && dbPlayer.Duty)
                {
                    resetFriskInventoryFlags(dbPlayer);
                }
                else
                {
                    resetDisabledInventoryFlag(dbPlayer);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async void requestInventory(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!dbPlayer.CanAccessRemoteEvent())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.InventoryOpened))
                return;

            await RequestInventory(dbPlayer);
        }

        public static void resetDisabledInventoryFlag(DbPlayer dbPlayer)
        {
            dbPlayer.ResetData("disableinv");
        }

        public static void resetFriskInventoryFlags(DbPlayer dbPlayer)
        {
            dbPlayer.ResetData("disableFriskInv");
            dbPlayer.ResetData("friskInvUserName");
            dbPlayer.ResetData("friskInvUserID");
            dbPlayer.ResetData("friskInvVeh");
            dbPlayer.ResetData("friskInvHouse");
        }

        public async void actualizeInventory(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            List<ClientContainerObject> containerList = new List<ClientContainerObject>
            {
                dbPlayer.Container.ConvertForClient(1)
            };

            // Find Now The Inventory
            string clientsending = "[";
            clientsending += NAPI.Util.ToJson(dbPlayer.Container.ConvertForClient(1));

            if (ItemsModule.Instance.findInventory(dbPlayer) != null)
            {
                Container Inventory = await ItemsModule.Instance.findInventory(dbPlayer);
                containerList.Add(Inventory.ConvertForClient(2));
                clientsending += "," + NAPI.Util.ToJson(Inventory.ConvertForClient(2));
            }
            clientsending += "]";

            ContainerManager.CheckFunkDisabling(dbPlayer.Container);

            dbPlayer.Player.TriggerNewClient("setInventoryItems", clientsending);
        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async Task dropInventoryItem(Player player, int slot, int amount, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (dbPlayer.Container == null)
            {
                return;
            }

            if (dbPlayer.RageExtension.IsInVehicle)
            {
                return;
            }

            if (dbPlayer.HasData("disableinv") && dbPlayer.GetData("disableinv"))
            {
                return; // Show Inventory
            }
            
            if (slot < 0 || slot > 47)
            {
                return;
            }
            
            if (dbPlayer.HasData("disableFriskInv") && dbPlayer.GetData("disableFriskInv"))
            {
                if (!dbPlayer.HasData("friskInvUserName") && !dbPlayer.HasData("friskInvVeh") && !dbPlayer.HasData("friskInvHouse"))
                {
                    if (!dbPlayer.GetData("friskInvUserName") && !dbPlayer.GetData("friskInvVeh") && !dbPlayer.GetData("friskInvHouse"))
                    {
                        dbPlayer.SendNewNotification("Fehler beim Durchsuchen!");
                        return;
                    }
                }
                
                if (dbPlayer.HasData("friskInvVeh"))
                {
                    SxVehicle vehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
                    if (vehicle == null || dbPlayer.Player.Position.DistanceTo(vehicle.Entity.Position) > 10.0f)
                    {
                        return;
                    }

                    if (dbPlayer.GetData("friskInvVeh") == vehicle.Data.Model)
                    {
                        ItemModel model = vehicle.Container.GetModelOnSlot(slot);
                        if (model == null)
                        {
                            return;
                        }

                        if (vehicle.Container == null || vehicle.Container.Slots[slot] == null || vehicle.Container.Slots[slot].Model == null)
                        {
                            return;
                        }


                        ItemModel BeschlagnahmtItem = ItemModelModule.Instance.GetById(AsservatenkammerModule.Instance.GetConvertionItemId(model.Id, model.Script.StartsWith("w_"), model.Script.StartsWith("ammo_")));

                        if (BeschlagnahmtItem == null) return;

                        if (!AsservatenkammerModule.Instance.IsAserItem(BeschlagnahmtItem.Id)) // Wenn kein AserItem (zb Handy etc) Einfach droppen
                        {
                            dbPlayer.SendNewNotification($"{vehicle.Container.Slots[slot].Amount} - {vehicle.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                        }
                        else
                        {
                            // Get Vehicle With Kofferraum open rofl
                            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehiclesFromTeamWithContainerOpen(dbPlayer.Player.Position, (int)dbPlayer.TeamId).FirstOrDefault();
                            if (sxVehicle == null || !sxVehicle.IsValid())
                            {
                                dbPlayer.SendNewNotification("Kein Fahrzeug zur Beschlagnahmung in der Nähe! (Kofferraum muss offen sein!)");
                                return;
                            }

                            if (!sxVehicle.Container.CanInventoryItemAdded(BeschlagnahmtItem, vehicle.Container.Slots[slot].Amount))
                            {
                                dbPlayer.SendNewNotification("Sie haben nicht genug Platz für eine Beschlagnahmung!");
                                return;
                            }

                            sxVehicle.Container.AddItem(BeschlagnahmtItem, vehicle.Container.Slots[slot].Amount);
                            dbPlayer.SendNewNotification($"{vehicle.Container.Slots[slot].Amount} - {vehicle.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                        }

                        vehicle.Container.RemoveFromSlot(slot, amount);
                        return;
                    }
                }

                if (dbPlayer.HasData("friskInvHouse"))
                {
                    if (dbPlayer.GetData("friskInvHouse") == dbPlayer.Player.Dimension && dbPlayer.DimensionType[0] == DimensionType.House && dbPlayer.HasData("inHouse"))
                    {
                        House house;
                        if ((house = HouseModule.Instance.Get(dbPlayer.GetData("inHouse"))) != null)
                        {
                            ItemModel model = house.Container.GetModelOnSlot(slot);
                            if (model == null)
                            {
                                return;
                            }

                            if (house.Container == null || house.Container.Slots[slot] == null || house.Container.Slots[slot].Model == null)
                            {
                                return;
                            }

                            dbPlayer.SendNewNotification($"{house.Container.Slots[slot].Amount} - {house.Container.Slots[slot].Model.Name} wurden entfernt.");

                            house.Container.RemoveFromSlot(slot, amount);
                            return;
                        }
                    }
                }

                try
                {
                    if (dbPlayer.HasData("friskInvUserID"))
                    {
                        DbPlayer findPlayer = Players.Players.Instance.FindPlayerById(dbPlayer.GetData("friskInvUserID"));
                        if (findPlayer == null || !findPlayer.IsValid())
                        {
                            dbPlayer.SendNewNotification("Buerger nicht gefunden!");
                            return;
                        }

                        if (findPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.0f)
                        {
                            dbPlayer.SendNewNotification("Buerger zu weit entfernt!");
                            return;
                        }

                        if (!findPlayer.IsCuffed && !findPlayer.IsTied && !findPlayer.IsInjured())
                        {
                            dbPlayer.SendNewNotification("Buerger nicht gefesselt / bewusstlos!");
                            return;
                        }

                        if (findPlayer.Container == null || findPlayer.Container.Slots[slot] == null || findPlayer.Container.Slots[slot].Model == null)
                        {
                            return;
                        }

                        ItemModel model = findPlayer.Container.GetModelOnSlot(slot);
                        if (model == null)
                        {
                            return;
                        }

                        if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null)
                        {
                            bool suxxess = ItemScript.backpack(findPlayer, model, true);

                            string message = suxxess ? "" : "nicht ";

                            dbPlayer.SendNewNotification($"Der Rucksack wurde {message}entfernt!");
                            findPlayer.SendNewNotification($"Der Beamte hat den Rucksack {message}entfernt!");

                            if (!suxxess)
                            {
                                return;
                            }
                            else
                            {
                                findPlayer.Container.RemoveFromSlot(slot, amount);
                                actualizeInventory(findPlayer.Player);
                                return;
                            }
                        }

                        // Falls Zivis iwann frisk entfernen können
                        if (dbPlayer.IsACop() && dbPlayer.IsInDuty())
                        {
                            ItemModel BeschlagnahmtItem = ItemModelModule.Instance.GetById(AsservatenkammerModule.Instance.GetConvertionItemId(model.Id, model.Script.StartsWith("w_"), model.Script.StartsWith("ammo_")));

                            if (BeschlagnahmtItem == null) return;

                            if (!AsservatenkammerModule.Instance.IsAserItem(BeschlagnahmtItem.Id)) // Wenn kein AserItem (zb Handy etc) Einfach droppen
                            {
                                dbPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                                findPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                            }
                            else
                            {
                                if(findPlayer.IsACop())
                                {
                                    dbPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                                    findPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                                    findPlayer.Container.RemoveFromSlot(slot, amount);
                                }

                                // Get Vehicle With Kofferraum open rofl
                                SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehiclesFromTeamWithContainerOpen(dbPlayer.Player.Position, (int)dbPlayer.TeamId).FirstOrDefault();
                                if (sxVehicle == null || !sxVehicle.IsValid())
                                {
                                    dbPlayer.SendNewNotification("Kein Fahrzeug zur Beschlagnahmung in der Nähe! (Kofferraum muss offen sein!)");
                                    return;
                                }

                                if (!sxVehicle.Container.CanInventoryItemAdded(BeschlagnahmtItem, findPlayer.Container.Slots[slot].Amount))
                                {
                                    dbPlayer.SendNewNotification("Sie haben nicht genug Platz für eine Beschlagnahmung!");
                                    return;
                                }
                                
                                sxVehicle.Container.AddItem(BeschlagnahmtItem, findPlayer.Container.Slots[slot].Amount);
                                dbPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                                findPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");

                                // FIB
                                if (dbPlayer.TeamId == (int)TeamTypes.TEAM_FIB)
                                {
                                    if (findPlayer.Container.Slots[slot].Data != null && findPlayer.Container.Slots[slot].Data.ContainsKey("fingerprint"))
                                    {
                                        dbPlayer.SendNewNotification($"Es konnte ein Fingerabdruck auf dieser Waffe festgestellt werden! (ID: {findPlayer.Container.Slots[slot].Data["fingerprint"]})");
                                    }
                                }

                                Attachments.AttachmentModule.Instance.ClearAllAttachments(findPlayer);
                            }
                        }

                        findPlayer.Container.RemoveFromSlot(slot, amount);

                        // Komisches zeug für funk und apps
                        if (findPlayer.Container.GetItemById(174).Amount < 1)
                        {
                            PhoneCall.CancelPhoneCall(findPlayer);
                            VoiceModule.Instance.turnOffFunk(findPlayer);
                        }
                        findPlayer.UpdateApps();
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }
            else
            {
                ItemModel model = dbPlayer.Container.GetModelOnSlot(slot);
                if (model == null) return;

                try
                {
                    // Check Cuff Die Death
                    if (!dbPlayer.CanInteract())
                    {
                        return;
                    }

                    if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null)
                    {
                        if (!ItemScript.backpack(dbPlayer, model, true))
                        {
                            return;
                        }
                    }

                    if (model.Id == 1134) // Waffengürtel
                    {
                        if (dbPlayer.GetPlayerWeaponsWeight() > PlayerWeapon.MaxPlayerWeaponWeight)
                        {
                            dbPlayer.SendNewNotification($"Der Gürtel ist derzeit befüllt!");
                            return;
                        }
                    }

                    if (model.CanDrugInfect())
                    {
                        dbPlayer.IncreasePlayerDrugInfection();
                    }

                    /*
                    ItemModel addModel = dbPlayer.Container.GetModelOnSlot(slot);
                    if(addModel != null)
                    {
                        ConcurrentDictionary<ItemModel, int> droppedItems = new ConcurrentDictionary<ItemModel, int>(); // Log meinte crasht hier, aber is doch nur n Dictionary?? Ich machs mal threadsicher, vielleicht wirds ja was
                        droppedItems.TryAdd(addModel, amount);
                    }*/

                    dbPlayer.Container.RemoveFromSlot(slot, amount);

                    if (dbPlayer.Container.GetItemAmount(174) < 1)
                    {
                        PhoneCall.CancelPhoneCall(dbPlayer);
                        VoiceModule.Instance.turnOffFunk(dbPlayer);
                    }
                    actualizeInventory(player);

                    Attachments.AttachmentModule.Instance.ClearAllAttachments(dbPlayer);

                    await dbPlayer.PlayInventoryInteractAnimation();

                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async Task giveInventoryItem(Player player, int slot, int amount, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (dbPlayer.HasData("disableinv") && dbPlayer.GetData("disableinv"))
            {
                return; // Show Inventory
            }

            if (!dbPlayer.CanAccessRemoteEvent())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!dbPlayer.HasData("giveitem"))
            {
                return;
            }

            DbPlayer destinationPlayer = Players.Players.Instance.FindPlayerById(dbPlayer.GetData("giveitem"));
            dbPlayer.ResetData("giveitem");
            if (destinationPlayer.IsInjured()) return;
            if (destinationPlayer == null || !destinationPlayer.IsValid())
            {
                dbPlayer.SendNewNotification("Niemand in der Nähe!");
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return;
            }

            if (destinationPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.0f)
            {
                dbPlayer.SendNewNotification("Zu weit entfernt!");
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return;
            }

            if (slot < 0 || slot > 47)
            {
                return;
            }

            Item item = dbPlayer.Container.GetItemOnSlot(slot);
            ItemModel model = item.Model;

            try
            {
                // Check Cuff Die Death
                if (!dbPlayer.CanInteract())
                {
                    return;
                }

                if (item.Id == 550)
                {
                    dbPlayer.SendNewNotification("Dieses Item koennen sie nicht weitergeben.");
                    return;
                }

                // Ostereier
                if(dbPlayer.RankId <= 0 && (item.Id == 1035 || item.Id == 1036 || item.Id == 1034))
                {
                    return;
                }

                if(item.Id == 1198)
                {
                    return;
                }

                if (model.Id == 1134) // Waffengürtel
                {
                    if (dbPlayer.GetPlayerWeaponsWeight() > PlayerWeapon.MaxPlayerWeaponWeight)
                    {
                        dbPlayer.SendNewNotification($"Der Gürtel ist derzeit befüllt!");
                        return;
                    }
                }

                if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null)
                {
                    return;
                }

                if (model.CanDrugInfect())
                {
                    dbPlayer.IncreasePlayerDrugInfection();
                }

                if (!destinationPlayer.Container.CanInventoryItemAdded(model, amount))
                {
                    dbPlayer.SendNewNotification("Kein Platz");
                    ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                    return;
                }


                if (model.AttachmentOnlyId > 0 && destinationPlayer.Container.GetAttachmentOnlyItem() != null)
                {
                    dbPlayer.SendNewNotification("Die Person trägt bereits etwas mit sich!");
                    ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                    return;
                }

                if (model.Id == 174)
                {
                    PhoneCall.CancelPhoneCall(dbPlayer);
                    VoiceModule.Instance.turnOffFunk(dbPlayer);
                }

                dbPlayer.Container.RemoveFromSlot(slot, amount);
                destinationPlayer.Container.AddItem(model, amount, item.Data);
                actualizeInventory(player);
                actualizeInventory(destinationPlayer.Player);
                dbPlayer.SendNewNotification($"Du hast {amount} {model.Name} gegeben!");
                destinationPlayer.SendNewNotification($"Du hast {amount} {model.Name} bekommen!");

                await dbPlayer.PlayInventoryGiveInteractAnimation();
                destinationPlayer.SyncAttachmentOnlyItems();

                if (ServerFeatures.IsActive("itemlog"))
                {
                    Logger.SaveToItemLog(dbPlayer.Id, dbPlayer.GetName(), model.Id, -amount, "" + destinationPlayer.Container.Type, (int)destinationPlayer.Id);
                    Logger.SaveToItemLog(destinationPlayer.Id, destinationPlayer.GetName(), model.Id, amount, "" + dbPlayer.Container.Type, (int)dbPlayer.Id);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async void moveItemToInventory(Player player, int sourceSlot, int destinationSlot, int inventoryType, int amount, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            ContainerMoveTypes containerMoveType = (ContainerMoveTypes)inventoryType;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!await ContainerInventoryActions.MoveItemToInventory(dbPlayer, containerMoveType, sourceSlot, destinationSlot, amount))
            {
                // Close Inventory on Client
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
            }

            return;

        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async void fillInventorySlots(Player player, int inventoryType, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }

                ContainerMoveTypes containerMoveType = (ContainerMoveTypes)inventoryType;

                // External Container
                Container eXternContainer = await ItemsModule.Instance.findInventory(dbPlayer);

                if (containerMoveType == ContainerMoveTypes.ExternInventory)
                {
                    if (eXternContainer == null)
                    {
                        return;
                    }
                }

                ContainerInventoryActions.FillInventorySlots(containerMoveType == ContainerMoveTypes.ExternInventory ? eXternContainer : dbPlayer.Container);
            }
            catch(Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async void moveItemInInventory(Player player, int sourceSlot, int destinationSlot, int inventoryType, int amount, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            ContainerMoveTypes containerMoveType = (ContainerMoveTypes)inventoryType;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!await ContainerInventoryActions.moveItemInInventory(dbPlayer, containerMoveType, sourceSlot, destinationSlot, amount))
            {
                // Close Inventory on Client
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
            }
            return;

        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async Task<bool> useInventoryItem(Player client, int slot, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return false;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return false;
            }

            bool usedSuccessfully = false;

            // Check Cuff Die Death
            if (!dbPlayer.CanInteract())
            {
                return false;
            }

            if (dbPlayer.HasData("disableinv") && dbPlayer.GetData("disableinv"))
            {
                return false; // Show Inventory
            }

            if (dbPlayer.HasData("disableFriskInv") && dbPlayer.GetData("disableFriskInv"))
            {
                return false; // Show Inventory
            }

            // Wenn man was trägt...
            if (dbPlayer.Attachments.Values.ToList().Where(a => a.IsCarry).Count() > 0)
            {
                return false;
            }

            if (dbPlayer.ActionsBlocked)
            {
                return false;
            }

            if (!dbPlayer.CanAccessRemoteEvent())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return false;
            }
            
            if (slot < 0 || slot > 47)
            {
                return false;
            }

            ItemModel model = dbPlayer.Container.GetModelOnSlot(slot);
            Item item = dbPlayer.Container.GetItemOnSlot(slot);

            if (model != null)
            {
                if (dbPlayer.IsInjured())
                {
                    dbPlayer.SendNewNotification("Sie koennen diese Funktion derzeit nicht benutzen.");
                    return false;
                }

                
                /*if (dbPlayer.lastnutritionitems.Count()>0&&dbPlayer.lastnutritionitems.Where(x => x.Itemid == item.Id && x.Time <= DateTime.Now.AddMinutes(-30)).Count() >= 2)
                {
                    dbPlayer.SendNewNotification("Igitt, immer der gleiche Fraß");
                    return false;
                }
                */

                if (HalloweenModule.isActive && dbPlayer.IsZombie()) return false;

                if(model.RestrictedToTeams.Count > 0)
                {
                    if (!model.RestrictedToTeams.Contains(dbPlayer.Team.Id)) return false;
                }

                dbPlayer.ActionsBlocked = true;

                // First Startswith Script
                if (model.Script.ToLower().StartsWith("w_"))
                {
                    usedSuccessfully = ItemScript.WeaponUnpack(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("food_"))
                {
                    usedSuccessfully = await ItemScript.AttachedFood(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("drink_"))
                {
                    usedSuccessfully = await ItemScript.AttachedDrink(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("bw_"))
                {
                    usedSuccessfully = ItemScript.WeaponUnpackCop(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("zw_"))
                {
                    usedSuccessfully = await ItemScript.ZerlegteWaffeUnpack(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("ammo_"))
                {
                    usedSuccessfully = await ItemScript.EquipAmo(dbPlayer, model, item.Amount);
                }
                else if (model.Script.ToLower().StartsWith("wc_"))
                {
                    usedSuccessfully = await ItemScript.EquipComponent(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("bammo_"))
                {
                    usedSuccessfully = await ItemScript.EquipAmoCop(dbPlayer, model, item.Amount);
                }
                else if (model.Script.ToLower().StartsWith("r_map"))
                {
                    usedSuccessfully = ItemScript.RessourceMap(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("tune_"))
                {
                    usedSuccessfully = ItemScript.TuningParts(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("bp_"))
                {
                    usedSuccessfully = ItemScript.backpack(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("alk_"))
                {
                    usedSuccessfully = await ItemScript.Alk(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("chest_"))
                {
                    usedSuccessfully = ItemScript.ChestUnpack(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("pkfzbrief_"))
                {
                    usedSuccessfully = ItemScript.PresentKFZBrief(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("pmoney_"))
                {
                    usedSuccessfully = ItemScript.PresentMoney(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("registervehicle_"))
                {
                    usedSuccessfully = ItemScript.VehicleRegister(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("unregistervehicle_"))
                {
                    usedSuccessfully = ItemScript.VehicleUnregister(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("divebottle_"))
                {
                    usedSuccessfully = ItemScript.Divebottle(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("dice_"))
                {
                    usedSuccessfully = await ItemScript.Dice(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("geschenk"))
                {
                    usedSuccessfully = ItemScript.Geschenk(dbPlayer);
                }
                else if (model.Script.ToLower().StartsWith("armortype_"))
                {
                    usedSuccessfully = await ItemScript.Armortype(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("vehicle_claw_"))
                {
                    usedSuccessfully = await ItemScript.VehicleClaw(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("outfit_"))
                {
                    usedSuccessfully = await ItemScript.Outfit(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("itemtocloth_"))
                {
                    usedSuccessfully = ItemScript.ItemToCloth(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("itemtoprop_"))
                {
                    usedSuccessfully = ItemScript.ItemToProp(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("autohaus_"))
                {
                    usedSuccessfully = ItemScript.UseCarsell(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("ccook_"))
                {
                    usedSuccessfully = await ItemScript.CCook(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("zp_"))
                {
                    usedSuccessfully = await ItemScript.ZerlegtePistole(dbPlayer, model);
                }
                else
                {
                    // Do Scripts
                    switch (model.Script)
                    {
                        case "wkiste":
                            usedSuccessfully = await ItemScript.WeaponChest(dbPlayer, model);
                            break;
                        case "wkiste2":
                            usedSuccessfully = await ItemScript.WeaponChest2(dbPlayer, model);
                            break;
                        case "present":
                            usedSuccessfully = ItemScript.PresentScript(dbPlayer, model);
                            break;
                        case "methcook":
                            usedSuccessfully = ItemScript.HeroinCook(dbPlayer, model);
                            break;
                        case "repair":
                            usedSuccessfully = await ItemScript.VehicleRepair(dbPlayer, model);
                            break;
                        case "geldboerse":
                            usedSuccessfully = ItemScript.Geldboerse(dbPlayer, model);
                            break;
                        case "brecheisen":
                            usedSuccessfully = await ItemScript.Brecheisen(dbPlayer, model);
                            break;
                        case "schweissgeraet":
                            usedSuccessfully = await ItemScript.Schweissgereat(dbPlayer, model);
                            break;
                        case "barriere_arrow":
                            usedSuccessfully = await ItemScript.BarriereArrow(dbPlayer, model);
                            break;
                        case "stinger":
                            usedSuccessfully = await ItemScript.Stringer(dbPlayer, model);
                            break;
                        case "barriere2":
                            usedSuccessfully = await ItemScript.barriere2(dbPlayer, model);
                            break;
                        case "barriere":
                            usedSuccessfully = await ItemScript.barriere(dbPlayer, model);
                            break;
                        case "light":
                            usedSuccessfully = await ItemScript.light(dbPlayer, model);
                            break;
                        case "wkasten":
                            usedSuccessfully = await ItemScript.wkasten(dbPlayer, model);
                            break;
                        case "uncuff":
                            usedSuccessfully = await ItemScript.uncuff(dbPlayer, model);
                            break;
                        case "joint":
                            usedSuccessfully = await ItemScript.joint(dbPlayer, model);
                            break;
                        case "paper":
                            usedSuccessfully = ItemScript.paper(dbPlayer, model);
                            break;
                        case "armor":
                            usedSuccessfully = await ItemScript.Armor(dbPlayer, model);
                            break;
                        case "underarmour":
                            usedSuccessfully = await ItemScript.UnderArmor(dbPlayer, model);
                            break;
                        case "barmor":
                            usedSuccessfully = await ItemScript.BArmor(dbPlayer, model);
                            break;
                        case "bunderarmour":
                            usedSuccessfully = await ItemScript.BUnderArmor(dbPlayer, model);
                            break;
                        case "medikit":
                            usedSuccessfully = await ItemScript.medikit(dbPlayer, model);
                            break;
                        case "fuel":
                            usedSuccessfully = ItemScript.fuel(dbPlayer, model);
                            break;
                        case "hookie":
                            usedSuccessfully = await ItemScript.hookie(dbPlayer, model);
                            break;
                        case "klappstuhl":
                            usedSuccessfully = await ItemScript.Klappstuhl(dbPlayer);
                            break;
                        case "klappstuhlb":
                            usedSuccessfully = await ItemScript.Klappstuhlb(dbPlayer);
                            break;
                        case "flagge":
                            usedSuccessfully = await ItemScript.Flagge(dbPlayer);
                            break;
                        case "firemagic":
                            usedSuccessfully = await ItemScript.Firemagic(dbPlayer);
                            break;
                        case "telefonguthaben":
                            usedSuccessfully = ItemScript.telefonguthaben(dbPlayer, model);
                            break;
                        case "carusorosso":
                            usedSuccessfully = await ItemScript.carusorosso(dbPlayer, model);
                            break;
                        case "blitzer70":
                            usedSuccessfully = await ItemScript.Blitzer70(dbPlayer, model);
                            break;
                        case "blitzer100":
                            usedSuccessfully = await ItemScript.Blitzer120(dbPlayer, model);
                            break;
                        case "b_koffer":
                            usedSuccessfully = ItemScript.Bargeldkoffer(dbPlayer, model, item);
                            break;
                        case "food":
                            usedSuccessfully = await ItemScript.Food(dbPlayer, model);
                            break;
                        case "drink":
                            usedSuccessfully = await ItemScript.Drink(dbPlayer, model);
                            break;
                        case "gps_tracker":
                            usedSuccessfully = ItemScript.GpsTracker(dbPlayer, model);
                            break;
                        case "plate_empty":
                            usedSuccessfully = await ItemScript.VehiclePlate(dbPlayer, item);
                            break;
                        case "vehiclerent":
                            usedSuccessfully = ItemScript.vehiclerent(dbPlayer, model);
                            break;
                        case "vehiclerentview":
                            usedSuccessfully = ItemScript.vehiclerentview(dbPlayer, model, item);
                            break;
                        case "vehicle_contract":
                            usedSuccessfully = ItemScript.VehicleContract(dbPlayer, item);
                            break;
                        case "marriage_certificate":
                            usedSuccessfully = ItemScript.Eheurkunde(dbPlayer, item);
                            break;
                        case "marriage_announce":
                            usedSuccessfully = await ItemScript.MarryAnnounce(dbPlayer, item);
                            break;
                        case "plate_used":
                            usedSuccessfully = await ItemScript.VehiclePlateUsed(dbPlayer, item);
                            break;
                        case "digging":
                            usedSuccessfully = ItemScript.Digging(dbPlayer);
                            break;
                        case "vehicle_note":
                            usedSuccessfully = await ItemScript.VehicleNote(dbPlayer, item);
                            break;
                        case "FMedikit":
                            usedSuccessfully = await ItemScript.FMedikit(dbPlayer, model);
                            break;
                        case "FArmor":
                            usedSuccessfully = await ItemScript.FArmor(dbPlayer, model);
                            break;
                        case "gaspberry":
                            usedSuccessfully = await ItemScript.Gaspberry(dbPlayer, model);
                            break;
                        case "KisteMethamphetamin":
                            usedSuccessfully = await ItemScript.KisteMethamphetamin(dbPlayer, model);
                            break;
                        case "zentrifuge":
                            usedSuccessfully = await ItemScript.Zentrifuge(dbPlayer, model);
                            break;
                        case "guentherclub":
                            usedSuccessfully = await ItemScript.GuentherClub(dbPlayer);
                            break;
                        case "remove_vehicle_claw":
                            usedSuccessfully = await ItemScript.RemoveVehicleClaw(dbPlayer, model);
                            break;
                        case "alarm_system":
                            usedSuccessfully = ItemScript.AlarmSystem(dbPlayer, model);
                            break;
                        case "JohannsDietrich":
                            usedSuccessfully = await ItemScript.JohannsDietrich(dbPlayer, model);
                            break;
                        case "detectdrugsair":
                            usedSuccessfully = await ItemScript.DrugtestAir(dbPlayer, model);
                            break;
                        case "batterieram":
                            usedSuccessfully = await ItemScript.BatterieRam(dbPlayer, model);
                            break;
                        case "checkenergy":
                            usedSuccessfully = ItemScript.VoltageTest(dbPlayer, model);
                            break;
                        case "bl_unpack":
                            usedSuccessfully = await ItemScript.BlackMoneyPack(dbPlayer, model, item, slot);
                            break;
                        case "jailtunnel":
                            if (Configurations.Configuration.Instance.JailescapeEnabled) usedSuccessfully = await ItemScript.JailTunnel(dbPlayer, model);
                            break;
                        case "computer":
                            usedSuccessfully = ItemScript.Computer(dbPlayer, model);
                            break;
                        case "sbenabletunnel":
                            usedSuccessfully = ItemScript.StaatsbankBlueprint(dbPlayer, model);
                            break;
                        case "sbdrill":
                            usedSuccessfully = await ItemScript.StaatsbankDrill(dbPlayer, model);
                            break;
                        case "sbhacking":
                            usedSuccessfully = await ItemScript.Hackingtool(dbPlayer, model);
                            break;
                        case "christmas":
                            usedSuccessfully = ItemScript.Present(dbPlayer, model);
                            break;
                        case "houserent":
                            usedSuccessfully = ItemScript.Houserent(dbPlayer, model);
                            break;
                        case "customdrug_meth":
                            usedSuccessfully = await ItemScript.CustomDrugMeth(dbPlayer, model);
                            break;
                        case "customdrug_weed":
                            usedSuccessfully = await ItemScript.CustomDrugWeed(dbPlayer, model);
                            break;
                        case "customdrug_jeff":
                            usedSuccessfully = await ItemScript.CustomDrugJeff(dbPlayer, model);
                            break;
                        case "customdrug_teflon":
                            usedSuccessfully = await ItemScript.CustomDrugTeflon(dbPlayer, model);
                            break;
                        case "cake_gb_kuche":
                            usedSuccessfully = await ItemScript.DivideCake(dbPlayer, model, CakeType.GeburtstagKuchen);
                            break;
                        case "cake_gb_torte":
                            usedSuccessfully = await ItemScript.DivideCake(dbPlayer, model, CakeType.GeburtstagTorte);
                            break;
                        case "cake_hz_kuchen":
                            usedSuccessfully = await ItemScript.DivideCake(dbPlayer, model, CakeType.HochzeitKuchen);
                            break;
                        case "cake_hz_torte":
                            usedSuccessfully = await ItemScript.DivideCake(dbPlayer, model, CakeType.HochzeitTorte);
                            break;
                        case "clothesbag":
                            usedSuccessfully = await ItemScript.ClothesBag(dbPlayer, model, item);
                            break;
                        case "packedclothesback":
                            usedSuccessfully = await ItemScript.PackedClothesBag(dbPlayer, model, item);
                            break;
                        case "scooter":
                            usedSuccessfully = await ItemScript.Scooter(dbPlayer, model);
                            break;
                        case "sellhousescript":
                            usedSuccessfully = await ItemScript.SellHouseScript(dbPlayer);
                            break;
                        case "cig":
                            usedSuccessfully = await ItemScript.SmokeCigarrette(dbPlayer);
                            break;
                        case "combatshield":
                            usedSuccessfully = await ItemScript.Combatshield(dbPlayer);
                            break;
                        case "sg_laundryback":
                            usedSuccessfully = true;
                            await ItemScript.LaundryTakeOut(dbPlayer, model, item);
                            break;
                        case "campingset":
                            usedSuccessfully = await ItemScript.CampingSet(dbPlayer);
                            break;
                        case "fire":
                            usedSuccessfully = await ItemScript.Fire(dbPlayer);
                            break;
                        case "fishing":
                            usedSuccessfully = await ItemScript.Fishing(dbPlayer);
                            break;
                        case "ntester":
                            usedSuccessfully = await ItemScript.NutritionTest(dbPlayer);
                            break;
                        case "medicnotime":
                            usedSuccessfully = await ItemScript.MedicStationaer(dbPlayer);
                            break;
                        case "flic_car":
                            usedSuccessfully = ItemScript.LicPlagiatCar(dbPlayer);
                            break;
                        case "flic_airplaneA":
                            usedSuccessfully = ItemScript.LicPlagiatPlaneA(dbPlayer);
                            break;
                        case "flic_truck":
                            usedSuccessfully = ItemScript.LicPlagiatLKW(dbPlayer);
                            break;
                        case "flic_airplaneB":
                            usedSuccessfully = ItemScript.LicPlagiatPlaneB(dbPlayer);
                            break;
                        case "flic_boat":
                            usedSuccessfully = ItemScript.LicPlagiatBoot(dbPlayer);
                            break;
                        case "flic_motorcycle":
                            usedSuccessfully = ItemScript.LicPlagiatBike(dbPlayer);
                            break;
                        case "flic_pbs":
                            usedSuccessfully = ItemScript.LicPlagiatPBS(dbPlayer);
                            break;
                        case "flic_biz":
                            usedSuccessfully = ItemScript.LicPlagiatBiz(dbPlayer);
                            break;
                        case "flic_gun":
                            usedSuccessfully = ItemScript.LicPlagiatGun(dbPlayer);
                            break;
                        case "flic_hunting":
                            usedSuccessfully = ItemScript.LicPlagiatHunting(dbPlayer);
                            break;
                    }
                }

                Logging.Logger.LogToItemUsed(dbPlayer.Id, model.Id, usedSuccessfully);

                // Remove Item On Use
                if (model.RemoveOnUse && usedSuccessfully)
                {
                    if (!model.Script.StartsWith("ammo_") && !model.Script.StartsWith("bammo_"))
                    {
                        dbPlayer.Container.RemoveItemSlotFirst(model, slot);
                    }

                    NutritionItemModule.Instance.UseItem(dbPlayer, item.Id);

                    //Log if used
                    ItemModelModule.Instance.LogItem((int)model.Id, (int)dbPlayer.TeamId, 1);
                }

                dbPlayer.ActionsBlocked = false;
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return true;
            }

            return false;
        }

        [RemoteEvent]
        public async void VehicleClawEvent(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!Regex.IsMatch(returnString, @"^[a-zA-Z0-9\s]+$"))
            {
                dbPlayer.SendNewNotification("Nur Buchstaben, Zahlen und Leerzeichen sind erlaubt");
                return;
            }
            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
            if (sxVehicle == null)
            {
                return;
            }

            await ItemScript.DoVehicleClaw(dbPlayer, sxVehicle, dbPlayer.Team.Id == 1 && dbPlayer.IsInDuty(), returnString);
        }
        
        [RemoteEvent]
        public void CheckGuentherPassword(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!Regex.IsMatch(returnString, @"^[0-9]*$"))
            {
                dbPlayer.SendNewNotification("Nur Zahlen sind erlaubt");
                return;
            }
            if (!int.TryParse(returnString, out int userPass)) return;

            int password = Utils.GeneratePassword(dbPlayer);
            if(password != userPass)
            {
                dbPlayer.SendNewNotification("Falsches Passwort. Kein Zutritt für Dich!");
                return;
            }

            Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread();
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.Player.TriggerNewClient("loadguenther");
                dbPlayer.Player.SetPosition(GuentherModule.Inside);
                dbPlayer.Player.SetRotation(214.797f);
                await Task.Delay(1000);
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                GuentherModule.DbPlayers.Add(dbPlayer);
                dbPlayer.Container.RemoveItem(GuentherModule.GuentherClubCardId, 1);
            });
        }
    }
}
