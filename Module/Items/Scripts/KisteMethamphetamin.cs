using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Laboratories;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> KisteMethamphetamin(DbPlayer dbPlayer, ItemModel ItemData)
        {
            int time = 5000;
            Chats.sendProgressBar(dbPlayer, time);
            Item item = null;
            MethlaboratoryModule.EndProductItemIds.ForEach(id =>
            {
                if (item == null)
                {
                    item = dbPlayer.Container.GetItemById((int)id);
                }
            });
            if (item == null)
            {
                dbPlayer.SendNewNotification("Nichts zum Analysieren gefunden.");
                return false;
            }

            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);

            await NAPI.Task.WaitForMainThread(time);
            dbPlayer.ResetData("userCannotInterrupt");
            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured()) return false;

            dbPlayer.Player.TriggerNewClient("freezePlayer", false);

            if (item == null)
            {
                dbPlayer.SendNewNotification("Nichts zum Wiegen gefunden.");
                return false;
            }
            foreach (KeyValuePair<string, dynamic> keyValuePair in item.Data)
            {
                if (keyValuePair.Key == "amount")
                {
                    string value = Convert.ToString(keyValuePair.Value);
                    dbPlayer.SendNewNotification($"In dieser Kiste befinden sich {value} Kristalle");
                }
            }
            dbPlayer.StopAnimation();
            return true;
        }
    }
}
