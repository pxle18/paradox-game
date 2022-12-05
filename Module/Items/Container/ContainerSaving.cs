using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMP_CNR.Module.Items
{
    public static class ContainerSaving
    {
        public static string GetSlotSaveQuery(this Container container, int slotId)
        {
            var savingDic = container.ConvertToSaving();
            if (!savingDic.ContainsKey(slotId)) return "";

            string SaveQuery = $"UPDATE `{container.GetTableName()}` SET `slot_{slotId}` = '{NAPI.Util.ToJson(savingDic[slotId])}'";
            SaveQuery += $" WHERE `id` = '{container.Id}';";

            return SaveQuery;
        }

        public static string GetSlotSaveQueryForVehicleBackup(this Container container, int slotId)
        {
            var savingDic = container.ConvertToSaving();
            if (!savingDic.ContainsKey(slotId)) return "";

            string SaveQuery = $"UPDATE `{container.GetTableName()}` SET `slot_{slotId}` = '{NAPI.Util.ToJson(savingDic[slotId])}'";
            SaveQuery += $" WHERE `id` = '{container.Id}';";

            return SaveQuery;
        }

        public static void SaveMetaData(this Container container)
        {

            string SaveQuery = $"UPDATE `{container.GetTableName()}` SET `max_weight` = '{container.MaxWeight}'";
            SaveQuery += $" WHERE `id` = '{container.Id}';";
            SaveQuery += $"UPDATE `{container.GetTableName()}` SET `max_slots` = '{container.MaxSlots}'";
            SaveQuery += $" WHERE `id` = '{container.Id}'";

            MySQLHandler.ExecuteAsync(SaveQuery, Sync.MySqlSyncThread.MysqlQueueTypes.Inventory);
        }

        public static void SaveMaxWeight(this Container container)
        {
            string SaveQuery = $"UPDATE `{container.GetTableName()}` SET `max_weight` = '{container.MaxWeight}'";
            SaveQuery += $" WHERE `id` = '{container.Id}';";

            MySQLHandler.ExecuteAsync(SaveQuery, Sync.MySqlSyncThread.MysqlQueueTypes.Inventory);
        }

        public static void SaveMaxSlots(this Container container)
        {
            string SaveQuery = $"UPDATE `{container.GetTableName()}` SET `max_slots` = '{container.MaxSlots}'";
            SaveQuery += $" WHERE `id` = '{container.Id}'";

            MySQLHandler.ExecuteAsync(SaveQuery, Sync.MySqlSyncThread.MysqlQueueTypes.Inventory);
        }

        public static Dictionary<int, List<SaveItem>> ConvertToSaving(this Container container)
        {
            Dictionary<int, List<SaveItem>> saveItems = new Dictionary<int, List<SaveItem>>();
            try
            {

                if (container == null) return saveItems;

                foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
                {
                    if (kvp.Value == null) continue;
                    if (!saveItems.ContainsKey(kvp.Key)) saveItems.Add(kvp.Key, new List<SaveItem>());
                    saveItems[kvp.Key].Add(new SaveItem(kvp.Value.Id, kvp.Value.Durability, kvp.Value.Amount, kvp.Value.Data));
                } // TODO Specials

                // Verify keys
                for (int i = 0; i < container.Slots.Count; i++)
                {
                    if (!saveItems.ContainsKey(i)) saveItems.Add(i, new List<SaveItem>());
                }
                
            }
            catch(Exception ex)
            {
                Logging.Logger.Crash(ex);
            }
            return saveItems;
        }
    }
}
