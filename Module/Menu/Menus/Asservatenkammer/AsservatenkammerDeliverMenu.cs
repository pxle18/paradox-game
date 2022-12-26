using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Asservatenkammer
{
    class AsservatenkammerDeliverMenuBuilder :MenuBuilder
    {
        public AsservatenkammerDeliverMenuBuilder() : base(PlayerMenu.AsservatenkammerDeliverMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Asservatenkammer");
            menu.Add(GlobalMessages.General.Close(), "");
            menu.Add("Abgabe", "Ware abgeben");
            menu.Add("Informationen", "Bestand prüfen");

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
                if (index == 1)
                {
                    // abgabe
                }
                else if(index == 2)
                {
                    // Informationen
                }

                return true;
            }
        }
    }
}
