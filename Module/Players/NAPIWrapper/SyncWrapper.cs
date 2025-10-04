using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.NAPIWrapper
{
    public class SyncWrapper : Module<SyncWrapper>
    {
        private static Thread SyncWrapperThread;

        protected override bool OnLoad()
        {
            // To be continue if RageEvent not really parse correctly
            SyncWrapperThread = new Thread(new ThreadStart(StartSyncWrapperThread));
            SyncWrapperThread.Start();

            return true;
        }

        public void OnPlayerVehicleStateChange(Player client, bool state)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.RageExtension.IsInVehicle = state;
        }
                
        private static void StartSyncWrapperThread()
        {
            SyncWrapperThread.IsBackground = true;
            SyncWrapperThread.Priority = ThreadPriority.BelowNormal;

            while (true)
            {
                try
                {
                    NAPI.Task.Run(() =>
                    {
                        foreach (DbPlayer dbPlayer in Players.Instance.GetValidPlayers())
                        {
                            if (dbPlayer == null || !dbPlayer.IsValid()) continue;

                            bool isInVehicleState = dbPlayer.Player.IsInVehicle;

                            if(isInVehicleState != dbPlayer.RageExtension.IsInVehicle)
                            {
                                dbPlayer.RageExtension.IsInVehicle = isInVehicleState;
                            }
                        }
                    });

                }
                catch (Exception e)
                {
                    Logger.Print(e.ToString());
                }

                Thread.Sleep(10000);
            }
        }
    }
}
