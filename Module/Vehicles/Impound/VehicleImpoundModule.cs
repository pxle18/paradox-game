using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Vehicles.Impound
{
    public sealed class VehicleImpoundModule : Module<VehicleImpoundModule>
    {
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            if (key != Key.E) return false;

            if (!dbPlayer.CanAccessMethod()) return false;
            if (dbPlayer.TeamId != (int) TeamTypes.TEAM_DPOS || !dbPlayer.IsInDuty()) return false;

            if (
                dbPlayer.Player.Position.DistanceTo(new Vector3(-434.009,6136,31.478)) < 10.0f ||
                dbPlayer.Player.Position.DistanceTo(new Vector3(1674.38,3823.05,34.342)) < 10.0f ||
                dbPlayer.Player.Position.DistanceTo(new Vector3(714.628,- 1383.5,26.229)) < 10.0f ||
                dbPlayer.Player.Position.DistanceTo(new Vector3(-793.599,-1501.04, -0.090427)) < 10.0f ||
                dbPlayer.Player.Position.DistanceTo(new Vector3(-3156.39, 1131.1, 20.8485)) < 10.0f ||
                dbPlayer.Player.Position.DistanceTo(new Vector3(2904.5,4383.5,50.2662)) < 10.0f ||
                dbPlayer.Player.Position.DistanceTo(new Vector3(400.954,-1632.14,29.292)) < 10.0f ||
                dbPlayer.Player.Position.DistanceTo(new Vector3(-1610.51,-818.22,9.89718)) < 10.0f 
               )
            {
                foreach (var Vehicle in VehicleHandler.Instance.GetAllVehicles())
                {
                    if (Vehicle == null || Vehicle.teamid == (int) TeamTypes.TEAM_DPOS) continue;
                    if (dbPlayer.Player.Position.DistanceTo(Vehicle.Entity.Position) < 5.0f)
                    {
                        dbPlayer.Player.SetData("impound_vehicle", Vehicle);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Beschlagnahmungszeit", Callback = "SetVehicleImpoundTime", Message = "Gib die Zeit der Beschlagnahmung in Minuten ein." });
                        return true;
                    }
                }
            }
            return false;



        }
    }
}
