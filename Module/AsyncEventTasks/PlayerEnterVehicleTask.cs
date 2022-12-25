using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Einreiseamt;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.AsyncEventTasks
{
    public static partial class AsyncEventTasks
    {
        public static async Task PlayerEnterVehicleTask(Player player, Vehicle vehicle, sbyte seat)
        {
            try
            {
                // Fetch needed stuff from Main-Thread
                await NAPI.Task.WaitForMainThread();
                if (player == null || vehicle == null || player.IsInVehicle == false)
                    return;

                var playerPosition = player.Position;
                var playerHeading = player.Heading;
                var playerDimension = player.Dimension;

                if (vehicle == null)
                    return;

                var vehiclePosition = vehicle.Position;
                float newVehicleHealth = NAPI.Vehicle.GetVehicleEngineHealth(vehicle) + NAPI.Vehicle.GetVehicleBodyHealth(vehicle);
                var vehicleMaxSpeed = vehicle.MaxSpeed;
                bool vehicleLocked = vehicle.Locked;
                bool engineStatus = vehicle.EngineStatus;

                // Switch to Worker Thread
                await Task.Delay(10);
                DbPlayer iPlayer = player.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid() || vehicle == null)
                {
                    return;
                }

                iPlayer.SetData("Teleport", 3);

                if (iPlayer.Dimension[0] == 0)
                {
                    iPlayer.MetaData.Dimension = playerDimension;
                    iPlayer.MetaData.Heading = playerHeading;
                    iPlayer.MetaData.Position = playerPosition;
                }

                if ((iPlayer.HasPerso[0] == 0 || iPlayer.Level < 3) && (vehicle.Model == (uint)VehicleHash.Hydra || vehicle.Model == (uint)VehicleHash.Lazer || vehicle.Model == (uint)VehicleHash.Rhino ||
                    vehicle.Model == (uint)VehicleHash.Hunter || vehicle.Model == (uint)VehicleHash.Savage || vehicle.Model == (uint)VehicleHash.Buzzard))
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"DRINGENDER-Anticheat-Verdacht: {iPlayer.GetName()} (ARMY VEHICLE ENTERED)");
                    Logging.Logger.LogToAcDetections(iPlayer.Id, Logging.ACTypes.VehicleControlAbuse, $"ARMY VEHICLE ENTER");

                    Anticheat.AntiCheatModule.Instance.ACBanPlayer(iPlayer, "Army Vehicle Enter");
                    return;
                }

                Modules.Instance.OnPlayerEnterVehicle(iPlayer, vehicle, seat);

                // TODO: Reihenfolge fixen für setzen der Data und Abfragen der Data, sonst Fahrzeugsync komplett bruch
                /*if (!vehicle.HasData("serverhash") || vehicle.GetData<string>("serverhash") != "1312asdbncawssd1ccbSh1")
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("anticheat", $"ANTI CARHACK " + iPlayer.GetName());
                    vehicle.Delete();
                    return;
                }*/

                //ac stuff
                iPlayer.SetData("ac_lastPos", vehiclePosition);

                if (iPlayer.IsNewbie())
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"DRINGENDER-Anticheat-Verdacht: {iPlayer.GetName()} (ohne Einreiseamt - Fahrzeug betreten)");
                    Logging.Logger.LogToAcDetections(iPlayer.Id, Logging.ACTypes.EinreseVehicleEnter, $"");
                    iPlayer.WarpOutOfVehicle(true);
                }

                SxVehicle sxVeh = vehicle.GetVehicle();
                if (sxVeh == null || !sxVeh.IsValid())
                    return;

                if (sxVeh != null && sxVeh.IsValid() && sxVeh.Data != null && sxVeh.Data.MaxSpeed > 0)
                {
                    iPlayer.Player.TriggerNewClient("setNormalSpeed", sxVeh.Entity, sxVeh.Data.MaxSpeed);
                }

                // Respawnstate
                sxVeh.respawnInteractionState = true;

                if (sxVeh.jobid > 0)
                {
                    if (player.VehicleSeat == 0 && sxVeh.jobid != iPlayer.job[0] && sxVeh.jobid != 99 &&
                        sxVeh.jobid != 999 && sxVeh.jobid != -1)
                    {
                        if (sxVeh.jobid == 999 && (iPlayer.RankId == 0))
                        {
                            iPlayer.WarpOutOfVehicle();
                        }
                    }
                }

                VehicleHandler.Instance.AddPlayerToVehicleOccupants(sxVeh, player.GetPlayer(), seat);
                player.TriggerNewClient("initialVehicleData", sxVeh.fuel.ToString().Replace(",", "."), sxVeh.Data.Fuel.ToString().Replace(",", "."), newVehicleHealth.ToString().Replace(",", "."), VehicleHandler.MaxVehicleHealth.ToString().Replace(",", "."), vehicleMaxSpeed.ToString().Replace(",", "."), vehicleLocked ? "true" : "false", string.Format("{0:0.00}", sxVeh.Distance).Replace(",", "."), engineStatus ? "true" : "false");

                await Task.Delay(1000);// Workaround for locked vehs

                if (iPlayer == null || !iPlayer.IsValid() || sxVeh == null || !sxVeh.IsValid() || sxVeh.Entity == null)
                    return;

                // Resync Entity Lock & Engine Status
                if (sxVeh.SyncExtension != null)
                {
                    NAPI.Task.Run(() =>
                    {
                        NAPI.Vehicle.SetVehicleEngineStatus(sxVeh.Entity, sxVeh.SyncExtension.EngineOn);
                        NAPI.Vehicle.SetVehicleLocked(sxVeh.Entity, sxVeh.SyncExtension.Locked);
                        vehicleLocked = sxVeh.SyncExtension.Locked;
                    });

                    iPlayer.Player.TriggerNewClient("setPlayerVehicleMultiplier", sxVeh.DynamicMotorMultiplier);
                    sxVeh.Entity.GetVehicle().LastInteracted = DateTime.Now;

                    if (seat == 0)
                        sxVeh.LastDriver = iPlayer.GetName();

                    if (sxVeh != null && sxVeh.IsValid() && sxVeh.Data != null && sxVeh.Data.MaxSpeed > 0)
                    {
                        iPlayer.Player.TriggerNewClient("setNormalSpeed", sxVeh.Entity, sxVeh.Data.MaxSpeed);
                    }

                    await Task.Delay(1000);
                    if (vehicleLocked || sxVeh.SyncExtension.Locked || iPlayer.IsTied || iPlayer.IsCuffed)
                    {
                        if (iPlayer.HasData("vehicleData"))
                        {
                            iPlayer.WarpOutOfVehicle();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Logger.Print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Logging.Logger.Print("VEHICLE ENTER TASK - ASYNC - EXCEPTION!");
                Logging.Logger.Crash(e);
                Logging.Logger.Print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
        }
    }
}
