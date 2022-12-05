using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Shops;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class TeamWardrobeClothesMenu : MenuBuilder
    {
        public TeamWardrobeClothesMenu() : base(PlayerMenu.TeamWardrobeClothes)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Fraktionskleiderschrank");
            menu.Add(MSG.General.Close());
            foreach (var kvp in ClothesShopModule.Instance.GetClothesSlots())
            {
                menu.Add(kvp.Value.Name, kvp.Value.Name);
            }

            /*foreach (var kvp in ClothesShops.Instance.GetPropsSlots())
            {
                menu.Add(kvp.Value, kvp.Value);
            }*/
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
                    MenuManager.DismissMenu(dbPlayer.Player, (uint) PlayerMenu.TeamWardrobeClothes);
                    ClothModule.SaveCharacter(dbPlayer);
                    return false;
                }

                index--;
                var slots = ClothesShopModule.Instance.GetClothesSlots();
                var count = index;
                var currKey = -1;
                foreach (var currSlot in slots)
                {
                    if (count == 0)
                    {
                        currKey = currSlot.Key;
                        break;
                    }

                    count--;
                }

                if (currKey < 0)
                {
                    return false;
                }

                dbPlayer.SetData("teamWardrobeSlot", currKey);
                var menu = MenuManager.Instance.Build(PlayerMenu.TeamWardrobeClothesSelection, dbPlayer);
                menu?.Show(dbPlayer);
                return false;
            }
        }
    }
}