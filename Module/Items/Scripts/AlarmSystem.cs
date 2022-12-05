using GTANetworkMethods;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool AlarmSystem(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.RageExtension.IsInVehicle) return false;
            {
                if (dbPlayer.Team.Id == (int)teams.TEAM_LSC)
                {
                    var vehicle = dbPlayer.Player.Vehicle.GetVehicle();
                    if (vehicle.databaseId == 0) return false;
                    if (!vehicle.GpsTracker)
                    {
                        //Vehicle has no gps tracker
                        var table = vehicle.IsTeamVehicle() ? "fvehicles" : "vehicles";
                        MySQLHandler.ExecuteAsync($"UPDATE {table} SET alarm_system = 1 WHERE id = {vehicle.databaseId}");
                        vehicle.GpsTracker = true;
                        dbPlayer.SendNewNotification("Die Alarmanlage wurde eingebaut.");
                    }
                    else
                    {
                        //Vehicle already has gps tracker
                        dbPlayer.SendNewNotification("Dieses Fahrzeug ist bereits mit einer AlarmAnlage ausgestattet.");
                        return false;
                    }
                }

            }
            return false;
        }
    }
}