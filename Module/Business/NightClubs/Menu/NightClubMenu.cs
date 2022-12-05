using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Business.NightClubs
{
    public class NightClubMenuBuilder : MenuBuilder
    {
        public NightClubMenuBuilder() : base(PlayerMenu.NightClubMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.TryData("nightclubId", out uint nightClubId)) return null;
            NightClub nightClub = NightClubModule.Instance.Get(nightClubId);
            if (nightClub == null) return null;
            
            var menu = new Menu.Menu(Menu, nightClub.Name);

            menu.Add($"Schließen");

            if (!nightClub.IsOwnedByBusines())
            {
                if (dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Owner && dbPlayer.GetActiveBusiness().BusinessBranch.NightClubId == 0 && dbPlayer.GetActiveBusiness().BusinessBranch.CanBuyBranch())
                {
                    menu.Add($"NightClub kaufen {nightClub.Price}$");
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else
                {
                    if (!dbPlayer.TryData("nightclubId", out uint nightClubId)) return false;
                    NightClub nightClub = NightClubModule.Instance.Get(nightClubId);
                    if (nightClub == null) return false;

                    if (!nightClub.IsOwnedByBusines())
                    {
                        if (dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Owner && dbPlayer.GetActiveBusiness().BusinessBranch.NightClubId == 0 && dbPlayer.GetActiveBusiness().BusinessBranch.CanBuyBranch())
                        {
                            // Kaufen
                            if (dbPlayer.GetActiveBusiness().TakeMoney(nightClub.Price))
                            {
                                dbPlayer.GetActiveBusiness().BusinessBranch.SetNightClub(nightClub.Id);
                                dbPlayer.SendNewNotification($"{nightClub.Name} erfolgreich fuer ${nightClub.Price} erworben!");
                                nightClub.OwnerBusiness = dbPlayer.GetActiveBusiness();
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(nightClub.Price));
                            }
                        }
                        return true;
                    }
                    return false;
                }
            }
        }
    }
}