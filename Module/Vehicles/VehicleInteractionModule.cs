using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Vehicles
{
    public sealed class VehicleInteractionModule : Module<VehicleInteractionModule>
    {

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key == Key.L)
            {
                if (dbPlayer.RageExtension.IsInVehicle)
                {
                    if(!dbPlayer.HasData("doorId")) // disable door schmutz
                        new VehicleEventHandler().handleVehicleLockInside(dbPlayer.Player);
                }
                else
                {
                    SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);

                    if (sxVehicle == null || !sxVehicle.IsValid()) return false;

                    new VehicleEventHandler().handleVehicleLockOutside(dbPlayer.Player, sxVehicle.entity);
                }
            }
            else if (key == Key.K)
            {
                if (dbPlayer.RageExtension.IsInVehicle)
                {
                    new VehicleEventHandler().handleVehicleDoorInside(dbPlayer.Player, 5);
                }
                else
                {
                    SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);

                    if (sxVehicle == null || !sxVehicle.IsValid()) return false;

                    new VehicleEventHandler().handleVehicleDoorOutside(dbPlayer.Player, sxVehicle.entity, 5);
                }
            }






            return false;
        }


    }
}
