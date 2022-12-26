using GTANetworkAPI;
using System;
using VMP_CNR.Handler;
using VMP_CNR.Module.Freiberuf;
using VMP_CNR.Module.Freiberuf.Mower;
using VMP_CNR.Module.Government;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles.Data;
using VMP_CNR.Module.Vehicles.Garages;

namespace VMP_CNR.Module.ShopTakeover
{
    public class ShopTakeoverManageMenu : MenuBuilder
    {
        public ShopTakeoverManageMenu() : base(PlayerMenu.ShopTakeoverAttackMenu) { }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            uint shopTakeoverId = dbPlayer.SetData("shopTakeoverId");
            var menu = new NativeMenu(Menu, "Shop-Takeover - Verwaltung");

            menu.Add("Schliessen");
            menu.Add($"Schutzgeld einsammeln (${");
            menu.Add("Revolter");

            return menu;
        }


        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                switch (index)
                {
                    case 0: return true;
                    case 1:

                    case 2:

                        break;
                }

                return true;
            }
        }

        public override IMenuEventHandler GetEventHandler() => new EventHandler();
    }
}