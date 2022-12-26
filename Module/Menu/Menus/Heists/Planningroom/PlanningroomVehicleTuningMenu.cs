using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomVehicleTuningMenuBuilder : MenuBuilder
    {
        public PlanningroomVehicleTuningMenuBuilder() : base(PlayerMenu.PlanningroomVehicleTuningMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasData("planningroom_vehicle_tuning")) return null;

            var menu = new NativeMenu(Menu, "Planningroom", "Fahrzeuge modifizieren");

            menu.Add($"Schließen");
            menu.Add($"Nummernschild ändern");
            menu.Add($"Farbe ändern");

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
                if (!dbPlayer.HasData("planningroom_vehicle_tuning")) return true;

                switch (index)
                {
                    case 0:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                    case 1:
                        MenuManager.DismissCurrent(dbPlayer);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Nummernschild ändern", Callback = "PlanningroomSetVehiclePlate", Message = "Geben Sie ein Nummernschild ein:" });
                        return true;
                    case 2:
                        MenuManager.DismissCurrent(dbPlayer);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Fahrzeugfarbe ändern", Callback = "PlanningroomSetVehicleColor", Message = "Geben Sie die Farben an BSP: (101 1):" });
                        return true;
                    default:
                        break;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
