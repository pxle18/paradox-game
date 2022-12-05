using System;
using System.Globalization;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Bargeldkoffer(DbPlayer dbPlayer, ItemModel ItemData, Item Item)
        {
            if(Item.Data.ContainsKey("DateTime") && Item.Data.ContainsKey("Mins"))
            {
                DateTime dateTime = DateTime.ParseExact(Item.Data["DateTime"], "ddMMyyyy", CultureInfo.InvariantCulture);
                int min = Convert.ToInt32(Item.Data["Mins"]);
            }
            return false;
        }
    }
}