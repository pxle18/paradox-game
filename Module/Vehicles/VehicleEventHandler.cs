using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.RemoteEvents;
using VMP_CNR.Module.Vehicles.Windows;

namespace VMP_CNR.Module.Vehicles
{
    public class VehicleEventHandler : Script
    {
        private const int RepairkitId = 38;

        public void ToggleDoorState(Player p_Client, Vehicle p_Vehicle, uint p_Door)
        {
            SxVehicle l_Vehicle = p_Vehicle.GetVehicle();
            if (l_Vehicle == null)
                return;

            if (!l_Vehicle.DoorStates.ContainsKey(p_Door))
                return;

            l_Vehicle.DoorStates[p_Door] = !l_Vehicle.DoorStates[p_Door];

            NAPI.Task.Run(() =>
            {
                var l_NearPlayers = NAPI.Player.GetPlayersInRadiusOfPosition(50.0f, p_Vehicle.Position);
                foreach (var l_Player in l_NearPlayers)
                {
                    l_Player.TriggerNewClient("syncVehicleDoor", p_Vehicle, p_Door, l_Vehicle.DoorStates[p_Door]);
                }
            });
        }

        public void ToggleTrunkState(DbPlayer dbPlayer, SxVehicle l_Vehicle, bool state) 
        {
            if (l_Vehicle == null || l_Vehicle.Data == null)
                return;

            uint trunkDoorDefault = 5;
            if(l_Vehicle.Data.VehDoorTrunk > 0)
            {
                trunkDoorDefault = l_Vehicle.Data.VehDoorTrunk;
            }

            if (!l_Vehicle.DoorStates.ContainsKey(trunkDoorDefault))
                return;

            // Do for kofferraum
            l_Vehicle.TrunkStateOpen = state;

            // Sync to actuall vehicle door States (Serverside)

            l_Vehicle.DoorStates[trunkDoorDefault] = state;
            if(l_Vehicle.Data.VehDoorTrunk2 > 0) // 2 tür
            {
                l_Vehicle.DoorStates[l_Vehicle.Data.VehDoorTrunk2] = state;
            }

            // kein for in range weil ragemp synced das mit magie :)
            if (l_Vehicle.Data.VehDoorTrunk2 > 0)
            {
                HashSet<uint> doors = new HashSet<uint>() { };
                doors.Add(trunkDoorDefault);
                doors.Add(l_Vehicle.Data.VehDoorTrunk2);

                foreach (DbPlayer targetPlayer in Players.Players.Instance.GetPlayersInRange(l_Vehicle.entity.Position, 80.0f))
                {
                    if (targetPlayer != null && targetPlayer.IsValid())
                    {
                        targetPlayer.Player.TriggerNewClient("syncVehicleDoors", l_Vehicle.entity, doors, state);
                    }
                }
            }
            else
            {
                foreach (DbPlayer targetPlayer in Players.Players.Instance.GetPlayersInRange(l_Vehicle.entity.Position, 80.0f))
                {
                    if (targetPlayer != null && targetPlayer.IsValid())
                    {
                        targetPlayer.Player.TriggerNewClient("syncVehicleDoor", l_Vehicle.entity, trunkDoorDefault, state);
                    }
                }
            }
        }

        [RemoteEvent]
        public async void syncSireneStatus(Player p_Client, Vehicle p_Vehicle, bool p_State, bool sound, string key)
        {
            if (!p_Client.CheckRemoteEventKey(key)) return;
            SxVehicle l_Vehicle = p_Vehicle.GetVehicle();
            if (l_Vehicle == null || !l_Vehicle.IsValid())
                return;

            if (l_Vehicle.HasData("lastSireneStateChange"))
            {
                DateTime lastSireneChange = l_Vehicle.GetData("lastSireneStateChange");
                if (lastSireneChange != null && lastSireneChange.AddSeconds(1) > DateTime.Now)
                {
                    await Task.Delay(500);
                }
            }
            else
                l_Vehicle.SetData("lastSireneStateChange", DateTime.Now);


            l_Vehicle.SirensActive = p_State;
            l_Vehicle.SilentSiren = !sound; // is sirene sound on, deshalb negative
        }

        [RemoteEvent]
        public async Task Silent_Sirene(Player p_Client, Vehicle p_Vehicle, string key)
        {
            if (!p_Client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = p_Client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            SxVehicle sxVeh = p_Vehicle.GetVehicle();
            if (sxVeh == null || !sxVeh.IsValid())
                return;

            if (sxVeh.HasData("lastSireneStateChange"))
            {
                DateTime lastSireneChange = sxVeh.GetData("lastSireneStateChange");
                if (lastSireneChange != null && lastSireneChange.AddSeconds(1) > DateTime.Now)
                {
                    await Task.Delay(500);
                }
            }
            else
                sxVeh.SetData("lastSireneStateChange", DateTime.Now);
            
            if (dbPlayer.Player.VehicleSeat == 0)
            {
                if (!sxVeh.SirensActive) return;

                sxVeh.SilentSiren = !sxVeh.SilentSiren;
                Vector3 playerPos = await dbPlayer.RageExtension.GetPositionAsync();

                foreach (DbPlayer xPlayer in Players.Players.Instance.GetPlayersInRange(playerPos, 350))
                {
                    xPlayer.Player.TriggerNewClient("refreshSireneState", sxVeh.entity, sxVeh.SirensActive, sxVeh.SilentSiren);
                }
            }
        }

        [RemoteEvent]
        public void requestNormalSpeed(Player p_Client, Vehicle p_Vehicle, string key)
        {
            if (!p_Client.CheckRemoteEventKey(key)) return;
            SxVehicle l_Vehicle = p_Vehicle.GetVehicle();
            DbPlayer l_DbPlayer = p_Client.GetPlayer();
            if (l_Vehicle == null|| l_DbPlayer == null) return;

            p_Client.TriggerNewClient("setNormalSpeed", p_Vehicle, l_Vehicle.Data.MaxSpeed);
        }

        [RemoteEvent]
        public void requestVehicleSyncData(Player p_Client, Vehicle p_RequestedVehicle, string key)
        {
            if (!p_Client.CheckRemoteEventKey(key)) return;
            DbPlayer l_DbPlayer = p_Client.GetPlayer();
            if (l_DbPlayer == null)
                return;

            SxVehicle l_SxVehicle = p_RequestedVehicle.GetVehicle();
            if (l_SxVehicle == null || !l_SxVehicle.IsValid() || l_SxVehicle.databaseId == 0)
                return;

            var l_Tuning        = l_SxVehicle.Mods;
            var l_DoorStates    = l_SxVehicle.DoorStates;

            try
            {
                string l_SerializedTuning = JsonConvert.SerializeObject(l_Tuning);
                string l_SerializedDoor = JsonConvert.SerializeObject(l_DoorStates);

                bool AnkerState = false;
                if (l_SxVehicle.HasData("anker") && l_SxVehicle.GetData("anker")) AnkerState = true;
                p_Client.TriggerNewClient("responseVehicleSyncData", p_RequestedVehicle, JsonConvert.SerializeObject(l_Tuning), 
                    JsonConvert.SerializeObject(l_DoorStates), l_SxVehicle.Data.LiveryIndex);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void requestSireneStatus(Player p_Client, Vehicle p_RequestedVehicle, string key)
        {
            if (!p_Client.CheckRemoteEventKey(key)) return;
            DbPlayer l_DbPlayer = p_Client.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid())
                return;

            SxVehicle l_SxVehicle = p_RequestedVehicle.GetVehicle();
            if (l_SxVehicle == null || !l_SxVehicle.IsValid())
                return;

            p_Client.TriggerNewClient("refreshSireneState", p_RequestedVehicle, l_SxVehicle.SirensActive, l_SxVehicle.SilentSiren);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_INFORMATION(Player client, Vehicle vehicle, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var dbVehicle = vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;

            // Respawnstate
            dbVehicle.respawnInteractionState = true;

            var msg = "";

            //vehicle information

            // number plate
            msg += "Nummernschild: " + dbVehicle.entity.NumberPlate;
            
            // vehicle model name
            if (dbVehicle.Data.modded_car == 1)
                msg += " Modell: " + dbVehicle.Data.mod_car_name;
            else
                msg += " Modell: " + dbVehicle.Data.Model;
            
            // vehicle serial number
            if (dbVehicle.Undercover)
            {
                msg += " Seriennummer: " + dbVehicle.entity.GetData<int>("nsa_veh_id");

                if (dbVehicle.teamid == (uint)teams.TEAM_FIB && dbPlayer.TeamId == (uint)teams.TEAM_FIB && dbPlayer.TeamRank >= 11)
                {
                    dbPlayer.SendNewNotification($"Interne Nummer: {dbVehicle.databaseId.ToString()}");
                }
                else if (dbPlayer.TeamId == dbVehicle.teamid)
                {
                    msg += $" Interne Nummer: {dbVehicle.databaseId.ToString()}";
                }
            }
            else
            {
                msg += " Seriennummer: " + dbVehicle.databaseId;
            }

            if(dbVehicle.CarsellPrice > 0)
            {
                msg += " VB $" + string.Format("{0:0,0}", dbVehicle.CarsellPrice);
            }

            dbPlayer.SendNewNotification(msg, PlayerNotification.NotificationType.INFO, "KFZ", 10000);
        }
                
        //[RemoteEventPermission]
        //[RemoteEvent]
        //public void REQUEST_VEHICLE_FlATBED_LOAD(Player client, Vehicle vehicle, string key)
        //{
        //    if (!client.CheckRemoteEventKey(key)) return;
        //    return;
            /*DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            if (!dbPlayer.IsInDuty() || dbPlayer.TeamId != (int) teams.TEAM_DPOS) return;
            var dbVehicle = vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;
            
            var offsetFlatbed = vehicle.GetModel().GetFlatbedVehicleOffset();
            if (offsetFlatbed == null)
                return;
            
            if (offsetFlatbed == null) return;

            // Respawnstate
            dbVehicle.respawnInteractionState = true;
            
            foreach (var dposVehicle in VehicleHandler.Instance.GetAllVehicles())
            {
                if (dposVehicle == null || dposVehicle.entity == null) continue;
                Vector3 offset = new Vector3(0,0,0);
                if (dposVehicle.entity.GetModel() == VehicleHash.Flatbed && offsetFlatbed != null
                                                                         && vehicle.Position.DistanceTo(
                                                                             dposVehicle.entity.Position) <=
                                                                         12.0f)
                {
                    offset = offsetFlatbed;
                }
                else
                {
                    continue;
                }
                
                if (dposVehicle.entity.HasData("loadedVehicle")) continue;
                
                var call = new NodeCallBuilder("attachTo").AddVehicle(dposVehicle.entity).AddInt(0).AddFloat(offset.X).AddFloat(offset.Y).AddFloat(offset.Z).AddFloat(0).AddFloat(0).AddFloat(0).AddBool(true).AddBool(false).AddBool(false).AddBool(false).AddInt(0).AddBool(false).Build();
                vehicle.Call(call);

                dposVehicle.entity.SetData("loadedVehicle", vehicle);
                vehicle.SetData("isLoaded", true);
                return;
            }*/
        //}

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_FlATBED_UNLOAD(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            return;
            /*DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent() || dbPlayer.isInjured() || !client.IsInVehicle) return;
            if (!dbPlayer.IsInDuty() || dbPlayer.TeamId != (int) teams.TEAM_DPOS) return;
            if ((VehicleHash)client.Vehicle.Model != VehicleHash.Flatbed &&
                (VehicleHash)client.Vehicle.Model != VehicleHash.Wastelander) return;
            var dbVehicle = client.Vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;

            if (!client.Vehicle.HasData("loadedVehicle")) return;
            Vehicle loadedVehicle = client.Vehicle.GetData("loadedVehicle");

            var call = new NodeCallBuilder("detach").Build();
            loadedVehicle.Call(call);

            client.Vehicle.ResetData("loadedVehicle");
            loadedVehicle.ResetData("isLoaded");*/
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public async Task REQUEST_VEHICLE_FRISK(Player player, Vehicle vehicle, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            if (vehicle == null) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid())
                return;

            if (dbPlayer.RageExtension.IsInVehicle) return;

            if (!dbPlayer.IsACop())
                return;

            if (!dbPlayer.CanAccessMethod())
                return;

            if (dbPlayer.TeamRank < 2)
                return;

            ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
            ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);

            var delVeh = vehicle.GetVehicle();
            if (delVeh == null || !delVeh.IsValid()) return;

            if (dbPlayer.Player.Position.DistanceTo(delVeh.entity.Position) > 10f) return;

            if (!dbPlayer.HasData("lastfriskveh") || dbPlayer.GetData("lastfriskveh") != delVeh.databaseId)
            {
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                Chat.Chats.sendProgressBar(dbPlayer, 8000);
                await Task.Delay(8000);

                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();
            }

            dbPlayer.SetData("lastfriskveh", delVeh.databaseId);

            if (dbPlayer.Player.Position.DistanceTo(delVeh.entity.Position) > 10f) return;

            delVeh.Container.ShowVehFriskInventory(dbPlayer, delVeh.Data.Model);

            Logger.SaveToFriskVehLog(dbPlayer.Id, (int)delVeh.databaseId, dbPlayer.GetName());
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_TOGGLE_ENGINE(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent() || !dbPlayer.RageExtension.IsInVehicle) return;
            var dbVehicle = client.Vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;

            // player is not in driver seat
            if (client.VehicleSeat != 0) return;
            if (!dbVehicle.CanInteract) return;
            if (!dbPlayer.CanControl(dbVehicle)) return;
            
            // Respawnstate
            dbVehicle.respawnInteractionState = true;

            // EMP
            if(dbVehicle.IsInAntiFlight())
            {
                client.Vehicle.GetVehicle().SyncExtension.SetEngineStatus(false);
                dbVehicle.SyncExtension.SetEngineStatus(false);
                return;
            }
            
            if (dbVehicle.fuel == 0 && dbVehicle.SyncExtension.EngineOn == false)
            {
                dbPlayer.SendNewNotification("Dieses Fahrzeug hat kein Benzin mehr!", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (dbVehicle.WheelClamp > 0)
            {
                dbPlayer.SendNewNotification("Dein Fahrzeug wurde mit einer Parkkralle festgesetzt und rührt sich keinen Meter mehr vom Fleck...", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (dbVehicle.Data != null && dbVehicle.Data.MaxSpeed > 0)
            {
                dbVehicle.Occupants.TriggerEventForOccupants("setNormalSpeed", dbVehicle.entity, dbVehicle.Data.MaxSpeed);
            }

            if (dbVehicle.SyncExtension.EngineOn == false)
            {
                if (HalloweenModule.isActive) return;
                if (dbVehicle.EngineDisabled) return;

                dbPlayer.SendNewNotification("Motor eingeschaltet!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                client.Vehicle.GetVehicle().SyncExtension.SetEngineStatus(true);

                // Sync Vehicle Lights
                if (dbVehicle.SirensActive)
                {
                    foreach (DbPlayer xPlayer in Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position, 350))
                    {
                        xPlayer.Player.TriggerNewClient("refreshSireneState", dbVehicle.entity, dbVehicle.SirensActive, dbVehicle.SilentSiren);
                    }
                }

                if (dbVehicle.entity.HasData("paintCar"))
                {
                    if (dbVehicle.entity.HasData("origin_color1") && dbVehicle.entity.HasData("origin_color2"))
                    {
                        int color1 = dbVehicle.entity.GetData<int>("origin_color1");
                        int color2 = dbVehicle.entity.GetData<int>("origin_color2");
                        dbVehicle.entity.PrimaryColor = color1;
                        dbVehicle.entity.SecondaryColor = color2;
                        dbVehicle.entity.ResetData("color1");
                        dbVehicle.entity.ResetData("color2");
                        dbVehicle.entity.ResetData("p_color1");
                        dbVehicle.entity.ResetData("p_color2");
                    }

                    dbVehicle.entity.ResetData("paintCar");
                }
            }
            else
            {
                dbPlayer.SendNewNotification("Motor ausgeschaltet!", notificationType: PlayerNotification.NotificationType.ERROR);
                client.Vehicle.GetVehicle().SyncExtension.SetEngineStatus(false);
            }
        }
        
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_TOGGLE_INDICATORS(Player client, string key)
        {
            // Not used anymore

            /*if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent() || !dbPlayer.RageExtension.IsInVehicle || client.VehicleSeat != 0) return;
            var dbVehicle = client.Vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;

            if (!client.Vehicle.HasSharedData("INDICATOR_0"))
            {
                client.Vehicle.SetSharedData("INDICATOR_0", true);
            }
            else
            {
                client.Vehicle.ResetSharedData("INDICATOR_0");
            }

            if (!client.Vehicle.HasSharedData("INDICATOR_1"))
            {
                client.Vehicle.SetSharedData("INDICATOR_1", true);
            }
            else
            {
                client.Vehicle.ResetSharedData("INDICATOR_1");
            }*/
        }

        public void handleVehicleLockInside(Player client)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent() || !dbPlayer.RageExtension.IsInVehicle) return;
            SxVehicle dbVehicle = client.Vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;

            if (!dbVehicle.CanInteract) return;

            if (!dbPlayer.CanControl(dbVehicle)) return;
            var l_Handler = new VehicleEventHandler();

            dbVehicle.respawnInteractionState = true;
            if (dbVehicle.SyncExtension.Locked)
            {
                // closed to open
                dbVehicle.SyncExtension.SetLocked(false);
                dbPlayer.SendNewNotification("Fahrzeug aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
            }
            else
            {
                // open to closed
                dbVehicle.SyncExtension.SetLocked(true);
                dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);
                if (dbVehicle.TrunkStateOpen)
                {
                    l_Handler.ToggleTrunkState(dbPlayer, dbVehicle, false);
                }
            }
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_TOGGLE_LOCK(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            handleVehicleLockInside(client);
        }



        public void handleVehicleLockOutside(Player client, Vehicle vehicle)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent()) return;
            var dbVehicle = vehicle.GetVehicle();
            if (dbVehicle == null || !dbVehicle.IsValid()) return;
            if (dbPlayer.Player.Position.DistanceTo(vehicle.Position) > 20f) return;

            if (!dbVehicle.CanInteract) return;

            // check Users rights to toogle Locked state
            if (!dbPlayer.CanControl(dbVehicle)) return;
            var l_Handler = new VehicleEventHandler();

            if (dbVehicle.SyncExtension.Locked)
            {
                // closed to open
                dbVehicle.SyncExtension.SetLocked(false);
                dbPlayer.SendNewNotification("Fahrzeug aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
            }
            else
            {
                // open to closed
                dbVehicle.SyncExtension.SetLocked(true);
                dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);

                if (dbVehicle.TrunkStateOpen)
                {
                    l_Handler.ToggleTrunkState(dbPlayer, dbVehicle, false);
                }
            }
        }

        
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_TOGGLE_LOCK_OUTSIDE(Player client, Vehicle vehicle, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            handleVehicleLockOutside(client, vehicle);
        }


        public void handleVehicleDoorInside(Player client, int door)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent() || !dbPlayer.RageExtension.IsInVehicle) return;
            var dbVehicle = client.Vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;

            if (!dbVehicle.CanInteract) return;
            // validate player opens a doors with permission
            var userseat = client.VehicleSeat;

            // validate player can open right doors
            if (userseat != 0 && userseat != door)
            {
                return;
            }
            // trunk handling
            if (door == 5)
            {
                // Locked vehicle can only close open doors
                if (dbVehicle.SyncExtension.Locked)
                {
                    dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }

                var l_Handler = new VehicleEventHandler();
                if (dbVehicle.TrunkStateOpen)
                {
                    // trunk was opened    
                    dbPlayer.SendNewNotification("Kofferraum zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    l_Handler.ToggleTrunkState(dbPlayer, dbVehicle, false);
                    return;
                }
                else
                {
                    // trunk was closed
                    dbPlayer.SendNewNotification("Kofferraum aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    l_Handler.ToggleTrunkState(dbPlayer, dbVehicle, true);
                    return;
                }
            }
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public  void REQUEST_VEHICLE_TOGGLE_DOOR(Player client, int door, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            handleVehicleDoorInside(client, door);
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_EJECT(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            try
            {
                if (dbPlayer.Player.VehicleSeat != 0)
                {
                    dbPlayer.SendNewNotification(
                        "Sie muessen Fahrer des Fahrzeuges sein!");
                    return;
                }

                var sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh == null || !sxVeh.IsValid() || sxVeh.GetOccupants() == null || sxVeh.GetOccupants().IsEmpty()) return;

                ComponentManager.Get<EjectWindow>().Show()(dbPlayer, sxVeh);
            }
            catch(Exception e)
            {
                Logger.Crash(e);
            }
        }

        public void handleVehicleDoorOutside(Player client, Vehicle vehicle, int door)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var dbVehicle = vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;
            if (dbPlayer.Player.Position.DistanceTo(vehicle.Position) > 20f) return;

            if (!dbVehicle.CanInteract) return;
            // bikes not supported
            if (dbVehicle.Data.ClassificationId == 2)
            {
                return;
            }

            if (dbVehicle.SyncExtension.Locked)
            {
                dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            // trunk handling -- bleibt 5 weil kommt vom client
            if (door == 5)
            {
                var l_Handler = new VehicleEventHandler();
                if (dbVehicle.TrunkStateOpen)
                {
                    // trunk was opened
                    dbPlayer.SendNewNotification("Kofferraum zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    l_Handler.ToggleTrunkState(dbPlayer, dbVehicle, false);
                    return;
                }
                else
                {
                    // trunk was closed
                    dbPlayer.SendNewNotification("Kofferraum aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    l_Handler.ToggleTrunkState(dbPlayer, dbVehicle, true);
                    return;
                }
            }

            // faction vehicle
            if (dbVehicle.teamid > 0)
            {
                if (dbPlayer.TeamId != dbVehicle.teamid)
                {
                    return;
                }
            }
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_TOGGLE_DOOR_OUTSIDE(Player client, Vehicle vehicle, int door, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            handleVehicleDoorOutside(client, vehicle, door);
        }

        
        [RemoteEventPermission]
        [RemoteEvent]
        public  async void REQUEST_VEHICLE_REPAIR(Player client, Vehicle vehicle, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent() || dbPlayer.RageExtension.IsInVehicle) return;
            var dbVehicle = vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;

            if (dbVehicle.entity.Position.DistanceTo(dbPlayer.Player.Position) > 10.0f) return;

            uint repairKitItem = RepairkitId;

            // verify player has required item
            if (dbPlayer.Container.GetItemAmount(repairKitItem) < 1)
            {
                return;
            }

            var x = new ItemsModuleEvents();
            await x.useInventoryItem(client, dbPlayer.Container.GetSlotOfSimilairSingleItems(repairKitItem), key);

            // verfiy player can interact
            if (dbPlayer.IsInjured() || dbPlayer.IsCuffed)
            {
                dbPlayer.SendNewNotification(
                    "Sie koennen diese Funktion derzeit nicht benutzen.");
            }
        }

        /*
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_TOGGLE_SEATBELT(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent() || !client.IsInVehicle) return;

            if (client.Seatbelt)
            {
                // seatbelt on to off
                client.Seatbelt = false;
                dbPlayer.SendNewNotification("Sitzgurt geöffnet!", title: "", notificationType: PlayerNotification.NotificationType.ERROR);
            }
            else
            {
                // seatbelt off to on
                client.Seatbelt = true;
                dbPlayer.SendNewNotification("Sitzgurt geschlossen!", title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
            }
        }*/
    }
}