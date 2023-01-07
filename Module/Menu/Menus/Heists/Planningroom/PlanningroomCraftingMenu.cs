using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomCraftingMenu : MenuBuilder
    {
        public PlanningroomCraftingMenu() : base(PlayerMenu.PlanningroomCraftingMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Schmiede Mitarbeiter");

            menu.Add($"Schließen");
            menu.Add($"Tresortür anfertigen");
            menu.Add($"Tresortür abholen");

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

                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    if (room.CasinoPlanLevel == 1)
                    {
                        room.CraftCasinoDoor(dbPlayer);
                        return true;
                    }

                    return true;
                }
                else if(index == 2)
                {
                    if (room.CasinoPlanLevel == 1)
                    {
                        room.DeliverCasinoDoor(dbPlayer);
                        return true;
                    }

                    return true;
                }

                return true;
            }
        }
    }
}
