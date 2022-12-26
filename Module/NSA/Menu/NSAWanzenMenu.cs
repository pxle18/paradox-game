using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.NSA.Menu
{
    public class NSAWanzenMenuBuilder : MenuBuilder
    {
        public NSAWanzenMenuBuilder() : base(PlayerMenu.NSAWanzeMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.NativeMenu(Menu, "NSA Aktive Wanzen");
            l_Menu.Add($"Schließen");
            l_Menu.Add("Abhören beenden");

            foreach (NSAWanze nSAPeilsender in NSAObservationModule.NSAWanzen.ToList().Where(w => w.active))
            {
                l_Menu.Add($"{nSAPeilsender.Name}");
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
                if(index == 0)
                {
                    return true;
                }
                else if(index == 1)
                {
                    NSAWanze nsaWanze = NSAObservationModule.NSAWanzen.ToList().Where(w => w.HearingAgents.Contains(dbPlayer)).FirstOrDefault();

                    if (nsaWanze != null)
                    {
                        dbPlayer.SendNewNotification("Abhören beendet.");

                        nsaWanze.HearingAgents.Remove(dbPlayer);
                        return true;
                    }

                    dbPlayer.SendNewNotification("Sie haben keine aktive Wanze.");
                    return true;
                }

                int i = 2;

                foreach (NSAWanze nSAPeilsender in NSAObservationModule.NSAWanzen.ToList().Where(w => w.active))
                {
                    if(i == index)
                    {
                        if(nSAPeilsender.PlayerId != 0)
                        {
                            DbPlayer target = Players.Players.Instance.GetByDbId(nSAPeilsender.PlayerId);
                            if (target == null || !target.IsValid()) return true;

                            if (!dbPlayer.RageExtension.IsInVehicle) return true;

                            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();

                            if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.Data.Id != 1296) return true;

                            NSAWanze nsaWanze = NSAObservationModule.NSAWanzen.ToList().Where(w => w.HearingAgents.Contains(dbPlayer)).FirstOrDefault();

                            if (nsaWanze != null)
                            {
                                dbPlayer.SendNewNotification("Du hörst bereits eine Wanze ab!");
                            }


                            if (dbPlayer.HasData("current_caller"))
                            {
                                dbPlayer.SendNewNotification("Während eines Anrufes kannst du keine Wanze mithören!");
                                return false;
                            }

                            if(dbPlayer.Player.Position.DistanceTo(target.Player.Position) > 400)
                            {
                                dbPlayer.SendNewNotification("Kein Empfang zur Wanze..");
                                return false;
                            }

                            nSAPeilsender.HearingAgents.Add(dbPlayer);


                            // Orten
                            dbPlayer.SendNewNotification("Wanze wird gestartet...!");
                            return true;
                        }
                        
                        return true;
                    }
                    i++;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
