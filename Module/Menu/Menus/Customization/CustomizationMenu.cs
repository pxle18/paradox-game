using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tattoo;

namespace VMP_CNR
{
    public class CustomizationMenuBuilder : MenuBuilder
    {
        public CustomizationMenuBuilder() : base(PlayerMenu.CustomizationMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Schönheitsklinik");

            menu.Add($"Schönheitschirugie");
            menu.Add($"Tattoos lasern");

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
                switch (index)
                {
                    case 0:
                        dbPlayer.StartCustomization();
                        MenuManager.DismissCurrent(dbPlayer);
                        return true;
                    case 1:
                        MenuManager.Instance.Build(PlayerMenu.TattooLaseringMenu, dbPlayer).Show(dbPlayer);
                        break;
                    default:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                }
                return false;
            }
        }
    }
}