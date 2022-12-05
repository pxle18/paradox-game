using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.MAZ;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Brecheisen(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract()) return false;
            // Check Door
            if (dbPlayer.TryData("doorId", out uint doorId))
            {
                var door = DoorModule.Instance.Get(doorId);
                if (door != null)
                {
                    if (door.OpenWithWelding || door.AdminUnbreakable || door.OpenWithHacking) return false;
                    if (!door.Locked)
                    {
                        dbPlayer.SendNewNotification("Tuer ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (door.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                        Chats.sendProgressBar(dbPlayer, 20000);

                    
                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);

                        await Task.Delay(20000);
                        dbPlayer.ResetData("userCannotInterrupt");

                        if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured()) return true;

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                        door.Break();
                        
                        dbPlayer.SendNewNotification("Tuer aufgebrochen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                        dbPlayer.StopAnimation();
                        return true;
                }
            }

            if (dbPlayer.Team.IsGangsters())
            {
                foreach (StaticContainer staticContainer in StaticContainerModule.Instance.GetAll().Values.ToList())
                {
                    if (staticContainer.Locked && staticContainer.Position.DistanceTo(dbPlayer.Player.Position) <= staticContainer.Range)
                    {
                        if (staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR1 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR2 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR3 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR4 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR5 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR6 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR7 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR8 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR9 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR10)
                        {
                            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);

                            DateTime actualDate = DateTime.Now;
                            if (dbPlayer.Team.LastOutfitPreQuest.AddHours(1) >= actualDate)
                            {
                                dbPlayer.SendNewNotification("Die Pre-Quest zur Beschaffung der Outfits ist nicht aktiv!", PlayerNotification.NotificationType.ERROR);
                                return false;
                            }

                            if (room.PlanningOutfitCounter >= 2)
                            {
                                dbPlayer.SendNewNotification("Es können nur maximal 2 Spinde zur selben Zeit geöffnet werden!", PlayerNotification.NotificationType.ERROR);
                                return false;
                            }

                            int time = 180000;
                            if (Configuration.Instance.DevMode) time = 60000;

                            dbPlayer.SendNewNotification("Sie beginnen nun damit den Spind aufzubrechen!", PlayerNotification.NotificationType.INFO);
                            room.PlanningOutfitCounter++;

                            Chats.sendProgressBar(dbPlayer, time);

                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            TeamModule.Instance.SendChatMessageToDepartments($"Es wurde ein nicht autorisierter Zugriff gemeldet! Deltares-Cooperation Secure System - Objekt {staticContainer.Name}!");

                            await Task.Delay(time);

                            dbPlayer.ResetData("userCannotInterrupt");
                            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured()) return true;
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.StopAnimation();

                            dbPlayer.SendNewNotification("Spind aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);

                            staticContainer.Container.ClearInventory();
                            staticContainer.Container.AddItem(PlanningModule.Instance.CasinoRequiredOutfitId, 1);

                            staticContainer.Locked = false;
                            room.PlanningOutfitCounter--;
                            PlanningModule.Instance.LastOutfitRob = DateTime.Now;
                            return true;
                        }
                    }
                }
            }

            // Check Jumppoint
            if (dbPlayer.TryData("jumpPointId", out int jumpPointId))
            {
                var jumpPoint = JumpPointModule.Instance.Get(jumpPointId);
                if (jumpPoint != null)
                {
                    if (jumpPoint.Unbreakable) return false;
                    if (jumpPoint.AdminUnbreakable) return false;
                    if (!jumpPoint.Locked)
                    {
                        dbPlayer.SendNewNotification("Eingang ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (jumpPoint.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    Chats.sendProgressBar(dbPlayer, 30000);

                    
                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetCannotInteract(true);

                    await Task.Delay(30000);

                    dbPlayer.SetCannotInteract(false);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    jumpPoint.Locked = false;
                    jumpPoint.LastBreak = DateTime.Now;
                    jumpPoint.Destination.Locked = false;
                    jumpPoint.Destination.LastBreak = DateTime.Now;

                    dbPlayer.SendNewNotification("Eingang aufgebrochen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                    dbPlayer.StopAnimation();
                    return true;
                }
            }

            var l_Vehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position, 3.0f);
            if (l_Vehicle == null)
                return false;

            if(l_Vehicle.teamid == (uint)teams.TEAM_ARMY && (dbPlayer.IsAGangster() || dbPlayer.IsBadOrga()))
            {
                if(l_Vehicle.Container != null && (l_Vehicle.Container.GetItemAmount(MAZModule.MilitaryChestId) > 0 || l_Vehicle.Container.GetItemAmount(MAZModule.WeaponChestId) > 0))
                {
                    Chats.sendProgressBar(dbPlayer, 45000);

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetCannotInteract(true);

                    await Task.Delay(45000);

                    if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured())
                    {
                        return true;
                    }
                    dbPlayer.SetCannotInteract(false);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                    l_Vehicle.SyncExtension.SetLocked(false);

                    dbPlayer.SendNewNotification("Fahrzeug aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    dbPlayer.StopAnimation();
                }
            } 

            if (l_Vehicle.entity.Model != (uint)VehicleHash.Journey && l_Vehicle.entity.Model != (uint)VehicleHash.Camper)
                return false;

            if (l_Vehicle.SyncExtension.Locked)
            {
                Chats.sendProgressBar(dbPlayer, 30000);
                
                foreach(DbPlayer insidePlayer in l_Vehicle.Visitors.ToList())
                {
                    if (insidePlayer == null || !insidePlayer.IsValid() || insidePlayer.Dimension[0] == 0 || insidePlayer.DimensionType[0] != DimensionType.Camper) continue;
                    insidePlayer.SendNewNotification($"Irgendetwas rappelt an der Tür...");
                }

                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetCannotInteract(true);

                await Task.Delay(30000);

                dbPlayer.SetCannotInteract(false);
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                l_Vehicle.SyncExtension.SetLocked(false);

                dbPlayer.SendNewNotification("Fahrzeug aufgebrochen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                dbPlayer.StopAnimation();
                return true;
            }
            else
            {
                dbPlayer.SendNewNotification( "Fahrzeug ist offen!");
                return false;
            }
        }
    }
}