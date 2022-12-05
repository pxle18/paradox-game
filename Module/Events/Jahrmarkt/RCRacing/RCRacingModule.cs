using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Events.Jahrmarkt.RCRacing
{
    public class RCRacingModule : Module<RCRacingModule>
    {
        public static bool RacingDeactivated = false;

        public static Vector3 StartFinishPosition = new Vector3(-1644.18, -1125.41, 16.7334);
        public static Vector3 Checkpoint1 = new Vector3(-1621.82, -1098.84, 26.7969);
        public static Vector3 Checkpoint2 = new Vector3(-1615.08, -1056.97, 26.5586);
        public static Vector3 Checkpoint3 = new Vector3(-1630.38, -1075.13, 17.0111);
        public static Vector3 Checkpoint4 = new Vector3(-1676.76, -1115.34, 21.4174);
        public static Vector3 Checkpoint5 = new Vector3(-1677.27, -1158.81, 19.7199);

        public static Vector3 RCRacingMenuPosition = new Vector3(-1647.99, -1126.14, 18.3381);

        public static uint RacingVehicleDataId = 883;

        public List<SxVehicle> RCRacingVehicles = new List<SxVehicle>();
        public List<DbPlayer> RCRacingPlayers = new List<DbPlayer>();

        protected override bool OnLoad()
        {
            if (!JahrmarktModule.isActive) base.OnLoad();
            List<SxVehicle> RCRacingVehicles = new List<SxVehicle>();
            List<DbPlayer> RCRacingPlayers = new List<DbPlayer>();

            ColShape rcracingshape = Spawners.ColShapes.Create(StartFinishPosition, 20);
            rcracingshape.SetData("racingRCColshape", 1);

            rcracingshape = Spawners.ColShapes.Create(Checkpoint1, 2);
            rcracingshape.SetData("racingRCColshape", 2);

            rcracingshape = Spawners.ColShapes.Create(Checkpoint2, 2);
            rcracingshape.SetData("racingRCColshape", 3);

            rcracingshape = Spawners.ColShapes.Create(Checkpoint3, 2);
            rcracingshape.SetData("racingRCColshape", 4);

            rcracingshape = Spawners.ColShapes.Create(Checkpoint4, 2);
            rcracingshape.SetData("racingRCColshape", 5);

            rcracingshape = Spawners.ColShapes.Create(Checkpoint5, 2);
            rcracingshape.SetData("racingRCColshape", 6);

            return base.OnLoad();
        }

        public override void OnTenSecUpdate()
        {
            if (!JahrmarktModule.isActive) return;
            if (!ServerFeatures.IsActive("jahrmarkt-rc")) return;

            foreach (SxVehicle sxVehicle in RCRacingVehicles.ToList())
            {
                if (sxVehicle != null && sxVehicle.entity != null && sxVehicle.entity.IsSeatFree(0))
                {
                    if (sxVehicle.entity.HasData("racingRCLeaveCheck"))
                    {
                        RCRacingVehicles.Remove(sxVehicle);
                        VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                    }
                    else sxVehicle.entity.SetData("racingRCLeaveCheck", 1);
                }
            }

            foreach (DbPlayer dbPlayer in RCRacingPlayers.ToList())
            {
                if (dbPlayer == null || !dbPlayer.IsValid())
                {
                    if (RCRacingPlayers.Contains(dbPlayer)) RCRacingPlayers.Remove(dbPlayer);
                    continue;
                }

                if (!dbPlayer.RageExtension.IsInVehicle)
                {
                    if (dbPlayer.HasData("rcRacingExitCheck"))
                    {
                        dbPlayer.RemoveFromRCRacing();
                    }
                    else dbPlayer.SetData("rcRacingExitCheck", 1);
                    continue;
                }
                if (dbPlayer.Player.Position.Z <= 14.5)
                {
                    dbPlayer.RemoveFromRCRacing();
                }
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (!JahrmarktModule.isActive) return false;
            if (!ServerFeatures.IsActive("jahrmarkt-rc")) return false;

            if (dbPlayer.Dimension[0] == 0 && key == Key.E)
            {
                if (dbPlayer.Player.Position.DistanceTo(RCRacingMenuPosition) < 2.0f)
                {
                    if (Crime.CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes) > 0)
                    {
                        dbPlayer.SendNewNotification("Gesucht können Sie nicht an einem Rennen teilnehmen!");
                        return true;
                    }
                    ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"RC Jahrmarkt Race", $"Wollen sie dem Jahrmarkt Race beitreten? (Einmalige Kosten pro Beitritt: 500$)", "RcRacingConfirm", "", ""));
                    return true;
                }
            }

            return false;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!JahrmarktModule.isActive) return false;
            if (!ServerFeatures.IsActive("jahrmarkt-rc")) return false;
            if (!dbPlayer.RageExtension.IsInVehicle) return false;

            if (dbPlayer.DimensionType[0] == DimensionType.RCRacing && colShape.HasData("racingRCColshape") && colShapeState == ColShapeState.Enter)
            {
                if (colShape.GetData<int>("racingRCColshape") == 1) // Start - End Colshape
                {
                    if (dbPlayer.HasData("racingRCState") && dbPlayer.GetData("racingRCState") == 6 && dbPlayer.HasData("racingRCRoundStartTime")) // has state 4 and is at start shape
                    {
                        // Track Time
                        DateTime startTime = dbPlayer.GetData("racingRCRoundStartTime");
                        // get diff
                        int milsec = Convert.ToInt32(DateTime.Now.Subtract(startTime).Milliseconds);
                        int min = Convert.ToInt32(DateTime.Now.Subtract(startTime).Minutes);
                        int sec = Convert.ToInt32(DateTime.Now.Subtract(startTime).Seconds);
                        int totalmil = Convert.ToInt32(DateTime.Now.Subtract(startTime).TotalMilliseconds);
                        dbPlayer.SendNewNotification($"Rundenzeit: {min}:{sec} {milsec} ms!");

                        dbPlayer.SendNewNotification($"1337Allahuakbar$racinbbesttime", duration: 14000);

                        

                        if(totalmil <= 40000) // Gold 
                        {
                            dbPlayer.SendNewNotification($"Rundenbonus (Gold Zeit!) 32 Jahrmarkt Punkte!");
                            dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, 48);
                        }
                        else if(totalmil <= 45000)
                        {
                            dbPlayer.SendNewNotification($"Rundenbonus (Silber Zeit!) 28 Jahrmarkt Punkte!");
                            dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, 42);
                        }
                        else if (totalmil <= 50000)
                        {
                            dbPlayer.SendNewNotification($"Rundenbonus (Bronce Zeit!) 26 Jahrmarkt Punkte!");
                            dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, 39);
                        }
                        else
                        {
                            dbPlayer.SendNewNotification($"Rundenbonus 22 Jahrmarkt Punkte!");
                            dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, 33);
                        }

                    }

                    dbPlayer.SetData("racingRCState", 1);
                    dbPlayer.SetData("racingRCRoundStartTime", DateTime.Now);
                    dbPlayer.SendNewNotification("Rundenzeit wird nun gemessen...");
                    SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                    if (sxVeh != null && sxVeh.IsValid())
                    {
                        sxVeh.Repair();
                        sxVeh.fuel = sxVeh.Data.Fuel;
                    }
                }
                else
                {
                    if (dbPlayer.HasData("racingRCState"))
                    {
                        if (colShape.GetData<int>("racingRCColshape") - 1 != dbPlayer.GetData("racingRCState"))
                        {
                            dbPlayer.SetData("racingRCState", 1);
                        }
                        else dbPlayer.SetData("racingRCState", colShape.GetData<int>("racingRCColshape"));
                    }
                    else dbPlayer.SetData("racingRCState", 1);
                }

            }

            return false;
        }
    }

    public class RCRacingConfirm : Script
    {
        [RemoteEvent]
        public async void RcRacingConfirm(Player p_Player, string pb_map, string none, string key )
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            
            if(!dbPlayer.TakeMoney(500))
            {
                dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(1000));
                return;
            }

            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            await dbPlayer.SetPlayerIntoRCRacing();
        }
    }
}
