using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.AnimationMenu
{
    public class AnimationShortCutMenuBuilder : MenuBuilder
    {
        public AnimationShortCutMenuBuilder() : base(PlayerMenu.AnimationShortCutMenu) { }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasData("animSlot")) return null;

            var menu = new Menu.NativeMenu(Menu, $"Animation für Slot {dbPlayer.GetData("animSlot")} auswählen");

            menu.Add($"Schließen");

            foreach (AnimationItem animationItem in AnimationItemModule.Instance.GetAll().Values)
            {

                if (animationItem.RestrictedToTeams.Contains((uint)TeamTypes.TEAM_IAA) && !dbPlayer.IsNSADuty) continue;
                else if (!animationItem.RestrictedToTeams.Contains((uint)TeamTypes.TEAM_IAA) && animationItem.RestrictedToTeams.Count > 0 && !animationItem.RestrictedToTeams.Contains(dbPlayer.TeamId)) continue;

                menu.Add($"{animationItem.Name}");
            }
            return menu;
        }

        public override IMenuEventHandler GetEventHandler() => new EventHandler();

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (!dbPlayer.HasData("animSlot")) return false;
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }

                int idx = 1;
                foreach (AnimationItem animationItem in AnimationItemModule.Instance.GetAll().Values)
                {
                    if (index == idx)
                    {
                        // Open Secound Menu
                        if (!dbPlayer.AnimationShortcuts.ContainsKey(dbPlayer.GetData("animSlot"))) return false;

                        if (animationItem.RestrictedToTeams.Contains((uint)TeamTypes.TEAM_IAA) && !dbPlayer.IsNSADuty) return false;
                        else if (!animationItem.RestrictedToTeams.Contains((uint)TeamTypes.TEAM_IAA) && animationItem.RestrictedToTeams.Count > 0 && !animationItem.RestrictedToTeams.Contains(dbPlayer.TeamId)) return false;

                        dbPlayer.AnimationShortcuts[dbPlayer.GetData("animSlot")] = animationItem.Id;
                        dbPlayer.SendNewNotification($"Animationsslot {dbPlayer.GetData("animSlot")} mit {animationItem.Name} belegt!");
                        dbPlayer.SaveAnimationShortcuts();
                        dbPlayer.UpdateAnimationShortcuts();
                        return true;
                    }
                    idx++;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
