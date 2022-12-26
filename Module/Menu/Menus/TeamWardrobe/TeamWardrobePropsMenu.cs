using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Shops;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class TeamWardrobePropsMenu : MenuBuilder
    {
        public TeamWardrobePropsMenu() : base(PlayerMenu.TeamWardrobeProps)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Accessoires");
            menu.Add(GlobalMessages.General.Close());
            foreach (var kvp in ClothesShopModule.Instance.GetPropsSlots())
            {
                menu.Add(kvp.Value.Name, kvp.Value.Name);
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
                    MenuManager.DismissMenu(dbPlayer.Player, (uint) PlayerMenu.TeamWardrobeProps);
                    ClothModule.SaveCharacter(dbPlayer);
                    return false;
                }

                index--;
                var slots = ClothesShopModule.Instance.GetPropsSlots();
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

                dbPlayer.SetData("teamWardrobePropsSlot", currKey);
                var menu = MenuManager.Instance.Build(PlayerMenu.TeamWardrobePropsSelection, dbPlayer);
                menu?.Show(dbPlayer);
                return false;
            }
        }
    }
}