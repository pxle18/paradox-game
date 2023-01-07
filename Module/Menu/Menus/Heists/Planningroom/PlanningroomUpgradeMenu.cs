using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomUpgradeMenuBuilder : MenuBuilder
    {
        public PlanningroomUpgradeMenuBuilder() : base(PlayerMenu.PlanningroomUpgradeMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);

            var menu = new NativeMenu(Menu, "Upgrade Menu");

            menu.Add($"Schließen");

            if (room.Bought && room.MainFloor > 0)
            {
                menu.Add($"Grundraum upgraden");
                if (room.MainFloor > 1)
                {
                    menu.Add($"Spiegeldecke upgraden");
                    menu.Add($"Einrichtungsstyle upgraden");
                    menu.Add($"Inneneinrichtung upgraden");
                    menu.Add($"Spielautomaten upgraden");
                }
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
                    dbPlayer.ResetData("planningRoomUpgradeSelection");
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                // Planningroom action
                else
                {
                    dbPlayer.SetData("planningRoomUpgradeSelection", index);
                    MenuManager.DismissCurrent(dbPlayer);
                    MenuManager.Instance.Build(PlayerMenu.PlanningroomUpgradeSelectionMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
            }
        }
    }
}
