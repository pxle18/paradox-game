using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Schwarzgeld.Menu
{
    public class ExchangeMenuBuilder : MenuBuilder
    {
        public ExchangeMenuBuilder() : base(PlayerMenu.ExchangeMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Module.Menu.Menu(Menu, "Ladenbesitzer", "Schwarzgeld Umwandlung");

            menu.Add($"Schließen");
            menu.Add($"Ladenbesitzer bestechen");
            menu.Add($"Schwarzgeld umwandeln");
            menu.Add($"Rechnung abgeben");

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
                // Ladenbesitzer bestechen
                else if (index == 1)
                {
                    //ExchangeModule.Instance.Bestechen(dbPlayer);

                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                // Schwarzgeld umwandeln
                else if (index == 2)
                {
                    //ExchangeModule.Instance.StartExchange(dbPlayer);

                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                // Finish
                else if (index == 3)
                {
                    //ExchangeModule.Instance.FinishExchange(dbPlayer);

                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }

                return false;
            }
        }
    }
}
