using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomPurchaseMenBuilder : MenuBuilder
    {
        public PlanningroomPurchaseMenBuilder() : base(PlayerMenu.PlanningroomPurchaseMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);

            var menu = new NativeMenu(Menu, "Anonymer Kontakt");

            menu.Add($"Schließen");

            if(!room.Bought)
            {
                menu.Add($"Planungsraum erwerben (2.000.000$)");
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
                PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);

                // Close menu
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    room.PurchasePlanningRoom(dbPlayer);

                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }

                return true;
            }
        }
    }
}
