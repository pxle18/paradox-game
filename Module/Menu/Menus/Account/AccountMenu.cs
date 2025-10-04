using VMP_CNR.Module.Business;
using VMP_CNR.Module.Jobs;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class AccountMenuBuilder : MenuBuilder
    {
        public AccountMenuBuilder() : base(PlayerMenu.Account)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, dbPlayer.GetName());
            //Todo: outsource
            int rp_multiplikator = 4;

            string str = "";
            string str2 = "";
            str = dbPlayer.Team.Name;

            menu.Add("Name: " + dbPlayer.GetName() + " | Level: " + dbPlayer.Level,
                ((dbPlayer.Level * rp_multiplikator) - dbPlayer.RP[0]) + " Stunden bis zum Levelaufstieg!");


            menu.Add("ID: " + dbPlayer.ForumId, "");
            menu.Add("Academic Punkte: " + dbPlayer.uni_points[0],
                "Geschaeftsmann: " + dbPlayer.uni_business[0] + " | Sparfuchs: " + dbPlayer.uni_economy[0] +
                " | Workaholic: " + dbPlayer.uni_workaholic[0]);
            menu.Add($"Firma:" + (dbPlayer.IsMemberOfBusiness() ? dbPlayer.GetActiveBusiness().Name : "Keine"));
            menu.Add("GWD Note: " + Content.GetGwdText(dbPlayer.grade[0]), "");

            str2 = dbPlayer.Rank.GetDisplayName();
            if (dbPlayer.RankId == 0 && dbPlayer.donator[0] > 0)
            {
                str2 = Content.General.GetDonorName(dbPlayer.donator[0]);
            }

            menu.Add("Void: ~b~" + str2, "");

            menu.Add("Bargeld: ~g~" + dbPlayer.Money[0] + "$", "");
            menu.Add("Wanteds: ~r~" + dbPlayer.Wanteds[0] + "/59", "");
            menu.Add("Immobilie: ~y~" + dbPlayer.OwnHouse[0],
                "HausID ist fuer supportzwecke wichtig!");

            if (dbPlayer.TeamRank > 0)
            {
                menu.Add("Organisation: " + str, "Rang: " + dbPlayer.TeamRank);
            }
            else
            {
                menu.Add("Organisation: " + str, "");
            }

            Job iJob;
            if ((iJob = dbPlayer.GetJob()) != null)
            {
                menu.Add(
                    "Beruf: ~b~" + iJob.Name + "~w~ | Erfahrung: ~g~" + dbPlayer.JobSkill[0] + "~w~/5000", "");
            }
            else
            {
                menu.Add("Beruf: ~b~Keiner", "");
            }

            menu.Add("Zeit seit PayDay: ~y~" + dbPlayer.PayDay[0] + " Minuten",
                "Noch " + ((dbPlayer.Level * rp_multiplikator) - dbPlayer.RP[0]) +
                " Stunden bis zum Levelaufstieg!");
            menu.Add("Lizenzen", "");
            menu.Add("Fahrzeugschluessel", "");
            menu.Add("Hausschluessel", "");
            menu.Add("Warns: " + dbPlayer.warns[0] + "/3", "");
            menu.Add("Handynummer: " + dbPlayer.handy[0],
                "Guthaben: $" + dbPlayer.guthaben[0]);
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
                    case 12:
                        MenuManager.Instance.Build(PlayerMenu.AccountLicense, dbPlayer).Show(dbPlayer);
                        break;
                    case 13:
                        MenuManager.Instance.Build(PlayerMenu.AccountVehicleKeys, dbPlayer).Show(dbPlayer);
                        break;
                    case 14:
                        MenuManager.Instance.Build(PlayerMenu.AccountHouseKeys, dbPlayer).Show(dbPlayer);
                        break;
                    default:
                        MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.Account);
                        break;
                }

                return false;
            }
        }
    }
}