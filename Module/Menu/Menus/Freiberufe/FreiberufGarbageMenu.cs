using VMP_CNR.Module.Freiberuf.Garbage;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class FreiberufGarbageMenuBuilder : MenuBuilder
    {
        public FreiberufGarbageMenuBuilder() : base(PlayerMenu.FreiberufGarbageMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Freiberuf Müllabfuhr", "Müllabfuhr Los Santos");
            menu.Add("Arbeit starten");
            menu.Add("Fahrzeug mieten 500$");
            menu.Add("Rückgabe");
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
                        GarbageJobModule.Instance.StartGarbageJob(dbPlayer);
                        break;
                    case 1:
                        GarbageJobModule.Instance.RentGarbageVeh(dbPlayer);
                        break;
                    case 2:
                        GarbageJobModule.Instance.FinishGarbageJob(dbPlayer);
                        break;
                    default:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                }

                return true;
            }
        }
    }
}
