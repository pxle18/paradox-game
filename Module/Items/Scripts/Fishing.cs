using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Freiberuf.Fishing;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Fishing(DbPlayer dbPlayer)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            await NAPI.Task.WaitForMainThread(0);

            if (dbPlayer.HasData("fishing"))
            {
                if (dbPlayer.Container.GetItemAmount(FishingModule.FishingRoItemId) <= 0) return false;

                dbPlayer.StopFishing();
                dbPlayer.ResetData("fishing");
            } 
            else
            {
                dbPlayer.StartFishing();
                dbPlayer.SetData("fishing", true);
            }

            return true;
        }

    }
}
