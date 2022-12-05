using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public enum CakeType : uint
    {
        GeburtstagKuchen = 1,
        GeburtstagTorte,
        HochzeitKuchen,
        HochzeitTorte
    }
    
    public static partial class ItemScript
    {
        public static int DivideTime = 10000;

        public static async Task<bool> DivideCake(DbPlayer dbPlayer, ItemModel itemModel, CakeType cakeType)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            
            uint newItemID = 1059;
            int amount = 5;
            switch (cakeType)
            {
                case CakeType.GeburtstagKuchen:
                    break; // Default Wert entspricht dem Ziel-Wert in diesem Fall
                case CakeType.GeburtstagTorte:
                    newItemID = 1060;
                    break;
                case CakeType.HochzeitKuchen:
                    newItemID = 1062;
                    break;
                case CakeType.HochzeitTorte:
                    newItemID = 1061;
                    break;
                default:
                    break;
            }

            if (!dbPlayer.Container.CanInventoryItemAdded(newItemID, amount))
            {
                dbPlayer.SendNewNotification($"Du hast nicht genug Platz für die aufgeteilten Stücke!");
                return false;
            }
            
            Chats.sendProgressBar(dbPlayer, DivideTime);
            await Task.Delay(DivideTime);

            dbPlayer.Container.AddItem(newItemID, amount);
            return true;
        }
    }
}