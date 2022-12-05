using System;
using VMP_CNR.Handler;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomVehicleModifyMenuBuilder : MenuBuilder
    {
        public PlanningroomVehicleModifyMenuBuilder() : base(PlayerMenu.PlanningroomVehicleModifyMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Planningroom", "Fahrzeugverwaltung");

            menu.Add($"Schließen");

            foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetClosestPlanningVehiclesFromTeam(dbPlayer.Player.Position, Convert.ToInt32(dbPlayer.Team.Id), 10.0f))
            {
                menu.Add($"{sxVehicle.GetName()} ({sxVehicle.databaseId})");
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if(index >= 1)
                {
                    int idx = 1;

                    foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetClosestPlanningVehiclesFromTeam(dbPlayer.Player.Position, Convert.ToInt32(dbPlayer.Team.Id), 10.0f))
                    {
                        if (index == idx)
                        {
                            dbPlayer.SetData("planningroom_vehicle_tuning", sxVehicle.databaseId);
                            MenuManager.DismissCurrent(dbPlayer);
                            MenuManager.Instance.Build(PlayerMenu.PlanningroomVehicleTuningMenu, dbPlayer).Show(dbPlayer);
                            return false;
                        }

                        idx++;
                    }
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
