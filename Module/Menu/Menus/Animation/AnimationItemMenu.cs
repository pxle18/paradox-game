using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.AnimationMenu;
using VMP_CNR.Module.Helper;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class AnimationItemMenuBuilder : MenuBuilder
    {
        public AnimationItemMenuBuilder() : base(PlayerMenu.AnimationMenuIn)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasData("animCat")) return null;
            int catId = dbPlayer.GetData("animCat");

            var menu = new Menu(Menu, AnimationCategoryModule.Instance.Get((uint)catId).Name);
            menu.Add("Schließen", "");
            foreach (KeyValuePair<uint, AnimationItem> kvp in AnimationItemModule.Instance.GetAll().Where(i => i.Value.CategoryId == catId))
            {
                if (kvp.Value.RestrictedToTeams.Contains((uint)teams.TEAM_IAA) && !dbPlayer.IsNSADuty) continue;
                else if (!kvp.Value.RestrictedToTeams.Contains((uint)teams.TEAM_IAA) && kvp.Value.RestrictedToTeams.Count > 0 && !kvp.Value.RestrictedToTeams.Contains(dbPlayer.TeamId)) continue;
                menu.Add(kvp.Value.Name);
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
                if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;
                if (!dbPlayer.HasData("animCat")) return false;
                int catId = dbPlayer.GetData("animCat");

                if(index == 0) // Close
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }

                int idx = 1;
                foreach (KeyValuePair<uint, AnimationItem> kvp in AnimationItemModule.Instance.GetAll().Where(i => i.Value.CategoryId == catId))
                {
                    if (kvp.Value.RestrictedToTeams.Contains((uint)teams.TEAM_IAA) && !dbPlayer.IsNSADuty) continue;
                    else if (!kvp.Value.RestrictedToTeams.Contains((uint)teams.TEAM_IAA) && kvp.Value.RestrictedToTeams.Count > 0 && !kvp.Value.RestrictedToTeams.Contains(dbPlayer.TeamId)) continue;

                    if (index == idx)
                    {
                        Task.Run(async () =>
                        {

                            if (kvp.Value.RestrictedToTeams.Contains((uint)teams.TEAM_IAA) && !dbPlayer.IsNSADuty) return;
                            else if (!kvp.Value.RestrictedToTeams.Contains((uint)teams.TEAM_IAA) && kvp.Value.RestrictedToTeams.Count > 0 && !kvp.Value.RestrictedToTeams.Contains(dbPlayer.TeamId)) return;

                            dbPlayer.StopAnimation();

                            if (kvp.Value.AttachmentId > 0)
                            {
                                Module.Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, kvp.Value.AttachmentId);

                                dbPlayer.StopAnimation(Module.Players.PlayerAnimations.AnimationLevels.User, true);
                            }

                            await Task.Delay(500);
                            if (dbPlayer != null || dbPlayer.IsValid())
                            {
                                AnimationExtension.StartAnimation(dbPlayer, kvp.Value);
                            }
                        });
                        return false;
                    }
                    idx++;
                }
                return false;
            }
        }
    }
}