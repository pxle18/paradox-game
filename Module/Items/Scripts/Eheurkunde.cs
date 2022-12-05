using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using static VMP_CNR.Module.Chat.Chats;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Eheurkunde(DbPlayer dbPlayer, Item item)
        {
            if (item.Data == null) return false;
            if (!item.Data.ContainsKey("Info")) return false;
            string info = (string) item.Data["Info"];

            dbPlayer.SendNewNotification(info, PlayerNotification.NotificationType.INFO, "Eheurkunde", 15000);

            return false;
        }

        public static async Task<bool> MarryAnnounce(DbPlayer dbPlayer, Item item)
        {
            if (item.Data == null) return false;
            if (!item.Data.ContainsKey("Info")) return false;
            string info = (string)item.Data["Info"];

            await Chats.SendGlobalMessage(info, COLOR.WHITE, ICON.WED, duration: 20000);

            foreach (var pPlayer in Players.Players.Instance.GetValidPlayers())
            {
                pPlayer.SendNewNotification("1337Allahuakbar$marry", duration: 20000);
            }

            return true;
        }
    }
}
