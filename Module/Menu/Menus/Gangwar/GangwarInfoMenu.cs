using System;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR
{
    public class GangwarInfoMenuBuilder : MenuBuilder
    {
        public GangwarInfoMenuBuilder() : base(PlayerMenu.GangwarInfo)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            GangwarTown gangwar = GangwarTownModule.Instance.GetByPosition(dbPlayer.Player.Position);
            var menu = new NativeMenu(Menu, gangwar.Name);

            menu.Add(GlobalMessages.General.Close(), "");

            menu.Add("Information", "Informationen zum Gebiet");

            if (dbPlayer.IsAGangster())
            {
                if (gangwar.OwnerTeam == null || dbPlayer.Team != gangwar.OwnerTeam)
                {
                    menu.Add("Gebiet angreifen", "");
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
                GangwarTown gangwar = GangwarTownModule.Instance.GetByPosition(dbPlayer.Player.Position);
                if (gangwar == null) return true;

                if (index == 1)
                {
                    dbPlayer.SendNewNotification($"Besitzer: {gangwar.OwnerTeam.Name} Letzter Angriff vor { Convert.ToInt32(DateTime.Now.Subtract(gangwar.LastAttacked).TotalHours)} Stunden",
                        PlayerNotification.NotificationType.INFO, $"Gebietsinformation {gangwar.Name}",
                        10000);
                }
                else if (index == 2)
                { 
                    if (dbPlayer.IsAGangster())
                    {
                        gangwar.Attack(dbPlayer);
                    }
                }
                return true;
            }
        }
    }
}