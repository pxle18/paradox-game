using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.ClawModule;
using VMP_CNR.Module.Computer.Apps.FahrzeuguebersichtApp;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.VehicleClawUebersichtApp
{
    public class VehicleClawUebersichtFunctions
    {

        public static List<Claw> GetVehicleClawByIdOrName(DbPlayer dbPlayer, Apps.VehicleClawUebersichtApp.SearchType searchType, String information)
        {
            List<Claw> overviewVehicles = new List<Claw>();
            switch (searchType)
            {

                case Apps.VehicleClawUebersichtApp.SearchType.PLAYERNAME:
                    overviewVehicles = ClawModule.ClawModule.Instance.GetAll().Values.Where(c => c.PlayerName.ToLower().Contains(information.ToLower())).ToList();
                    break;
                case Apps.VehicleClawUebersichtApp.SearchType.VEHICLEID:
                    uint vehicleId = Convert.ToUInt32(information);
                    overviewVehicles = ClawModule.ClawModule.Instance.GetAll().Values.Where(c => c.VehicleId == vehicleId).ToList();
                    break;
            }

            return overviewVehicles;
        }
    }
}
