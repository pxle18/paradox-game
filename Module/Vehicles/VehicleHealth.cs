using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Vehicles
{
    public static class VehicleHealth
    {
        // repair kit id

        public static void Repair(this SxVehicle vehicle)
        {
            Task.Run(async() =>
            {
                await NAPI.Task.WaitForMainThread();
                vehicle.RepairState = true;
                vehicle.entity.Repair();

                //set Health to max
                vehicle.entity.Health = VehicleHandler.MaxVehicleHealth;

                // Resync max speed cap
                try
                {
                    await Task.Delay(500);

                    if (vehicle != null && vehicle.Data != null && vehicle.Data.MaxSpeed > 0)
                    {
                        vehicle.Occupants.TriggerEventForOccupants("setNormalSpeed", vehicle.entity, vehicle.Data.MaxSpeed);
                    }

                    await Task.Delay(1500);

                    if (vehicle != null && vehicle.Data != null && vehicle.Data.MaxSpeed > 0)
                    {
                        vehicle.Occupants.TriggerEventForOccupants("setNormalSpeed", vehicle.entity, vehicle.Data.MaxSpeed);
                    }
                }
                catch(Exception e)
                {
                    Logging.Logger.Crash(e);
                }
            });
        }

        public static void SetHealth(this SxVehicle vehicle, float health)
        {
            NAPI.Task.Run(() =>
            {
                vehicle.RepairState = true;
                vehicle.entity.Health = health;
            });
        }
    }
}