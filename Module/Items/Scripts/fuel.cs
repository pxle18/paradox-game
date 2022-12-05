using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool fuel(DbPlayer dbPlayer, ItemModel ItemData)
        {
            var closestVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position, 3.0f);
            if (closestVehicle == null)
            {
                return false;
            }

            if (closestVehicle.SyncExtension.Locked)
            {
                dbPlayer.SendNewNotification( "Das Fahrzeug muss zum Tanken aufgeschlossen sein!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }
            if (closestVehicle.fuel < closestVehicle.Data.Fuel)
            {
                if (closestVehicle.Data.Fuel - closestVehicle.fuel <= 20)
                {
                    closestVehicle.fuel = closestVehicle.Data.Fuel;
                }
                else
                {
                    closestVehicle.fuel += 20.0;
                }
            }
            else
            {
                dbPlayer.SendNewNotification("Fahrzeug ist bereits voll.", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            dbPlayer.PlayAnimation(AnimationScenarioType.Animation,
                Main.AnimationList["fixing"].Split()[0],
                Main.AnimationList["fixing"].Split()[1], 4, false, AnimationLevels.UserUsing);

            // RefreshInventory
            return true;
        }
    }
}