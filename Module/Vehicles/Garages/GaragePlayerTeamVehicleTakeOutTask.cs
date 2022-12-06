using System;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tasks;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Vehicles.Garages
{
    public class GaragePlayerTeamVehicleTakeOutTask : SqlTask
    {
        private readonly uint vehicleId;
        private readonly DbPlayer dbPlayer;
        private readonly Garage garage;
        private readonly GarageSpawn spawnPosition;

        public GaragePlayerTeamVehicleTakeOutTask(Garage garage, uint vehicleId, DbPlayer dbPlayer, GarageSpawn spawnPosition)
        {
            this.vehicleId = vehicleId;
            this.dbPlayer = dbPlayer;
            this.garage = garage;
            this.spawnPosition = spawnPosition;
        }

        public override string GetQuery()
        {

            Teams.Team currTeam = dbPlayer.Team;

            // Wenn NSA Duty und IAA Garage ist...
            if (dbPlayer.IsNSADuty && garage.Teams.Contains((uint)TeamTypes.TEAM_IAA)) currTeam = TeamModule.Instance.Get((uint)TeamTypes.TEAM_IAA);

            return $"UPDATE `fvehicles` SET `inGarage` = '0', `lastGarage`='{garage.Id}' WHERE `inGarage` = '1' AND `id` = '{vehicleId}' AND `team` = '{currTeam.Id}';";
        }

        public override void OnFinished(int result)
        {
            if (result != 1) return;
            VehiclesModule.LoadServerTeamVehicle(garage, vehicleId, dbPlayer, spawnPosition);
        }
    }
}