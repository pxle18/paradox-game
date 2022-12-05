using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Houserent(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if(dbPlayer.OwnHouse[0] > 0)
            {
                if (dbPlayer.HasData("houseId") && dbPlayer.GetData("houseId") == dbPlayer.OwnHouse[0])
                {
                    Menu.MenuManager.Instance.Build(Menu.PlayerMenu.HouseRentContract, dbPlayer).Show(dbPlayer);
                    dbPlayer.SendNewNotification("Sie stellen nun den Mietvertrag aus!");
                }
                else
                {
                    dbPlayer.SendNewNotification("Sie müssen an Ihrem Haus sein!");
                }
            }

            return false;
        }
    }
}