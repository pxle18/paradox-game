using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.Banks;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using static VMP_CNR.Module.Sync.SyncThread;

namespace VMP_CNR.Module.AsyncEventTasks
{
    public static partial class AsyncEventTasks
    {
        public static async Task PlayerExitVehicleTask(Player player, Vehicle handle)
        {
            // Main Thread Work
            await NAPI.Task.WaitForMainThread();
            var vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(handle);
            if (vehicle == null)
                return;

            var playerPosition = player.Position;
            var playerDimension = player.Dimension;
            var playerHeading = player.Heading;
            var vehiclePosition = vehicle.Position;

            // Back to Worker Thread
            await Task.Delay(5);
            //Todo: maybe save vehicle and player position here
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            // AC
            iPlayer.SetData("Teleport", 2);

            if (vehicle != null)
            {
                //ac stuff
                iPlayer.SetData("ac_lastPos", vehiclePosition);
            }

            if (iPlayer.Dimension[0] == 0)
            {
                iPlayer.MetaData.Dimension = playerDimension;
                iPlayer.MetaData.Heading = playerHeading;
                iPlayer.MetaData.Position = playerPosition;
            }

            if (iPlayer.PlayingAnimation)
                iPlayer.PlayingAnimation = false;
            
                
            if (iPlayer.HasData("paintCar"))
            {
                if (vehicle.HasData("color1") && vehicle.HasData("color2"))
                {
                    await NAPI.Task.WaitForMainThread();
                    int color1 = vehicle.GetData<int>("color1");
                    int color2 = vehicle.GetData<int>("color2");

                    await Task.Delay(5);
                    vehicle.PrimaryColor = color1;
                    vehicle.SecondaryColor = color2;
                    vehicle.ResetData("color1");
                    vehicle.ResetData("color2");
                    iPlayer.ResetData("p_color1");
                    iPlayer.ResetData("p_color2");
                }

                iPlayer.ResetData("paintCar");
            }

            if (vehicle != null)
            {
                SxVehicle sxVeh = vehicle.GetVehicle();
                if (sxVeh != null)
                {
                    // Respawnstate
                    sxVeh.respawnInteractionState = true;
                    sxVeh.DynamicMotorMultiplier = sxVeh.Data.Multiplier;

                    if (iPlayer != null && iPlayer.IsValid())
                    {
                        if (iPlayer.HasData("neonCar"))
                        {
                            if (sxVeh.neon != "")
                            {
                                sxVeh.LoadNeon();
                                iPlayer.ResetData("neonCar");
                            }
                        }

                        // ResetMods
                        if (iPlayer.HasData("hornCar")) iPlayer.ResetData("hornCar");
                        if(iPlayer.HasData("perlCar")) iPlayer.ResetData("perlCar");

                        // ResetMods
                        if (iPlayer.HasData("tuneIndex")) iPlayer.ResetData("tuneIndex");
                        if (iPlayer.HasData("tuneSlot")) iPlayer.ResetData("tuneSlot");
                        if (iPlayer.HasData("tuneVeh")) iPlayer.ResetData("tuneVeh");
                    }


                    if (sxVeh.Occupants.HasPlayerOccupant(iPlayer))
                        sxVeh.Occupants.RemovePlayer(iPlayer);
                }
            }
            
        }
    }
}
