using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Asservatenkammer;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Robbery;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Hackingtool(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract()) return false;
            // Check Door
            if (dbPlayer.TryData("doorId", out uint doorId))
            {
                var door = DoorModule.Instance.Get(doorId);
                if (door != null)
                {
                    if (!door.OpenWithWelding && !door.OpenWithHacking) return false;
                    if (door.AdminUnbreakable) return false;
                    if (!door.Locked)
                    {
                        dbPlayer.SendNewNotification("Tuer ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (door.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    // Check Duty Cops
                    if(TeamModule.Instance.DutyCops < 10)
                    {
                        dbPlayer.SendNewNotification("Die Sicherheitssysteme lassen das nicht zu!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }

                    int time = 240000;

                    Chats.sendProgressBar(dbPlayer, time);

                    TeamModule.Instance.SendChatMessageToDepartments($"Es wird gerade versucht eine Sicherheitstuer zu hacken! Cunningham-Cooperation Secure System - Object {door.Name}!");

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetCannotInteract(true);

                    await NAPI.Task.WaitForMainThread(time);

                    dbPlayer.SetCannotInteract(false);

                    if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    door.Break();

                    dbPlayer.SendNewNotification("Tuer aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    dbPlayer.StopAnimation();
                    return true;
                }
            }

            if (dbPlayer.Player.Position.DistanceTo(StaatsbankRobberyModule.HackingPoint) < 2.0f &&
                StaatsbankRobberyModule.Instance.IsActive &&
                StaatsbankRobberyModule.Instance.RobberTeam == dbPlayer.Team)
            {

                Door Maindoor = DoorModule.Instance.Get((uint)StaatsbankRobberyModule.MainDoorId);
                Door Maindoor2 = DoorModule.Instance.Get(Maindoor.Pair);

                Door SideDoor = DoorModule.Instance.Get((uint)StaatsbankRobberyModule.SideDoorId);
                Door SideDoor2 = DoorModule.Instance.Get(SideDoor.Pair);

                if (Maindoor == null || SideDoor == null || Maindoor2 == null || SideDoor2 == null) return false;

                if (StaatsbankRobberyModule.Instance.DoorHacked)
                {
                    dbPlayer.SendNewNotification("Die Sicherheitssysteme blockieren deinen Hackvorgang!");
                    return false;
                }

                Chats.sendProgressBar(dbPlayer, 60000);

                dbPlayer.SendNewNotification("Du beginnst dich in den Computer zu hacken!");

                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@prison_heistig1_P1_guard_checks_bus", "loop");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetData("userCannotInterrupt", true);

                await NAPI.Task.WaitForMainThread(60000);

                dbPlayer.ResetData("userCannotInterrupt");
                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();

                dbPlayer.SendNewNotification("Du konntest durch den Computer die Haupttüren für 5 Minuten verschließen!");

                Maindoor.SetLocked(true);
                Maindoor.LastBreak = DateTime.Now;
                Maindoor2.LastBreak = DateTime.Now;

                SideDoor.SetLocked(true);
                SideDoor.LastBreak = DateTime.Now;
                SideDoor2.LastBreak = DateTime.Now;

                StaatsbankRobberyModule.Instance.DoorHacked = true;
                return true;
            }

            HousesVoltage housevoltage = HousesVoltageModule.Instance.GetAll().Values.Where(hv => hv.Position.DistanceTo(dbPlayer.Player.Position) < 3.0f).FirstOrDefault();
            if (housevoltage != null)
            {
                Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.HackingVoltageMenu, dbPlayer).Show(dbPlayer);
                return false;
            }

            // Vespucci Bank
            if (dbPlayer.Player.Position.DistanceTo(LifeInvaderRobberyModule.Instance.RobPosition) < 2.0f)
            {
                await LifeInvaderRobberyModule.Instance.StartRob(dbPlayer);
                return true;
            }

            if (dbPlayer.Player.Position.DistanceTo(AsservatenkammerModule.AserHackPosition) < 2.0f)
            {
                
                // Nur als Gangler, nur bei mind 20 cops, nur alle 2h, nicht parallel
                if ((!Configurations.Configuration.Instance.DevMode) && (!dbPlayer.IsAGangster() || TeamModule.Instance.DutyCops < 20 || AsservatenkammerModule.Instance.AserHackActive || AsservatenkammerModule.Instance.LastAserHack.AddHours(2) > DateTime.Now))
                {
                    dbPlayer.SendNewNotification("Die Sicherheitssysteme blockieren deinen Hackvorgang!");
                    return false;
                }

                AsservatenkammerModule.Instance.AserHackActive = true;
                Chats.sendProgressBar(dbPlayer, 240000);

                dbPlayer.SendNewNotification("Du beginnst dich in den Computer zu hacken!");

                TeamModule.Instance.SendChatMessageToDepartments("Es wurde ein Sicherheitseinbruch in der LSPD Asservatenkammer gemeldet!");

                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@prison_heistig1_P1_guard_checks_bus", "loop");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetData("userCannotInterrupt", true);

                await NAPI.Task.WaitForMainThread(240000);

                dbPlayer.ResetData("userCannotInterrupt");
                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();

                dbPlayer.SendNewNotification("Du konntest die LSPD Asservatenkammer aufschließen!");
                AsservatenkammerModule.Instance.LastAserHack = DateTime.Now;

                StaticContainer AserKammer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.ASERLSPD);

                // Half Items at rob...
                foreach(Item item in AserKammer.Container.Slots.Values)
                {
                    if(item != null && item.Id > 0)
                    {
                        if (item.Amount > 10) item.Amount = item.Amount / 2;
                    }
                }

                StaticContainerModule.Instance.Get((uint)StaticContainerTypes.ASERLSPD).Locked = false;
                return true;
            }
            return false;
        }
    }
}