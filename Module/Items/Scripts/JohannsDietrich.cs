using GTANetworkAPI;
using System;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> JohannsDietrich(DbPlayer dbPlayer, ItemModel ItemData)
        {
            // Check Door
            if (dbPlayer.TryData("houseId", out uint houseId))
            {
                House house = HouseModule.Instance.Get(houseId);
                if (house != null)
                {
                    if (house.LastBreak.AddMinutes(10) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden
                    int breakTime = 300000;
                    if (Configurations.Configuration.Instance.DevMode)
                        breakTime = 5000;
                    Chats.sendProgressBar(dbPlayer, breakTime);

                    DbPlayer owner = Players.Players.Instance.FindPlayer(house.OwnerId);
                    if(owner != null && owner.IsValid() && house.Price >= 100000)
                    {
                        owner.SendNewNotification($"Hookers House Guardian: Ein stiller Alarm an deinem Haus wurde ausgelöst.", title:"Hookers House Guardian", duration:30000);
                    }
                    if(house.Price >= 2500000)
                    {
                        if(owner != null)
                            TeamModule.Instance.SendChatMessageToDepartments($"Hookers House Guardian: Ein stiller Alarm am Haus {house.Id} von {owner.GetName()} wurde ausgelöst.");
                        else
                            TeamModule.Instance.SendChatMessageToDepartments($"Hookers House Guardian: Ein stiller Alarm am Haus {house.Id} wurde ausgelöst.");
                    }

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetData("userCannotInterrupt", true);

                    await NAPI.Task.WaitForMainThread(breakTime);
                    dbPlayer.ResetData("userCannotInterrupt");

                    if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    house.Break();

                    dbPlayer.SendNewNotification("Haus aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    dbPlayer.StopAnimation();
                    return true;
                }
            }
            bool callBack = await Schweissgereat(dbPlayer, ItemData);
            if (!callBack)
                dbPlayer.SendNewNotification("Besonderer Dietrich zum Knacken von Schlössern.");
            return callBack;
        }
    }
}
