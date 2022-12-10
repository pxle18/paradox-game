using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.AnimationMenu.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.RemoteEvents;

namespace VMP_CNR.Module.AnimationMenu
{
    public class AnimationShortcutEventHandler : Script
    {
        [RemoteEventPermission]
        [RemoteEvent]
        public async Task REQUEST_ANIMATION_USE(Player client, int slot, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            await Task.Run(() =>
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return;

                if (slot == 0) // Stop Animation
                {
                    dbPlayer.StopAnimation();
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                }
                else if (slot == 1) // Configure
                {
                    var list = AnimationItemModule.Instance.GetAll().Values
                        .Select(animationItem => new AnimationShortcutWindow.SimpleAnimation(animationItem.Id, animationItem.Name))
                        .ToList();

                    ComponentManager.Get<AnimationShortcutWindow>().Show()(
                        dbPlayer,
                        list
                    );

                    // MenuManager.Instance.Build(PlayerMenu.AnimationShortCutSlotMenu, dbPlayer).Show(dbPlayer);

                    return;
                }
                else
                {
                    if (dbPlayer.AnimationShortcuts.ContainsKey((uint)slot))
                    {
                        if (AnimationItemModule.Instance.Contains((uint)dbPlayer.AnimationShortcuts[(uint)slot]))
                        {
                            AnimationItem animationItem = AnimationItemModule.Instance.Get((uint)dbPlayer.AnimationShortcuts[(uint)slot]);
                            if (animationItem == null) return;

                            if (animationItem.RestrictedToTeams.Contains((uint)TeamTypes.TEAM_IAA) && !dbPlayer.IsNSADuty) return;
                            else if (!animationItem.RestrictedToTeams.Contains((uint)TeamTypes.TEAM_IAA) && animationItem.RestrictedToTeams.Count > 0 && !animationItem.RestrictedToTeams.Contains(dbPlayer.TeamId)) return;

                            dbPlayer.StopAnimation();

                            if (animationItem.AttachmentId > 0)
                            {
                                Module.Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, animationItem.AttachmentId);

                                dbPlayer.StopAnimation(Module.Players.PlayerAnimations.AnimationLevels.User, true);
                            }

                            AnimationExtension.StartAnimation(dbPlayer, animationItem);
                            return;
                        }
                    }
                    return;
                }
            });
        }
    }

}
