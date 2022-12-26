using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class ExchangeTrashMenu : MenuBuilder
    {
        public ExchangeTrashMenu() : base(PlayerMenu.ExchangeTrashMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Schrottplatz Mitarbeiter");

            menu.Add($"Schließen");
            menu.Add($"Muell entsorgen");

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
                    if (room.Bought && room.MainFloor == 0 && room.MainFloorCleanup != 0)
                    {
                        room.DeliverPlanningroomTrash(dbPlayer);
                        return false;
                    }
                    else if (room.Bought && room.BasementLevel == 0 && room.BasementCleanUp != 0 && room.MainFloor > 1)
                    {
                        room.RecyclePlanningRoomTrash(dbPlayer);
                        return false;
                    }

                    return true;
                }

                return true;
            }
        }
    }
}
