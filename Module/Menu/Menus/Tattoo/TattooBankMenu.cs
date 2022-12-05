using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Banks.Windows;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tattoo;
using VMP_CNR.Module.Tattoo.Windows;

namespace VMP_CNR
{
    public class TattooBankMenuBuilder : MenuBuilder
    {
        public TattooBankMenuBuilder() : base(PlayerMenu.TattooBankMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.TryData("tattooShopId", out uint tattooShopId)) return null;
            var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
            if (tattooShop == null) return null;

            if (!dbPlayer.GetActiveBusinessMember().Manage || dbPlayer.GetActiveBusinessMember().BusinessId != tattooShop.BusinessId) return null;

            var menu = new Menu(Menu, "Tattoo Shop");


            menu.Add($"Bank öffnen");
            menu.Add($"Tattoowieren");

            menu.Add(GlobalMessages.General.Close());
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
                if (!dbPlayer.TryData("tattooShopId", out uint tattooShopId)) return false;
                var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
                if (tattooShop == null) return false;

                switch (index)
                {
                    case 0: // Bank
                        if (!dbPlayer.GetActiveBusinessMember().Manage || dbPlayer.GetActiveBusinessMember().BusinessId != tattooShop.BusinessId) return false;

                        MenuManager.DismissCurrent(dbPlayer);

                        Business biz = BusinessModule.Instance.GetById((uint)tattooShop.BusinessId);
                        if (biz != null)
                        {
                            ComponentManager.Get<BankWindow>().Show()(dbPlayer, "TattooStudioBank", dbPlayer.GetName(), dbPlayer.Money[0], tattooShop.Bank, 0, biz.BankHistory);
                        }
                        break;
                    case 1: // Tattowieren

                        if (dbPlayer.Player.Position.DistanceTo(tattooShop.Position) > 5.0f) return false;

                        List<ClientTattoo> cTattooList = new List<ClientTattoo>();

                        if (tattooShop.tattooLicenses.Count <= 0)
                        {
                            return false;
                        }

                        foreach (TattooAddedItem tattooAddedItem in tattooShop.tattooLicenses)
                        {
                            AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get((uint)tattooAddedItem.AssetsTattooId);
                            if (assetsTattoo.GetHashForPlayer(dbPlayer) != "")
                            {
                                cTattooList.Add(new ClientTattoo(assetsTattoo.GetHashForPlayer(dbPlayer), assetsTattoo.ZoneId, assetsTattoo.Price, assetsTattoo.Name));
                            }
                        }

                        dbPlayer.SetTattooClothes();

                        MenuManager.DismissCurrent(dbPlayer);
                        ComponentManager.Get<TattooShopWindow>().Show()(dbPlayer, cTattooList);
                        break;
                    default:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                }
                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}