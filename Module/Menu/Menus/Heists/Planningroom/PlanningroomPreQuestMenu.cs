using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomPreQuestMenu : MenuBuilder
    {
        public PlanningroomPreQuestMenu() : base(PlayerMenu.PlanningroomPreQuestMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Vorbereitungen");

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
                    dbPlayer.SetData("planningroom_pre_quest", index);
                    MenuManager.DismissCurrent(dbPlayer);
                    MenuManager.Instance.Build(PlayerMenu.PlanningroomPreQuestSelectionMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }

                return true;
            }
        }
    }
}
