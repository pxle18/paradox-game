using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Business.NightClubs;
using System.Threading.Tasks;
using VMP_CNR.Module.Schwarzgeld;
using VMP_CNR.Module.NSA;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Teams.Shelter;
using VMP_CNR.Handler;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.Animal.Windows
{
    
    /*public class PetBuyShopWindow : Window<Func<DbPlayer, PetShop, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "vehiclerent")] private VehicleRentShop vehiclerent { get; }

            public ShowEvent(DbPlayer dbPlayer, VehicleRentShop vehicleRentShop) : base(dbPlayer)
            {
                vehicleRentShop.ActualizeLeftAmount();

                vehiclerent = vehicleRentShop;
            }
        }

        public PetBuyShopWindow() : base("PetBuy")
        {
        }

        public override Func<DbPlayer, VehicleRentShop, bool> Show()
        {
            return (player, vehiclerent) => OnShow(new ShowEvent(player, vehiclerent));
        }

        [RemoteEvent]
        public async void VehicleRentAction(Player client, uint vehicleRentShopId, uint vehicleRentShopItemId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = client.GetPlayer();

                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                VehicleRentShop vehicleRentShop = VehicleRentShopModule.Instance.GetAll().Values.Where(s => s.Id == vehicleRentShopId && s.Position.DistanceTo(dbPlayer.Player.Position) < 5.0f).FirstOrDefault();


                if (vehicleRentShop != null)
                {
                    vehicleRentShop.ActualizeLeftAmount();

                    if (vehicleRentShop.FreeToRent <= 0)
                    {
                        dbPlayer.SendNewNotification("Keine freien Fahrzeuge zur Verfügung!");
                        return;
                    }

                    SxVehicle sxVehicle = VehicleHandler.Instance.GetJobVehicles().Where(js => js.ownerId == dbPlayer.Id && js.jobid == ((int)VehicleRentShopModule.FakeJobVehicleRentShopId + (int)vehicleRentShop.Id)).FirstOrDefault();
                    if (sxVehicle != null)
                    {
                        dbPlayer.SendNewNotification("Sie haben sich hier bereits ein Fahrzeug gemietet!");
                        return;
                    }

                    VehicleRentShopItem vehicleRentShopItem = vehicleRentShop.ShopItems.Where(i => i.Id == vehicleRentShopItemId).FirstOrDefault();
                    if (vehicleRentShopItem != null)
                    {
                        // Get Spawn
                        VehicleRentShopSpawn vehicleRentShopSpawn = vehicleRentShop.GetFreeSpawnPosition();
                        if (vehicleRentShopSpawn == null)
                        {
                            dbPlayer.SendNewNotification("Kein Ausparkpunkt verfügbar!");
                            return;
                        }

                        if (!dbPlayer.TakeMoney(vehicleRentShopItem.Price))
                        {
                            dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(vehicleRentShopItem.Price));
                            return;
                        }

                        // Spawn Vehicle and set vehicle data
                        SxVehicle rentVeh = VehicleHandler.Instance.CreateServerVehicle(vehicleRentShopItem.VehicleModelId, false, vehicleRentShopSpawn.Position, vehicleRentShopSpawn.Heading, -1, -1, 0, true, true, false, 0, dbPlayer.GetName(), 0, ((int)VehicleRentShopModule.FakeJobVehicleRentShopId + (int)vehicleRentShop.Id), dbPlayer.Id, plate: "Miet KFZ");
                        
                        while (rentVeh.entity == null)
                        {
                            await NAPI.Task.WaitForMainThread(100);
                        }

                        if (rentVeh != null && !VehicleRentShopModule.Instance.ShopRentsVehicles.ContainsKey(vehicleRentShop.Id))
                        {
                            VehicleRentShopModule.Instance.ShopRentsVehicles[vehicleRentShop.Id].Add(rentVeh);
                        }

                        dbPlayer.SendNewNotification($"Sie haben sich für $ {vehicleRentShopItem.Price} ein {rentVeh.GetName()} gemietet!");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
    }*/
}
