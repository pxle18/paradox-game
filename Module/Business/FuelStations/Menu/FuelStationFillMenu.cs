using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Business.Raffinery;
using VMP_CNR.Module.Chat;
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

namespace VMP_CNR.Module.Business.FuelStations
{
    public class FuelStationFillMenuBuilder : MenuBuilder
    {
        public FuelStationFillMenuBuilder() : base(PlayerMenu.FuelStationFillMenu)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.NativeMenu(Menu, "Benzin liefern");

            menu.Add($"Schließen");

            List<SxVehicle> sxVehicles = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(dbPlayer, 11.0f);
            if (sxVehicles != null && sxVehicles.Count() > 0)
            {
                foreach (SxVehicle sxVehicle in sxVehicles)
                {
                    menu.Add($"Aus Fahrzeug {sxVehicle.GetName()} ({sxVehicle.databaseId}) abgeben");
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
                
                if (!dbPlayer.RageExtension.IsInVehicle || !dbPlayer.HasData("fillLiter")) return false;

                List<SxVehicle> sxVehicles = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(dbPlayer, 11.0f);
                if (sxVehicles != null && sxVehicles.Count() > 0)
                {
                    int count = 1;
                    foreach (SxVehicle sxVehicle in sxVehicles)
                    {
                        if (index == count)
                        {
                            int liter = dbPlayer.GetData("fillLiter");
                            if (liter < 1 || liter > 10000)
                            {
                                dbPlayer.SendNewNotification("Falsche Menge!");
                                return false;
                            }

                            // check for gas stations
                            var fuel = FuelStationModule.Instance.GetStaionByGas(dbPlayer.Player.Position);
                            if(fuel == null)
                            {
                                fuel = FuelStationModule.Instance.GetThis(dbPlayer.Player.Position);
                            }
                            if (fuel != null)
                            {
                                if (sxVehicle != null && sxVehicle.IsValid())
                                {
                                    // Get Amount FUEL can added
                                    int amountFuelCanTake = fuel.Container.GetMaxItemAddedAmount(FuelStationModule.BenzinModelId);

                                    if(sxVehicle.Container.GetItemAmount(FuelStationModule.BenzinModelId) < amountFuelCanTake)
                                    {
                                        amountFuelCanTake = sxVehicle.Container.GetItemAmount(FuelStationModule.BenzinModelId);
                                    }

                                    if (liter > amountFuelCanTake)
                                    {
                                        dbPlayer.SendNewNotification($"Maximal {amountFuelCanTake} einladbar!");
                                        return false;
                                    }

                                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                                    {
                                        Chats.sendProgressBar(dbPlayer, (100 * liter));

                                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                                        dbPlayer.SetData("userCannotInterrupt", true);
                                        sxVehicle.CanInteract = false;

                                        await Task.Delay(100 * liter);

                                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                                        dbPlayer.ResetData("userCannotInterrupt");

                                        // reset fuel fill counter
                                        if (liter > sxVehicle.Container.GetItemAmount(FuelStationModule.BenzinModelId) || liter > amountFuelCanTake)
                                        {
                                            dbPlayer.SendNewNotification($"Maximal {amountFuelCanTake} einladbar!");
                                            return;
                                        }

                                        sxVehicle.CanInteract = true;

                                        sxVehicle.Container.MoveIntoAnother(fuel.Container, FuelStationModule.BenzinModelId, liter);
                                        dbPlayer.SendNewNotification($"Sie haben {liter} Liter ausgeladen.");
                                        Logging.Logger.AddToFuelStationInsertLog(fuel.Id, dbPlayer.Id, liter);
                                    }));
                                }
                            }
                            MenuManager.DismissCurrent(dbPlayer);
                            return true;
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