using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Storage
{
    public class StorageRoom : Loadable<uint>
    {
        public uint Id { get; }
        public int Ausbaustufe { get; set; }
        public uint OwnerId { get; set; }
        public int Price { get; }
        public Vector3 Position { get; }
        public float Heading { get; }
        public bool Locked { get; set; }
        public Container Container { get; set; }
        public bool CocainLabor { get; set; }
        public bool MainFlagged { get; set; }

        public StorageRoom(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Ausbaustufe = reader.GetInt32("ausbaustufe");
            OwnerId = reader.GetUInt32("owner_id");
            Price = reader.GetInt32("price");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            CocainLabor = reader.GetInt32("cocain_labor") == 1;
            Locked = true;
            ColShape colShape = Spawners.ColShapes.Create(Position, 2.0f);
            colShape.SetData("storageRoomDataId", Id);
            MainFlagged = reader.GetInt32("main_storage") == 1;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public bool IsBuyable()
        {
            return OwnerId == 0;
        }

        public void SetOwnerTo(DbPlayer dbPlayer)
        {
            OwnerId = dbPlayer.Id;
            MySQLHandler.ExecuteAsync($"UPDATE storage_rooms SET `owner_id` = '{OwnerId}' WHERE id = '{Id}';");
        }

        public void UpdateMainFlaggToDb()
        {
            MySQLHandler.ExecuteAsync($"UPDATE storage_rooms SET `main_storage` = '{(MainFlagged ? "1":"0")}' WHERE id = '{Id}';");
        }

        public void RemoveOwner()
        {
            OwnerId = 0;
            MySQLHandler.ExecuteAsync($"UPDATE storage_rooms SET `owner_id` = '0' WHERE id = '{Id}';");
        }

        public void SetMainFlagg(DbPlayer dbPlayer)
        {
            StorageRoom actuallStorageMain = StorageRoomModule.Instance.GetAll().Where(sr => sr.Value.MainFlagged).FirstOrDefault().Value;

            if(actuallStorageMain == this)
            {
                dbPlayer.SendNewNotification($"Dieses Lager ist bereits als Hauptlager markiert!");
                return;
            }

            if(actuallStorageMain != null)
            {
                actuallStorageMain.MainFlagged = false;
                actuallStorageMain.UpdateMainFlaggToDb();
            }

            dbPlayer.SendNewNotification($"Dieses Lager ist nun als Hauptlager markiert!");
            MainFlagged = true;
            UpdateMainFlaggToDb();
            return;
        }

        public void Upgrade(DbPlayer dbPlayer)
        {
            if(Ausbaustufe == StorageRoomAusbaustufenModule.Instance.GetAll().Keys.Max())
            {
                dbPlayer.SendNewNotification("Maximale Stufe bereits erreicht!");
                return;
            }
            else
            {
                StorageRoomAusbaustufe storageRoomAusbaustufe = StorageRoomAusbaustufenModule.Instance.Get((uint)Ausbaustufe + 1);
                if (storageRoomAusbaustufe == null) return;
                if (HasRequiredItems(storageRoomAusbaustufe.RequiredItems))
                {
                    if(dbPlayer.TakeMoney(storageRoomAusbaustufe.RequiredMoney)) 
                    {
                        RemoveRequiredItems(storageRoomAusbaustufe.RequiredItems);
                        Ausbaustufe++;
                        MySQLHandler.ExecuteAsync($"UPDATE storage_rooms SET `ausbaustufe` = '{Ausbaustufe}' WHERE id = '{Id}';");

                        Container.ChangeWeight(storageRoomAusbaustufe.ToWeight);
                        Container.ChangeSlots(storageRoomAusbaustufe.ToSlots);
                        
                        dbPlayer.SendNewNotification("Die Lagerhalle wurde erfolgreich ausgebaut!", PlayerNotification.NotificationType.SUCCESS);
                    }
                    else
                    {
                        dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(storageRoomAusbaustufe.RequiredMoney));
                    }
                }
                else
                {
                    dbPlayer.SendNewNotification(RequiredItemString(storageRoomAusbaustufe.RequiredItems));
                }
            }
        }

        public void UpgradeCocain(DbPlayer dbPlayer)
        {
            if (CocainLabor)
            {
                dbPlayer.SendNewNotification("Kokainlabor bereits ausgebaut!");
                return;
            }
            else
            {
                string storageRoomAusbaustufeCocain = "300:75,312:10,310:25"; // 75 Eisen, 10 Zement, 25 Holzplanken
                if (HasRequiredItems(storageRoomAusbaustufeCocain))
                {
                    RemoveRequiredItems(storageRoomAusbaustufeCocain);
                    MySQLHandler.ExecuteAsync($"UPDATE storage_rooms SET `cocain_labor` = '1' WHERE id = '{Id}';");
                    CocainLabor = true;
                    dbPlayer.SendNewNotification("Kokainlabor erfolgreich ausgebaut!");
                    return;
                }
                else
                {
                    dbPlayer.SendNewNotification(RequiredItemString(storageRoomAusbaustufeCocain));
                }
            }
        }
        
        public bool HasRequiredItems(string requiredItemsCocain)
        {
            if (!string.IsNullOrEmpty(requiredItemsCocain))
            {
                var splittedItems = requiredItemsCocain.Split(',');
                foreach (var requiredItems in splittedItems)
                {
                    if (!requiredItems.Contains(":")) continue;
                    var split = requiredItems.Split(":");

                    if (!uint.TryParse(split[0], out var itemModelId)) continue;
                    if (!int.TryParse(split[1], out var amount)) continue;

                    if (Container.GetItemAmount(itemModelId) < amount)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool RemoveRequiredItems(string requiredItemsString)
        {
            if (!string.IsNullOrEmpty(requiredItemsString))
            {
                var splittedItems = requiredItemsString.Split(',');
                foreach (var requiredItems in splittedItems)
                {
                    if (!requiredItems.Contains(":")) continue;
                    var split = requiredItems.Split(":");

                    if (!uint.TryParse(split[0], out var itemModelId)) continue;
                    if (!int.TryParse(split[1], out var amount)) continue;

                    Container.RemoveItem(itemModelId, amount);
                }
            }
            return true;
        }

        public string RequiredItemString(string requiredItemsString)
        {
            var result = "Benoetigt: ";
            if (!string.IsNullOrEmpty(requiredItemsString))
            {
                var splittedItems = requiredItemsString.Split(',');
                foreach (var requiredItems in splittedItems)
                {
                    if (!requiredItems.Contains(":")) continue;
                    var split = requiredItems.Split(":");

                    if (!uint.TryParse(split[0], out var itemModelId)) continue;
                    if (!int.TryParse(split[1], out var amount)) continue;

                    result += "" + ItemModelModule.Instance.Get(itemModelId).Name + " (" + amount + "), ";
                }
            }
            return result;
        }
    }
}