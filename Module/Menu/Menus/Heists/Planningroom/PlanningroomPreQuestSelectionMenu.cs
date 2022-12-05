using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomPreQuestSelectionMenu : MenuBuilder
    {
        public PlanningroomPreQuestSelectionMenu() : base(PlayerMenu.PlanningroomPreQuestSelectionMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Vorbereitungen");

            menu.Add($"Schließen");

            menu.Add($"Fahrzeug besorgen");
            menu.Add($"Outfit besorgen");
            menu.Add($"Extra besorgen");

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
                int heistId = dbPlayer.GetData("planningroom_pre_quest");

                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    PlanningModule.Instance.StartVehiclePreQuest(dbPlayer, heistId);
                    return true;
                }
                else if(index == 2)
                {
                    PlanningModule.Instance.StartOutfitPreQuest(dbPlayer, heistId);
                    return true;
                }
                else if (index == 3)
                {
                    PlanningModule.Instance.StartExtraPreQuest(dbPlayer, heistId);
                    return true;
                }

                return true;
            }
        }
    }
}
