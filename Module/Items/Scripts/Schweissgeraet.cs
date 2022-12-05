using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Banks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Laboratories;
using VMP_CNR.Module.MAZ;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Robbery;
using VMP_CNR.Module.Shops;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static int SchweissgeraetTimeToBreakDoor = 90000;

        public static async Task<bool> Schweissgereat(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract()) return false;
            if (!ServerFeatures.IsActive("schweissgeraet"))
            {
                dbPlayer.SendNewNotification("Dieses Feature ist derzeit deaktiviert. Wir arbeiten an der Lösung des Problems und melden uns, sobald das Schweißgerät wieder zur Verfügung steht.");
                return false;
            }
            // Put PlayerDimension in Dimension[0] Prevent AntiCheat-Messages
            dbPlayer.Dimension[0] = dbPlayer.Player.Dimension;

            // Check Door
            if (dbPlayer.TryData("doorId", out uint doorId))
            {
                if(dbPlayer.Dimension[0] != 0)
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("anticheat",
                                $"ANTICHEAT (Door Break in Abuse Dimension) {dbPlayer.GetName()}");
                    return false;
                }
                var door = DoorModule.Instance.Get(doorId);

                if (door != null)
                {
                    if (!door.OpenWithWelding || door.AdminUnbreakable || door.OpenWithHacking) return false;
                    if (!door.Locked)
                    {
                        dbPlayer.SendNewNotification("Tuer ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (door.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    int time = SchweissgeraetTimeToBreakDoor;

                    Chats.sendProgressBar(dbPlayer, time);

                    if (!door.LessSecurity || door.LessSecurityChanged.AddMinutes(5) < DateTime.Now)
                    {
                        // Mind 8 soldaten ID && SG
                        if(door.Group == 1 && TeamModule.Instance.Get((int)teams.TEAM_ARMY).GetTeamMembers().Where(ip => ip != null && ip.IsValid() && ip.Duty).Count() > 5)
                        {
                            TeamModule.Instance.SendChatMessageToDepartments($"Es wird gerade versucht eine Sicherheitstuer aufzubrechen! Cunningham-Cooperation Secure System - Object {door.Name}!");
                        }
                        else TeamModule.Instance.SendChatMessageToDepartments($"Es wird gerade versucht eine Sicherheitstuer aufzubrechen! Cunningham-Cooperation Secure System - Object {door.Name}!");
                    }

                    Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.WELDING, true);

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetCannotInteract(true);

                    await Task.Delay(time);

                    if (dbPlayer.IsInjured())
                    {
                        dbPlayer.SendNewNotification("Vorgang abgebrochen - du bist verletzt am Boden!");
                        dbPlayer.SetCannotInteract(false);
                        dbPlayer.StopAnimation();
                        dbPlayer.ApplyDeathEffects();
                        return true;
                    }

                    dbPlayer.SetCannotInteract(false);
                    if (!dbPlayer.CanInteract()) return true;

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    door.Break();

                    if (door.LessSecurity && door.LessSecurityChanged.AddMinutes(5) > DateTime.Now) TeamModule.Instance.SendChatMessageToDepartments($"Es wird gerade versucht eine Sicherheitstuer aufzubrechen! Cunningham-Cooperation Secure System - Object {door.Name}!");

                    dbPlayer.SendNewNotification("Tuer aufgebrochen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                    dbPlayer.StopAnimation();
                    return true;
                }
            }

            // Check Jumppoint
            if (dbPlayer.TryData("jumpPointId", out int jumpPointId))
            {
                var jumpPoint = JumpPointModule.Instance.Get(jumpPointId);
                if (jumpPoint != null)
                {
                    if (!jumpPoint.Unbreakable) return false;
                    if (jumpPoint.AdminUnbreakable) return false;

                    if (dbPlayer.Dimension[0] != 0)
                    {
                        Players.Players.Instance.SendMessageToAuthorizedUsers("anticheat",
                                    $"ANTICHEAT (Jumppoint Break in Abuse Dimension) {dbPlayer.GetName()}");
                        return false;
                    }

                    if (!jumpPoint.Locked)
                    {
                        dbPlayer.SendNewNotification("Eingang ist bereits aufgeschlossen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }

                    if (jumpPoint.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    int time = SchweissgeraetTimeToBreakDoor;

                    Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByJumppointId(jumpPointId);
                    Weaponlaboratory weaponlaboratory = WeaponlaboratoryModule.Instance.GetLaboratoryByJumppointId(jumpPointId);
                    Cannabislaboratory cannabislaboratory = CannabislaboratoryModule.Instance.GetLaboratoryByJumppointId(jumpPointId);
                    Heroinlaboratory heroinlaboratory = HeroinlaboratoryModule.Instance.GetLaboratoryByJumppointId(jumpPointId);

                    if (methlaboratory != null)
                    {
                        time = LaboratoryModule.TimeToBreakDoor;
                        if (!MethlaboratoryModule.Instance.CanMethLaboratyRaided(methlaboratory, dbPlayer))
                        {
                            dbPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                            return false;
                        }
                        methlaboratory.LaborMemberCheckedOnHack = true;
                        TeamModule.Instance.Get(methlaboratory.TeamId).SendNotification("Das Sicherheitssystem des Methlabors meldet einen Alarm...", time:30000);
                    }
                    else if (weaponlaboratory != null)
                    {
                        time = LaboratoryModule.TimeToBreakDoor;
                        if (!WeaponlaboratoryModule.Instance.CanWeaponLaboratyRaided(weaponlaboratory, dbPlayer))
                        {
                            dbPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                            return false;
                        }
                        weaponlaboratory.LaborMemberCheckedOnHack = true;
                        TeamModule.Instance.Get(weaponlaboratory.TeamId).SendNotification("Das Sicherheitssystem des Waffenlabors meldet einen Alarm...", time: 30000);
                    }
                    else if (cannabislaboratory != null)
                    {
                        time = LaboratoryModule.TimeToBreakDoor;
                        if (!CannabislaboratoryModule.Instance.CanCannabislaboratyRaided(cannabislaboratory, dbPlayer))
                        {
                            dbPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                            return false;
                        }

                        cannabislaboratory.LaborMemberCheckedOnHack = true;
                        TeamModule.Instance.Get(cannabislaboratory.TeamId).SendNotification("Das Sicherheitssystem des Cannabislabors meldet einen Alarm...", time: 30000);
                    }
                    else if (heroinlaboratory != null)
                    {
                        time = LaboratoryModule.TimeToBreakDoor;
                        if (!HeroinlaboratoryModule.Instance.CanHeroinlaboratyRaided(heroinlaboratory, dbPlayer))
                        {
                            dbPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                            return false;
                        }

                        heroinlaboratory.LaborMemberCheckedOnHack = true;
                        TeamModule.Instance.Get(heroinlaboratory.TeamId).SendNotification("Das Sicherheitssystem des Heroinlabors meldet einen Alarm...", time: 30000);
                    }
                    else
                    {
                        TeamModule.Instance.SendChatMessageToDepartments($"Es wird gerade versucht eine Sicherheitstuer aufzubrechen! Cunningham-Cooperation Secure System - Object {jumpPoint.Name}!");
                    }

                    Chats.sendProgressBar(dbPlayer, time);

                    Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.WELDING, true);

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetCannotInteract(true);

                    await Task.Delay(time);

                    dbPlayer.SetCannotInteract(false);

                    if (!dbPlayer.CanInteract()) return true;

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    jumpPoint.Locked = false;
                    jumpPoint.LastBreak = DateTime.Now;
                    jumpPoint.Destination.Locked = false;
                    jumpPoint.Destination.LastBreak = DateTime.Now;

                    dbPlayer.SendNewNotification("Eingang aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    dbPlayer.StopAnimation();
                    return true;
                }
            }

            // Shoprob
            var closestShop = ShopsModule.Instance.GetRobableShopAtPos(dbPlayer.Player.Position, 12.0f);

            // Shop
            if (closestShop != null && closestShop.RobPosition.X != 0 && dbPlayer.Player.Position.DistanceTo(closestShop.RobPosition) < 1.5)
            {
                if (dbPlayer.Dimension[0] != 0)
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("anticheat",
                                $"ANTICHEAT (Shoprob in Abuse Dimension) {dbPlayer.GetName()}");
                    return false;
                }

                if (dbPlayer.Level < 5)
                {
                    dbPlayer.SendNewNotification("Du musst mindestens Level 5 sein um einen Shop auszurauben!");
                    return true;
                }

                if (RobberyModule.Instance.IsAnyShopInRobbing())
                {
                    dbPlayer.SendNewNotification("Ein Store wird bereits ausgeraubt!");
                    return true;
                }

                if (RobberyModule.Instance.Get((int)closestShop.Id) != null)
                {
                    dbPlayer.SendNewNotification(
                        "Dieser Store wurde bereits ausgeraubt!");
                    return true;
                }

                if (TeamModule.Instance.DutyCops < 8 && !Configurations.Configuration.Instance.DevMode)
                {
                    dbPlayer.SendNewNotification(
                        "Es muessen mindestens 8 Beamte im Dienst sein!");
                    return true;
                }


                if (dbPlayer.Player.Dimension != 0)
                {
                    DatabaseLogging.Instance.LogAdminAction(dbPlayer.Player, dbPlayer.GetName(), adminLogTypes.perm,
                        "Community-Ausschluss Shop Auto Cheat", 0, Configurations.Configuration.Instance.DevMode);
                    Players.Players.Instance.SendMessageToAuthorizedUsers("anticheat",
                        "Haus Bug Use " + dbPlayer.GetName());
                    dbPlayer.warns[0] = 3;
                    SocialBanHandler.Instance.AddEntry(dbPlayer.Player);
                    dbPlayer.Player.SendNotification("ANTI CHEAT (IM SUPPORT MELDEN) (dimension not 0)");
                    dbPlayer.Player.Kick("ANTI CHEAT (IM SUPPORT MELDEN)!");
                    return true;
                }

                dbPlayer.StopAnimation();
                if (!dbPlayer.IsCuffed && !dbPlayer.IsTied && !dbPlayer.IsInjured())
                {
                    RobberyModule.Instance.Add((int)closestShop.Id, dbPlayer, 1, copinterval: Utils.RandomNumber(2, 10), endinterval: Utils.RandomNumber(25, 35));
                }

                dbPlayer.SendNewNotification("Sie beginnen nun damit den Tresor aufzuschweißen!");

                Chats.sendProgressBar(dbPlayer, 60000);

                Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.WELDING, true);

                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetCannotInteract(true);

                await Task.Delay(60000);

                dbPlayer.SetCannotInteract(false);
                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();

                dbPlayer.SendNewNotification("Tresor aufgebrochen! Du wirst regelmäßig Geld erhalten.", notificationType: PlayerNotification.NotificationType.SUCCESS);
                return true;
            }

            Bank bank = BankModule.Instance.GetAll().Values.ToList().Where(b => b.Position.DistanceTo(dbPlayer.Player.Position) < 1.0f).FirstOrDefault();

            if(bank != null && bank.Type == 1) // 1 = ATM Only
            {
                if (dbPlayer.Dimension[0] != 0)
                {
                    Players.Players.Instance.SendMessageToAuthorizedUsers("anticheat",
                                $"ANTICHEAT (ATM Break in Abuse Dimension) {dbPlayer.GetName()}");
                    return false;
                }

                if (dbPlayer.Level < 5)
                {
                    dbPlayer.SendNewNotification("Du musst mindestens Level 5 sein um einen ATM auszurauben!");
                    return false;
                }

                // Restart Sperre & ausgewählte atms only
                if(!bank.ActivatedToBreak || !RobberyModule.Instance.CanAtmRobbed())
                {
                    dbPlayer.SendNewNotification("Das Sicherheitssystem ist aktiv, aufbrechen wäre aktuell zu riskant!");
                    return false;
                }

                if (dbPlayer.IsACop() || dbPlayer.IsAGangster()) return false; // ehm neeee!

                if(RobberyModule.Instance.RobbedAtms.ContainsKey(dbPlayer.Id) && RobberyModule.Instance.RobbedAtms[dbPlayer.Id] >= 3)
                {
                    dbPlayer.SendNewNotification("Du hast bereits zu viele Automaten ausgeraubt!");
                    return false;
                }

                if (bank.LastBreaked.AddMinutes(BankModule.MinToBreakAgain) > DateTime.Now)
                {
                    dbPlayer.SendNewNotification("Dieser Automat wurde bereits aufgeschweißt!");
                    return false;
                }

                // Update Last Breaked
                bank.UpdateLastBreaked();

                dbPlayer.SendNewNotification("Sie beginnen nun damit den Automaten aufzuschweißen!");

                TeamModule.Instance.SendChatMessageToDepartmentsInRange("Ein Bankautomat wird in ihrer Nähe aufgebrochen...", bank.Position, 100);

                Chats.sendProgressBar(dbPlayer, 100000);

                Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.WELDING, true);

                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetCannotInteract(true);

                await Task.Delay(100000);

                dbPlayer.SetCannotInteract(false);
                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured() || dbPlayer.Player.Position.DistanceTo(bank.Position) > 2.0f) return false;
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();

                if (RobberyModule.Instance.RobbedAtms.ContainsKey(dbPlayer.Id))
                {
                    RobberyModule.Instance.RobbedAtms[dbPlayer.Id]++;
                }
                else RobberyModule.Instance.RobbedAtms.Add(dbPlayer.Id, 1);

                dbPlayer.SendNewNotification("Bankautomat aufgebrochen! .", notificationType: PlayerNotification.NotificationType.SUCCESS);


                Random rnd = new Random();
                var erhalt = rnd.Next(Convert.ToInt32(bank.ActMoney * 0.5), bank.ActMoney);

                // cap auf 15k
                if(erhalt > 15000)
                {
                    erhalt = rnd.Next(10000, 16000);
                }

                dbPlayer.Container.AddItem(RobberyModule.MarkierteScheineID, erhalt);
                dbPlayer.SendNewNotification($"${erhalt} erbeutet!", title: "Raubüberfall");

                bank.ActMoney -= erhalt;
                return false;
            }

            StaatsbankTunnel tunnel = StaatsbankRobberyModule.Instance.StaatsbankTunnels.Where(t => t.IsActiveForTeam == dbPlayer.Team).FirstOrDefault();

            // Staatsbankrob
            if (tunnel != null && StaatsbankRobberyModule.Instance.IsActive && StaatsbankRobberyModule.Instance.RobberTeam == dbPlayer.Team)
            {
                if(!tunnel.IsOutsideOpen && dbPlayer.Player.Position.DistanceTo(tunnel.Position) < 3.0f)
                {
                    dbPlayer.SendNewNotification("Sie beginnen nun damit die Gitterstäbe aufzuschweißen!");

                    Chats.sendProgressBar(dbPlayer, 60000);

                    Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.WELDING, true);

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetCannotInteract(true);

                    await Task.Delay(60000);

                    dbPlayer.SetCannotInteract(false);
                    if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.StopAnimation();

                    dbPlayer.SendNewNotification("Gitterstäbe aufgeschweißt!", notificationType: PlayerNotification.NotificationType.SUCCESS);

                    dbPlayer.Team.SendNotification("Die Gitterstäbe wurden aufgeschweißt, es kann nun ein Tunnel gegraben werden!");

                    tunnel.IsOutsideOpen = true;
                    return true;
                }
            }

            if (StaatsbankRobberyModule.Instance.IsActive && StaatsbankRobberyModule.Instance.RobberTeam != null && dbPlayer.Team == StaatsbankRobberyModule.Instance.RobberTeam)
            {
                foreach (StaticContainer staticContainer in StaticContainerModule.Instance.GetAll().Values.ToList())
                {
                    if (staticContainer.Locked && staticContainer.Position.DistanceTo(dbPlayer.Player.Position) <= staticContainer.Range)
                    {
                        if (staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK1 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK2 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK3
                            || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK4 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK5 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK6
                            || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK7 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK8)
                        {
                            if(StaatsbankRobberyModule.Instance.CountInBreakTresor >= 2)
                            {
                                dbPlayer.SendNewNotification("Es können nur maximal 2 Schließfächer zur selben Zeit geöffnet werden!");
                                return false;
                            }

                            int time = 480000;
                            if (Configuration.Instance.DevMode) time = 60000;
                            // Aufschließen lul
                            dbPlayer.SendNewNotification("Sie beginnen nun damit das Schließfach aufzuschweißen!");
                            StaatsbankRobberyModule.Instance.CountInBreakTresor++;

                            Chats.sendProgressBar(dbPlayer, time);

                            Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.WELDING, true);

                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetCannotInteract(true);

                            await Task.Delay(time);

                            dbPlayer.SetCannotInteract(false);
                            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.StopAnimation();

                            dbPlayer.SendNewNotification("Schließfach aufgeschweißt!", notificationType: PlayerNotification.NotificationType.SUCCESS);

                            StaatsbankRobberyModule.Instance.LoadContainerBankInv(staticContainer.Container);
                            staticContainer.Locked = false;
                            StaatsbankRobberyModule.Instance.CountInBreakTresor--;
                            return true;
                        }
                    }
                }
            }

            if (VespucciBankRobberyModule.Instance.IsActive && VespucciBankRobberyModule.Instance.RobberTeam != null && dbPlayer.Team == VespucciBankRobberyModule.Instance.RobberTeam)
            {
                foreach (StaticContainer staticContainer in StaticContainerModule.Instance.GetAll().Values.ToList())
                {
                    if (staticContainer.Locked && staticContainer.Position.DistanceTo(dbPlayer.Player.Position) <= staticContainer.Range)
                    {
                        if (staticContainer.Id == (uint)StaticContainerTypes.VESPUCCIBANK1 || staticContainer.Id == (uint)StaticContainerTypes.VESPUCCIBANK2
                            || staticContainer.Id == (uint)StaticContainerTypes.VESPUCCIBANK3 || staticContainer.Id == (uint)StaticContainerTypes.VESPUCCIBANK4
                            || staticContainer.Id == (uint)StaticContainerTypes.VESPUCCIBANK5)
                        {
                            if (VespucciBankRobberyModule.Instance.CountInBreakTresor >= 1)
                            {
                                dbPlayer.SendNewNotification("Es kann nur maximal 1 Schließfach zur selben Zeit geöffnet werden!");
                                return false;
                            }

                            int time = 480000;
                            if (Configuration.Instance.DevMode) time = 60000;
                            // Aufschließen lul
                            dbPlayer.SendNewNotification("Sie beginnen nun damit das Schließfach aufzuschweißen!");
                            VespucciBankRobberyModule.Instance.CountInBreakTresor++;

                            Chats.sendProgressBar(dbPlayer, time);

                            Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.WELDING, true);

                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetCannotInteract(true);

                            await Task.Delay(time);

                            dbPlayer.SetCannotInteract(false);
                            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.StopAnimation();

                            dbPlayer.SendNewNotification("Schließfach aufgeschweißt!", notificationType: PlayerNotification.NotificationType.SUCCESS);

                            VespucciBankRobberyModule.Instance.LoadContainerBankInv(staticContainer.Container);
                            staticContainer.Locked = false;
                            VespucciBankRobberyModule.Instance.CountInBreakTresor--;
                            return true;
                        }
                    }
                }
            }

            if(MAZModule.Instance.IsMAZActive())
            {
                StaticContainer staticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.MAZ);
                if (staticContainer != null && staticContainer.Position.DistanceTo(dbPlayer.Player.Position) < 8.0f)
                {
                    MAZ.MAZ maz = MAZModule.Instance.GetAll().Values.Where(m => m.IsActive).FirstOrDefault();

                    if (maz == null || maz.Position.DistanceTo(dbPlayer.Player.Position) > 20.0f) return false;

                    if (!dbPlayer.IsAGangster() && !dbPlayer.IsBadOrga() && dbPlayer.TeamId != (uint)teams.TEAM_ARMY) return false;

                    if(!staticContainer.Locked)
                    {
                        dbPlayer.SendNewNotification("Die Fracht ist bereits offen!");
                        return false;
                    }

                    if(MAZModule.Instance.MAZIsSomeoneOpening)
                    {
                        dbPlayer.SendNewNotification("Die Fracht wird bereits aufgeschweißt!");
                        return false; ;
                    }
                    MAZModule.Instance.MAZIsSomeoneOpening = true;
                    int time = 900000; // normal 15 min
                    if (dbPlayer.TeamId == (int)teams.TEAM_ARMY) time = 600000; // Army 10 min
                    if (Configuration.Instance.DevMode) time = 60000;

                    // Aufschließen lul
                    dbPlayer.SendNewNotification("Sie beginnen nun damit die Fracht zu öffnen!");

                    Chats.sendProgressBar(dbPlayer, time);

                    Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.WELDING, true);

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetCannotInteract(true);

                    await Task.Delay(time);

                    if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured())
                    {
                        MAZModule.Instance.MAZIsSomeoneOpening = false;
                        return true;
                    }
                    dbPlayer.SetCannotInteract(false);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.StopAnimation();

                    dbPlayer.SendNewNotification("Fracht geöffnet!", notificationType: PlayerNotification.NotificationType.SUCCESS);

                    staticContainer.Locked = false;
                    MAZModule.Instance.MAZIsSomeoneOpening = false;
                    return true;
                }
            }

            if (LifeInvaderRobberyModule.Instance.IsActive && LifeInvaderRobberyModule.Instance.RobberTeam != null && dbPlayer.Team == LifeInvaderRobberyModule.Instance.RobberTeam)
            {
                foreach (StaticContainer staticContainer in StaticContainerModule.Instance.GetAll().Values.ToList())
                {
                    if (staticContainer.Locked && staticContainer.Position.DistanceTo(dbPlayer.Player.Position) <= staticContainer.Range)
                    {
                        if (staticContainer.Id == (uint)StaticContainerTypes.LIFEINVADERROB)
                        {
                            if(!LifeInvaderRobberyModule.Instance.IsHacked)
                            {
                                dbPlayer.SendNewNotification("Sie müssen zuerst die Sicherheitssysteme ausschalten (Hacking)!");
                                return true;
                            }

                            int time = 600000;
                            if (Configuration.Instance.DevMode) time = 60000;
                            // Aufschließen lul
                            dbPlayer.SendNewNotification("Sie beginnen nun damit den Serverschrank aufzuschweißen!");

                            Chats.sendProgressBar(dbPlayer, time);

                            Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.WELDING, true);

                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetCannotInteract(true);

                            await Task.Delay(time);

                            dbPlayer.SetCannotInteract(false);
                            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.StopAnimation();

                            dbPlayer.SendNewNotification("Serverschrank aufgeschweißt!", notificationType: PlayerNotification.NotificationType.SUCCESS);

                            LifeInvaderRobberyModule.Instance.LoadContainerLifeInvader(staticContainer.Container);
                            staticContainer.Locked = false;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}