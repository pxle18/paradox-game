using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool telefonguthaben(DbPlayer dbPlayer, ItemModel ItemData)
        {
            // Guthaben
            if (dbPlayer.guthaben[0] >= 900)
            {
                dbPlayer.SendNewNotification(
                    
                    "Sie haben das maximale Limit an Guthaben erreicht!");
                return false;
            }

            dbPlayer.guthaben[0] = dbPlayer.guthaben[0] + 100;
            dbPlayer.SendNewNotification("Sie haben $100 Guthaben verwendet!");
            return true;
        }
    }
}