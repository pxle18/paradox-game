using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.AnimationMenu
{
    public class AnimationShortCutSlotMenuBuilder : MenuBuilder
    {
        public AnimationShortCutSlotMenuBuilder() : base(PlayerMenu.AnimationShortCutSlotMenu)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.NativeMenu(Menu, "Slot auswählen");

            menu.Add($"Schließen");

            foreach(KeyValuePair<uint, uint> kvp in dbPlayer.AnimationShortcuts)
            {
                if (kvp.Key == 0 || kvp.Key == 1) continue;// system keys 

                menu.Add($"Slot {kvp.Key}");
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

                int idx = 1;
                foreach (KeyValuePair<uint, uint> kvp in dbPlayer.AnimationShortcuts)
                {
                    if (kvp.Key == 0 || kvp.Key == 1) continue;// system keys 

                    if(index == idx)
                    {
                        // Open Secound Menu
                        dbPlayer.SetData("animSlot", kvp.Key);
                        MenuManager.Instance.Build(PlayerMenu.AnimationShortCutMenu, dbPlayer).Show(dbPlayer);
                        return false;
                    }
                    idx++;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
