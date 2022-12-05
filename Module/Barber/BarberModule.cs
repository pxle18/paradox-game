using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Assets.Beard;
using VMP_CNR.Module.Assets.Chest;
using VMP_CNR.Module.Assets.Hair;
using VMP_CNR.Module.Assets.HairColor;
using VMP_CNR.Module.Barber.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tattoo;

namespace VMP_CNR.Module.Barber
{
    public sealed class BarberModule : Module<BarberModule>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(BarberShopModule) };
        }
        
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle) return false;
            if (!dbPlayer.TryData("barberShopId", out uint barberShopId)) return false;
            var barberShop = BarberShopModule.Instance.Get(barberShopId);
            if (barberShop == null) return false;

            if (dbPlayer.Player.Position.DistanceTo(barberShop.Position) > 5.0f) return false;

            int gender = dbPlayer.Customization.Gender;

            try
            {
                NAPI.Task.Run(() =>
                {
                    dbPlayer.Player.ClearAccessory(0);
                    dbPlayer.Player.ClearAccessory(1);
                    dbPlayer.Player.ClearAccessory(2);
                    dbPlayer.Player.ClearAccessory(6);
                    dbPlayer.Player.ClearAccessory(7);
                    TattooShopFunctions.SetTattooClothes(dbPlayer);
                });
                
                if (dbPlayer.Customization.Gender == 0)
                {
                    ComponentManager.Get<BarberShopWindow>().Show()(dbPlayer, BarberShopModule.Instance.MaleListJsonBarberObject[barberShopId]);
                }
                else
                {
                    ComponentManager.Get<BarberShopWindow>().Show()(dbPlayer, BarberShopModule.Instance.FemaleListJsonBarberObject[barberShopId]);
                }

            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return true;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.TryData("barberShopId", out uint barberShopId)) return false;
            switch (colShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("barberShopId", barberShopId);
                    var barberShop = BarberShopModule.Instance.Get(barberShopId);
                    dbPlayer.SendNewNotification("Benutze \"E\" um dir die Haare schneiden zu lassen", title: barberShop.Name, notificationType: PlayerNotification.NotificationType.INFO);
                    return false;
                case ColShapeState.Exit:
                    if (!dbPlayer.HasData("barberShopId")) return false;
                    dbPlayer.ResetData("barberShopId");
                    return false;
                default:
                    return false;
            }
        }
    }
}