using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Blitzer;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool ChestUnpack(DbPlayer dbPlayer, ItemModel ItemData)
        {
            string itemScript = ItemData.Script;

            if(!uint.TryParse(itemScript.Split('_')[1], out uint itemModelId))
            {
                return false;
            }

            if(!int.TryParse(itemScript.Split('_')[2], out int itemAmount))
            {
                return false;
            }

            if (itemModelId == 40) // Schutzweste
            {
                switch (dbPlayer.TeamId)
                {
                    case (int)teams.TEAM_FIB:
                        itemModelId = 712;
                        break;
                    case (int)teams.TEAM_ARMY:
                        itemModelId = 722;
                        break;
                    case (int)teams.TEAM_POLICE:
                        itemModelId = 697;
                        break;
                    default:
                        break;
                }
            }

            ItemModel itemModel = ItemModelModule.Instance.Get(itemModelId);
            int addedWeight = itemModel.Weight * itemAmount;

            if((dbPlayer.Container.GetInventoryFreeSpace() + ItemData.Weight) < addedWeight)
            {
                dbPlayer.SendNewNotification("So viel kannst du nicht tragen!");
                return false;
            }

            dbPlayer.Container.AddItem(itemModel, itemAmount);
            return true;
        }
    }
}