using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.VehicleRentShops.Windows;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.VehicleRentShops
{
    public class VehicleRentShopModule : SqlModule<VehicleRentShopModule, VehicleRentShop, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `vehiclerent_shop`;";
        }

        public static int FakeJobVehicleRentShopId = 2500; // + id

        public Dictionary<uint, List<SxVehicle>> ShopRentsVehicles = new Dictionary<uint, List<SxVehicle>>();

        protected override void OnLoaded()
        {
            ShopRentsVehicles = new Dictionary<uint, List<SxVehicle>>();
            base.OnLoaded();
        }

        protected override void OnItemLoaded(VehicleRentShop loadable)
        {
            ShopRentsVehicles.Add(loadable.Id, new List<SxVehicle>());
            base.OnItemLoaded(loadable);
        }

        public override void OnVehicleDeleteTask(SxVehicle sxVehicle)
        {
            foreach (uint key in ShopRentsVehicles.Keys.ToList())
            {
                if (!ShopRentsVehicles.ContainsKey(key)) continue;

                if (ShopRentsVehicles[key].ToList().Contains(sxVehicle))
                {
                    ShopRentsVehicles[key].Remove(sxVehicle);
                }
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            // exits
            if (dbPlayer == null || !dbPlayer.IsValid() || key != Key.E || dbPlayer.RageExtension.IsInVehicle) return false;

            VehicleRentShop vehicleRentShop = VehicleRentShopModule.Instance.GetAll().Values.Where(vr => vr.Position.DistanceTo(dbPlayer.Player.Position) < 5.0f).FirstOrDefault();

            if(vehicleRentShop != null)
            {
                ComponentManager.Get<VehicleRentShopWindow>().Show()(dbPlayer, vehicleRentShop);
                return true;
            }

            return false;
        }

        public override void OnMinuteUpdate()
        {
            foreach(uint key in ShopRentsVehicles.Keys.ToList())
            {
                if (!ShopRentsVehicles.ContainsKey(key)) continue;

                foreach (SxVehicle sxVehicle in ShopRentsVehicles[key].ToList())
                {
                    if (sxVehicle == null || !sxVehicle.IsValid())
                    {
                        ShopRentsVehicles[key].Remove(sxVehicle);
                        continue;
                    }

                    DbPlayer owner = Players.Players.Instance.GetByDbId(sxVehicle.ownerId);
                    if (owner == null || !owner.IsValid() || owner.Player.Position.DistanceTo(sxVehicle.entity.Position) > 100.0f)
                    {
                        if (!sxVehicle.HasData("rentremovecheck"))
                        {
                            sxVehicle.SetData("rentremovecheck", 1);
                        }

                        int gbRemInt = sxVehicle.GetData("rentremovecheck");

                        VehicleRentShop shop = VehicleRentShopModule.Instance.Get(key);
                        if(gbRemInt >= 5 && shop != null && sxVehicle.entity.Position.DistanceTo(shop.Position) < 40.0f) 
                        {
                            VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                            continue;
                        }

                        if (gbRemInt >= 16)
                        {
                            VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                            continue;
                        }
                        else
                        {
                            sxVehicle.SetData("rentremovecheck", gbRemInt + 1);
                            continue;
                        }
                    }
                    else
                    {
                        if (sxVehicle.HasData("rentremovecheck"))
                        {
                            sxVehicle.ResetData("rentremovecheck");
                        }
                    }
                }
            }
        }
    }

    public class VehicleRentShopItemModule : SqlModule<VehicleRentShopItemModule, VehicleRentShopItem, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(VehicleRentShopModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `vehiclerent_shop_items`;";
        }

        protected override void OnItemLoaded(VehicleRentShopItem loadable)
        {

            VehicleRentShop rentShop = VehicleRentShopModule.Instance.Get(loadable.VehicleRentShopId);

            rentShop.ShopItems.Add(loadable);
            base.OnItemLoaded(loadable);
        }
    }


    public class VehicleRentShopSpawnModule : SqlModule<VehicleRentShopSpawnModule, VehicleRentShopSpawn, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(VehicleRentShopModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `vehiclerent_shop_spawns`;";
        }

        protected override void OnItemLoaded(VehicleRentShopSpawn loadable)
        {

            VehicleRentShop rentShop = VehicleRentShopModule.Instance.Get(loadable.VehicleRentShopId);

            rentShop.ShopSpawns.Add(loadable);
            base.OnItemLoaded(loadable);
        }
    }
}
