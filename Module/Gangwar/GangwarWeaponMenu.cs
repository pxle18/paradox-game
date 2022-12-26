using System;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teamfight;

namespace VMP_CNR.Module.Gangwar
{
    public class GangwarWeaponMenu : MenuBuilder {

        public GangwarWeaponMenu() : base(PlayerMenu.GangwarWeaponMenu) {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer) {
            Menu.NativeMenu menu = new Menu.NativeMenu(Menu, "Gangwar - Waffenkits");

            menu.Add(GlobalMessages.General.Close());
            menu.Add("Advancedrifle");
            menu.Add("Bullpuprifle");
            menu.Add("Assaultrifle");
            menu.Add("Gusenberg");
            return menu;
        }

        public override IMenuEventHandler GetEventHandler() {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (index == 0) return true;
                TeamfightFunctions.GiveWeaponKit(dbPlayer, index);
                dbPlayer.SetData("gangwar_weaponKit", index);
                return true;
            }
        }
    }
}