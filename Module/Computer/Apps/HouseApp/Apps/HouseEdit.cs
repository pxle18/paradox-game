using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.HouseApp.Apps
{
    public class HouseEdit : SimpleApp
    {
        public HouseEdit() : base("HouseEdit") {}

        [RemoteEvent]
        public void requestHouseData(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.OwnHouse[0] == 0)
            {
                dbPlayer.SendNewNotification("Du besitzt kein Haus.");
                return;
            }

            House house = HouseModule.Instance.GetByOwner(dbPlayer.Id);
            if (house == null)
                return;

            TriggerNewClient(client, "responseHouseData", house.InventoryCash);
        }

        [RemoteEvent]
        public void withDrawHouseCash(Player client, int amount, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (dbPlayer.OwnHouse[0] == 0) return;

            House iHouse;

            if ((iHouse = HouseModule.Instance.GetThisHouseFromPos(dbPlayer.Player.Position)) == null || iHouse.Id != dbPlayer.OwnHouse[0])
            {
                dbPlayer.SendNewNotification("Sie befinden sich nicht an Ihrem Haus!", title: "Hauskasse", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (amount > 0 && amount <= iHouse.InventoryCash)
            {
                iHouse.InventoryCash -= amount;
                dbPlayer.GiveMoney(amount);
                dbPlayer.SendNewNotification($"Sie haben { amount }$ aus Ihrer Hauskasse entnommen.", title: "Hauskasse", notificationType: PlayerNotification.NotificationType.SUCCESS);
                iHouse.SaveHouseBank();
                dbPlayer.Save();
            }
            else
            {
                dbPlayer.SendNewNotification("Ungueltiger Betrag!", title: "Hauskasse", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }
        }
    }
}
