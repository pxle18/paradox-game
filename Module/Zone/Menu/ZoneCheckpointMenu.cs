//using VMP_CNR.Module.Menu;
//using VMP_CNR.Module.Players;
//using VMP_CNR.Module.Players.Db;

//namespace VMP_CNR.Module.Zone
//{
//    public class ZoneCheckpointMenuBuilder : MenuBuilder
//    {
//        public ZoneCheckpointMenuBuilder() : base(PlayerMenu.ZoneCPMenu)
//        {
//        }

//        public override Menu.Menu Build(DbPlayer dbPlayer)
//        {
//            if (!dbPlayer.IsACop() || dbPlayer.TeamRank < 6 || !dbPlayer.IsInDuty())
//            {
//                return null;
//            }

//            var menu = new Menu.Menu(Menu, "Checkpoint Verwaltung");

//            menu.Add($"Schließen");
//            menu.Add($"Checkpoint oeffnen");
//            menu.Add($"Checkpoint schließen");
//            return menu;
//        }

//        public override IMenuEventHandler GetEventHandler()
//        {
//            return new EventHandler();
//        }

//        private class EventHandler : IMenuEventHandler
//        {
//            public bool OnSelect(int index, DbPlayer dbPlayer)
//            {
//                if (index == 1) // open
//                {
//                    ZoneModule.Instance.OpenCheckpoint(dbPlayer.Player.Position, true);
//                    return true;
//                }
//                else if (index == 2) // close
//                {
//                    ZoneModule.Instance.OpenCheckpoint(dbPlayer.Player.Position, false);
//                    return true;
//                }
//                else
//                {
//                    MenuManager.DismissCurrent(dbPlayer);
//                    return false;
//                }
//            }
//        }
//    }
//}