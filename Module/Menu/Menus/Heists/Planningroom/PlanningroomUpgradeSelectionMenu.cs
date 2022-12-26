using System;
using System.Collections.Generic;
using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomUpgradeSelectionMenuBuilder : MenuBuilder
    {
        public PlanningroomUpgradeSelectionMenuBuilder() : base(PlayerMenu.PlanningroomUpgradeSelectionMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);

            NativeMenu menu = new NativeMenu(Menu, "Menu");
            menu.Add($"Schließen");
            if (!dbPlayer.HasData("planningRoomUpgradeSelection")) return menu;
            int selection = dbPlayer.GetData("planningRoomUpgradeSelection");
            if (!PlanningUpgrades.Upgrades.TryGetValue(selection, out Upgrade upgrade)) return menu;

            Dictionary<int, int> upgrades = new Dictionary<int, int>();
            for(int i = 0; i < upgrade.UpgradeNames.Count; i++)
            {
                if(upgrade.UpgradeNames[i] == "Holzoptik")
                {
                    if (room.MainFloorMirrorLevel != 0 && room.MainFloorInteriorLevel != 0 && room.MainFloorSlotMachineLevel != 0)
                    {
                        menu.Add($"{upgrade.UpgradeNames[i]} ausbauen");
                    }
                }
                else
                {
                    menu.Add($"{upgrade.UpgradeNames[i]} ausbauen");
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
                // Planningroom upgrade
                else
                {
                    if (!dbPlayer.HasData("planningRoomUpgradeSelection")) return false;
                    int selection = dbPlayer.GetData("planningRoomUpgradeSelection");
                    if (!PlanningUpgrades.Upgrades.TryGetValue(selection, out Upgrade upgrade)) return false;
                    if (upgrade.Id == 1)
                        index++;
                    room.UpgradePlanningRoom(dbPlayer, (int)upgrade.Id, index);

                    dbPlayer.ResetData("planningRoomUpgradeSelection");
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
            }
        }
    }
}
