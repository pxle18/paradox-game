using VMP_CNR.Handler;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Account
{
    public class AccountVehicleKeyChooseMenuBuilder : MenuBuilder
    {
        public AccountVehicleKeyChooseMenuBuilder() : base(PlayerMenu.AccountKeyChooseMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Schluessel");
            menu.Add(GlobalMessages.General.Close(), "");

            menu.Add("Spieler geben", "Schluessel an Spieler geben");

            if (dbPlayer.IsMemberOfBusiness())
            {
                if (dbPlayer.GetActiveBusiness() != null)
                {
                    menu.Add("~b~" + dbPlayer.GetActiveBusiness().Name, "Schluessel im Business hinterlegen");
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
                    MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.AccountKeyChooseMenu);
                    return false;
                }
                else if (index == 1) // Spieler
                {
                    MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.AccountKeyChooseMenu);
                    dbPlayer.Player.CreateUserDialog(Dialogs.menu_keys_input, "inputtext");
                }
                else if (index == 2) // Business
                {
                    if (dbPlayer.IsMemberOfBusiness())
                    {
                        if (dbPlayer.GetActiveBusiness() != null)
                        {
                            //(uint) dbPlayer.Player.GetData("sKeyId")
                            dbPlayer.GetActiveBusiness().AddVehicleKey((uint) dbPlayer.GetData("sKeyId"), VehicleHandler.Instance.GetPlayerVehicleNameByDatabaseId((uint)dbPlayer.GetData("sKeyId")));
                            dbPlayer.SendNewNotification(
                                $"Schluessel {dbPlayer.GetData("sKeyId")} fuer Business {dbPlayer.GetActiveBusiness().Name} hinterlegt!");
                        }
                    }

                    MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.AccountKeyChooseMenu);
                    return false;
                }
                return false;
            }
        }
    }
}