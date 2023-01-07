using VMP_CNR.Module.Business;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Business
{
    public class BusinessSafeMenuBuilder : MenuBuilder
    {
        public BusinessSafeMenuBuilder() : base(PlayerMenu.BusinessSafe)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var biz = BusinessModule.Instance.GetById((uint) -dbPlayer.Player.Dimension);
            if (biz == null) return null;

            var menu = new NativeMenu(Menu, "Business Tower");

            menu.Add("Geldtresor", "Geld abheben/einzahlen");

            menu.Add(GlobalMessages.General.Close(), "");
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
                    {
                            // Menue später fuer Tresor    
                            break;
                    }
                    default:
                        break;
                }

                return true;
            }
        }
    }
}