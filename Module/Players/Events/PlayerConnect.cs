using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tasks;
using VMP_CNR.Handler.Webhook;

namespace VMP_CNR.Module.Players.Events
{
    public class PlayerConnect : Script
    {
        public static void OnPlayerConnected(Player player)
        {
            if (player == null) return;

            Logger.Debug("OnPlayerConnectEvent " + player.Name);

            // 🚀 WEBHOOK LOGGING - Player Connect
            try
            {
                VoidEventLogger.LogPlayerConnect(player);
            }
            catch (Exception ex)
            {
                Logger.Print($"[WEBHOOK] Fehler beim Loggen von PlayerConnect: {ex.Message}");
            }

            player.SetPosition(new Vector3(17.4809, 637.872, 210.595));

            //Unsichtbar, Freeze

            NAPI.Task.Run(() =>
            {
                player.Transparency = 0;
                player.Dimension = 1337; // There is no PlayerID at this point, so count it up
            });

            player.TriggerEvent("OnPlayerReady");

            if (!Configuration.Instance.IsServerOpen)
            {
                player.SendNotification("Server wird heruntergefahren");
                player.Kick();
                return;
            }

            player.SetData("loginStatusCheck", 1);
            player.TriggerNewClient("freezePlayer", true);

            SynchronizedTaskManager.Instance.Add(
                new PlayerLoginTask(player)
            );
        }
    }
}
