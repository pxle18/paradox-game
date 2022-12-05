using System;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static int TuningRadius = 5;

        public static bool TuningParts(DbPlayer dbPlayer, ItemModel ItemData)
        {
            dbPlayer.SendNewNotification("Das Teil passt nicht mehr!", PlayerNotification.NotificationType.ERROR);
            return false;

            /* D E A K T I V I E R T
             * if (!(dbPlayer.TeamId == 0))
            {
                dbPlayer.SendNewNotification("Nur geschultes Personal vom LSC kann Tuningteile anbringen!");
                return false;
            }

            var playerPosition = dbPlayer.Player.Position;
            if (playerPosition.DistanceTo(new GTANetworkAPI.Vector3(733.606, -1083.12, 22.1689)) > TuningRadius ||
                playerPosition.DistanceTo(new GTANetworkAPI.Vector3(-212.341, -1326.57, 30.8904)) > TuningRadius ||
                playerPosition.DistanceTo(new GTANetworkAPI.Vector3(-1156.03, -2012.04, 13.1803)) > TuningRadius ||
                playerPosition.DistanceTo(new GTANetworkAPI.Vector3(-334.72, -135.036, 39.0096)) > TuningRadius ||
                playerPosition.DistanceTo(new GTANetworkAPI.Vector3(1178.52, 2639.42, 37.7538)) > TuningRadius)
            {
                dbPlayer.SendNewNotification("Sie müssen in einer Werkstatt sein.", PlayerNotification.NotificationType.ERROR, "Fehler");
                return false;
            }

            if (!Configurations.Configuration.Instance.TuningActive || !dbPlayer.IsValid()) return false;
            if (!dbPlayer.RageExtension.IsInVehicle || dbPlayer.Player.Vehicle == null) return false;

            // Get Closest Car
            SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVeh != null && sxVeh.IsValid() && (sxVeh.IsPlayerVehicle() || sxVeh.IsTeamVehicle()))
            {
                string[] parts = ItemData.Script.ToLower().Replace("tune_", "").Split('_');
                int modid = Convert.ToInt32(parts[0]);
            
                dbPlayer.SetData("tuneVeh", sxVeh.databaseId);
                dbPlayer.SetData("tuneSlot", modid);
                dbPlayer.SetData("tuneIndex", 0);
            
                dbPlayer.Player.TriggerNewClient("hideInventory");
                dbPlayer.watchDialog = 0;
                dbPlayer.ResetData("invType");
            
                MenuManager.Instance.Build(PlayerMenu.MechanicTune, dbPlayer).Show(dbPlayer);
                return true;
            }
            else
            {
                dbPlayer.SendNewNotification(
                     "Dieses Fahrzeug können Sie nicht tunen!");
                return false;
            }*/
        }
    }
}