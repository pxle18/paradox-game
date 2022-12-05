using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tattoo;

namespace VMP_CNR
{
    public class TattooBuyMenuBuilder : MenuBuilder
    {
        public TattooBuyMenuBuilder() : base(PlayerMenu.TattooBuyMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.TryData("tattooShopId", out uint tattooShopId)) return null;
            var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
            if (tattooShop == null || tattooShop.BusinessId != 0) return null;

            if(!dbPlayer.GetActiveBusinessMember().Owner)
            {
                dbPlayer.SendNewNotification("Sie muessen ein Business besitzen!");
            }

            var menu = new Menu(Menu, "TattooShop");

            menu.Add($"Schließen");
            menu.Add($"Shop erwerben {tattooShop.Price}$");

            menu.Add(MSG.General.Close());
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
                if(index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                if(index == 1)
                {
                    // Buy
                    if (!dbPlayer.TryData("tattooShopId", out uint tattooShopId)) return false;
                    var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
                    if (tattooShop == null || tattooShop.BusinessId != 0) return false;

                    if (!dbPlayer.GetActiveBusinessMember().Owner)
                    {
                        dbPlayer.SendNewNotification("Sie muessen ein Business besitzen!");
                    }

                    if (!dbPlayer.TakeMoney(tattooShop.Price))
                    {
                        dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(tattooShop.Price));
                        return false;
                    }

                    tattooShop.SetBusiness((int)dbPlayer.GetActiveBusinessMember().BusinessId);
                    dbPlayer.SendNewNotification("Tattoshop erworben!");
                    return true;
                }
                MenuManager.DismissCurrent(dbPlayer);
                return false;
            }
        }
    }
}