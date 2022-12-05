using System.Collections.Generic;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class AccountHouseKeyMenuBuilder : MenuBuilder
    {
        public AccountHouseKeyMenuBuilder() : base(PlayerMenu.AccountHouseKeys)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Schluessel");
            menu.Add(GlobalMessages.General.Close(), "");


            if (dbPlayer.OwnHouse[0] > 0)
            {
                menu.Add(
                    "~g~Hausschluessel " + dbPlayer.OwnHouse[0],
                    "Klicken um Schluessel zu vergeben");
            }

            var keys = dbPlayer.HouseKeys;
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    if (key != 0)
                    {
                        menu.Add(
                            "Hausschluessel " + key,
                            "~r~Klicken um Schluessel fallen zu lassen!");
                    }
                }
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
                if (index == 0) // General Close
                {
                    return true;
                }

                if (index == 1)
                {
                    if (dbPlayer.OwnHouse[0] > 0)
                    {
                        dbPlayer.SetData("sKeyId", (uint) dbPlayer.OwnHouse[0]);

                        // Chose Menu
                        MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.AccountHouseKeys);
                        dbPlayer.Player.CreateUserDialog(Dialogs.menu_housekeys_input, "inputtext");
                    }

                    return false;
                }

                int idx = 2;
                var keys = dbPlayer.HouseKeys;
                if (keys.Count > 0)
                {
                    foreach (uint key in keys)
                    {
                        if (key != 0)
                        {
                            if (idx == index)
                            {
                                HouseKeyHandler.Instance.DeleteHouseKey(dbPlayer, HouseModule.Instance.Get(key));
                                dbPlayer.SendNewNotification(

                                    "Sie haben den Schluessel fuer das Haus " +
                                    key + " fallen gelassen!");
                                MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.AccountHouseKeys);
                                return true;
                            }

                            idx++;
                        }
                    }
                }

                return true;
            }
        }
    }
}