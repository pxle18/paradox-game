using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Staatsgefaengnis;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task LaundryTakeOut(DbPlayer dbPlayer, ItemModel model, Item item)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.CanInteract()) return;

            LaundryPosition laundryPosition = LaundryModule.Instance.GetAll().Values.Where(l => l.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f).FirstOrDefault();

            if (laundryPosition != null && dbPlayer.Container.GetItemAmount(model) >= 1)
            {
                if (!dbPlayer.IsSGJobActive(SGJobs.WASHING)) return;

                if (!Configurations.Configuration.Instance.DevMode && laundryPosition.LastInteracted.AddMinutes(5) <= DateTime.Now)
                {
                    dbPlayer.SendNewNotification("Das Bettlaken wurde bereits gereinigt!");
                    return;
                }

                if(item.Data != null && item.Data.ContainsKey("washed"))
                {
                    dbPlayer.SendNewNotification("Du kannst schmutzige und saubere Wäsche nicht zusammentun!");
                    return;
                }

                if (item.Data != null && item.Data.ContainsKey("laundry"))
                {
                    int laundry = item.Data["laundry"];
                    if (laundry >= LaundryModule.MaxTakeAbleLaundryPerBasket)
                    {
                        dbPlayer.SendNewNotification($"Wäschekorb ist voll, maximal {LaundryModule.MaxTakeAbleLaundryPerBasket} Bettlaken einsammelbar! Bring ihn zur Wäscherei.");
                        return;
                    }
                    else
                    {
                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);

                        dbPlayer.Player.SetRotation(laundryPosition.Heading);

                        await NAPI.Task.WaitForMainThread(500);

                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mini@cpr@char_a@cpr_str", "cpr_kol_idle");

                        Chat.Chats.sendProgressBar(dbPlayer, 10000);
                        await NAPI.Task.WaitForMainThread(10000);

                        laundryPosition.LastInteracted = DateTime.Now;

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");

                        dbPlayer.StopAnimation();

                        laundry++;
                        dbPlayer.SendNewNotification($"Bettlaken aufgesammelt, Wäschekorb enthält derzeit {laundry} Bettlaken.");
                        item.Data["laundry"] = laundry;

                        return;
                    }
                }
                else
                {
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetData("userCannotInterrupt", true);

                    dbPlayer.Player.SetRotation(laundryPosition.Heading);

                    await NAPI.Task.WaitForMainThread(500);

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mini@cpr@char_a@cpr_str", "cpr_kol_idle");

                    Chat.Chats.sendProgressBar(dbPlayer, 10000);
                    await NAPI.Task.WaitForMainThread(10000);

                    laundryPosition.LastInteracted = DateTime.Now;

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.ResetData("userCannotInterrupt");

                    dbPlayer.StopAnimation();

                    dbPlayer.SendNewNotification($"Bettlaken aufgesammelt, Wäschekorb enthält derzeit 1 Bettlaken.");
                    item.Data.Add("laundry", 1);
                }
            }
        }
    }
}
