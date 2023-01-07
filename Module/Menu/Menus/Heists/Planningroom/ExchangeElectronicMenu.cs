using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class ExchangeElectronicMenu : MenuBuilder
    {
        public ExchangeElectronicMenu() : base(PlayerMenu.ExchangeElectronicMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Eletronik Umwandlung");

            menu.Add($"Schließen");
            menu.Add($"Materialien umwandeln");

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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    PlanningModule.Instance.ExchangeElectronicItem(dbPlayer);
                    return false;
                }

                return true;
            }
        }
    }
}
