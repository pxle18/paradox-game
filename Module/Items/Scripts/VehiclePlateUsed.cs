using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> VehiclePlateUsed(DbPlayer dbPlayer, Item item)
        {
            if (item.Data == null) return false;
            if (!item.Data.ContainsKey("Plate")) return false;
            string plate = (string)item.Data["Plate"];

            //if player is in vehicle put on plate
            if (dbPlayer.RageExtension.IsInVehicle)
            {
                SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVehicle == null) return false;

                if (sxVehicle.IsPlayerVehicle() && sxVehicle.ownerId != dbPlayer.Id)
                {
                    dbPlayer.SendNewNotification("Nicht dein Fahrzeug!");
                    return false;
                }

                if (sxVehicle.IsTeamVehicle() && sxVehicle.teamid != dbPlayer.Team.Id)
                {
                    dbPlayer.SendNewNotification("Nicht dein Fahrzeug!");
                    return false;
                }

                if (sxVehicle.SyncExtension.EngineOn)
                {
                    dbPlayer.SendNewNotification("Der Motor des Fahrzeugs muss für diesen Vorgang ausgeschaltet sein.");
                    return false;
                }

                dbPlayer.SendNewNotification("Nummernschild wird angebracht...");
                dbPlayer.Container.RemoveItem(item.Model, 1);
                sxVehicle.CanInteract = false;
                dbPlayer.SetCannotInteract(true);
                Chats.sendProgressBar(dbPlayer, 60000);
                await GTANetworkAPI.NAPI.Task.WaitForMainThread(60000);
                dbPlayer.SendNewNotification("Sitzt, wackelt und kriegt Luft... " + plate);
                sxVehicle.Entity.NumberPlate = plate;
                dbPlayer.SetCannotInteract(false);
                sxVehicle.CanInteract = true;
                Logging.Logger.AddVehiclePlateLog(dbPlayer.Id, sxVehicle.databaseId, plate);

                return false; // false weil wird oben ja schon entfernt
            }
            else
            {
                //show numberplate
                dbPlayer.SendNewNotification(plate, PlayerNotification.NotificationType.INFO, "Kennzeichen");
                return false;
            }
        }
    }
}