using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using GTANetworkMethods;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Progressbar.Extensions;

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

                dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl),
                        Main.AnimationList["fixing"].Split()[0],
                        Main.AnimationList["fixing"].Split()[1]
               );

                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetCannotInteract(true);

                bool finishedProgressbar = await dbPlayer.RunProgressBar(() =>
                {
                    dbPlayer.SetArmor(armorvalue, true);

                    return System.Threading.Tasks.Task.CompletedTask;
                }, "Schutzweste", "Du ziehst eine Schutzweste.", 4 * 1000);

                dbPlayer.SetCannotInteract(false);
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();

                return finishedProgressbar;
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
                return false;
            }
        }
    }
}
