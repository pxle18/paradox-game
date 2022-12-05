using System;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.AirFlightControl;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Freiberuf.Trucker.Menu
{
    public class TruckerJobStartMenu : MenuBuilder
    {
        public TruckerJobStartMenu() : base(PlayerMenu.TruckerJobStartMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer dbPlayer)
        {
            if (dbPlayer == null) return null;

            TruckerDepot truckerDepot = TruckerDepotLoadModule.Instance.GetByPosition(dbPlayer.Player.Position);
            if (truckerDepot == null) return null;

            Module.Menu.Menu menu = new Module.Menu.Menu(Menu, "Trucker - Lieferungen");

            menu.Add($"Schließen");
            menu.Add($"Aktuelle Lieferung abbrechen");

            if (dbPlayer.HasActiveTruckerQuest()) return menu;

            foreach (TruckerQuest truckerQuest in TruckerQuestLoadModule.Instance.GetAll().Values
                .Where(q => q.SourceTruckerDepot == truckerDepot.Id).ToList())
            {
                TruckerDepot destinationDepot = TruckerDepotLoadModule.Instance.GetAll().Values
                    .FirstOrDefault(a => a.Id == truckerQuest.DestinationTruckerDepot);
                if (destinationDepot == null) continue;

                TimeSpan timeSpan = truckerQuest.avaiableAt - DateTime.Now;

                string usable = (timeSpan.TotalMinutes > 0
                    ? $"[in {Convert.ToInt32(timeSpan.TotalMinutes)} min verfügbar]"
                    : "[verfügbar]");
                menu.Add($"Lieferung nach {destinationDepot.Name} - {usable}");
            }

            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new TruckerJobStartMenu.EventHandler();
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
                    if (!dbPlayer.HasActiveTruckerQuest())
                    {
                        dbPlayer.SendNewNotification("Sie haben keinen aktiven Lieferauftrag!");
                        return true;
                    }

                    SxVehicle sxVeh = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(dbPlayer, 50.0f).FirstOrDefault(
                        cv => cv.Data != null && cv.Data.Id == 40 && cv.databaseId > 0
                    );

                    if (sxVeh != null && sxVeh.IsValid()) {
                        if (sxVeh.HasData("trucker_loaded_" + dbPlayer.Id))
                            sxVeh.ResetData("trucker_loaded_" + dbPlayer.Id);
                    }

                    if (TruckerJobModule.Instance.ActiveQuests.ContainsKey(dbPlayer.Id))
                    {
                        TruckerJobModule.Instance.ActiveQuests.Remove(dbPlayer.Id);
                    }

                    dbPlayer.SendNewNotification("Sie haben ihren aktuellen Lieferauftrag abgebrochen!");

                    return true;
                }
                else
                {
                    int idx = 2;

                    if (dbPlayer.HasActiveTruckerQuest()) return false;

                    TruckerDepot truckerDepot = TruckerDepotLoadModule.Instance.GetByPosition(dbPlayer.Player.Position);
                    if (truckerDepot == null) return true;

                    foreach (TruckerQuest truckerQuest in TruckerQuestLoadModule.Instance
                        .GetAll().Values.Where(q => q.SourceTruckerDepot == truckerDepot.Id).ToList())
                    {
                        TruckerDepot destinationDepot = TruckerDepotLoadModule.Instance.GetAll().Values
                            .FirstOrDefault(a => a.Id == truckerQuest.DestinationTruckerDepot);
                        if (destinationDepot == null) continue;

                        if (idx == index)
                        {
                            if (truckerQuest.avaiableAt > DateTime.Now)
                            {
                                dbPlayer.SendNewNotification("Diese Lieferung ist noch nicht verfügbar!");
                                return true;
                            }

                            // Check if Vehicle is in Range
                            SxVehicle sxVeh = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(dbPlayer, 50.0f).FirstOrDefault(
                                cv => cv.Data != null && cv.Data.Id == 40 && cv.databaseId > 0
                            );

                            if (sxVeh == null || !sxVeh.IsValid())
                            {
                                dbPlayer.SendNewNotification("Du brauchst einen Pounder um diesen Job auszuüben.");
                                return true;
                            }

                            truckerQuest.avaiableAt = DateTime.Now.AddMinutes(truckerQuest.DelayMin);

                            TruckerJobModule.Instance.ActiveQuests.Add(dbPlayer.Id, truckerQuest.Id);

                            dbPlayer.SendNewNotification(
                                $"Auftrag von {truckerDepot.Name} nach {destinationDepot.Name} angenommen.",
                                PlayerNotification.NotificationType.SUCCESS
                            );
                            dbPlayer.SendNewNotification(
                                $"Bitte belade deinen Pounder."
                            );
                            dbPlayer.Player.TriggerNewClient(
                                "setPlayerGpsMarker",
                                destinationDepot.LoadingPoint.X,
                                destinationDepot.LoadingPoint.Y
                            );

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