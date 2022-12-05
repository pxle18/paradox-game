using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Present(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.Container.GetInventoryFreeSpace() < 10000 || dbPlayer.Container.MaxSlots - dbPlayer.Container.GetUsedSlots() < 2)
            {
                dbPlayer.SendNewNotification("Du benoetigst mehr Platz in den Taschen! (30kg & 2 Plätze)");
                return false;
            }

            if(ItemData.Id == 1198) // großes Geschenk
            {
                dbPlayer.Container.AddItem(504); // 25% KFZ
                dbPlayer.Container.AddItem(552); // Teddy
                dbPlayer.GiveMoney(dbPlayer.Level * 10000);

                dbPlayer.SendNewNotification($"Du hast {dbPlayer.Level * 10000}$ erhalten!");
            }
            else
            {
                dbPlayer.SendNewNotification("Etwas ist gewaltig schief gelaufen...");
                return false;
            }
            dbPlayer.SendNewNotification("Du hast ein " + ItemData.Name + " geoeffnet");
            // RefreshInventory
            return true;
        }
    }
}