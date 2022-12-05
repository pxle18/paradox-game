using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Divebottle(DbPlayer dbPlayer, ItemModel itemModel)
        {
            int texture = int.Parse(itemModel.Script.Split("_")[1]);
            dbPlayer.SetClothes(8, 123, texture);

            dbPlayer.SendNewNotification("Taucherflasche angezogen");

            return true;
        }
    }
}