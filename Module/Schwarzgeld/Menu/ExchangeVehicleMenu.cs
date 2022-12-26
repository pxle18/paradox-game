using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Schwarzgeld.Menu
{
    public class ExchangeVehicleMenuBuilder : MenuBuilder
    {
        public ExchangeVehicleMenuBuilder() : base(PlayerMenu.ExchangeVehicleMenu)
        {
        }

        public override Module.Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new Module.Menu.NativeMenu(Menu, "Fahrzeug", "Schwarzgeld Umwandlung");

            menu.Add($"Schließen");
            menu.Add($"Einladen");
            menu.Add($"Ausladen");

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
                // Close menu
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                // Einladen
                else if (index == 1)
                {
                    //ExchangeModule.Instance.LoadVehicle(dbPlayer);

                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                // Ausladen
                else if (index == 2)
                {
                    //ExchangeModule.Instance.VehicleTakeOut(dbPlayer);

                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }

                return false;
            }
        }
    }
}
