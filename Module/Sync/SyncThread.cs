using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Schwarzgeld;
using VMP_CNR.Module.Freiberuf.Mower;
using GTANetworkAPI;

namespace VMP_CNR.Module.Sync
{
    public class AsyncThread
    {
        private static Thread m_AsyncThread;
        private static ConcurrentQueue<Task> m_AsyncTasks = new ConcurrentQueue<Task>();

        public AsyncThread()
        {
            m_AsyncThread = new Thread(new ThreadStart(StartAsyncThread));
            m_AsyncThread.Start();
        }

        private void StartAsyncThread()
        {
            m_AsyncThread.IsBackground = true;
            m_AsyncThread.Priority = ThreadPriority.BelowNormal;
            
            AppDomain l_CurrentDomain = AppDomain.CurrentDomain;
            l_CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);
            
            while(true)
            {
                try
                {
                    if (m_AsyncTasks.TryDequeue(out Task l_Task))
                    {
                        l_Task.RunSynchronously();
                    }
                    else
                        Thread.Sleep(100);
                }
                catch(Exception e)
                {
                    Logger.Print(e.ToString());
                }
            }
        }

        private void ExceptionHandler(object p_Sender, UnhandledExceptionEventArgs p_Args) => Logger.Crash((Exception)p_Args.ExceptionObject);
        
        public void AddToAsyncThread(Task l_Task) => m_AsyncTasks.Enqueue(l_Task);
    }

    public class SyncThread
    {
        private static SyncThread _instance;

        public static SyncThread Instance => _instance ?? (_instance = new SyncThread());

        public static DateTime LastSyncMinuteCheck = DateTime.Now;
        
        public class VehicleWorker
        {
            public static void UpdateVehicleData()
            {
                if (!ServerFeatures.IsActive("vehicles-cleanup"))
                    return;

                foreach (SxVehicle sxVeh in VehicleHandler.Instance.GetAllVehicles())
                {
                    if (sxVeh == null || sxVeh.entity == null) continue;

                    var occupants = sxVeh.GetOccupants();

                    /* Keine Ahnung ob ich das richtig verstanden habe, aber:
                     * respawnInteractionState wird bei Motor an etc auf true gesetzt
                     * Wenn das Fahrzeug leer ist, nicht aufgeladen und der respawnInteractionState auf true ist, soll der respawnInteractionState auf false gesetzt werden
                     * Wenn der InteractionState 180 Minuten (je nachdem, siehe Code) auf false ist, das Fahrzeug leer und nicht aufgeladen ist, wird das Fahrzeug eingeparkt
                     * Richtig so? I hope.
                     */
                    if (!sxVeh.respawnInteractionState && occupants.IsEmpty() && !sxVeh.entity.HasData("isLoaded"))
                    {
                        sxVeh.respawnInterval++;
                        if ((sxVeh.respawnInterval >= 180 && sxVeh.IsPlayerVehicle()) /*|| (sxVeh.respawnInterval >= 300 && sxVeh.IsTeamVehicle())*/ || (sxVeh.respawnInterval >= 10 && sxVeh.jobid > 0))
                        {
                            CheckDeletion(sxVeh);
                        }
                    }
                    else if (sxVeh.respawnInteractionState && occupants.IsEmpty() && !sxVeh.entity.HasData("isLoaded"))
                    {
                        sxVeh.respawnInterval = 0;
                        sxVeh.respawnInteractionState = false;
                    }

                    if (sxVeh.SyncExtension.EngineOn)
                    {
                        var driver = occupants.GetDriver();
                        if (driver != null)
                        {
                            sxVeh.fuel -= sxVeh.Data.FuelConsumption / 100.0;
                            if (sxVeh.fuel <= 0)
                            {
                                sxVeh.fuel = 0;
                                sxVeh.SyncExtension.SetEngineStatus(false);
                            }
                        }
                    }
                }
            }

            private static void CheckDeletion(SxVehicle sxVeh)
            {
                try
                {
                    if (sxVeh.IsPlayerVehicle())
                        sxVeh.SetPrivateCarGarage(1);
                    else if (sxVeh.IsTeamVehicle())
                        sxVeh.SetTeamCarGarage(true);
                    else
                        VehicleHandler.Instance.DeleteVehicle(sxVeh);
                }
                catch (Exception e)
                {
                    Console.WriteLine(@"Failure in RemoveVehiclesSync: " + e.Message);
                }
            }
            
            public static void AntiFlightSystemIsland()
            {
                NAPI.Task.Run(() =>
                {
                    foreach (var vehicle in VehicleHandler.Instance.GetAllVehicles())
                    {
                        if (!vehicle.IsInAntiFlight()) continue;

                        vehicle.entity.Locked = true;
                        
                        if(vehicle.TrunkStateOpen)
                        {
                            vehicle.TrunkStateOpen = false;
                        }

                        vehicle.SyncExtension.SetEngineStatus(false);
                        vehicle.entity.Locked = true;
                        vehicle.entity.SetData("EMPWarning", 0);
                        vehicle.entity.ResetData("EMPWarning");

                        foreach (var dbPlayer in vehicle.GetOccupants().GetLegacyDictionary().Values)
                        {
                            if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                            dbPlayer.SendNewNotification("FLUGABWEHR - EMP | SIE BETRETEN EINE SPERRZONE!", notificationType: PlayerNotification.NotificationType.ERROR);
                        }
                    }
                });
            }
        }

        private class SystemMinWorkers
        {
            public static void CheckMin()
            {
                if (LastSyncMinuteCheck.AddSeconds(50) <= DateTime.Now)
                {
                    LastSyncMinuteCheck = DateTime.Now;

                    try
                    {
                        Modules.Instance.OnPlayerMinuteUpdate();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    try
                    {
                        Modules.Instance.OnMinuteUpdate();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }
                }
            }

            public static async Task CheckMinAsync()
            {
                try
                {
                    await Modules.Instance.OnMinuteUpdateAsync();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static void CheckTwoMin()
            {
                try
                {
                    Modules.Instance.OnTwoMinutesUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static void CheckTenSec()
            {
                try
                {
                    Modules.Instance.OnTenSecUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static async Task CheckTenSecAsync()
            {
                try
                {
                    await Modules.Instance.OnTenSecUpdateAsync();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static void CheckFiveSec()
            {
                try
                {
                    Modules.Instance.OnFiveSecUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }
            
            public static void CheckFiveMin()
            {
                try
                {
                    Modules.Instance.OnFiveMinuteUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static void CheckFifteenMin()
            {
                try
                {
                    Modules.Instance.OnFifteenMinuteUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }
        }

        public class PlayerWorker
        {
            private const int RpMultiplikator = 4;

            public static readonly Random Rnd = new Random();
            
            public static async Task ChangeDbPositions()
            {
                await NAPI.Task.WaitForMainThread(0);

                foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers())
                {
                    if (iPlayer == null || !iPlayer.IsValid() || iPlayer.Player == null) continue;

                    // Jumppoints unfreeze
                    if (iPlayer.FreezedUntil != null && iPlayer.FreezedUntil < DateTime.Now)
                    {
                        iPlayer.FreezedUntil = null;
                    }
                    if (!iPlayer.MetaData.SaveBlocked)
                    {
                        if (iPlayer.HasData("lastPosition"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("lastPosition");
                        }
                        else if (iPlayer.HasData("CamperEnterPos"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("CamperEnterPos");
                        }
                        else if (iPlayer.HasData("AirforceEnterPos"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("AirforceEnterPos");
                        }
                        else if (iPlayer.HasData("SubmarineEnterPos"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("SubmarineEnterPos");
                        }
                        else if (iPlayer.HasData("NSAEnterPos"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("NSAEnterPos");
                        }
                        else if (iPlayer.Player.Dimension == 0 && iPlayer.DimensionType[0] != DimensionType.House)
                        {
                            iPlayer.MetaData.Dimension = iPlayer.Player.Dimension;
                            iPlayer.MetaData.Heading = iPlayer.Player.Heading;
                            iPlayer.MetaData.Position = iPlayer.Player.Position;
                        }
                        iPlayer.MetaData.Armor = iPlayer.Player.Armor;
                        iPlayer.MetaData.Health = iPlayer.Player.Health;
                    }
                }
            }

            public static void DoAnimations()
            {
                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers())
                    {
                        if (iPlayer == null || !iPlayer.IsValid()) continue;
                        
                        if (!iPlayer.AnimationScenario.Active) continue;
                        if (!iPlayer.AnimationScenario.Repeat &&
                            iPlayer.AnimationScenario.StartTime.AddSeconds(iPlayer.AnimationScenario.Lifetime) <
                            DateTime.Now)
                        {
                            iPlayer.StopAnimation();
                        }

                        // to far from packed player
                        if (iPlayer.HasData("follow"))
                        {
                            DbPlayer followedPlayer = Players.Players.Instance.FindPlayer(iPlayer.GetData("follow"));
                            if (followedPlayer != null && followedPlayer.IsValid() && !followedPlayer.IsInjured() && !iPlayer.IsInjured() && !iPlayer.RageExtension.IsInVehicle)
                            {
                                if (followedPlayer.Player.Position.DistanceTo(iPlayer.Player.Position) > 10.0f)
                                {
                                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");
                                    iPlayer.Player.TriggerNewClient("freezePlayer", true);
                                    await NAPI.Task.WaitForMainThread(2000);
                                    iPlayer.Player.TriggerNewClient("freezePlayer", false);
                                    iPlayer.StopAnimation();
                                }
                            }
                        }
                    }
                }));
            }
        }


        public SyncThread()
        {
        }

        public static void Init()
        {
            _instance = new SyncThread();
        }

        public async Task Start()
        {
            /*
             * DO ColShapes
             * 4700 MS
             */
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        PlayerWorker.DoAnimations();
                        VehicleWorker.AntiFlightSystemIsland();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(4700);
                }
            }, TaskCreationOptions.LongRunning);


            /*
             * DO 
             * 5000 MS
             */
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SystemMinWorkers.CheckFiveSec();
                        await PlayerWorker.ChangeDbPositions();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(5000);
                }
            }, TaskCreationOptions.LongRunning);

            /*
             * DO 
             * 10000 MS
             */
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        await SystemMinWorkers.CheckTenSecAsync();
                        SystemMinWorkers.CheckTenSec();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(10000);
                }
            }, TaskCreationOptions.LongRunning);
            
            /*
             * UPDATING Server messages
             * 1 Minute
             */
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        await SystemMinWorkers.CheckMinAsync();
                        VehicleWorker.UpdateVehicleData();
                        await Main.OnUpdateHandler();
                        await Main.OnMinHandler();
                        SystemMinWorkers.CheckMin();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(60000);
                }
            }, TaskCreationOptions.LongRunning);

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SystemMinWorkers.CheckTwoMin();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(120000);
                }
            }, TaskCreationOptions.LongRunning);

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SystemMinWorkers.CheckFiveMin();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(300000);
                }
            }, TaskCreationOptions.LongRunning);


            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SchwarzgeldModule.Instance.SchwarzgeldContainerCheck();
                        MeertraeubelModul.Instance.UpdateMeertraubelContainer();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(300000);
                }
            }, TaskCreationOptions.LongRunning);

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SystemMinWorkers.CheckFifteenMin();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(900000);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}