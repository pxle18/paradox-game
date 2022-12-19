using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using GTANetworkMethods;
using VMP_CNR.Handler;
using VMP_CNR.Module.Asservatenkammer;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items.Scripts;
using VMP_CNR.Module.JobFactions.Mine;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Buffs;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.RemoteEvents;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Workstation;

namespace VMP_CNR.Module.Items
{
    public static class ContainerInventoryActions
    {
        public static async Task<bool> MoveItemToInventory(DbPlayer dbPlayer, ContainerMoveTypes containerMoveType, int sourceSlot, int destinationslot, int amount)
        {
            // Player Validationchecks
            if (!dbPlayer.CanUseAction() || !dbPlayer.CanAccessRemoteEvent()) return false;
            
            // External Container
            Container eXternContainer = await ItemsModule.Instance.findInventory(dbPlayer);
            if (eXternContainer == null || eXternContainer.Locked) return false;

            ItemModel model = containerMoveType == ContainerMoveTypes.SelfInventory ? dbPlayer.Container.GetModelOnSlot(sourceSlot) : eXternContainer.GetModelOnSlot(sourceSlot);

            // Return on wrong amount, wrong model or not found external container
            if (amount <= 0 || model == null) return false;

            // Anti Spam / Anti Dupe
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.ItemMove))
            {
                dbPlayer.SendNewNotification($"Du kannst nur 1x pro halbe Sekunde ein Item bewegen.", PlayerNotification.NotificationType.ERROR, "Fehler!");
                return false;
            }
            
            // From Self To Another
            if (containerMoveType == ContainerMoveTypes.SelfInventory)
            {
                if (!dbPlayer.Container.Slots.ContainsKey(sourceSlot) || dbPlayer.Container.Slots[sourceSlot] == null) return false;

                if (amount > dbPlayer.Container.Slots[sourceSlot].Amount) return false;

                // Rucksack
                if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null && !ItemScript.backpack(dbPlayer, model, true))
                {
                    dbPlayer.SendNewNotification( $"Der Rucksack konnte nicht entfernt werden!");
                    return false;
                }

                if (model.Id == 1134) // Waffengürtel
                {
                    if (dbPlayer.GetPlayerWeaponsWeight() > PlayerWeapon.MaxPlayerWeaponWeight)
                    {
                        dbPlayer.SendNewNotification($"Der Gürtel ist derzeit befüllt!");
                        return false;
                    }
                }

                // Geschenk
                if (model.Id == 1198) return false;


                // Ostereier
                if (dbPlayer.RankId <= 0 && (model.Id == 1035 || model.Id == 1036 || model.Id == 1034))
                {
                    return false;
                }

                //Prüfen ob gegenüber das Item in der Anzahl aufnehmen kann
                if (!eXternContainer.CanInventoryItemAdded(model, amount))
                {
                    dbPlayer.SendNewNotification("Das Inventar reicht nicht aus!");
                    return false;
                }

                if (eXternContainer.IsItemRestrictedForContainer(model, dbPlayer))
                {
                    dbPlayer.SendNewNotification("Dieses Item koennen sie hier nicht einlagern!");
                    return false;
                }
                
                if(eXternContainer.Type == ContainerTypes.WORKSTATIONFUEL || eXternContainer.Type == ContainerTypes.WORKSTATIONINPUT)
                {
                    Workstation.Workstation workstation = dbPlayer.GetWorkstation();
                    if(workstation != null)
                    {
                        if(eXternContainer.Type == ContainerTypes.WORKSTATIONFUEL && workstation.FuelItemId != model.Id)
                        {
                            dbPlayer.SendNewNotification("Dieses Item koennen sie hier nicht einlagern!");
                            return false;
                        }
                        
                        if (eXternContainer.Type == ContainerTypes.WORKSTATIONINPUT)
                        {
                            if (!workstation.SourceConvertItems.ContainsKey(model.Id))
                            {
                                dbPlayer.SendNewNotification("Dieses Item koennen sie hier nicht einlagern!");
                                return false;
                            }
                            
                            if ((eXternContainer.GetItemAmount(model.Id) + amount) > workstation.LimitedSourceSize)
                            {
                                dbPlayer.SendNewNotification($"Sie können hier maximal {workstation.LimitedSourceSize} {model.Name} einlagern!");
                                return false;
                            }
                        }
                    }
                }

                if (eXternContainer.Type == ContainerTypes.REFUND)
                {
                    dbPlayer.SendNewNotification("In das Erstattungsinventar können keine Gegenstände eingelagert werden.");
                    return false;
                }
                
                if (eXternContainer.Type == ContainerTypes.STATIC && eXternContainer.Id == (uint)StaticContainerTypes.ASERLSPD)
                {
                    dbPlayer.SendNewNotification("Hier kannst du keine Gegenstände verstauen");
                    return false;
                }
                
                MoveItemToAnotherContainer(dbPlayer.Container, eXternContainer, sourceSlot, destinationslot, amount);

                // Drug Infections
                if (model.CanDrugInfect()) dbPlayer.IncreasePlayerDrugInfection();

                if (eXternContainer != null)
                {
                    if (ServerFeatures.IsActive("itemlog"))
                    {
                        Logger.SaveToItemLog(dbPlayer.Id, dbPlayer.GetName(), model.Id, -amount, "" + eXternContainer.Type, (int) eXternContainer.Id);
                    }
                }
                if (eXternContainer != null && eXternContainer.Type == ContainerTypes.VEHICLE)
                {
                    SxVehicle sxVehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(eXternContainer.Id);

                    if(sxVehicle != null && !dbPlayer.VehicleKeys.ContainsKey(sxVehicle.databaseId) && sxVehicle.ownerId != dbPlayer.Id)
                    {
                        if (ServerFeatures.IsActive("itemlog"))
                        {
                            Logger.SaveToItemLogExploit(dbPlayer.Id, dbPlayer.GetName(), model.Id, -amount, "" + eXternContainer.Type, (int)eXternContainer.Id);
                        }
                    }
                }

                await dbPlayer.PlayInventoryInteractAnimation();

                ContainerManager.CheckFunkDisabling(dbPlayer.Container);
                return true;
            }

            // From Another to Self
            if (containerMoveType == ContainerMoveTypes.ExternInventory)
            {
                if (!eXternContainer.Slots.ContainsKey(sourceSlot) || eXternContainer.Slots[sourceSlot] == null) return false;
                if (amount > eXternContainer.Slots[sourceSlot].Amount) return false;

                // Teamshelter ohne rechte...
                if (eXternContainer.Type == ContainerTypes.SHELTER)
                {
                    if (!dbPlayer.TeamRankPermission.Inventory) return false;
                }

                //Prüfen ob gegenüber das Item in der Anzahl aufnehmen kann
                if (!dbPlayer.Container.CanInventoryItemAdded(model, amount))
                {
                    dbPlayer.SendNewNotification("Das Inventar reicht nicht aus!");
                    return false;
                }

                if (dbPlayer.Container.IsItemRestrictedForContainer(model)) // großes Geschenk
                {
                    dbPlayer.SendNewNotification("Dieses Item koennen Sie hier nicht entnehmen!");
                    return false;
                }

                if(AsservatenkammerModule.Instance.IsAserItem(model.Id))
                {
                    StaticContainer asserContainer = StaticContainerModule.Instance.Get(eXternContainer.Id);
                    if(asserContainer != null && asserContainer.Id == (uint)StaticContainerTypes.ASERLSPD)
                    {
                        if(asserContainer.Locked) // bei aufbruch offen deswegen abfrage hier...
                            return false;
                    }
                    else return false; // disablen wir erstmal bis andere Lösung
                }

                if (eXternContainer.IsItemRestrictedToTakeOut(model))
                {
                    dbPlayer.SendNewNotification("Dieses Item koennen Sie hier nicht entnehmen!");
                    return false;
                }

                if (eXternContainer.Type == ContainerTypes.MINEBASESTORAGE)
                {
                    if (!dbPlayer.TeamRankPermission.Inventory)
                    {
                        dbPlayer.SendNewNotification("Dieses Item koennen Sie hier nicht entnehmen!");
                        return false;
                    }
                }

                if (eXternContainer.Type == ContainerTypes.MINECONTAINERSCHMELZEOUTPUT)
                {
                    if (!dbPlayer.TeamRankPermission.Inventory)
                    {
                        dbPlayer.SendNewNotification("Dieses Item koennen Sie hier nicht entnehmen!");
                        return false;
                    }
                }

                if(eXternContainer.Type == ContainerTypes.VEHICLE || eXternContainer.Type == ContainerTypes.FVEHICLE)
                {
                    if (ServerFeatures.IsActive("ac-vehicleinventory"))
                    {
                        SxVehicle sxVehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(eXternContainer.Id);
                        if (sxVehicle != null && sxVehicle.IsValid())
                        {
                            float detectrangeCorrection = 1.0f;

                            if(sxVehicle.Data.ClassificationId == 5 || sxVehicle.Data.ClassificationId == 3 || sxVehicle.Data.ClassificationId == 8 || sxVehicle.Data.ClassificationId == 9)
                            {
                                detectrangeCorrection = 3.0f;
                            }

                            if (dbPlayer.Player.Position.Z + detectrangeCorrection < sxVehicle.Entity.Position.Z)
                            {
                                if (!dbPlayer.CanControl(sxVehicle))
                                {
                                    Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Anticheat-Verdacht: {dbPlayer.GetName()} (Fahrzeug Inventar Zugriff Z Koodrinate Differenz (evtl unter Fahrzeug TP)) gegeben.");

                                    Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.InventarZ, $"VId {sxVehicle.databaseId} Item: {model.Name}({model.Id})");
                                }
                            }
                        }
                    }
                }

                if(eXternContainer.Type == ContainerTypes.FVEHICLE)
                {
                    SxVehicle sxvehicle = null;
                    if (VehicleHandler.TeamVehicles.ContainsKey((uint)TeamTypes.TEAM_MINE1))
                        sxvehicle = VehicleHandler.TeamVehicles[(uint)TeamTypes.TEAM_MINE1].ToList().Where(v => v.databaseId == eXternContainer.Id).FirstOrDefault();

                    if (sxvehicle == null || !sxvehicle.IsValid())
                    {
                        if (VehicleHandler.TeamVehicles.ContainsKey((uint)TeamTypes.TEAM_MINE2))
                            sxvehicle = VehicleHandler.TeamVehicles[(uint)TeamTypes.TEAM_MINE2].ToList().Where(v => v.databaseId == eXternContainer.Id).FirstOrDefault();
                    }

                    // Found? Player NOT mine1 or mine2
                    if (sxvehicle != null && sxvehicle.IsValid())
                    {
                        if(model.Id == JobMineFactionModule.Instance.AluBarren || model.Id == JobMineFactionModule.Instance.Batterien
                            || model.Id == JobMineFactionModule.Instance.BronceBarren || model.Id == JobMineFactionModule.Instance.IronBarren)
                        {
                            dbPlayer.SendNewNotification("Dieses Item koennen Sie hier nicht entnehmen!");
                            return false;
                        }
                    }
                }
                
                if (eXternContainer.Type == ContainerTypes.STATIC)
                {
                    var temp = StaticContainerModule.Instance.Get(eXternContainer.Id);
                    if (temp != null)
                    {
                        if (temp.TeamId != 0 && !dbPlayer.TeamRankPermission.Inventory)
                        {
                            dbPlayer.SendNewNotification("Du hast nicht die Befugniss hier Gegenstände rauszunehmen!");
                            return false;
                        }
                    }
                }

                if(model.AttachmentOnlyId > 0 && dbPlayer.Container.GetAttachmentOnlyItem() != null)
                {
                    dbPlayer.SendNewNotification("Du trägst bereits etwas mit dir!");
                    return false;
                }

                MoveItemToAnotherContainer(eXternContainer, dbPlayer.Container, sourceSlot, destinationslot, amount);
                
                // Drug Infections
                if (model.CanDrugInfect()) dbPlayer.IncreasePlayerDrugInfection();

                if (ServerFeatures.IsActive("itemlog"))
                {
                    Logger.SaveToItemLog(dbPlayer.Id, dbPlayer.GetName(), model.Id, amount, "" + eXternContainer.Type, (int) eXternContainer.Id);
                }



                await dbPlayer.PlayInventoryInteractAnimation();

                return true;
            }
            return false;
        }

        public static async Task<bool> moveItemInInventory(DbPlayer dbPlayer, ContainerMoveTypes containerMoveType, int sourceSlot, int destinationslot, int amount)
        {
            // Player Validationchecks
            if (!dbPlayer.CanUseAction() || !dbPlayer.CanAccessRemoteEvent()) return false;

            // External Container
            Container eXternContainer = await ItemsModule.Instance.findInventory(dbPlayer);
            if (containerMoveType == ContainerMoveTypes.ExternInventory && (eXternContainer == null || eXternContainer.Locked)) return false;

            ItemModel model = containerMoveType == ContainerMoveTypes.SelfInventory ? dbPlayer.Container.GetModelOnSlot(sourceSlot) : eXternContainer.GetModelOnSlot(sourceSlot);

            // Return on wrong amount, wrong model or not found external container
            if (amount <= 0 || model == null) return false;

            // In Self Inventory
            if (containerMoveType == ContainerMoveTypes.SelfInventory)
            {
                if (!dbPlayer.Container.Slots.ContainsKey(sourceSlot) || dbPlayer.Container.Slots[sourceSlot] == null) return false;
                if (amount > dbPlayer.Container.Slots[sourceSlot].Amount) return false;
                
                // Rucksack
                if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null && !ItemScript.backpack(dbPlayer, model, true))
                {
                    dbPlayer.SendNewNotification( $"Der Rucksack konnte nicht verschoben werden!");
                    return false;
                }

                MoveItemInContainer(dbPlayer.Container, sourceSlot, destinationslot, amount);
                return true;
            }

            // From Another to Self
            if (containerMoveType == ContainerMoveTypes.ExternInventory)
            {
                if (eXternContainer.Type == ContainerTypes.STATIC && (eXternContainer.Id == 3 || eXternContainer.Id == 4))
                {
                    if (!dbPlayer.TeamRankPermission.Inventory) return false;
                }
                if (!eXternContainer.Slots.ContainsKey(sourceSlot) || eXternContainer.Slots[sourceSlot] == null) return false;
                if (amount > eXternContainer.Slots[sourceSlot].Amount) return false;

                MoveItemInContainer(eXternContainer, sourceSlot, destinationslot, amount);
                return true;
            }
            return false;
        }

        public static void FillInventorySlots(Container container)
        {
            try
            {
                List<Item> newContainerSort = new List<Item>();
                if (container == null || container.Slots == null)
                    return;

                var itrSource = container.Slots.Where(i => i.Value != null && i.Value.Id != 0 && i.Value.Model.MaximumStacksize > 1);
                if (itrSource == null)
                    return;

                foreach (KeyValuePair<int, Item> item in itrSource.ToList())
                {
                    if (item.Value == null)
                        continue;

                    // stackable
                    if (item.Value.Data.Count <= 0)
                    {
                        Item xItem = newContainerSort.Where(i => i.Id == item.Value.Id).FirstOrDefault();
                        if (xItem != null)
                        {
                            xItem.Amount += item.Value.Amount;
                        }
                        else newContainerSort.Add(item.Value);
                    }
                    else newContainerSort.Add(item.Value);

                    container.RemoveAllFromSlot(item.Key, true);
                }


                foreach (Item item in newContainerSort)
                {
                    container.AddItem(item.Id, item.Amount, item.Data, -1, true);
                }

                container.SaveAll();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static void MoveItemToAnotherContainer(Container sourceContainer, Container externContainer, int sourceSlot, int destinationSlot, int amount)
        {
            if (!sourceContainer.Slots.ContainsKey(sourceSlot) || sourceContainer.Slots[sourceSlot] == null) return;

            externContainer.AddItem(sourceContainer.Slots[sourceSlot].Model, amount, sourceContainer.Slots[sourceSlot].Data, destinationSlot);
            sourceContainer.RemoveItemSlotFirst(sourceContainer.Slots[sourceSlot].Model, sourceSlot, amount);
        }

        public static void MoveItemInContainer(Container container, int sourceSlot, int destinationSlot, int amount)
        {
            // new function
            container.MoveItemInContainer(container.Slots[sourceSlot].Model, amount, container.Slots[sourceSlot].Data, sourceSlot, destinationSlot);
        }
    }
}
