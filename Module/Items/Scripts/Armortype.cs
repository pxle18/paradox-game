using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using GTANetworkMethods;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Armortype(DbPlayer dbPlayer, ItemModel itemModel, Item item)
        {
            try
            {
                if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;

                if (!int.TryParse(itemModel.Script.Split("_")[1], out int type)) return false;

                if ((!dbPlayer.IsCopPackGun() || !dbPlayer.IsInDuty()) && type > 0)
                {
                    return false;
                }

                int armorvalue = 100;

                if (item.Id == 1142 || item.Id == 1141)
                {
                    if (item.Data == null || !item.Data.ContainsKey("armorvalue"))
                    {
                        return true;
                    }
                    else armorvalue = Convert.ToInt32(item.Data["armorvalue"]);
                }
                else
                {
                    if (dbPlayer.VisibleArmorType != type)
                        dbPlayer.SaveArmorType(type);
                    dbPlayer.VisibleArmorType = type;
                }


                dbPlayer.IsInTask = true;
                Chats.sendProgressBar(dbPlayer, 4000);
                dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetCannotInteract(true);
                await System.Threading.Tasks.Task.Delay(4000);
                dbPlayer.SetCannotInteract(false);
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();

                dbPlayer.SetArmor(armorvalue, true);
                return true;
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
                return false;
            }
        }
    }
}
