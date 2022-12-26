using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Shops;
using VMP_CNR.Module.Clothes.Team;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class TeamWardrobeClothesSelectionMenu : MenuBuilder
    {
        public TeamWardrobeClothesSelectionMenu() : base(PlayerMenu.TeamWardrobeClothesSelection)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasData("teamWardrobeSlot"))
            {
                return null;
            }

            int slotId = dbPlayer.GetData("teamWardrobeSlot");
            var slots = ClothesShopModule.Instance.GetClothesSlots();
            if (!slots.ContainsKey(slotId))
            {
                return null;
            }

            var slot = slots[slotId];
            var menu = new NativeMenu(Menu, slot.Name);
            menu.Add(GlobalMessages.General.Close());

            if (dbPlayer.IsFreeMode())
            {
                var clothesForSlot = ClothModule.Instance.GetTeamWarerobe(dbPlayer, slotId);
                if (clothesForSlot != null && clothesForSlot.Count > 0)
                {
                    foreach (var cloth in clothesForSlot)
                    {
                        menu.Add(cloth.Name);
                    }
                }
            }
            else
            {
                var teamSkin = TeamSkinModule.Instance.GetTeamSkin(dbPlayer);
                if (teamSkin == null)
                {
                    return null;
                }

                var clothes = teamSkin.Clothes.Where(cloth => cloth.Slot == slotId).ToList();
                if (clothes.Count == 0)
                {
                    return null;
                }

                foreach (var cloth in clothes)
                {
                    menu.Add(cloth.Name);
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
                if (index == 0)
                {
                    MenuManager.DismissMenu(dbPlayer.Player, (uint) PlayerMenu.TeamWardrobeClothesSelection);
                    ClothModule.SaveCharacter(dbPlayer);
                    return false;
                }

                index--;
                if (!dbPlayer.HasData("teamWardrobeSlot"))
                {
                    return false;
                }

                int slotId = dbPlayer.GetData("teamWardrobeSlot");
                if (dbPlayer.IsFreeMode())
                {
                    List<Cloth> clothesForSlot = ClothModule.Instance.GetTeamWarerobe(dbPlayer, slotId);
                    if (index >= clothesForSlot.Count)
                    {
                        return false;
                    }

                    var cloth = clothesForSlot[index];
                    if (dbPlayer.Character.Clothes.ContainsKey(cloth.Slot))
                    {
                        dbPlayer.Character.Clothes[cloth.Slot] = cloth.Id;
                    }
                    else
                    {
                        dbPlayer.Character.Clothes.Add(cloth.Slot, cloth.Id);
                    }
                }
                else
                {
                    var teamSkin = TeamSkinModule.Instance.GetTeamSkin(dbPlayer);
                    if (teamSkin == null)
                    {
                        return false;
                    }

                    var clothes = teamSkin.Clothes.Where(cloth => cloth.Slot == slotId).ToList();
                    if (clothes.Count == 0 || index >= clothes.Count)
                    {
                        return false;
                    }

                    var currCloth = clothes[index];
                    if (dbPlayer.Character.Clothes.ContainsKey(currCloth.Slot))
                    {
                        dbPlayer.Character.Clothes[currCloth.Slot] = currCloth.Id;
                    }
                    else
                    {
                        dbPlayer.Character.Clothes.Add(currCloth.Slot, currCloth.Id);
                    }
                }

                ClothModule.Instance.RefreshPlayerClothes(dbPlayer);
                ClothModule.SaveCharacter(dbPlayer);
                return false;
            }
        }
    }
}