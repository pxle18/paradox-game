using System;
using System.Globalization;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Geldboerse(DbPlayer dbPlayer, ItemModel ItemData)
        {
            int price = new Random().Next(100, 1100);
            dbPlayer.GiveMoney(price);
            dbPlayer.SendNewNotification("Geldboerse geoeffnet! $" + price);
            return true;
        }
    }
}