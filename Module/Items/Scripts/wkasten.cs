using System;
using System.Linq;
using GTANetworkAPI;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Blitzer;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClawModule;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> wkasten(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract()) return false;

            //if in vehicle remove numberplate
            if (dbPlayer.RageExtension.IsInVehicle)
            {
                SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();

                if (sxVehicle.IsPlayerVehicle() && sxVehicle.ownerId != dbPlayer.Id)
                {
                    dbPlayer.SendNewNotification("Nicht dein Fahrzeug!");
                    return false;
                }

                if (sxVehicle.IsTeamVehicle() && sxVehicle.teamid != dbPlayer.Team.Id)
                {
                    dbPlayer.SendNewNotification("Nicht dein Fahrzeug!");
                    return false;
                }

                if (sxVehicle.SyncExtension.EngineOn)
                {
                    dbPlayer.SendNewNotification("Der Motor des Fahrzeugs muss für diesen Vorgang ausgeschaltet sein");
                    return false;
                }

                sxVehicle.CanInteract = false;
                dbPlayer.SendNewNotification("Nummernschild wird entfernt...");
                Chats.sendProgressBar(dbPlayer, 25000);
                dbPlayer.SetCannotInteract(true);
                await NAPI.Task.WaitForMainThread(25000);
                dbPlayer.SetCannotInteract(false);
                sxVehicle.entity.NumberPlate = "";
                dbPlayer.SendNewNotification("Sie haben das Kennzeichen entfernt");
                sxVehicle.CanInteract = true;

                return true;
            }
            else
            {
                //not in vehicle -> trying to remove PoliceObject
                if (dbPlayer.IsCuffed || dbPlayer.IsTied)
                {
                    return false;
                }
                else
                {
                    SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
                    if (sxVehicle != null && dbPlayer.Team.Id == (int)teams.TEAM_POLICE && dbPlayer.IsInDuty())
                    {
                        if (sxVehicle.WheelClamp == 0)
                        {
                            dbPlayer.SendNewNotification("An diesem Fahrzeug ist keine Kralle angebracht...");
                            return false;
                        }

                        Chats.sendProgressBar(dbPlayer, 60000);
                        dbPlayer.PlayAnimation((int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetCannotInteract(true);
                        await NAPI.Task.WaitForMainThread(60000);
                        dbPlayer.SetCannotInteract(false);
                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.StopAnimation();

                        if (sxVehicle != null && sxVehicle.IsValid() && sxVehicle.entity.Position.DistanceTo(dbPlayer.Player.Position) < 10.0)
                        {
                            if (dbPlayer.IsInjured() || dbPlayer.IsCuffed || dbPlayer.IsTied) return false;
                            sxVehicle.WheelClamp = 0;
                            String updateString = $"UPDATE {(sxVehicle.IsTeamVehicle() ? "fvehicles" : "vehicles")} SET WheelClamp = '0' WHERE id = '{sxVehicle.databaseId}'";
                            MySQLHandler.ExecuteAsync(updateString);
                            dbPlayer.SendNewNotification("Die Parkkralle wurde erfolgreich geöffnet und entfernt.");
                            Logger.AddVehicleClawLog(dbPlayer.Id, sxVehicle.databaseId, "wkasten", true);
                            Claw claw = new Claw();
                            claw.Id = ClawModule.ClawModule.Instance.GetAll().OrderByDescending(c => c.Key).First().Key + 1;
                            claw.PlayerId = dbPlayer.Id;
                            claw.PlayerName = dbPlayer.GetName();
                            claw.Reason = "wkasten";
                            claw.VehicleId = sxVehicle.databaseId;
                            claw.TimeStamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                            claw.Status = false;
                            ClawModule.ClawModule.Instance.Add(claw.Id, claw);
                            return true;
                        }
                        return false;


                    }
                    else
                    {
                        PoliceObject pObject;
                        if ((pObject = PoliceObjectModule.Instance.GetNearest(dbPlayer.Player.Position)) != null)
                        {
                            if (!dbPlayer.Container.CanInventoryItemAdded(pObject.Item, 1))
                            {
                                dbPlayer.SendNewNotification("Dein inventar ist voll!");
                                return false;
                            }

                            // Remove Blitzer if Item is one
                            // TODO JEFF: An deine Blitzer Änderungen anpassen!
                            /*if (pObject.Item.Id == 484)
                            {
                                if (dbPlayer.Team.Id != (uint) teams.TEAM_POLICE && dbPlayer.Team.Id != (uint)teams.TEAM_COUNTYPD) return false;
                                BlitzerModule.Instance.RemoveBlitzer(BlitzerModule.Instance.GetNearestBlitzer(dbPlayer));
                            }
                            else if (pObject.Item.Id == 485)
                            {
                                if (dbPlayer.Team.Id != (uint) teams.TEAM_POLICE && dbPlayer.Team.Id != (uint)teams.TEAM_COUNTYPD) return false;
                                BlitzerModule.Instance.RemoveBlitzer(BlitzerModule.Instance.GetNearestBlitzer(dbPlayer));
                            }*/

                            PoliceObjectModule.Instance.Delete(pObject);
                            dbPlayer.PlayAnimation(
                                (int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetCannotInteract(true);
                            await NAPI.Task.WaitForMainThread(4000);
                            dbPlayer.SetCannotInteract(false);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.StopAnimation();

                            dbPlayer.Container.AddItem(pObject.Item, 1);
                            dbPlayer.SendNewNotification("Erfolgreich entfernt!");
                            return true;
                        }
                    }






                }
            }
            return false;
        }
    }
}