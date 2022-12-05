using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool VehicleContract(DbPlayer dbPlayer, Item item)
        {
            if (item.Data == null) return false;
            if (!item.Data.ContainsKey("Info")) return false;
            string info = (string)item.Data["Info"];

            dbPlayer.SendNewNotification(info, PlayerNotification.NotificationType.INFO, "Fahrzeugkaufvertrag", 15000);

            return false;
        } 
    }
}