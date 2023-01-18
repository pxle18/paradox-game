using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Laboratories;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Zentrifuge(DbPlayer dbPlayer, ItemModel itemModel)
        {
            if (dbPlayer.DimensionType[0] == DimensionTypes.Methlaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryAnalyzePosition) > 2.0f)
                {
                    dbPlayer.SendNewNotification("Die Zentrifuge muss am richtigen Ort verwendet werden (blauer Kreis)");
                    return false;
                }
                Chats.sendProgressBar(dbPlayer, LaboratoryModule.TimeToAnalyze);
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

                await NAPI.Task.WaitForMainThread(LaboratoryModule.TimeToAnalyze);
                dbPlayer.ResetData("userCannotInterrupt");

                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;

                dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                if (item == null)
                {
                    dbPlayer.SendNewNotification("Nichts zum Analysieren gefunden.");
                    return false;
                }
                string message = "Die Analyse hat folgendes ergeben:  ";
                if (!item.Data.TryGetValue("quality", out dynamic quality) || !item.Data.TryGetValue("amount", out dynamic amount)) return false;

                message += $"Reinheitsgrad ({quality * 100}%), Menge ({amount} Kristalle)";
                dbPlayer.SendNewNotification(message, duration: 30000);
                dbPlayer.StopAnimation();
                return true;
            }
            else if (dbPlayer.DimensionType[0] == DimensionTypes.Cannabislaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryComputerPosition) > 2.0f)
                {
                    dbPlayer.SendNewNotification("Die Zentrifuge muss am richtigen Ort verwendet werden (Computer)");
                    return false;
                }
                Chats.sendProgressBar(dbPlayer, LaboratoryModule.TimeToAnalyze);
                Item item = null;
                CannabislaboratoryModule.EndProductItemIds.ForEach(id =>
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

                await NAPI.Task.WaitForMainThread(LaboratoryModule.TimeToAnalyze);
                dbPlayer.ResetData("userCannotInterrupt");

                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;

                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                
                string message = "Die Analyse hat folgendes ergeben:  ";
                if (!item.Data.TryGetValue("quality", out dynamic quality) || !item.Data.TryGetValue("amount", out dynamic amount)) return false;

                message += $"Reinheitsgrad ({quality * 100}%), Menge ({amount} Kristalle)";
                dbPlayer.SendNewNotification(message, duration: 30000);
                dbPlayer.StopAnimation();
                return true;
            }
            return false;
        }
    }
}