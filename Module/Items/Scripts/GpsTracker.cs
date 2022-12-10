using GTANetworkMethods;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool GpsTracker(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.RageExtension.IsInVehicle) return false;
            {
                if (dbPlayer.job[0] != (int) JobTypes.JOB_MECH)
                {
                    dbPlayer.SendNewNotification( GlobalMessages.Error.NoPermissions());
                    return false;
                }
                /*
                if (dbPlayer.jobskill[0] < 2500)
                {
                        dbPlayer.SendNewNotification(
                        
                        "Ihnen fehlt noch die notwendige Erfahrung! (2500)");
                        return false;
                }
                */
                var vehicle = dbPlayer.Player.Vehicle.GetVehicle();
                if (vehicle.databaseId == 0) return false;
                if (!vehicle.GpsTracker)
                {
                    //Vehicle has no gps tracker
                    var table = vehicle.IsTeamVehicle() ? "fvehicles" : "vehicles";
                    MySQLHandler.ExecuteAsync($"UPDATE {table} SET gps_tracker = 1 WHERE id = {vehicle.databaseId}");
                    vehicle.GpsTracker = true;
                    dbPlayer.SendNewNotification("Der GPS-Tracker wurde eingebaut.");
                    dbPlayer.JobSkillsIncrease(2);
                }
                else
                {
                    //Vehicle already has gps tracker
                    dbPlayer.SendNewNotification("Dieses Fahrzeug ist bereits mit einem GpsTracker ausgestattet.");
                    return false;
                }
                // RefreshInventory
                return true;
            }
        }
    }
}