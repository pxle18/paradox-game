using System;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Players;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> BlackMoneyPack(DbPlayer dbPlayer, ItemModel ItemData, Item item, int slot)
        {
            if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;

            int amount = item.Amount;

            Chats.sendProgressBar(dbPlayer, 3000);

            // Remove
            dbPlayer.Container.RemoveAllFromSlot(slot);

            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            
            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

            dbPlayer.SetData("userCannotInterrupt", true);

            await Task.Delay(3000);

            dbPlayer.SetData("userCannotInterrupt", false);
            dbPlayer.StopAnimation();
           
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);

            dbPlayer.GiveBlackMoney(amount);
            dbPlayer.SendNewNotification($"Sie haben ${amount} Schwarzgeld entpackt!");
            return true;
        }
    }
}
