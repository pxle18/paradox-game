using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClawModule;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using Logger = VMP_CNR.Module.Logging.Logger;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {

        public static async Task<bool> VehicleClaw(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            bool isStaatClaw = ItemData.Id == 731;


            if ((dbPlayer.TeamId == (int)teams.TEAM_POLICE && isStaatClaw && dbPlayer.IsInDuty()) || (dbPlayer.Team.IsGangsters() && !isStaatClaw))
            {
                //LSPD hat eine Staatskralle oder Badfraktion hat eine Badkralle

                SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
                if (sxVehicle == null)
                {
                    dbPlayer.SendNewNotification("Kein Fahrzeug in der Nähe!");
                    return false;
                }

                if (sxVehicle.WheelClamp > 0)
                {
                    dbPlayer.SendNewNotification("An diesem Fahrzeug ist bereits eine Kralle angebracht...");
                    return false;
                }

                if (isStaatClaw)
                {
                    GTANetworkAPI.NAPI.Task.Run(() => ComponentManager.Get<TextInputBoxWindow>().Show()(
                        dbPlayer, new TextInputBoxWindowObject() { Title = "Parkkralle", Callback = "VehicleClawEvent", Message = "Gib einen Grund ein wieso die Parkkralle angebracht wird." }));
                    return true;
                }
                else
                {
                    await DoVehicleClaw(dbPlayer, sxVehicle, isStaatClaw, "");
                    return true;
                }
            }
            return false;
        }


        public static async Task<bool> RemoveVehicleClaw(DbPlayer dbPlayer, ItemModel ItemData)
        {
            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
            if (sxVehicle == null)
            {
                dbPlayer.SendNewNotification("Kein Fahrzeug in der Nähe!");
                return false;
            }

            if (sxVehicle.WheelClamp == 0)
            {
                dbPlayer.SendNewNotification("An diesem Fahrzeug ist keine Kralle angebracht...");
                return false;
            }

            Chats.sendProgressBar(dbPlayer, 60000);
            dbPlayer.PlayAnimation((int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            await NAPI.Task.WaitForMainThread(60000);

            if (sxVehicle == null || !sxVehicle.IsValid()) return false;

            if (sxVehicle != null && sxVehicle.IsValid() && sxVehicle.entity.Position.DistanceTo(dbPlayer.Player.Position) < 10.0)
            {
                if (dbPlayer.IsInjured() || dbPlayer.IsCuffed || dbPlayer.IsTied) return false;
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();
                sxVehicle.WheelClamp = 0;
                String updateString = $"UPDATE {(sxVehicle.IsTeamVehicle() ? "fvehicles" : "vehicles")} SET WheelClamp = '0' WHERE id = '{sxVehicle.databaseId}'";
                MySQLHandler.ExecuteAsync(updateString);
                dbPlayer.SendNewNotification("Du hast die Parkkralle erfolgreich abgeflexxt.");
                Logger.AddVehicleClawLog(dbPlayer.Id, sxVehicle.databaseId, "", true);
                return true;
            }

            return false;
        }


        public static async Task<bool> DoVehicleClaw(DbPlayer dbPlayer, SxVehicle sxVehicle, bool isStaatsClaw, string reason)
        {
            if (dbPlayer.IsInjured() || dbPlayer.IsCuffed || dbPlayer.IsTied) return false;
            if (reason == "")
            {
                dbPlayer.SendNewNotification("Kein Grund angegeben");
                return false;
            }

            Chats.sendProgressBar(dbPlayer, 60000);
            dbPlayer.PlayAnimation((int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mini@cpr@char_a@cpr_str", "cpr_kol_idle");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            if (sxVehicle.AlarmSystem)
            {
                //Vehicle has alarm system tuned
                if (sxVehicle.ownerId != 0)
                {
                    DbPlayer findPlayer = Players.Players.Instance.FindPlayer(sxVehicle.ownerId);
                    findPlayer.SendNewNotification($"Dein Fahrzeug [{sxVehicle.databaseId}] - [{(sxVehicle.Data.IsModdedCar == 1 ? sxVehicle.Data.mod_car_name : sxVehicle.Data.Model)}] schlägt Alarm", PlayerNotification.NotificationType.ERROR, "HOOKER SECURITY SYSTEM");
                }
                else if (sxVehicle.IsTeamVehicle())
                {
                    Team team = sxVehicle.Team;
                    team.SendNotification("HOOKER SECURITY SYSTEM", $"Dein Fahrzeug [{sxVehicle.databaseId}] - [{(sxVehicle.Data.IsModdedCar == 1 ? sxVehicle.Data.mod_car_name : sxVehicle.Data.Model)}] schlägt Alarm");
                }
            }

            await NAPI.Task.WaitForMainThread(60000);

            if (dbPlayer.IsInjured() || dbPlayer.IsCuffed || dbPlayer.IsTied) return false;
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();

            if (sxVehicle != null && sxVehicle.IsValid() && sxVehicle.entity.Position.DistanceTo(dbPlayer.Player.Position) < 5.0)
            {
                sxVehicle.WheelClamp = isStaatsClaw ? 1 : 2;
                String updateString = $"UPDATE {(sxVehicle.IsTeamVehicle() ? "fvehicles" : "vehicles")} SET WheelClamp = '{sxVehicle.WheelClamp}' WHERE id = '{sxVehicle.databaseId}'";
                MySQLHandler.ExecuteAsync(updateString);

                dbPlayer.SendNewNotification("Du hast die Parkkralle erfolgreich angebracht...");
                Logger.AddVehicleClawLog(dbPlayer.Id, sxVehicle.databaseId, reason, false);
                if (isStaatsClaw)
                {
                    Claw claw = new Claw();
                    //claw.Id = ClawModule.ClawModule.Instance.GetAll().OrderByDescending(c => c.Key).First().Key + 1;
                    claw.Id = ClawModule.ClawModule.Instance.GetAll().Max(c => c.Key) +1;
                    claw.PlayerId = dbPlayer.Id;
                    claw.PlayerName = dbPlayer.GetName();
                    claw.Reason = reason;
                    claw.VehicleId = sxVehicle.databaseId;
                    claw.TimeStamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    claw.Status = true;
                    ClawModule.ClawModule.Instance.Add(claw.Id, claw);
                }

                sxVehicle.SyncExtension.SetEngineStatus(false);
                return true;
            }
            return false;
        }
    }
}
