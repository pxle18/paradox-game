using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.DPOSnew
{
    public class DposModule : Module<DposModule>
    {
        // Catch Vehicles everywhere
        //
        // Cargobob => Fahrzeuge => HQ
        // 
        //391.222 -1620.54 29.2919 318.221 Stadt
        //1626.07 3787.31 34.7895 302.914 Sandy
        //-452.157 6107.88 30.6977 249.651 
        // Befehle /catch /drop
        //
        //

        [CommandPermission]
        [Command]
        public void Commandvehcatch(Player player)
        {
            try
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (!dbPlayer.IsValid()) return;
                if (dbPlayer.TeamId != (int)TeamTypes.TEAM_DPOS || !dbPlayer.IsInDuty()) return;
                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh == null) return;
                if (!sxVeh.Data.Model.Contains("Cargobob")) { dbPlayer.SendNewNotification("Bitte verwende einen Cargobob"); return; }
                var catchVeh = new SxVehicle();
                var whitelist = new List<uint> { 1, 2, 4, 5, 6, 7, 10 };
                foreach (var veh in VehicleHandler.Instance.GetClosestVehicles(sxVeh.Entity.Position,15))
                {
                    if (whitelist.Contains(veh.Data.ClassificationId) && veh.Occupants.IsEmpty())
                    {
                        catchVeh = veh;
                        break;
                    }
                }
            
                if (catchVeh.databaseId<=0) { dbPlayer.SendNewNotification("Kein geeignetes KFZ gefunden"); return; }
                ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject("Bergung KFZ", $"Fahrzeug: {catchVeh.Data.Model} - {catchVeh.databaseId}", "CatchConfirm", catchVeh.databaseId.ToString(), ""));
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }


        /*
        [CommandPermission]
        [Command]
        public void Commandflatbed(Player player, string commandParams)
        {
            return;
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                DbPlayer dbPlayer = player.GetPlayer();

                if (!dbPlayer.CanAccessMethod())
                {
                    dbPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                int color1 = 0;
                int color2 = 0;

                if (commandParams == "") return;
                var command = commandParams.Split(" ");

                if (command.Length >= 2) int.TryParse(command[1], out color1);
                if (command.Length == 3) int.TryParse(command[2], out color2);

                var data = uint.TryParse(command[0], out var id)
                    ? VehicleDataModule.Instance.GetDataById(id)
                    : VehicleDataModule.Instance.GetDataByName(command[0]);

                if (data == null) return;
                if (data.Disabled) return;

                NAPI.Task.Run(async () =>
                {
                    NetHandle myveh = VehicleHandler.Instance.CreateServerVehicle(
                    data.Id, true, player.Position,
                    player.Rotation.Z, color1, color2, dbPlayer.Player.Dimension, true, false, false, 0, dbPlayer.Player.Name,
                    0, 999, (uint)dbPlayer.Id, 200, 1000, "", "", 0, null, null, true).entity;

                    await NAPI.Task.WaitForMainThread(2000);

                    NAPI.Entity.AttachEntityToEntity(myveh, player.Vehicle, "chassis", new Vector3(), new Vector3());
                });

            }));

        }*/
    }

    public class DPOSEvents : Script
    {
        [RemoteEvent]
        public void CatchConfirm(Player p_Player, string vehId, string none, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer.TeamId != (int)TeamTypes.TEAM_DPOS || !dbPlayer.IsInDuty()) return;

            var Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(Convert.ToUInt32(vehId));
            if (dbPlayer == null || !dbPlayer.IsValid() || Vehicle == null)
            {
                return;
            }

            if (Vehicle.IsPlayerVehicle() && Vehicle.databaseId > 0)
            {
                Vehicle.SetPrivateCarGarage(1, 31);
                dbPlayer.SendNewNotification(
                    "Fahrzeug wurde geborgen (Provision 1000$)");
                dbPlayer.GiveMoney(1000);
            }
            else if (Vehicle.IsTeamVehicle())
            {
                Vehicle.SetTeamCarGarage(true);

                if (Vehicle.teamid != (int)TeamTypes.TEAM_DPOS)
                {
                    dbPlayer.SendNewNotification(
                        "Fahrzeug wurde geborgen! (Provision 1000$)");
                    dbPlayer.GiveMoney(1000);
                }
            }
            else
            {
                VehicleHandler.Instance.DeleteVehicleByEntity(Vehicle.Entity);
                dbPlayer.SendNewNotification("Fahrzeug wurde geborgen! (Provision 500$)");
                dbPlayer.GiveMoney(500);
            }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
    }

}