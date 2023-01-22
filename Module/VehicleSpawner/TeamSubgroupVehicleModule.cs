using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.VehicleSpawner
{
    class TeamSubgroupVehicleModule : SqlModule<TeamSubgroupVehicleModule, TeamSubgroupVehicle, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemsModule), typeof(ItemModelModule), typeof(VehicleDataModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_subgroup_vehicles` WHERE !(`pos_x` = '0') AND `inGarage` = '0' AND `lastGarage` > 0;";
        }

        protected override void OnItemLoaded(TeamSubgroupVehicle ugVehicle)
        {
            var data = VehicleDataModule.Instance.GetDataById((uint)ugVehicle.Model);
            if (data == null) return;
            if (data.Disabled) return;

            SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(data.Id, ugVehicle.Registered,
                                ugVehicle.Position, ugVehicle.Rotation,
                                ugVehicle.Color1, ugVehicle.Color2, 0, ugVehicle.GpsTracker, true, true,
                                0, TeamModule.Instance.Get(ugVehicle.TeamSubgroupId).ShortName,
                                ugVehicle.Id, 0, 0, ugVehicle.Fuel,
                                VehicleHandler.MaxVehicleHealth, ugVehicle.Tuning, "", 0, ContainerManager.LoadContainer(ugVehicle.Id, ContainerTypes.UGVEHICLE), ugVehicle.Plate, false, false, ugVehicle.WheelClamp, ugVehicle.AlarmSystem, ugVehicle.lastGarage, false, ugVehicle.CarSellPrice,null, ugVehicle.TeamSubgroupId);
            Logging.Logger.Debug($"UGVEHICLE {ugVehicle.Model} {TeamModule.Instance.Get(ugVehicle.TeamSubgroupId).ShortName} loaded");
            xVeh.SetTeamSubgroupCarGarage(false);
        }
    }
}
