using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.AirFlightControl.Menu
{
    public class AirFlightJobStartMenu : MenuBuilder
    {
        public AirFlightJobStartMenu() : base(PlayerMenu.AirFlightJobStartMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer p_DbPlayer)
        {
            if (p_DbPlayer == null) return null;

            AirFlightAirport airFlightAirport = AirFlightAirportModule.Instance.GetByPosition(p_DbPlayer.Player.Position);
            if (airFlightAirport == null) return null;

            var l_Menu = new Module.Menu.NativeMenu(Menu, "Fracht Lieferungen");

            l_Menu.Add($"Schließen");
            l_Menu.Add($"Aktuelle Lieferung abbrechen");

            if (p_DbPlayer.HasActiveAirflightQuest()) return l_Menu;

            foreach (AirFlightAirportQuests airFlightAirportQuest in AirFlightAirportQuestsModule.Instance.GetAll().Values.Where(q => q.SourceAirport == airFlightAirport.Id).ToList())
            {
                AirFlightAirport destinationAirport = AirFlightAirportModule.Instance.GetAll().Values.Where(a => a.Id == airFlightAirportQuest.DestinationAirport).FirstOrDefault();
                if (destinationAirport == null) continue;

                TimeSpan timeSpan = airFlightAirportQuest.avaiableAt - DateTime.Now;

                string usable = (timeSpan.TotalMinutes > 0 ? $"[in {Convert.ToInt32(timeSpan.TotalMinutes)} min verfügbar]" : "[verfügbar]"); 
                l_Menu.Add($"Lieferung nach {destinationAirport.Name} - {usable}");
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else if (index == 1)
                {
                    if (!dbPlayer.HasActiveAirflightQuest())
                    {
                        dbPlayer.SendNewNotification("Sie haben keinen aktiven Lieferauftrag!");
                        return true;
                    }

                    foreach (SxVehicle sxVeh in VehicleHandler.Instance.GetAllVehiclesPlayerCanControl(dbPlayer))
                    {
                        if(sxVeh.HasData("airflight_loaded"))
                        {
                            sxVeh.ResetData("airflight_loaded");
                        }
                    }
                    
                    if(AirFlightJobModule.Instance.ActiveQuests.ContainsKey(dbPlayer.Id))
                    {
                        AirFlightJobModule.Instance.ActiveQuests.Remove(dbPlayer.Id);
                    }
                    dbPlayer.SendNewNotification("Sie ihren aktiven Lieferauftrag abgebrochen!");

                    return true;
                }
                else
                {
                    int idx = 2;

                    if (dbPlayer.HasActiveAirflightQuest()) return false;

                    AirFlightAirport airFlightAirport = AirFlightAirportModule.Instance.GetByPosition(dbPlayer.Player.Position);
                    if (airFlightAirport == null) return true;

                    foreach (AirFlightAirportQuests airFlightAirportQuest in AirFlightAirportQuestsModule.Instance.GetAll().Values.Where(q => q.SourceAirport == airFlightAirport.Id).ToList())
                    {
                        AirFlightAirport destinationAirport = AirFlightAirportModule.Instance.GetAll().Values.Where(a => a.Id == airFlightAirportQuest.DestinationAirport).FirstOrDefault();
                        if (destinationAirport == null) continue;

                        if (idx == index)
                        {
                            if(airFlightAirportQuest.DestinationAirport == 1 || airFlightAirportQuest.SourceAirport == 1)
                            {
                                if(AirFlightControlModule.Instance.TowerPlayers.Count() <= 0)
                                {
                                    dbPlayer.SendNewNotification("Diese Lieferung ist nur verfügbar wenn Air-Control besetzt ist!");
                                    return true;
                                }
                            }

                            if(airFlightAirportQuest.avaiableAt > DateTime.Now)
                            {
                                dbPlayer.SendNewNotification("Diese Lieferung ist noch nicht verfügbar!");
                                return true;
                            }

                            // Check if Vehicle is in Range
                            SxVehicle sxVeh = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(dbPlayer, 50.0f).Where(
                                cv => cv.Data != null && cv.Data.ClassificationId == 9 && cv.databaseId > 0).FirstOrDefault();

                            if(sxVeh == null || !sxVeh.IsValid())
                            {
                                dbPlayer.SendNewNotification("Kein geeignetes Flugzeug in der nähe oder nicht!");
                                return true;
                            }

                            airFlightAirportQuest.avaiableAt = DateTime.Now.AddMinutes(airFlightAirportQuest.DelayMin);

                            AirFlightJobModule.Instance.ActiveQuests.Add(dbPlayer.Id, airFlightAirportQuest.Id);

                            dbPlayer.SendNewNotification($"Auftrag von {airFlightAirport.Name} nach {destinationAirport.Name} angenommen. Bitte laden Sie ihr Flugzeug (markiert auf der Karte (Rote Flugzeug markierung))");
                            dbPlayer.Player.TriggerNewClient("setPlayerGpsMarker", airFlightAirport.LoadingPoint.X, airFlightAirport.LoadingPoint.Y);

                            return true;
                        }
                        else idx++;
                    }

                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
            }
        }
    }
}
