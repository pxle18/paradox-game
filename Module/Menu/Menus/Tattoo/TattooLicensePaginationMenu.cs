using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tattoo;

namespace VMP_CNR
{
    public class TattooLicensePaginationMenuBuilder : MenuBuilder
    {
        public TattooLicensePaginationMenuBuilder() : base(PlayerMenu.TattooLicensePaginationMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasTattooShop())
            {
                dbPlayer.SendNewNotification("Du besitzt keinen Tattoo-Shop und kannst entsprechend keine Lizenzen erwerben!", PlayerNotification.NotificationType.ERROR);
                return null;
            }

            List<TattooLicense> licenses = TattooLicenseModule.Instance.GetAll().Values.ToList();
            if (licenses == null || licenses.Count == 0)
            {
                dbPlayer.SendNewNotification("Es werden aktuell keine Tattoo-Lizenzen zum Kauf angeboten!");
                return null;
            }

            uint PagesAmount = (uint) (licenses.Count() / 100);
            if (PagesAmount == 0)
                PagesAmount = 1;

            var tattooshop = TattooShopFunctions.GetTattooShop(dbPlayer);
            if (tattooshop == null)
            {
                dbPlayer.SendNewNotification("Der Lizenzenshop konnte dich keinem Tattooladen zuordnen. Melde dies bitte im Void-Bugtracker!", PlayerNotification.NotificationType.ERROR);
                return null;
            }

            var menu = new NativeMenu(Menu, "Tattoo Licenses");
            menu.Add(GlobalMessages.General.Close());

            for (uint itr = 0; itr < PagesAmount; itr++)
            {
                menu.Add($"Seite {itr + 1}");
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
                if (index > 0)
                {
                    uint page = (uint)index;
                    dbPlayer.SetData("tattooLicensePage", page);
                    MenuManager.Instance.Build(PlayerMenu.TattooLicenseMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
                else
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
            }
        }
    }
}