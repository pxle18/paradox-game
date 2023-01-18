using System.Threading.Tasks;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Chat
{
    public sealed class Chats
    {
        public const string MsgVehicleInfo = "[KFZ]: ";
        public const string MsgVehicleShop = "[Auto-Haendler]: ";
        public const string MsgAdvert = "[Werbung]: ";
        public const string MsgNews = "[News]: ";
        public const string MsgServerCc = "[Fahrzeugchat]: ";
        public const string MsgLeistelle = "[Leitstelle]: ";
        public const string MsgHandy = "[Handy]: ";
        public const string MsgBusiness = "[Business]: ";

        public enum ICON
        {
            GLOB,
            DEV,
            GOV,
            WED,
            CASINO
        }
        public enum COLOR
        {
            WHITE,
            ORANGE,
            LIGHTBLUE,
            RED,
            CHARTREUSE,
            LIGHTGREEN
        }
        public static async Task SendGlobalMessage(string message, COLOR color, ICON icon, int duration = 10000)
        {
            await Task.Run(() =>
            {
                  foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
                  {
                      dbPlayer.Player.TriggerNewClient("sendGlobalNotification", message, duration, color.ToString().ToLower(), icon.ToString().ToLower());
                  }
            });
        }
        public static async Task SendCayoMessage(string message, COLOR color, ICON icon, int duration = 10000)
        {
            await Task.Run(() =>
            {
                foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers())
                {
                    if (iPlayer.HasData("cayoPerico") || iPlayer.HasData("cayoPerico2"))
                    {
                        iPlayer.Player.TriggerEvent("sendGlobalNotification", message, duration, color.ToString().ToLower(), icon.ToString().ToLower());
                    }
                }
            });
        }

        public static void sendProgressBar(DbPlayer dbPlayer, int timeInMs, bool isCancellable = false)
        {
            dbPlayer.IsProgressBarRunning = true;
            dbPlayer.Player.TriggerNewClient("sendProgressbar", timeInMs);
        }

        public static void StopProgressbar(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsProgressBarRunning) return;

            dbPlayer.IsProgressBarRunning = false;
            dbPlayer.Player.TriggerNewClient("stopProgressbar");
        }
    }
}