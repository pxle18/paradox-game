using GTANetworkAPI;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Business.NightClubs
{
    public class NightClubsEvents : Script
    {
        [RemoteEvent]
        public void SetNightClubName(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (dbPlayer.Player.Dimension == 0) return;
            var nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
            if (nightClub == null) return;

            if (nightClub.IsOwnedByBusines())
            {
                if (nightClub.GetOwnedBusiness() == dbPlayer.GetActiveBusiness() && dbPlayer.GetActiveBusinessMember() != null && dbPlayer.GetActiveBusinessMember().NightClub) // Member of business and has rights
                {
                    if (Regex.IsMatch(returnstring, @"^[a-zA-Z ]+$") && NightClubModule.Instance.GetAll().Where(fs => fs.Value.Name.ToLower() == returnstring.ToLower()).Count() == 0)
                    {
                        nightClub.SetName(returnstring);
                        dbPlayer.SendNewNotification("Name wurde geaendert!");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Dieser Name ist nicht gueltig!");
                        return;
                    }
                }
            }
        }

        [RemoteEvent]
        public void SetNightClubItemPrice(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            
            if (dbPlayer.Player.Dimension == 0 || !dbPlayer.HasData("nightClubItemEdit")) return;
            if (!Int32.TryParse(returnstring, out int price)) return;
            uint itemId = (uint)dbPlayer.GetData("nightClubItemEdit");
            var nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
            if (nightClub == null) return;

            if (nightClub.IsOwnedByBusines())
            {
                if (nightClub.GetOwnedBusiness() == dbPlayer.GetActiveBusiness() && dbPlayer.GetActiveBusinessMember() != null && dbPlayer.GetActiveBusinessMember().NightClub) // Member of business and has rights
                {
                    if (price < 0 || price > 1000000) return;
                    NightClubItem nightClubItem = NightClubItemModule.Instance.GetAll().Values.FirstOrDefault(nci => nci.NightClubId == nightClub.Id && nci.ItemId == itemId);
                    if (nightClubItem == null || nightClubItem.NightClubId != nightClub.Id) return;

                    nightClubItem.SetPrice(price);
                    dbPlayer.SendNewNotification($"Preis von {nightClubItem.Name} auf ${price} geaendert!");
                    return;
                }
            }
        }
    }
}
