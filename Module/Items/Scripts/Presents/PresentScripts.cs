using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Items.Scripts.Presents;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool PresentScript(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.Container.GetInventoryFreeSpace() < 30000 || dbPlayer.Container.MaxSlots - dbPlayer.Container.GetUsedSlots() < 2)
            {
                dbPlayer.SendNewNotification("Du benoetigst mehr Platz in den Taschen! (30kg & 2 Platz)");
                return false;
            }

            List<Present> presents = PresentModule.Instance.GetByItemId(ItemData.Id);

            if(presents.Count > 0)
            {
                Present ResultPresent = presents[new Random().Next(presents.Count)];
                ItemModel Result = ResultPresent.ResultItem;

                dbPlayer.Container.AddItem(Result, ResultPresent.Amount);

                dbPlayer.SendNewNotification("Du hast ein " + ItemData.Name + " geoeffnet und " + Result.Name + " erhalten!");
                return true;
            }
            
            // RefreshInventory
            return false;
        }
    }
}
