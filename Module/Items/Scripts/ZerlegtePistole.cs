using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Camper;
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
        public static async Task<bool> ZerlegtePistole(DbPlayer dbPlayer, ItemModel ItemData)
        {

            if (dbPlayer.RageExtension.IsInVehicle) return false;
           
            string[] args = ItemData.Script.Split('_');
            if (!UInt32.TryParse(args[1], out uint newItemId)) return false;
            ItemModel newItem = ItemModelModule.Instance.Get(newItemId);
            if (newItem == null) return false;

            if(!dbPlayer.Container.CanInventoryItemAdded(newItem, 1))
            {
                dbPlayer.SendNewNotification("Du hast nicht genug platz im Inventar!");
                return false;
            }

            dbPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);

            Chats.sendProgressBar(dbPlayer, 20000);
            await NAPI.Task.WaitForMainThread(20000);

            dbPlayer.ResetData("userCannotInterrupt");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();

            dbPlayer.SendNewNotification($"Du hast {ItemData.Name} entpackt!");

            dbPlayer.Container.AddItem(newItemId, 1);
            return true;
        }
    }
}