using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Clothes.Shops;
using VMP_CNR.Module.Clothes.Team;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class TeamWardrobePropsSelectionMenu : MenuBuilder
    {
        public TeamWardrobePropsSelectionMenu() : base(PlayerMenu.TeamWardrobePropsSelection)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasData("teamWardrobePropsSlot"))
            {
                return null;
            }

            int slotId = dbPlayer.GetData("teamWardrobePropsSlot");
            var slots = ClothesShopModule.Instance.GetPropsSlots();
            if (!slots.ContainsKey(slotId))
            {
                return null;
            }

            var slot = slots[slotId];
            var menu = new Menu(Menu, slot.Name);
            menu.Add(MSG.General.Close());
            menu.Add("Leer");

            if (dbPlayer.IsFreeMode())
            {
                List<Prop> propsForSlot = PropModule.Instance.GetTeamWarerobe(dbPlayer, slotId);
                if (propsForSlot != null && propsForSlot.Count > 0)
                {
                    foreach (var prop in propsForSlot)
                    {
                        menu.Add(prop.Name);
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

                var clothes = teamSkin.Props.Where(cloth => cloth.Slot == slotId).ToList();
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
                    MenuManager.DismissMenu(dbPlayer.Player, (uint) PlayerMenu.TeamWardrobePropsSelection);
                    ClothModule.SaveCharacter(dbPlayer);
                    return false;
                }

                if (!dbPlayer.HasData("teamWardrobePropsSlot"))
                {
                    return false;
                }

                int slotId = dbPlayer.GetData("teamWardrobePropsSlot");
                if (index == 1)
                {
                    dbPlayer.Player.ClearAccessory(slotId);
                    dbPlayer.Character.EquipedProps.Remove(slotId);
                    ClothModule.SaveCharacter(dbPlayer);
                    return false;
                }

                index -= 2;
                if (dbPlayer.IsFreeMode())
                {
                    List<Prop> clothesForSlot = PropModule.Instance.GetTeamWarerobe(dbPlayer, slotId);
                    if (index >= clothesForSlot.Count)
                    {
                        return false;
                    }

                    var cloth = clothesForSlot[index];
                    if (dbPlayer.Character.EquipedProps.ContainsKey(cloth.Slot))
                    {
                        dbPlayer.Character.EquipedProps[cloth.Slot] = cloth.Id;
                    }
                    else
                    {
                        dbPlayer.Character.EquipedProps.Add(cloth.Slot, cloth.Id);
                    }
                }
                else
                {
                    var teamSkin = TeamSkinModule.Instance.GetTeamSkin(dbPlayer);
                    if (teamSkin == null)
                    {
                        return false;
                    }

                    var clothes = teamSkin.Props.Where(cloth => cloth.Slot == slotId).ToList();
                    if (clothes.Count == 0 || index >= clothes.Count)
                    {
                        return false;
                    }

                    var currCloth = clothes[index];
                    if (dbPlayer.Character.EquipedProps.ContainsKey(currCloth.Slot))
                    {
                        dbPlayer.Character.EquipedProps[currCloth.Slot] = currCloth.Id;
                    }
                    else
                    {
                        dbPlayer.Character.EquipedProps.Add(currCloth.Slot, currCloth.Id);
                    }
                }

                ClothModule.Instance.RefreshPlayerClothes(dbPlayer);
                ClothModule.SaveCharacter(dbPlayer);
                return false;
            }
        }
    }
}