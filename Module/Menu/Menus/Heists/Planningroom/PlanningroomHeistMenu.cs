using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomHeistMenu : MenuBuilder
    {
        public PlanningroomHeistMenu() : base(PlayerMenu.PlanningroomHeistMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Heists");

            menu.Add($"Schließen");
            menu.Add($"Casino");

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
                    PlanningModule.Instance.StartHeist(dbPlayer, index);
                    return true;
                }

                return true;
            }
        }
    }
}
