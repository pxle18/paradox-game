using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkMethods;
using VMP_CNR.Module.Logging;
using VMP_CNR.Handler;
using VMP_CNR.Module.Vehicles;
using System.Data.Common;

namespace VMP_CNR.Module.Items
{
    public enum ContainerMoveTypes
    {
        SelfInventory = 1,
        ExternInventory = 2,
    }

    public enum ContainerTempIds
    {
        Bank = 1,
        Juwe = 2,
    }

    public class Container
    {
        public uint Id { get; }
        public ContainerTypes Type { get; }
        public int MaxWeight { get; set; }
        public int MaxSlots { get; set; }
        public Dictionary<int, Item> Slots { get; }
        public bool IsUsed { get; }
        public Dictionary<int, DateTime> IntelligentContainerSaving { get; set; }

        public bool Locked { get; set; }
        public Container(MySqlDataReader reader)
        {
            try
            {
                Id = reader.GetUInt32("id");
                Type = (ContainerTypes)Enum.ToObject(typeof(ContainerTypes), reader.GetInt32("type"));
                MaxWeight = reader.GetInt32("max_weight");
                MaxSlots = reader.GetInt32("max_slots");
                IsUsed = false;
                Slots = new Dictionary<int, Item>();
                Locked = false;

                SaveItem dbItem = new SaveItem(0, 0, 0, null);
                int maxSlots = ContainerManager.GetMaxSlots(Type);
                
                for (int i = 0; i < maxSlots; i++)
                {
                    try
                    {
                        string columnName = "slot_" + i;
                        // Prüfen ob die Spalte existiert
                        if (HasColumn(reader, columnName))
                        {
                            string slotData = reader.GetString(columnName);
                            dbItem = (slotData == "" || slotData == "[]" ? new SaveItem(0, 0, 0, null) : JsonConvert.DeserializeObject<List<SaveItem>>(slotData).First()) ?? new SaveItem(0, 0, 0, null);
                        }
                        else
                        {
                            // Spalte existiert nicht, leeren Slot erstellen
                            dbItem = new SaveItem(0, 0, 0, null);
                        }
                        
                        if (dbItem.Id != 0 && !ItemModelModule.Instance.GetAll().ContainsKey(dbItem.Id))
                            dbItem = new SaveItem(0, 0, 0, null);

                        Slots.Add(i, new Item(dbItem.Id, dbItem.Durability, dbItem.Amount, dbItem.Data));
                    }
                    catch (Exception ex)
                    {
                        // Fehler beim Laden des Slots - leeren Slot erstellen
                        Logger.Print($"Error loading slot {i} for container {Id}: {ex.Message}");
                        Slots.Add(i, new Item(0, 0, 0, null));
                    }
                }

                IntelligentContainerSaving = new Dictionary<int, DateTime>();
                ContainerManager.CheckStaticContainerInserting(this);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
        public Container(DbDataReader reader)
        {
            try
            {
                Id = reader.GetUInt32("id");
                Type = (ContainerTypes)Enum.ToObject(typeof(ContainerTypes), reader.GetInt32("type"));
                MaxWeight = reader.GetInt32("max_weight");
                MaxSlots = reader.GetInt32("max_slots");
                IsUsed = false;
                Slots = new Dictionary<int, Item>();
                Locked = false;

                SaveItem dbItem = new SaveItem(0, 0, 0, null);
                int maxSlots = ContainerManager.GetMaxSlots(Type);
                
                for (int i = 0; i < maxSlots; i++)
                {
                    try
                    {
                        string columnName = "slot_" + i;
                        // Prüfen ob die Spalte existiert
                        if (HasColumn(reader, columnName))
                        {
                            string slotData = reader.GetString(columnName);
                            dbItem = (slotData == "" || slotData == "[]" ? new SaveItem(0, 0, 0, null) : JsonConvert.DeserializeObject<List<SaveItem>>(slotData).First()) ?? new SaveItem(0, 0, 0, null);
                        }
                        else
                        {
                            // Spalte existiert nicht, leeren Slot erstellen
                            dbItem = new SaveItem(0, 0, 0, null);
                        }
                        
                        if (dbItem.Id != 0 && !ItemModelModule.Instance.GetAll().ContainsKey(dbItem.Id))
                            dbItem = new SaveItem(0, 0, 0, null);

                        Slots.Add(i, new Item(dbItem.Id, dbItem.Durability, dbItem.Amount, dbItem.Data));
                    }
                    catch (Exception ex)
                    {
                        // Fehler beim Laden des Slots - leeren Slot erstellen
                        Logger.Print($"Error loading slot {i} for container {Id}: {ex.Message}");
                        Slots.Add(i, new Item(0, 0, 0, null));
                    }
                }

                IntelligentContainerSaving = new Dictionary<int, DateTime>();
                ContainerManager.CheckStaticContainerInserting(this);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public Container(uint id, ContainerTypes type, int maxWeight, int maxSlots, Dictionary<int, Item> slots)
        {
            Id = id;
            Type = type;
            MaxWeight = maxWeight;
            MaxSlots = maxSlots;
            Slots = slots;

            IsUsed = false;
            Slots = new Dictionary<int, Item>();
            Locked = false;

            SaveItem dbItem = new SaveItem(0, 0, 0, null);
            for (int i = 0; i < ContainerManager.GetMaxSlots(type); i++)
            {
                Slots.Add(i, new Item(dbItem.Id, dbItem.Durability, dbItem.Amount, dbItem.Data));
            }

            IntelligentContainerSaving = new Dictionary<int, DateTime>();
            
            ContainerManager.CheckStaticContainerInserting(this);
        }
        
        private bool HasColumn(MySqlDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName) >= 0;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
        
        private bool HasColumn(DbDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName) >= 0;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
    }
}
