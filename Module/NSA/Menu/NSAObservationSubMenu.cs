using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;
using VMP_CNR.Module.Voice;

namespace VMP_CNR.Module.NSA.Menu
{
    public class NSAObservationsSubMenuMenuBuilder : MenuBuilder
    {
        public NSAObservationsSubMenuMenuBuilder() : base(PlayerMenu.NSAObservationsSubMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.HasData("nsa_target_player_id")) return null;

            DbPlayer targetOne = Players.Players.Instance.FindPlayerById(p_DbPlayer.GetData("nsa_target_player_id"));
            if (targetOne == null || !targetOne.IsValid()) return null;

            NSAObservation nSAObservation = NSAObservationModule.ObservationList.ToList().FirstOrDefault(o => o.Value.PlayerId == targetOne.Id).Value;
            if (nSAObservation == null) return null;

            var l_Menu = new Module.Menu.Menu(Menu, "NSA Observation (" + targetOne.GetName() + ")");
            l_Menu.Add($"Schließen");


            if (!nSAObservation.Agreed)
            {
                if (p_DbPlayer.IsNSADuty && p_DbPlayer.IsNSAState == (int)NSARangs.LEAD)
                {
                    l_Menu.Add($"Observation genehmigen");
                }
                else
                {
                    l_Menu.Add($"Genehmigung ausstehend!");
                }
            }
            else
            {

                l_Menu.Add($"Observation beenden");
                l_Menu.Add($"Ortung starten/beenden");
                l_Menu.Add($"Banktransaktionen");
                l_Menu.Add($"Fahrzeug Schlüssel");
                l_Menu.Add("Gespräch mithören/beenden");

                if(p_DbPlayer.IsNSAState >= (int)NSA.NSARangs.NORMAL)
                {
                    l_Menu.Add($"Handy clonen (SMS)");
                }
            }
            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (!dbPlayer.HasData("nsa_target_player_id")) return false;

                DbPlayer targetOne = Players.Players.Instance.FindPlayerById(dbPlayer.GetData("nsa_target_player_id"));
                if (targetOne == null || !targetOne.IsValid()) return false;

                NSAObservation nSAObservation = NSAObservationModule.ObservationList.ToList().FirstOrDefault(o => o.Value.PlayerId == targetOne.Id).Value;
                if (nSAObservation == null) return false;

                if (!dbPlayer.IsNSADuty) return false;

                switch (index)
                {
                    case 0:
                        MenuManager.DismissCurrent(dbPlayer);
                        return true;
                    case 1:
                        if (!nSAObservation.Agreed)
                        {
                            if (dbPlayer.IsNSAState == (int)NSARangs.LEAD)
                            {
                                NSAObservationModule.Instance.AgreeObservation(dbPlayer, nSAObservation);
                            }
                            return true;
                        }
                        else
                        {
                            NSAObservationModule.Instance.RemoveObservation(dbPlayer, dbPlayer.GetData("nsa_target_player_id"));
                            dbPlayer.SendNewNotification("Observation beendet!");
                            return true;
                        }
                    case 2:
                        
                        if(dbPlayer.HasData("nsaOrtung"))
                        {
                            dbPlayer.ResetData("nsaOrtung");
                            dbPlayer.SendNewNotification("Ortung beendet!");
                            return true;
                        }

                        dbPlayer.SetData("nsaOrtung", targetOne.Id);
                        dbPlayer.SendNewNotification($"Ortung von {targetOne.GetName()} gestartet!");

                        dbPlayer.Player.TriggerNewClient("setPlayerGpsMarker", targetOne.Player.Position.X, targetOne.Player.Position.Y);
                        Logger.AddFindLog(dbPlayer.Id, targetOne.Id);
                        return true;
                    case 3:
                        Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.NSABankMenu, dbPlayer).Show(dbPlayer);
                        return false;
                    case 4:
                        Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.NSAVehicleObersvationListMenu, dbPlayer).Show(dbPlayer);
                        return false;
                    case 5:
                        if (dbPlayer.HasData("nsa_activePhone"))
                        {
                            dbPlayer.Player.TriggerNewClient("setCallingPlayer", "");
                            dbPlayer.ResetData("nsa_activePhone");
                            dbPlayer.SendNewNotification("Mithören beendet!");
                            return true;
                        }
                        else
                        {
                            // Enable this if list with obersvations is active
                            if (!targetOne.HasData("current_caller")) return false;
                            if (targetOne.IsInAdminDuty()) return false;

                            DbPlayer ConPlayer = TelefonInputApp.GetPlayerByPhoneNumber(targetOne.GetData("current_caller"));
                            if (ConPlayer == null || !ConPlayer.IsValid()) return false;
                            if (ConPlayer.IsInAdminDuty()) return false;

                            if (NSAObservationModule.ObservationList.Where(o => o.Value.PlayerId == ConPlayer.Id || o.Value.PlayerId == targetOne.Id).Count() == 0 && !targetOne.IsACop() && !ConPlayer.IsACop()
                                && !targetOne.IsAMedic() && !ConPlayer.IsAMedic())
                            {
                                dbPlayer.SendNewNotification("Spieler ist nicht fuer eine Observation freigegeben!");
                                return false;
                            }


                            string voiceHashPush = targetOne.VoiceHash + "~3~0~0~2;" + ConPlayer.VoiceHash;
                            dbPlayer.Player.TriggerNewClient("setCallingPlayer", voiceHashPush);

                            dbPlayer.SetData("nsa_activePhone", targetOne.handy[0]);

                            dbPlayer.SendNewNotification("Mithören gestartet " + targetOne.handy[0]);
                            NSAModule.Instance.SendMessageToNSALead($"{dbPlayer.GetName()} hört nun das Telefonat von {targetOne.GetName()} mit.");
                            return false;
                        }

                    case 6:
                        if (dbPlayer.IsNSAState <= (int)NSARangs.LIGHT) return false;

                        if (targetOne == null || !targetOne.IsValid() || targetOne.Id == dbPlayer.Id) return false;

                        if (targetOne.PhoneSettings.flugmodus)
                        {
                            dbPlayer.SendNewNotification("Smartphone konnte nicht gecloned werden!");
                            return false;
                        }

                        dbPlayer.SendNewNotification($"Smartphone von {targetOne.GetName()} wird gecloned!");

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {

                            Chats.sendProgressBar(dbPlayer, (15 * 1000));
                            await NAPI.Task.WaitForMainThread(15 * 1000);

                            if (targetOne == null || !targetOne.IsValid()) return;
                            dbPlayer.SetData("nsa_smclone", targetOne.Id);
                            dbPlayer.SendNewNotification($"Smartphone von {targetOne.GetName()} wurde gecloned!");

                            NSAModule.Instance.SendMessageToNSALead($"{dbPlayer.GetName()} hat das Handy von {targetOne.GetName()} gecloned.");
                        }));
                        return false;
                    default:
                        MenuManager.DismissCurrent(dbPlayer);
                        return true;
                }
            }
        }
    }
}
