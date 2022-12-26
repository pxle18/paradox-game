using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu.Menus.Asservatenkammer
{
    class AsservatenkammerMenuBuilder : MenuBuilder
    {
        public AsservatenkammerMenuBuilder() : base(PlayerMenu.AsservatenkammerMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Asservatenkammer");
            menu.Add(GlobalMessages.General.Close(), "");
            menu.Add("Transport", "Brickade beladen");

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
                    // transport
                }

                return true;
            }
        }
    }
}
