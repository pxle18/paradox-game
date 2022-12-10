using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Events.Jahrmarkt.RCRacing
{
    public static class RCRacingPlayerModule
    {
        public static async Task SetPlayerIntoRCRacing(this DbPlayer dbPlayer)
        {
            // Spawn Vehicle & Player
            dbPlayer.SetDimension(dbPlayer.Id);
            dbPlayer.Dimension[0] = dbPlayer.Id;
            dbPlayer.DimensionType[0] = DimensionType.RCRacing;

            dbPlayer.Player.SetPosition(RCRacingModule.StartFinishPosition);

            var sxVehicle = VehicleHandler.Instance.CreateServerVehicle(RCRacingModule.RacingVehicleDataId, false,
                RCRacingModule.StartFinishPosition, 319.275f, Main.rndColor(),
                Main.rndColor(), dbPlayer.Id, true, true, false, 0, dbPlayer.GetName(), 0, 999, dbPlayer.Id);

            while (sxVehicle.Entity == null)
            {
                await NAPI.Task.WaitForMainThread(100);
            }

            if (sxVehicle != null && sxVehicle.Entity != null) dbPlayer.Player.SetIntoVehicleSave(sxVehicle.Entity, -1);

            RCRacingModule.Instance.RCRacingVehicles.Add(sxVehicle);

            if (!RCRacingModule.Instance.RCRacingPlayers.Contains(dbPlayer)) RCRacingModule.Instance.RCRacingPlayers.Add(dbPlayer);

            dbPlayer.SetData("racingRCRoundStartTime", DateTime.Now);
        }

        public static void RemoveFromRCRacing(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.Player.SetPosition(RCRacingModule.RCRacingMenuPosition);
            dbPlayer.SetDimension(0);
            dbPlayer.Dimension[0] = 0;
            dbPlayer.DimensionType[0] = DimensionType.World;

            dbPlayer.ResetData("rcRacingExitCheck");
            dbPlayer.ResetData("racingRCRoundStartTime");

            // Cleanup Vehicles

            // Remove Player
            if (RCRacingModule.Instance.RCRacingPlayers.Contains(dbPlayer)) RCRacingModule.Instance.RCRacingPlayers.Remove(dbPlayer);

            SxVehicle playerRacingVehicle = RCRacingModule.Instance.RCRacingVehicles.ToList().Where(rv => rv.ownerId == dbPlayer.Id).FirstOrDefault();
            if (playerRacingVehicle == null) return;

            RCRacingModule.Instance.RCRacingVehicles.Remove(playerRacingVehicle);
            VehicleHandler.Instance.DeleteVehicle(playerRacingVehicle);
        }
    }
}
