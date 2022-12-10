using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> VehicleRepair(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            //Items.Instance.UseItem(ItemData.id, dbPlayer);
            var closestVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position, 3);
            if (closestVehicle != null)
            {
             
                if(closestVehicle.SyncExtension.EngineOn || closestVehicle.Entity.EngineStatus)
                {
                    dbPlayer.SendNewNotification("Der Motor muss zum reparieren ausgeschaltet sein!");
                    return false;
                }

                dbPlayer.PlayAnimation(
                    (int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mini@repair", "fixing_a_ped");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetData("userCannotInterrupt", true);

                Chats.sendProgressBar(dbPlayer, 20000);
                await Task.Delay(20000);

                dbPlayer.ResetData("userCannotInterrupt");
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();

                if (closestVehicle.SyncExtension.EngineOn || closestVehicle.Entity.EngineStatus)
                {
                    dbPlayer.SendNewNotification("Der Motor muss zum reparieren ausgeschaltet sein!");
                    return false;
                }

                await NAPI.Task.WaitForMainThread();
                var playerPosition = dbPlayer.Player.Position;
                var vehiclePosition = closestVehicle.Entity.Position;

                await Task.Delay(5);
                if (vehiclePosition.DistanceTo(playerPosition) > 10) 
                    return false;

                closestVehicle.Repair();
                return true;
            }

            return false;
        }
    }
}