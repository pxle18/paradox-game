using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomKellerUpgradeMenu : MenuBuilder
    {
        public PlanningroomKellerUpgradeMenu() : base(PlayerMenu.PlanningroomKellerUpgradeMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);

            var menu = new NativeMenu(Menu, "Keller Upgrade Menu");

            menu.Add($"Schließen");

            if (room.Bought && room.MainFloor > 1 && room.BasementLevel > 1)
            {
                menu.Add($"Mechaniker Upgrade");
                menu.Add($"Hacker Upgrade");
                menu.Add($"Waffen Upgrade");
                menu.Add($"Umkleide Upgrade");
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

                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    room.UpgradePlanningRoom(dbPlayer, 7, 1);
                    return false;
                }
                else if (index == 2)
                {
                    room.UpgradePlanningRoom(dbPlayer, 8, 1);
                    return false;
                }
                else if (index == 3)
                {
                    room.UpgradePlanningRoom(dbPlayer, 9, 1);
                    return false;
                }
                else if (index == 4)
                {
                    room.UpgradePlanningRoom(dbPlayer, 10, 1);
                    return false;
                }

                return true;
            }
        }
    }
}
