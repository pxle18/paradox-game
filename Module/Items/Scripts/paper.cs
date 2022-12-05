using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool paper(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.Container.GetItemAmount(8) < 1)
            {
                dbPlayer.SendNewNotification(
                    
                    "Ohne Grindedweed, kannst du keinen Joint bauen!");
                return false;
            }

            dbPlayer.Container.RemoveItem(8);
            dbPlayer.Container.AddItem(159);
            dbPlayer.SendNewNotification(
                
                "Du hast etwas Grindedweed in einem Paper zu einem Joint gedreht!");

            // RefreshInventory
            return true;
        }
    }
}