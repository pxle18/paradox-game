using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Business.Raffinery;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Farming;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Tattoo;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Business.Raffinery
{
    public class FarmProcessMenuBuilder : MenuBuilder
    {
        public FarmProcessMenuBuilder() : base(PlayerMenu.FarmProcessMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.Menu(Menu, "Verarbeitung");

            menu.Add($"Schließen");

            List<SxVehicle> sxVehicles = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(dbPlayer, 20.0f);
            if(sxVehicles != null && sxVehicles.Count() > 0)
            {
                foreach(SxVehicle sxVehicle in sxVehicles)
                {
                    menu.Add($"{sxVehicle.GetName()} ({sxVehicle.databaseId}) verarbeiten");
                }
            }
            
            return menu;
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
                    return false;
                }

                var farmProcess = FarmProcessModule.Instance.GetByPosition(dbPlayer.Player.Position);
                if (farmProcess == null) return false;

                if (dbPlayer.RageExtension.IsInVehicle) return false;

                List<SxVehicle> sxVehicles = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(dbPlayer, 20.0f);
                if (sxVehicles != null && sxVehicles.Count() > 0)
                {
                    int count = 1;
                    foreach (SxVehicle sxVehicle in sxVehicles)
                    {
                        if (index == count)
                        {
                            NAPI.Task.Run(() =>
                            {
                                // Fahrzeug verarbeiten
                                if (sxVehicle == null || !sxVehicle.IsValid()) return;
                                if (!sxVehicle.CanInteract) return;
                                // Motor muss aus sein
                                if (sxVehicle.SyncExtension.EngineOn)
                                {
                                    dbPlayer.SendNewNotification("Motor muss ausgeschaltet sein!");
                                    return;
                                }
                                // zugeschlossen
                                if (!sxVehicle.SyncExtension.Locked)
                                {
                                    dbPlayer.SendNewNotification("Fahrzeug muss zugeschlossen sein!");
                                    return;
                                }
                                if (sxVehicle.TrunkStateOpen)
                                {
                                    dbPlayer.SendNewNotification("Der Kofferaum muss zugeschlossen sein!");
                                    return;
                                }

                                FarmProcessModule.Instance.FarmProcessAction(farmProcess, dbPlayer, sxVehicle.Container, sxVehicle);
                                MenuManager.DismissCurrent(dbPlayer);
                                return;
                            });
                        }
                        else count++;
                    }
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}