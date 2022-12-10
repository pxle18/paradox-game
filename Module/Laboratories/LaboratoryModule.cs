using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Laboratories
{
    public class LaboratoryModule : Module<LaboratoryModule>
    {
        public static int TimeToImpound = 90000;
        public static int TimeToFrisk = 30000;
        public static int TimeToAnalyze = 30000;
        public static int TimeToBreakDoor = 600000;
        public static int TimeToHack = 60000;

        public static int RepairPrice = 1000000;

        public static int HoursDisablingAfterHackAttack = 6;
        protected override bool OnLoad()
        {
            // Frisk Menu
            MenuManager.Instance.AddBuilder(new LaboratoryOpenInvMenu());
            if (Configurations.Configuration.Instance.DevMode)
            {
                TimeToImpound = 3000;
                TimeToFrisk = 3000;
                TimeToAnalyze = 3000;
                TimeToBreakDoor = 3000;
                TimeToHack = 3000;
            }
            return true;
        }
        public bool IsImpoundVehicle(SxVehicle vehicle)
        {
            return vehicle.Entity.Model == (uint)VehicleHash.Brickade ||
                    vehicle.Entity.Model == (uint)VehicleHash.Burrito ||
                    vehicle.Entity.Model == (uint)VehicleHash.Burrito2 ||
                    vehicle.Entity.Model == (uint)VehicleHash.Burrito3 ||
                    vehicle.Entity.Model == (uint)VehicleHash.Burrito4 ||
                    vehicle.Entity.Model == (uint)VehicleHash.Burrito5 ||
                    vehicle.Entity.Model == (uint)VehicleHash.Gburrito ||
                    vehicle.Entity.Model == (uint)VehicleHash.Gburrito2 || vehicle.Data.Id == 1273 || // Fib Sprinter 
                    vehicle.Entity.Model == (int)VehicleHash.Benson;
        }
    }

    public class LaboratoryEvents : Script
    {
    }
}
