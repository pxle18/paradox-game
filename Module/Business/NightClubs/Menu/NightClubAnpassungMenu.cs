using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Business.NightClubs
{
    public class NightClubAnpassungMenuBuilder : MenuBuilder
    {
        public NightClubAnpassungMenuBuilder() : base(PlayerMenu.NightClubAnpassung)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            if (dbPlayer.Player.Dimension == 0) return null;
            NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
            if (nightClub == null) return null;

            // Check Rights
            if (!nightClub.IsOwnedByBusines() || !dbPlayer.IsMemberOfBusiness() || !dbPlayer.GetActiveBusinessMember().NightClub || dbPlayer.GetActiveBusiness().BusinessBranch.NightClubId != nightClub.Id) return null;

            var menu = new Menu.NativeMenu(Menu, nightClub.Name);

            menu.Add($"Schließen");
            menu.Add($"Interrior");
            menu.Add($"Dekoration");
            menu.Add($"Lichter");
            menu.Add($"Effekte");
            menu.Add($"Clubname");
            menu.Add($"Eingangsbeleuchtung");
            menu.Add($"Sicherheitssystem");

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
                if (dbPlayer.Player.Dimension == 0) return false;
                NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
                if (nightClub == null) return false;

                if (!nightClub.IsOwnedByBusines() || !dbPlayer.IsMemberOfBusiness() || !dbPlayer.GetActiveBusinessMember().NightClub || dbPlayer.GetActiveBusiness().BusinessBranch.NightClubId != nightClub.Id) return false;

                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else if (index > 0)
                {
                    dbPlayer.SetData("NightClubData", index);
                    MenuManager.Instance.Build(PlayerMenu.NightClubAnpassungHandler, dbPlayer).Show(dbPlayer);
                }

                return false;
            }
        }
    }
}
