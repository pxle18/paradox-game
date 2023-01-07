using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class TeamWardrobeMenuBuilder : MenuBuilder
    {
        public TeamWardrobeMenuBuilder() : base(PlayerMenu.TeamWardrobe)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Kleiderschrank");
            menu.Add("Skins");
            menu.Add("Kleidung");
            menu.Add("Accessoires");
            menu.Add("Outfits");
            menu.Add(GlobalMessages.General.Close());
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
                switch (index)
                {
                    case 0:
                        MenuManager.Instance.Build(PlayerMenu.TeamWardrobeSkins, dbPlayer).Show(dbPlayer);
                        break;
                    case 1:
                        MenuManager.Instance.Build(PlayerMenu.TeamWardrobeClothes, dbPlayer).Show(dbPlayer);
                        break;
                    case 2:
                        MenuManager.Instance.Build(PlayerMenu.TeamWardrobeProps, dbPlayer).Show(dbPlayer);
                        break;
                    case 3:
                        MenuManager.Instance.Build(PlayerMenu.OutfitsMenu, dbPlayer).Show(dbPlayer);
                        break;
                    default:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                }

                return false;
            }
        }
    }
}