using System.Collections.Generic;
using VMP_CNR.Module.AnimationMenu;
using VMP_CNR.Module.Farming;
using VMP_CNR.Module.Helper;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class AnimationMenuBuilder : MenuBuilder
    {
        public AnimationMenuBuilder() : base(PlayerMenu.AnimationMenuOv)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Animation Menu");
            menu.Add("Schließen", "");
            menu.Add("Animation beenden", "");
            foreach (KeyValuePair<uint, AnimationCategory> kvp in AnimationCategoryModule.Instance.GetAll())
            {
                if (kvp.Value == null) continue;
                menu.Add($"{kvp.Value.Name}", "");
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
                var player = dbPlayer.Player;
                if (index == 0)
                {
                    return true;
                }
                else if (index == 1)
                {
                    if (!dbPlayer.CanInteract()) return false;
                    dbPlayer.StopAnimation();
                    MenuManager.DismissCurrent(dbPlayer);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    return true;
                }
                else
                {
                    int idx = 0;

                    foreach (KeyValuePair<uint, AnimationCategory> kvp in AnimationCategoryModule.Instance.GetAll())
                    {
                        if (kvp.Value == null) continue;
                        if(index-2 == idx)
                        {
                            // Animation Category Clicked
                            dbPlayer.SetData("animCat", (int)kvp.Value.Id);
                            break;
                        }
                        idx++;
                    }
                    
                    MenuManager.Instance.Build(PlayerMenu.AnimationMenuIn, dbPlayer).Show(dbPlayer);
                    return false;
                }
            }
        }
    }

    public static class AnimationExtension
    {
        public static bool StartAnimation(DbPlayer dbPlayer, AnimationItem animationItem)
        {
            var player = dbPlayer.Player;
            if (animationItem == null)
            {
                return true;
            }

            if (!AnimationMenuModule.Instance.animFlagDic.ContainsKey((uint)animationItem.AnimFlag)) return true;

            dbPlayer.PlayAnimation(AnimationMenuModule.Instance.animFlagDic[(uint)animationItem.AnimFlag], animationItem.AnimDic, animationItem.AnimName);
            return false;
        }
    }
}