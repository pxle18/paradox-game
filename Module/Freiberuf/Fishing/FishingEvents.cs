using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Freiberuf.Fishing
{
    public class FishingEvents : Script
    {
        [RemoteEvent]
        public void startFishing(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle)
                    return;

                SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position, 10.0f);

                if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.Data.ClassificationId != 3) return;

                FishingSpot fishingSpot = FishingModule.Instance.GetAll().Values.ToList().Where(fs => fs.Position.DistanceTo(dbPlayer.Player.Position) < fs.Range).FirstOrDefault();
                if (fishingSpot == null)
                {
                    dbPlayer.SendNewNotification("Du musst dich in einem Fischfang Bereich befinden!");
                    return;
                }

                if (dbPlayer.Container.GetItemAmount(FishingModule.FishingRoItemId) <= 0)
                {
                    dbPlayer.SendNewNotification("Du hast keine Angel!");
                    return;
                }

                if (!dbPlayer.HasData("fishing_koeder"))
                {
                    dbPlayer.SendNewNotification("Es ist kein Köder an der Angel!");
                    return;
                }


                if (!FishingModule.Instance.ContainsPlayerFishing(dbPlayer))
                {
                    Task.Run(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, (3000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        dbPlayer.SetCannotInteract(true);

                        await Task.Delay(3000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        dbPlayer.SetCannotInteract(false);

                        FishingModule.Instance.AddToFishing(dbPlayer);

                        // Add Angel
                        Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, 4, true);
                        dbPlayer.SendNewNotification("Angeln gestartet, warte bis ein Fisch anbeißt...");

                        dbPlayer.Player.TriggerNewClient("setAngelState", true);
                    });
                }
                return;
            }
            catch(Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void stopFishing(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle)
                return;

            if (FishingModule.Instance.ContainsPlayerFishing(dbPlayer))
            {
                FishingModule.Instance.RemoveFromFishing(dbPlayer);

                Task.Run(async () =>
                {
                    Chats.sendProgressBar(dbPlayer, (3000));

                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetData("userCannotInterrupt", true);
                    dbPlayer.SetCannotInteract(true);

                    await Task.Delay(3000);

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.ResetData("userCannotInterrupt");
                    dbPlayer.SetCannotInteract(false);

                    // remove Angel
                    Attachments.AttachmentModule.Instance.ClearAllAttachments(dbPlayer);
                    dbPlayer.SendNewNotification("Angeln eingeholt!");
                    dbPlayer.ResetData("fishing_koeder");

                    dbPlayer.StopAnimation();

                    dbPlayer.Player.TriggerNewClient("setAngelState", false);

                    if (dbPlayer.HasData("fishing_fish"))
                    {
                        FishingSpot fishingSpot = FishingModule.Instance.GetAll().Values.ToList().Where(fs => fs.Position.DistanceTo(dbPlayer.Player.Position) < fs.Range).FirstOrDefault();
                        if (fishingSpot != null)
                        {
                            dbPlayer.Player.TriggerNewClient("setFishState", false);
                            dbPlayer.ResetData("fishing_fish");

                            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position, 10.0f);

                            if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.Data.ClassificationId != 3) return;

                            int resultFish = FishingModule.NormalFishes[new Random().Next(FishingModule.NormalFishes.Count())]; // Sadrine default...

                            // get Fish
                            if (fishingSpot.RareSpotFishId != 0)
                            {
                                if (new Random().Next(1, 100) <= 5)
                                {
                                    resultFish = fishingSpot.RareSpotFishId;
                                }
                            }

                            dbPlayer.SendNewNotification($"Du hast einen {ItemModelModule.Instance.Get((uint)resultFish).Name} gefangen!");

                            if (!dbPlayer.Container.CanInventoryItemAdded((uint)resultFish, 1))
                            {
                                dbPlayer.SendNewNotification("Du kannst nicht mehr so viel tragen... Fisch wieder freigelassen!");
                                return;
                            }
                            dbPlayer.Container.AddItem((uint)resultFish, 1);
                        }
                    }
                });
            }
            return;
        }

        [RemoteEvent]
        public async Task addKoeder(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle)
                    return;


                SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position, 10.0f);

                if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.Data.ClassificationId != 3) return;

                FishingSpot fishingSpot = FishingModule.Instance.GetAll().Values.ToList().Where(fs => fs.Position.DistanceTo(dbPlayer.Player.Position) < fs.Range).FirstOrDefault();
                if (fishingSpot == null)
                {
                    dbPlayer.SendNewNotification("Du musst dich in einem Fischfang Bereich befinden!");
                    return;
                }

                if (dbPlayer.Container.GetItemAmount(FishingModule.FishingRoItemId) <= 0)
                {
                    dbPlayer.SendNewNotification("Du hast keine Angel!");
                    return;
                }

                // add köder...
                if (dbPlayer.Container.GetItemAmount(FishingModule.KoederItemId) <= 0)
                {
                    dbPlayer.SendNewNotification("Du hast keine Köder!");
                    return;
                }

                if (dbPlayer.HasData("fishing_koeder"))
                {
                    dbPlayer.SendNewNotification("Es ist bereits ein Köder an der Angel!");
                    return;
                }


                if (!FishingModule.Instance.ContainsPlayerFishing(dbPlayer))
                {
                    Chats.sendProgressBar(dbPlayer, (3000));

                    dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetData("userCannotInterrupt", true);
                    dbPlayer.SetCannotInteract(true);

                    await Task.Delay(3000);

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.ResetData("userCannotInterrupt");
                    dbPlayer.SetCannotInteract(false);
                    dbPlayer.StopAnimation();

                    if (!dbPlayer.HasData("fishing_koeder"))
                    {
                        dbPlayer.SetData("fishing_koeder", true);
                        dbPlayer.Container.RemoveItem(FishingModule.KoederItemId, 1);
                        return;
                    }
                    // Add Angel
                    dbPlayer.SendNewNotification("Köder angebracht");
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
    }
}
