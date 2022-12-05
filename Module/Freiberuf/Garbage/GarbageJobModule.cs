using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Attachments;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Outfits;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.Freiberuf.Garbage
{
    public class GarbageJobModule : Module<GarbageJobModule>
    {
        public int GarbageJobVehMarkId = 21;

        public int GarbageJobVehicleLimit = 3;

        // City Los Santos
        public Vector3 GarbageNpc = new Vector3(-321.972, -1545.62, 31.0199);
        public float GarbageNpcHeading = 355.979f;

        public Vector3 VehicleSpawn = new Vector3(-317.022, -1539.3, 27.3804);
        public float VehicleSpawnRotation = 343.406f;
        public Vector3 GarbageEmptyPoint = new Vector3(-350.78, -1557.68, 25.2201);

        // Sandy
        public Vector3 GarbageNpcSandy = new Vector3(2340.74, 3126.53, 48.2087);
        public float GarbageNpcSandyHeading = 349.077f;

        public Vector3 VehicleSpawnSandy = new Vector3(2357.19, 3133.57, 47.9239);
        public float VehicleSpawnSandyRotation = 258.609f;
        public Vector3 GarbageEmptyPointSandy = new Vector3(2339.69, 3110.95, 48.209);


        // Paleto
        public Vector3 GarbageNpcPaleto = new Vector3(-195.94, 6265.58, 31.4893);
        public float GarbageNpcPaletoHeading = 26.4282f;

        public Vector3 VehicleSpawnPaleto = new Vector3(-179.787, 6286.81, 31.2002);
        public float VehicleSpawnPaletoRotation = 41.4033f;
        public Vector3 GarbageEmptyPointPaleto = new Vector3(-176.793, 6268.79, 32.3237);


        public float VehicleLoadageLimit = 10000.0f;

        public int Reward = 3;

        public List<DbPlayer> GarbageJobPlayers = new List<DbPlayer>();

        public List<SxVehicle> GarbageJobVehicles = new List<SxVehicle>();

        public List<SxVehicle> GarbageJobVehiclesSandy = new List<SxVehicle>();

        public List<SxVehicle> GarbageJobVehiclesPaleto = new List<SxVehicle>();

        public override bool Load(bool reload = false)
        {
            GarbageJobPlayers = new List<DbPlayer>();
            GarbageJobVehicles = new List<SxVehicle>();
            GarbageJobVehiclesSandy = new List<SxVehicle>();
            GarbageJobVehiclesPaleto = new List<SxVehicle>();

            // Spawn npc
            new Npc(PedHash.GarbageSMY, GarbageNpc, GarbageNpcHeading, 0);
            new Npc(PedHash.GarbageSMY, GarbageNpcPaleto, GarbageNpcPaletoHeading, 0);
            new Npc(PedHash.GarbageSMY, GarbageNpcSandy, GarbageNpcSandyHeading, 0);

            // Create Menu
            MenuManager.Instance.AddBuilder(new FreiberufGarbageMenuBuilder());

            // Display notification
            PlayerNotifications.Instance.Add(GarbageNpc, "Freiberuf Müllabfuhr", "Benutze \"E\" um den Freiberuf zu starten!");
            PlayerNotifications.Instance.Add(GarbageEmptyPoint, "Freiberuf Müllabfuhr", "Benutze \"E\" um den Müllwagen zu leeren!");

            PlayerNotifications.Instance.Add(GarbageNpcPaleto, "Freiberuf Müllabfuhr", "Benutze \"E\" um den Freiberuf zu starten!");
            PlayerNotifications.Instance.Add(GarbageEmptyPointPaleto, "Freiberuf Müllabfuhr", "Benutze \"E\" um den Müllwagen zu leeren!");

            PlayerNotifications.Instance.Add(GarbageNpcSandy, "Freiberuf Müllabfuhr", "Benutze \"E\" um den Freiberuf zu starten!");
            PlayerNotifications.Instance.Add(GarbageEmptyPointSandy, "Freiberuf Müllabfuhr", "Benutze \"E\" um den Müllwagen zu leeren!");
            return true;
        }

        public int GetGabrageId(DbPlayer dbPlayer)
        {
            int garbageId = 1; // LS
            if (dbPlayer.Player.Position.DistanceTo(GarbageNpcSandy) < 100) garbageId = 2;
            if (dbPlayer.Player.Position.DistanceTo(GarbageNpcPaleto) < 100) garbageId = 3;

            return garbageId;
        }

        private void RemoveOldVehiclesCity()
        {
            Task.Run(async () =>
            {
                // Get Vehicles
                foreach (SxVehicle sxVehicle in GarbageJobVehicles.ToList())
                {
                    if (sxVehicle == null || !sxVehicle.IsValid())
                    {
                        GarbageJobVehicles.Remove(sxVehicle);
                        continue;
                    }

                    await NAPI.Task.WaitForMainThread();
                    DbPlayer owner = Players.Players.Instance.GetByDbId(sxVehicle.ownerId);
                    if (owner == null || !owner.IsValid() || owner.Player.Position.DistanceTo(sxVehicle.entity.Position) > 100.0f)
                    {
                        if (!sxVehicle.HasData("gbremovecheck"))
                        {
                            sxVehicle.SetData("gbremovecheck", 1);
                        }

                        int gbRemInt = sxVehicle.GetData("gbremovecheck");

                        if (gbRemInt >= 10)
                        {
                            VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                        }
                        else
                        {
                            sxVehicle.SetData("gbremovecheck", gbRemInt + 1);
                        }
                    }
                    else
                    {
                        if (sxVehicle.HasData("gbremovecheck"))
                        {
                            sxVehicle.ResetData("gbremovecheck");
                        }
                    }
                }
            });
        }

        private void RemoveOldVehiclesPaleto()
        {
            Task.Run(async () =>
            {
                // Get Vehicles Paleto
                foreach (SxVehicle sxVehicle in GarbageJobVehiclesPaleto.ToList())
                {
                    if (sxVehicle == null || !sxVehicle.IsValid())
                    {
                        GarbageJobVehiclesPaleto.Remove(sxVehicle);
                        continue;
                    }

                    await NAPI.Task.WaitForMainThread(0);
                    DbPlayer owner = Players.Players.Instance.GetByDbId(sxVehicle.ownerId);
                    if (owner == null || !owner.IsValid() || owner.Player.Position.DistanceTo(sxVehicle.entity.Position) > 100.0f)
                    {
                        if (!sxVehicle.HasData("gbremovecheck"))
                        {
                            sxVehicle.SetData("gbremovecheck", 1);
                        }

                        int gbRemInt = sxVehicle.GetData("gbremovecheck");

                        if (gbRemInt >= 10)
                        {
                            VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                        }
                        else
                        {
                            sxVehicle.SetData("gbremovecheck", gbRemInt + 1);
                        }
                    }
                    else
                    {
                        if (sxVehicle.HasData("gbremovecheck"))
                        {
                            sxVehicle.ResetData("gbremovecheck");
                        }
                    }
                }
            });
        }

        private void RemoveOldVehiclesSandy()
        {
            Task.Run(async () =>
            {
                // Get Vehicles Sandy
                foreach (SxVehicle sxVehicle in GarbageJobVehiclesSandy.ToList())
                {
                    if (sxVehicle == null || !sxVehicle.IsValid())
                    {
                        GarbageJobVehiclesSandy.Remove(sxVehicle);
                        continue;
                    }

                    await NAPI.Task.WaitForMainThread(0);
                    DbPlayer owner = Players.Players.Instance.GetByDbId(sxVehicle.ownerId);
                    if (owner == null || !owner.IsValid() || owner.Player.Position.DistanceTo(sxVehicle.entity.Position) > 100.0f)
                    {
                        if (!sxVehicle.HasData("gbremovecheck"))
                        {
                            sxVehicle.SetData("gbremovecheck", 1);
                        }

                        int gbRemInt = sxVehicle.GetData("gbremovecheck");

                        if (gbRemInt >= 10)
                        {
                            VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                        }
                        else
                        {
                            sxVehicle.SetData("gbremovecheck", gbRemInt + 1);
                        }
                    }
                    else
                    {
                        if (sxVehicle.HasData("gbremovecheck"))
                        {
                            sxVehicle.ResetData("gbremovecheck");
                        }
                    }
                }
            });
        }

        private void ActualizeMarkers()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (DbPlayer dbPlayer in GarbageJobPlayers.ToList())
                {
                    if (dbPlayer == null || !dbPlayer.IsValid())
                    {
                        GarbageJobPlayers.Remove(dbPlayer);
                        continue;
                    }

                    List<CustomMarkerClientObject> clientSendData = new List<CustomMarkerClientObject>();

                    int limitInt = 0;
                    foreach (House house in HouseModule.Instance.GetAll().Values.ToList().Where(h => h.TrashAmount >= 100 && h.Position.DistanceTo(dbPlayer.Player.Position) < 500))
                    {
                        if (limitInt >= 30) break;
                        clientSendData.Add(new CustomMarkerClientObject() { MarkerId = 318, Position = house.Position, Color = 10, Name = "" });
                        limitInt++;
                    }

                    if (dbPlayer.HasData("garbageId"))
                    {
                        // Deliver Point
                        if (dbPlayer.GetData("garbageId") == 1) clientSendData.Add(new CustomMarkerClientObject() { MarkerId = 318, Position = GarbageEmptyPoint, Color = 2, Name = "Mülldepot Abgabe" });
                        else if (dbPlayer.GetData("garbageId") == 2) clientSendData.Add(new CustomMarkerClientObject() { MarkerId = 318, Position = GarbageEmptyPointSandy, Color = 2, Name = "Mülldepot Abgabe" });
                        else if (dbPlayer.GetData("garbageId") == 3) clientSendData.Add(new CustomMarkerClientObject() { MarkerId = 318, Position = GarbageEmptyPointPaleto, Color = 2, Name = "Mülldepot Abgabe" });
                    }


                    dbPlayer.Player.TriggerNewClient("setcustommarks", CustomMarkersKeys.GarbageJob, true, NAPI.Util.ToJson(clientSendData));
                }
            }));
        }

        public override void OnMinuteUpdate()
        {
            RemoveOldVehiclesCity();
            RemoveOldVehiclesPaleto();
            RemoveOldVehiclesSandy();

            ActualizeMarkers();
        }

        public override void OnVehicleDeleteTask(SxVehicle sxVehicle)
        {
            if(GarbageJobVehicles.ToList().Contains(sxVehicle))
            {
                GarbageJobVehicles.Remove(sxVehicle);
            }
            if (GarbageJobVehiclesSandy.ToList().Contains(sxVehicle))
            {
                GarbageJobVehiclesSandy.Remove(sxVehicle);
            }
            if (GarbageJobVehiclesPaleto.ToList().Contains(sxVehicle))
            {
                GarbageJobVehiclesPaleto.Remove(sxVehicle);
            }
        }

        // Start garbage job
        public void StartGarbageJob(DbPlayer dbPlayer)
        {
            if (dbPlayer.IsInDuty()) return;

            if (GarbageJobPlayers.ToList().Contains(dbPlayer)) return;

            // Set job data
            GarbageJobPlayers.Add(dbPlayer);

            OutfitsModule.Instance.SetPlayerOutfit(dbPlayer, 65);

            dbPlayer.SetData("garbageId", GetGabrageId(dbPlayer));

            // Notify user
            dbPlayer.SendNewNotification("Beginnen sie mit der Arbeit!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
        }

        // Start garbage job
        public void RentGarbageVeh(DbPlayer dbPlayer)
        {
            if(!GarbageJobPlayers.ToList().Contains(dbPlayer) || !dbPlayer.HasData("garbageId"))
            {
                dbPlayer.SendNewNotification("Sie müssen den Job zuerst starten!");
                return;
            }

            // Remove vehicle if exists
            dbPlayer.RemoveJobVehicleIfExist(GarbageJobVehMarkId);

            Vector3 VehicleSpawnPositionDefault = VehicleSpawn;
            float VehicleSpawnRotationDefault = VehicleSpawnRotation;

            int garbageId = dbPlayer.GetData("garbageId");

            if(garbageId == 2)
            {
                VehicleSpawnPositionDefault = VehicleSpawnSandy;
                VehicleSpawnRotationDefault = VehicleSpawnSandyRotation;
            }
            else if(garbageId == 3)
            {
                VehicleSpawnPositionDefault = VehicleSpawnPaleto;
                VehicleSpawnRotationDefault = VehicleSpawnPaletoRotation;
            }

            if (garbageId == 1)
            {
                if(GarbageJobVehicles.Count() >= GarbageJobVehicleLimit)
                {
                    dbPlayer.SendNewNotification("Alle Müllfahrzeuge in diesem Depot sind bereits unterwegs!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                    return;
                }
            }
            else if (garbageId == 2)
            {
                if (GarbageJobVehiclesSandy.Count() >= GarbageJobVehicleLimit)
                {
                    dbPlayer.SendNewNotification("Alle Müllfahrzeuge in diesem Depot sind bereits unterwegs!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                    return;
                }
            }
            else if (garbageId == 3)
            {
                if (GarbageJobVehiclesPaleto.Count() >= GarbageJobVehicleLimit)
                {
                    dbPlayer.SendNewNotification("Alle Müllfahrzeuge in diesem Depot sind bereits unterwegs!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                    return;
                }
            }

                // Check if a vehicle is blocking spawn point
            if (!dbPlayer.IsJobVehicleAtPoint(VehicleSpawnPositionDefault))
            {
                NAPI.Task.Run(async () =>
                {
                    // Spawn Vehicle and set vehicle data
                    SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(VehicleDataModule.Instance.GetData((uint)VehicleHash.Trash).Id, false, VehicleSpawnPositionDefault, VehicleSpawnRotationDefault, 58, 58, 0, true, true, false, 0, dbPlayer.GetName(), 0, GarbageJobVehMarkId, dbPlayer.Id, plate: "Trash INC");

                    while (xVeh.entity == null)
                    {
                        await NAPI.Task.WaitForMainThread(100);
                    }

                    xVeh.entity.SetData<int>("loadage", 0);

                    if (garbageId == 1) GarbageJobVehicles.Add(xVeh);
                    else if (garbageId == 2) GarbageJobVehiclesSandy.Add(xVeh);
                    else if (garbageId == 3) GarbageJobVehiclesPaleto.Add(xVeh);

                    // Notify user
                    dbPlayer.SendNewNotification("Ihr Fahrzeug steht bereit, beginnen sie mit der Arbeit!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                });
            }
        }

        // Maybe add multiplicator ? Example: Player drives to paletto should he get the same amount?
        public async Task loadTrashIntoVehicle(DbPlayer dbPlayer, Vehicle vehicle)
        {
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle || !GarbageJobPlayers.ToList().Contains(dbPlayer) || !dbPlayer.HasData("trash_amount") || dbPlayer.GetData("trash_amount") == 0) return;

            // Get job vehicle
            SxVehicle JobVehicle = vehicle.GetVehicle();

            if(JobVehicle.entity.Model != (uint)VehicleHash.Trash && JobVehicle.entity.Model != (uint)VehicleHash.Trash2)
            {
                return;
            }

            if (!dbPlayer.Attachments.ContainsKey((int)Attachment.TRASH))
            {
                dbPlayer.SendNewNotification("Du hast keinen Müllsack!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            if (JobVehicle.entity.Position.DistanceTo(dbPlayer.Player.Position) > 13) return;

            if(JobVehicle != null && JobVehicle.IsValid())
            {
                if(JobVehicle.entity.IsSeatFree(0))
                {
                    dbPlayer.SendNewNotification("Es muss ein Fahrer im Müllwagen sein!");
                    return;
                }

                // Check if vehicle got loadage data
                if (!JobVehicle.entity.HasData("loadage")) return;

                // Get new amount and reset user data
                int newLoadage = JobVehicle.entity.GetData<int>("loadage") + dbPlayer.GetData("trash_amount");
                dbPlayer.ResetData("trash_amount");

                // Add amount to vehicle
                JobVehicle.entity.SetData<int>("loadage", newLoadage);

                var l_Handler = new VehicleEventHandler();
                l_Handler.ToggleDoorState(dbPlayer.Player, JobVehicle.entity, 5);

                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetCannotInteract(true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "missfbi4prepp1", "_bag_throw_garbage_man");

                await Task.Delay(1500);
                // Remove attachments
                AttachmentModule.Instance.RemoveAttachment(dbPlayer, (int)Attachment.TRASH);

                l_Handler.ToggleDoorState(dbPlayer.Player, JobVehicle.entity, 5);
                dbPlayer.StopAnimation();
                dbPlayer.SetCannotInteract(false);
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);


                int Müllbestand = JobVehicle.entity.GetData<int>("loadage");

                // Inform user
                dbPlayer.SendNewNotification($"Neuer Müllbestand im Fahrzeug: {Müllbestand}");

                foreach(DbPlayer occu in JobVehicle.GetOccupants().GetLegacyDictionary().Values)
                {
                    if(occu != null && occu.IsValid() && occu.RageExtension.IsInVehicle && occu.Player.VehicleSeat == 0)
                    {
                        occu.SendNewNotification($"Neuer Müllbestand im Fahrzeug: {Müllbestand}");
                    }
                }
            }
        }

        public void PickupTrash(DbPlayer dbPlayer, House house)
        {
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle || !GarbageJobPlayers.ToList().Contains(dbPlayer)) return;

            SxVehicle jobVeh = FreiberufFunctions.GetNearestJobVehicle(dbPlayer, GarbageJobVehMarkId, 50.0f);

            if (jobVeh == null || !jobVeh.IsValid())
            {

                dbPlayer.SendNewNotification("Du musst einen Müllwagen in der Nähe haben!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            // Check current vehicle loadage
            float currentVehicleLoadage = jobVeh.entity.GetData<int>("loadage");

            // Check if vehicle can store trash
            if (currentVehicleLoadage >= VehicleLoadageLimit)
            {
                dbPlayer.Player.TriggerNewClient("setPlayerGpsMarker", GarbageEmptyPoint.X, GarbageEmptyPoint.Y);
                dbPlayer.SendNewNotification("Der Müllwagen ist zu voll, entleere diesen zuerst. Die GPS Koordinaten wurden eingetragen!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            // Check if user is already carying a bag
            if (dbPlayer.Attachments.ContainsKey((int)Attachment.TRASH))
            {
                dbPlayer.SendNewNotification("Du kannst nur einen Müllsack tragen!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            // Check trash amount
            if (house.TrashAmount >= 100.0f)
            {
                Task.Run(async () =>
                {
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetCannotInteract(true);
                    dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "missfbi4prepp1", "_bag_pickup_garbage_man");

                    await Task.Delay(2500);
                    dbPlayer.StopAnimation();
                    dbPlayer.SetCannotInteract(false);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                    // Attach object and add data to player data
                    AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachment.TRASH);
                });

                float trashAmount = house.TrashAmount;

                // Distance Awars
                if (house.Position.DistanceTo(GarbageNpc) < 1000)
                {
                    trashAmount = (trashAmount * 0.75f); // -25%
                }
                else if (house.Position.DistanceTo(GarbageNpc) > 2500)
                {
                    trashAmount = (trashAmount * 1.25f); // +25%
                }
                else if (house.Position.DistanceTo(GarbageNpc) > 5000)
                {
                    trashAmount = (trashAmount * 1.75f); // +75%
                }


                dbPlayer.SetData("trash_amount", Convert.ToInt32(trashAmount));

                // Empty trash stand
                house.TrashAmount = 0.0f;
                house.SaveTrash();

                // Notify user
                dbPlayer.SendNewNotification("Du hast einen Müllsack aufgehoben, entlade diesen in deinen Müllwagen!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }
            else
            {
                // Inform user trash is empty
                dbPlayer.SendNewNotification("Diese Mülltonne ist leer!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }
        }

        public override void OnFifteenMinuteUpdate()
        {
            foreach(House house in HouseModule.Instance.GetAll().Values.ToList().Where(h => h.OwnerId != 0))
            {
                house.TrashAmount += (2 + house.Maxrents);
                house.SaveTrash();
            }
        }

        // Empty Garbage vehicle
        public async Task EmptyGarbageVehicle(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !GarbageJobPlayers.ToList().Contains(dbPlayer) || dbPlayer.HasData("empty_progress") || !dbPlayer.CanInteract() || !dbPlayer.HasData("garbageId")) return;

            // Check if user is inside vehicle
            if (dbPlayer.RageExtension.IsInVehicle)
            {
                dbPlayer.SendNewNotification("Um den Müllwagen zu entleeren darfst du nicht in einem Fahrzeug sitzen!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            SxVehicle garbageVehicle = FreiberufFunctions.GetJobVehicle(dbPlayer, GarbageJobVehMarkId);
            if (garbageVehicle == null)
                return;

            Vehicle vehicleEntity = garbageVehicle.entity;
            if (vehicleEntity == null)
                return;

            // Check vehicle distance
            if (dbPlayer.Player.Position.DistanceTo(vehicleEntity.Position) >= 10.0f)
            {
                dbPlayer.SendNewNotification("Der Müllwagen ist zu weit entfernt!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            await NAPI.Task.WaitForMainThread(0);

            // Get vehicle trash amount
            if (!vehicleEntity.HasData("loadage"))
                return;

            float currentVehicleLoadage = vehicleEntity.GetData<int>("loadage");

            // Check if vehicle got trash loaded
            if (currentVehicleLoadage == 0.0f)
            {
                dbPlayer.SendNewNotification("Der Müllwagen ist leer!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }


            dbPlayer.SetData("empty_progress", true);

            // Get time
            int time = ((int)Math.Round(currentVehicleLoadage, 0) / 50) * 1000;

            // add timer
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetCannotInteract(true);

            // Show progress bar
            Chats.sendProgressBar(dbPlayer, time);
            await Task.Delay(time);

            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);

            // Reset vehicle trash amount
            FreiberufFunctions.GetJobVehicle(dbPlayer, GarbageJobVehMarkId).entity.SetData<int>("loadage", 0);

            // Get reward
            int money = (int)Math.Round(currentVehicleLoadage, 0) * Reward;

            // Add money
            dbPlayer.GiveMoney(money);

            // Inform user
            dbPlayer.SendNewNotification($"Der Müllwagen wurde entleert. Du hast {money}$ für deine Arbeit erhalten. Bringe den Wagen nun zurück oder beginn eine weitere Tour!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
            dbPlayer.ResetData("empty_progress");
        }

        // Finish garbage job
        public void FinishGarbageJob(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !GarbageJobPlayers.ToList().Contains(dbPlayer)) return;

            SxVehicle jobVehicle = FreiberufFunctions.GetJobVehicle(dbPlayer, GarbageJobVehMarkId);

            if(jobVehicle != null && jobVehicle.IsValid())
            {
                // Check if vehicle still got trash inside
                float currentVehicleLoadage = FreiberufFunctions.GetJobVehicle(dbPlayer, GarbageJobVehMarkId).entity.GetData<int>("loadage");

                if (currentVehicleLoadage > 0.0f)
                {
                    dbPlayer.SendNewNotification("Bitte gehe den Müllwagen zuerst entleeren!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                    return;
                }
                VehicleHandler.Instance.DeleteVehicle(jobVehicle, false);
            }

            // Reset user data
            GarbageJobPlayers.Remove(dbPlayer);

            // Remove Blips
            dbPlayer.Player.TriggerNewClient("clearcustommarks", CustomMarkersKeys.GarbageJob);

            // Remove Outfit
            if (dbPlayer.HasData("outfitactive")) dbPlayer.ResetData("outfitactive");

            dbPlayer.ApplyCharacter();

            // Notify user
            dbPlayer.SendNewNotification("Du hast den Freiberuf beendet!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if(GarbageJobPlayers.ToList().Contains(dbPlayer))
            {
                GarbageJobPlayers.Remove(dbPlayer);
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle) return false;

            // Show menu
            if (dbPlayer.Player.Position.DistanceTo(GarbageJobModule.Instance.GarbageNpc) < 2.0f ||
                dbPlayer.Player.Position.DistanceTo(GarbageJobModule.Instance.GarbageNpcSandy) < 2.0f ||
                dbPlayer.Player.Position.DistanceTo(GarbageJobModule.Instance.GarbageNpcPaleto) < 2.0f)
            {
                MenuManager.Instance.Build(PlayerMenu.FreiberufGarbageMenu, dbPlayer).Show(dbPlayer);
                return true;
            }

            if (!GarbageJobPlayers.ToList().Contains(dbPlayer)) return false;

            House house = GetTrashHouseObjectFromPosition(dbPlayer.Player.Position);


            if (!dbPlayer.HasData("garbageId")) return false;
            // Pickup Trash
            if (house != null)
            {
                PickupTrash(dbPlayer, house);
                return true;
            }
            // Empty trash vehicle at Garbage started Position
            else if (dbPlayer.GetData("garbageId") == 1 && dbPlayer.Player.Position.DistanceTo(GarbageEmptyPoint) <= 3.0f)
            {
                NAPI.Task.Run(async () =>
                {
                    await EmptyGarbageVehicle(dbPlayer);
                });
                return true;
            }
            else if (dbPlayer.GetData("garbageId") == 2 && dbPlayer.Player.Position.DistanceTo(GarbageEmptyPointSandy) <= 3.0f)
            {
                NAPI.Task.Run(async () =>
                {
                    await EmptyGarbageVehicle(dbPlayer);
                });
                return true;
            }
            else if (dbPlayer.GetData("garbageId") == 3 && dbPlayer.Player.Position.DistanceTo(GarbageEmptyPointPaleto) <= 3.0f)
            {
                NAPI.Task.Run(async () =>
                {
                    await EmptyGarbageVehicle(dbPlayer);
                });
                return true;
            }

            return false;
        }
        
        public House GetTrashHouseObjectFromPosition(Vector3 position)
        {
            return HouseModule.Instance.GetAll().FirstOrDefault(house => house.Value.Position.DistanceTo(position) <= 2.0f).Value;
        }
    }

    public class GarbageEvents : Script
    {
        [RemoteEvent]
        public async void Pressed_E_Garbage_Vehicle(Player player, Vehicle vehicle, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid() || player == null || vehicle == null) return;

            if (dbPlayer.HasData("trash_amount") && GarbageJobModule.Instance.GarbageJobPlayers.ToList().Contains(dbPlayer) && dbPlayer.Attachments.ContainsKey((int)Attachment.TRASH))
            {
                float distance = Math.Abs(vehicle.Heading - dbPlayer.Player.Heading);

                if (distance > 45) return;


                await GarbageJobModule.Instance.loadTrashIntoVehicle(dbPlayer, vehicle);
            }
        }
    }
}
