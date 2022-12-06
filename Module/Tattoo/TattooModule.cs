using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Assets.Hair;
using VMP_CNR.Module.Assets.HairColor;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Barber.Windows;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tattoo.Windows;

namespace VMP_CNR.Module.Tattoo
{
    public class ClientTattoo
    {
        public string TattooHash { get; }
        public int ZoneId { get; }
        public int Price { get; }
        public string Name { get; }

        public ClientTattoo(string tattooHash, int zoneId, int price, string name)
        {
            TattooHash = tattooHash;
            ZoneId = zoneId;
            Price = price;
            Name = name;
        }
    }

    public sealed class TattooModule : Module<TattooModule>
    {
        public readonly Vector3 LicenseShopPosition = new Vector3(36.7647, -172.212, 55.3232);

        public override Type[] RequiredModules()
        {
            return new[] {typeof(TattooShopModule), typeof(AssetsTattooZoneModule), typeof(AssetsTattooModule)};
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer == null || dbPlayer.RageExtension.IsInVehicle || !dbPlayer.IsValid()) return false;

            // Handle tattoo license shop.
            if (dbPlayer.Player.Position.DistanceTo(LicenseShopPosition) < 5.0f)
            {
                try
                {
                    if (!dbPlayer.HasTattooShop())
                    {
                        dbPlayer.SendNewNotification(
                            "Du besitzt keinen Tattoo-Shop und kannst entsprechend keine Lizenzen erwerben!",
                            PlayerNotification.NotificationType.ERROR);

                        return true;
                    }

                    var licenses = TattooLicenseModule.Instance.GetAll().Values.ToList();
                    if (licenses.Count == 0)
                    {
                        dbPlayer.SendNewNotification("Es werden aktuell keine Tattoo-Lizenzen zum Kauf angeboten!");

                        return true;
                    }

                    TattooShop tattooShopLicense = dbPlayer.GetTattooShop();
                    if (tattooShopLicense == null)
                    {
                        dbPlayer.SendNewNotification(
                            "Der Lizenzenshop konnte dich keinem Tattooladen zuordnen. Melde dies bitte im PARADOX-Bugtracker!",
                            PlayerNotification.NotificationType.ERROR
                        );

                        return true;
                    }

                    ComponentManager.Get<TattooLicenseWindow>().Show()(
                        dbPlayer,
                        AssetsTattooZoneModule.Instance.GetAll().Values.ToList()
                    );
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }

                // MenuManager.Instance.Build(PlayerMenu.TattooLicensePaginationMenu, dbPlayer).Show(dbPlayer);
                return true;
            }

            if (!dbPlayer.TryData("tattooShopId", out uint tattooShopId)) return false;
            var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
            if (tattooShop == null) return false;

            if (dbPlayer.Player.Position.DistanceTo(tattooShop.Position) > 5.0f) return false;

            if (tattooShop.BusinessId == 0)
            {
                MenuManager.Instance.Build(PlayerMenu.TattooBuyMenu, dbPlayer).Show(dbPlayer);
                return true;
            }

            List<ClientTattoo> cTattooList = new List<ClientTattoo>();

            if (tattooShop.tattooLicenses.Count <= 0)
            {
                return false;
            }

            if (dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Manage &&
                dbPlayer.GetActiveBusinessMember().BusinessId == tattooShop.BusinessId)
            {
                MenuManager.Instance.Build(PlayerMenu.TattooBankMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            else
            {
                foreach (TattooAddedItem tattooAddedItem in tattooShop.tattooLicenses)
                {
                    AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get((uint) tattooAddedItem.AssetsTattooId);
                    if (assetsTattoo.GetHashForPlayer(dbPlayer) != "" && assetsTattoo.ZoneId == 0)
                    {
                        cTattooList.Add(new ClientTattoo(assetsTattoo.GetHashForPlayer(dbPlayer), assetsTattoo.ZoneId,
                            assetsTattoo.Price, assetsTattoo.Name));
                    }
                }

                dbPlayer.SetTattooClothes();

                ComponentManager.Get<TattooShopWindow>().Show()(dbPlayer, cTattooList);
                return true;
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.TryData("tattooShopId", out uint tattooShopId)) return false;
            switch (colShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("tattooShopId", tattooShopId);

                    return false;
                case ColShapeState.Exit:
                    if (!dbPlayer.HasData("tattooShopId")) return false;
                    dbPlayer.ResetData("tattooShopId");

                    // Resett Tattoo Sync
                    dbPlayer.ApplyCharacter();

                    return false;
                default:
                    return false;
            }
        }
    }
}