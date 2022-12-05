using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Business.NightClubs
{
    public class NightClubManageMenuBuilder : MenuBuilder
    {
        public NightClubManageMenuBuilder() : base(PlayerMenu.NightClubManageMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            if (dbPlayer.Player.Dimension == 0) return null;
            NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
            if (nightClub == null) return null;
            
            // Check Rights
            if (!nightClub.IsOwnedByBusines() || !dbPlayer.IsMemberOfBusiness() || !dbPlayer.GetActiveBusinessMember().NightClub || dbPlayer.GetActiveBusiness().BusinessBranch.NightClubId != nightClub.Id) return null;

            var menu = new Menu.Menu(Menu, nightClub.Name);

            menu.Add($"Schließen");
            menu.Add($"NightClub umbenennen");
            menu.Add($"Verkaufspreise anpassen");
            menu.Add($"NightClub anpassen");
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

                // Check Rights
                if (!nightClub.IsOwnedByBusines() || !dbPlayer.IsMemberOfBusiness() || !dbPlayer.GetActiveBusinessMember().NightClub || dbPlayer.GetActiveBusiness().BusinessBranch.NightClubId != nightClub.Id) return false;
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else if (index == 1) // Namechange
                {
                    // Name
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "NightClub Name", Callback = "SetNightClubName", Message = "Gib einen neuen Namen ein (max 32 Stellen)." });
                    return true;
                }
                else if (index == 2) // Edit prices
                {
                    MenuManager.Instance.Build(PlayerMenu.NightClubPriceMenu, dbPlayer).Show(dbPlayer);
                }
                else if (index == 3) // Nightclub anpassen
                {
                    MenuManager.Instance.Build(PlayerMenu.NightClubAnpassung, dbPlayer).Show(dbPlayer);
                }
                
                return false;
            }
        }
    }
}