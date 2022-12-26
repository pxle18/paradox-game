using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Clothes.Outfits
{
    public class OutfitsMenuBuilder : MenuBuilder
    {
        public OutfitsMenuBuilder() : base(PlayerMenu.OutfitsMenu)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.NativeMenu(Menu, "Outfits");
            menu.Add(GlobalMessages.General.Close());
            menu.Add("Aktuelles Outfit speichern");
            foreach (Outfit outfit in dbPlayer.Outfits.ToList())
            {
                menu.Add(outfit.Name, "");
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
                    MenuManager.DismissMenu(dbPlayer.Player, (uint)PlayerMenu.OutfitsMenu);
                    ClothModule.SaveCharacter(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    // Saving...
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Outfit speichern", Callback = "SaveOutfit", Message = "Wie soll das Outfit heißen?" });
                    return true;
                }

                int idx = 2;
                foreach (Outfit outfit in dbPlayer.Outfits.ToList())
                {
                    if(idx == index)
                    {
                        dbPlayer.SetData("outfit", outfit);
                        MenuManager.Instance.Build(PlayerMenu.OutfitsSubMenu, dbPlayer).Show(dbPlayer);
                        return false;
                    }
                    idx++;
                }
                return false;
            }
        }
    }
}