using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class ItemOrderMenuBuilder : MenuBuilder
    {
        public ItemOrderMenuBuilder() : base(PlayerMenu.ItemOrderMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Verarbeitung");
            menu.Add("Waren verarbeiten");
            menu.Add("Waren entnehmen");
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
                        MenuManager.Instance.Build(PlayerMenu.ItemOrderItemsMenu, dbPlayer).Show(dbPlayer);
                        break;
                    case 1:
                        MenuManager.Instance.Build(PlayerMenu.ItemOrderOrdersMenu, dbPlayer).Show(dbPlayer);
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