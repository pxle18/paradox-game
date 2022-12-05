using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.Items
{
    public class ItemModelModule : SqlModule<ItemModelModule, ItemModel, uint>
    {
        public ConcurrentDictionary<int, Dictionary<int, int>> LogItems = new ConcurrentDictionary<int, Dictionary<int, int>>();

        public void LogItem(int itemId, int teamId, int amount)
        {
            try
            {
                if (LogItems.ContainsKey(itemId))
                {
                    if (LogItems[itemId].ContainsKey(teamId))
                    {
                        LogItems[itemId][teamId] += amount;
                    }
                    else
                    {
                        LogItems[itemId].Add(teamId, amount);
                    }
                }
                else
                {
                    LogItems.TryAdd(itemId, new Dictionary<int, int>());
                    LogItems[itemId].Add(teamId, amount);
                }
            }
            catch(Exception e)
            {
                Logger.Crash(e);
            }
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `items_gd`;";
        }

        protected override void OnLoaded()
        {
            Logging.Logger.Debug($"Loaded {this.GetAll().Count} Items");
            base.OnLoaded();
        }

        public ItemModel GetItemByNameOrTag(string name)
        {
            try
            {
                // try parse itemId
                if (uint.TryParse(name, out uint itemId))
                {
                    if (Instance.GetAll().ContainsKey(itemId)) return Instance.Get(itemId);
                }
                
                // find by name
                if(Instance.GetAll().Where(item => item.Value.Name.ToLower().Contains(name.ToLower())).Count() > 0)
                {
                    return Instance.GetAll().First(item => item.Value.Name.ToLower().Contains(name)).Value;
                }

                // By script
                if (Instance.GetAll().Where(item => item.Value.Script.ToLower().Contains(name.ToLower())).Count() > 0)
                {
                    return Instance.GetAll().First(item => item.Value.Script.ToLower().Contains(name.ToLower())).Value;
                }
                
                return null;
            }
            catch (Exception e)
            {
                Logger.Print($"Failed GetItemByNameOrTag {e.Message}");
            }

            return null;
        }

        public ItemModel GetByType(ItemModelTypes type)
        {
            return Instance.GetAll().FirstOrDefault(i => i.Value.ItemModelType == type).Value;
        }

        public ItemModel GetById(uint id)
        {
            return Instance.GetAll().FirstOrDefault(i => i.Value.Id == id).Value;
        }

        public ItemModel GetByScript(string script)
        {
            var l_ItemDatas = Instance.GetAll();
            ItemModel l_Result = null;
            foreach (var l_Data in l_ItemDatas)
            {
                string l_Script = l_Data.Value.Script;
                if (!l_Data.Value.Script.Contains(script.ToLower()) && !string.Equals(l_Data.Value.Script, script.ToLower(),
                                    StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if(l_Data.Value != null) l_Result = l_Data.Value;
                break;
            }
                
            return l_Result;
        }
    }

    public static class ItemModelPlayerExtension
    {
        public static void SyncAttachmentOnlyItems(this DbPlayer dbPlayer)
        {
            if(dbPlayer.HasAttachmentOnlyItem() && !dbPlayer.RageExtension.IsInVehicle && !dbPlayer.IsCuffed && !dbPlayer.IsTied && dbPlayer.CanInteract())
            {
                Item xItem = dbPlayer.Container.GetAttachmentOnlyItem();
                if (xItem != null)
                {
                    Attachments.AttachmentModule.Instance.RemoveAllAttachments(dbPlayer);
                    Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, xItem.Model.AttachmentOnlyId, true);
                }
            }
        }

        public static bool HasAttachmentOnlyItem(this DbPlayer dbPlayer)
        {
            if(dbPlayer != null && dbPlayer.IsValid() && dbPlayer.Container != null)
            {
                Item xItem = dbPlayer.Container.GetAttachmentOnlyItem();
                if(xItem != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
