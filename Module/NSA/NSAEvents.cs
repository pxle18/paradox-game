using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Telefon.App;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Weapons.Component;

namespace VMP_CNR.Module.NSA
{
    public class NSAFunctions : Script
    {
        [RemoteEvent]
        public void NSACheckNumber(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsNSADuty) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;


            if (Int32.TryParse(returnstring, out int number))
            {
                if (number == 0) return;
                DbPlayer targetPlayer = TelefonInputApp.GetPlayerByPhoneNumber(number);
                if (targetPlayer == null || !targetPlayer.IsValid()) return;

                dbPlayer.SendNewNotification($"Rufnummer {number} ist {targetPlayer.GetName()} zugewiesen!");

                NSAModule.Instance.SendMessageToNSALead($"{dbPlayer.GetName()} hat eine Rufnummerabfrage für die {number} getätigt!");
                return;
            }
            dbPlayer.SendNewNotification("Ungültige Nummer!");
            return;
        }

        [RemoteEvent]
        public void NSASuspendate(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;

            if (!dbPlayer.IsValid() || (dbPlayer.TeamId != (int)TeamTypes.TEAM_FIB && dbPlayer.GovLevel.ToLower() != "a" && dbPlayer.IsNSAState != (int)NSARangs.LEAD)) return;

            DbPlayer target = Players.Players.Instance.FindPlayer(returnstring);
            if (target != null && target.IsValid())
            {
                if(target.Suspension)
                {
                    target.Suspension = false;

                    dbPlayer.SendNewNotification($"Sie haben {target.GetName()} Suspendierung aufgehoben!");
                    TeamModule.Instance.SendChatMessageToDepartments($"{dbPlayer.GetName()} hat die Suspendierung von {target.GetName()} aufgehoben!");

                    target.Save();
                    return;
                }

                if(target.IsACop() || target.TeamId == (int)TeamTypes.TEAM_MEDIC || target.TeamId == (int)TeamTypes.TEAM_DPOS || target.TeamId == (int)TeamTypes.TEAM_DRIVINGSCHOOL)
                {
                    target.RemoveWeapons();
                    target.ResetAllWeaponComponents();
                    target.SetDuty(false);

                    target.Suspension = true;

                    TeamModule.Instance.SendChatMessageToDepartments($"{dbPlayer.GetName()} hat {target.GetName()} des Dienstes suspendiert!");

                    dbPlayer.SendNewNotification($"Sie haben {target.GetName()} des Dienstes suspendiert!");

                    target.Save();
                    return;
                }
            }

            return;
        }

        [RemoteEvent]
        public void NSASetPeilsender(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;

            if (!dbPlayer.IsNSADuty || dbPlayer.IsNSAState < (int)NSA.NSARangs.LIGHT) return;

            // Prüfe Fahrzeug in der Nähe
            SxVehicle sxVeh = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
            if(sxVeh != null && sxVeh.IsValid())
            {
                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    Chats.sendProgressBar(dbPlayer, (20 * 1000));
                    await NAPI.Task.WaitForMainThread(20 * 1000);
                    
                    NSAObservationModule.NSAPeilsenders.Add(new NSAPeilsender() { CreatorId = dbPlayer.Id, Name = returnstring, VehicleId = sxVeh.databaseId, Added = DateTime.Now });
                    dbPlayer.SendNewNotification($"Peilsender ({returnstring}) wurde an Fahrzeug {sxVeh.databaseId} angebracht!");

                    NSAModule.Instance.SendMessageToNSALead($"{dbPlayer.GetName()} hat einen Peilsender ({returnstring}) angebracht!");

                    dbPlayer.Container.RemoveItem(696);
                }));
                return;
            }
            
            return;
        }

        [RemoteEvent]
        public void NSASetWanze(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsNSADuty) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;

            // Prüfe Fahrzeug in der Nähe
            DbPlayer target = Players.Players.Instance.GetClosestPlayerForPlayerWhoIsCuffed(dbPlayer, 1.5f);
            if (target != null && target.IsValid() && target.IsCuffed)
            {
                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    Chats.sendProgressBar(dbPlayer, (8000));

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                    Chats.sendProgressBar(dbPlayer, 8000);
                    await NAPI.Task.WaitForMainThread(8000);

                    dbPlayer.StopAnimation();
                    NSAObservationModule.NSAWanzen.Add(new NSAWanze() { CreatorId = dbPlayer.Id, Name = returnstring, PlayerId = target.Id, Added = DateTime.Now, active = true, HearingAgents = new List<DbPlayer>() }) ;
                    dbPlayer.SendNewNotification($"Wanze ({returnstring}) wurde an Spieler {target.GetName()} angebracht!");

                    NSAModule.Instance.SendMessageToNSALead($"{dbPlayer.GetName()} hat eine Wanze an {target.GetName()} angebracht!");

                    dbPlayer.Container.RemoveItem(1075);
                }));
                return;
            }
            else
            {
                dbPlayer.SendNewNotification("Spieler nicht gefunden, nicht in der Nähe oder keine fesseln!");
            }

            return;
        }

        [RemoteEvent]
        public void NSAClonePlayer(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsNSADuty) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;

            DbPlayer target = Players.Players.Instance.FindPlayer(returnstring);
            if (target != null && target.IsValid())
            {
                dbPlayer.SendNewNotification("Person Identifying System started...");
                dbPlayer.SetData("clonePerson", target.Id);

                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    Chats.sendProgressBar(dbPlayer, (3 * 1000));
                    await NAPI.Task.WaitForMainThread(3 * 1000);
                    
                    dbPlayer.ApplyCharacter();
                    
                }));
            }

            return;
        }

        [RemoteEvent]
        public void BankHistoryCallback(Player p_Player, string p_Name, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid() || !l_DbPlayer.IsNSADuty)
                return;

            if (!MySQLHandler.IsValidNoSQLi(l_DbPlayer, p_Name)) return;

            var l_TargetPlayer = Players.Players.Instance.FindPlayer(p_Name);
            if (l_TargetPlayer == null || !l_TargetPlayer.IsValid())
                return;

            l_DbPlayer.SetData("nsa_bank_check", l_TargetPlayer.Id);
        }

        [RemoteEvent]
        public void NSAAddPhoneHearing(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsNSADuty) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;

            if (Int32.TryParse(returnstring, out int number))
            {
                DbPlayer targetPlayer = TelefonInputApp.GetPlayerByPhoneNumber(number);
                if (targetPlayer == null || !targetPlayer.IsValid()) return;

                // Enable this if list with obersvations is active
                if (!targetPlayer.HasData("current_caller")) return;
                if (targetPlayer.IsInAdminDuty()) return;
                
                DbPlayer ConPlayer = TelefonInputApp.GetPlayerByPhoneNumber(targetPlayer.GetData("current_caller"));
                if (ConPlayer == null || !ConPlayer.IsValid()) return;
                if (ConPlayer.IsInAdminDuty()) return;

                if(NSAObservationModule.ObservationList.Where(o => o.Value.PlayerId == ConPlayer.Id || o.Value.PlayerId == targetPlayer.Id).Count() == 0 && !targetPlayer.IsACop() && !ConPlayer.IsACop()
                    && !targetPlayer.IsAMedic() && !ConPlayer.IsAMedic())
                {
                    dbPlayer.SendNewNotification("Spieler ist nicht fuer eine Observation freigegeben!");
                    return;
                }
                
                //string voiceHashPush = targetPlayer.VoiceHash + "~3~0~0~2;" + ConPlayer.VoiceHash;
                
                dbPlayer.SetData("nsa_activePhone", number);
                
                dbPlayer.SendNewNotification("Mithören gestartet " + targetPlayer.handy[0]);

                NSAModule.Instance.SendMessageToNSALead($"{dbPlayer.GetName()} hört nun das Gespräch von {ConPlayer.GetName()} ab!");
                return;
            }
            dbPlayer.SendNewNotification("Ungültige Nummer!");
            return;
        }

        [RemoteEvent]
        public void NSAChangePhoneNumber(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsNSADuty) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;

            if (Int32.TryParse(returnstring, out int number))
            {
                if (PlayerNameModule.Instance.GetAll().ToList().Where(pn => pn.Value.HandyNr == number && pn.Value.Id != dbPlayer.Id).Count() > 0)
                {
                    dbPlayer.SendNewNotification($"Rufnummer bereits in Benutzung!");
                    return;
                }
                
                if(dbPlayer.HasData("nsaChangedNumber"))
                {
                    uint oldNumber = dbPlayer.GetData("nsaChangedNumber");

                    if(oldNumber == number)
                    {
                        dbPlayer.handy[0] = (uint)number;
                        dbPlayer.SendNewNotification($"Rufnummer zurückgesetzt!");
                        dbPlayer.ResetData("nsaChangedNumber");
                        return;
                    }
                }

                dbPlayer.SetData("nsaChangedNumber", dbPlayer.handy[0]);
                dbPlayer.handy[0] = (uint)number;
                dbPlayer.SendNewNotification($"Rufnummer auf {number} geändert!");
                return;
            }
            dbPlayer.SendNewNotification("Ungültige Nummer!");
            return;
        }

        [RemoteEvent]
        public void AddObservationPlayer(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsNSADuty) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;

            DbPlayer l_FindPlayer = Players.Players.Instance.FindPlayer(returnstring);
            if (l_FindPlayer == null || !l_FindPlayer.IsValid())
                return;

            if(NSAObservationModule.ObservationList.ContainsKey(l_FindPlayer.Id))
            {
                dbPlayer.SendNewNotification("Spieler ist bereits in Observation!");
                return;
            }
            else
            {
                dbPlayer.SetData("nsaAddPlayer", l_FindPlayer.Id);
                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Obersvation", Callback = "AddObservationPlayerReason", Message = "Geben Sie einen Grund für die Observation an:" });
            }
        }


        [RemoteEvent]
        public void AddObservationPlayerReason(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsNSADuty) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;
            if (!dbPlayer.HasData("nsaAddPlayer")) return;
            DbPlayer l_FindPlayer = Players.Players.Instance.GetByDbId(dbPlayer.GetData("nsaAddPlayer"));
            if (l_FindPlayer == null || !l_FindPlayer.IsValid())
                return;

            if (Regex.IsMatch(returnstring, @"^[a-zA-Z ._]+$"))
            {

                if (NSAObservationModule.ObservationList.ContainsKey(l_FindPlayer.Id))
                {
                    dbPlayer.SendNewNotification("Spieler ist bereits in Observation!");
                    return;
                }
                else
                {
                    NSAObservationModule.Instance.AddObservation(dbPlayer, l_FindPlayer, returnstring);
                    dbPlayer.SendNewNotification($"{l_FindPlayer.GetName()} zur Observation hinzugefügt! (Grund: {returnstring})");
                    dbPlayer.ResetData("nsaAddPlayer");
                }
            }
            else
            {
                dbPlayer.SendNewNotification("Grund enthält ungültige Zeichen!");
                return;
            }
        }

        [RemoteEvent]
        public void SetCarColorNSA(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || (!dbPlayer.IsNSADuty && dbPlayer.TeamId != (uint)TeamTypes.TEAM_FIB)) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;
            if (!dbPlayer.HasData("nsa_work_vehicle")) return;

            if (returnstring.Length < 2 || !returnstring.Contains(" ")) return;

            string[] splittedReturn = returnstring.Split(" ");
            if (splittedReturn.Length != 2) return;

            if (!Int32.TryParse(splittedReturn[0], out int color1)) return;
            if (!Int32.TryParse(splittedReturn[1], out int color2)) return;

            SxVehicle sxVehicle = VehicleHandler.Instance.FindTeamVehicle(dbPlayer.TeamId, (uint)dbPlayer.GetData("nsa_work_vehicle"));
            if (sxVehicle == null || !sxVehicle.IsValid()) return;

            sxVehicle.color1 = color1;
            sxVehicle.color2 = color2;

            sxVehicle.entity.PrimaryColor = color1;
            sxVehicle.entity.SecondaryColor = color2;

            dbPlayer.SendNewNotification($"Fahrzeugfarbe auf {color1} {color2} geändert!");
            return;
        }

        [RemoteEvent]
        public void SetCarPlateNSA(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || (!dbPlayer.IsNSADuty && dbPlayer.TeamId != (uint)TeamTypes.TEAM_FIB)) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnstring)) return;
            if (!dbPlayer.HasData("nsa_work_vehicle")) return;

            if (returnstring.Length < 2) return;
            
            SxVehicle sxVehicle = VehicleHandler.Instance.FindTeamVehicle(dbPlayer.TeamId, (uint)dbPlayer.GetData("nsa_work_vehicle"));
            if (sxVehicle == null || !sxVehicle.IsValid()) return;
            
            sxVehicle.plate = returnstring;

            sxVehicle.entity.NumberPlate = returnstring;

            dbPlayer.SendNewNotification($"Kennzeichen auf {returnstring} geändert!");
            return;
        }
    }
}
