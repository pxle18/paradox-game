using GTANetworkAPI;
using System;
using System.Linq;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Houses
{
    public class HouseRentEvents : Script
    {
        [RemoteEvent]
        public void HouseRentAskTenant(Player player, string tenant, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.Container.GetItemAmount(HouseRentModule.ItemHouseRentContract) <= 0 || !dbPlayer.HasData("TenantSlot"))
            {
                dbPlayer.SendNewNotification("Sie benötigen einen Mietvertrag!");
                return;
            }

            if (!dbPlayer.HasData("houseId") || dbPlayer.GetData("houseId") != dbPlayer.ownHouse[0])
            {
                dbPlayer.SendNewNotification("Sie müssen an Ihrem Haus sein!");
                return;
            }

            if (string.IsNullOrEmpty(tenant)) return;
            var target = Players.Players.Instance.FindPlayer(tenant);
            if (target == null || !target.IsValid()) return;

            if (target.Player.Position.DistanceTo(dbPlayer.Player.Position) < 5.0f)
            {
                if (target.IsTenant() || target.ownHouse[0] > 0)
                {
                    dbPlayer.SendNewNotification("Person hat bereits eine Mietwohnung oder ein Haus!");
                    return;
                }

                target.SetData("HouseSeller", dbPlayer.GetName());
                target.SetData("TenantSlot", dbPlayer.GetData("TenantSlot"));

                ComponentManager.Get<ConfirmationWindow>().Show()(target, new ConfirmationObject("Mietvertrag", $"Hiermit schließen Sie einen Mietvertrag mit dem Vermieter: { dbPlayer.GetName() }", "HouseRentAddTenant", "", ""));
            }
            else
            {
                dbPlayer.ResetData("TenantSlot");
                dbPlayer.SendNewNotification("Mieter nicht gefunden oder nicht in Ihrer Nähe!");
                return;
            }
        }

        [RemoteEvent]
        public void HouseRentAddTenant(Player player, string param1, string param2, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            string userToFind = dbPlayer.GetData("HouseSeller");
            string tenantSlotString = dbPlayer.GetData("TenantSlot").ToString();
            int tenantSlot = Convert.ToInt32(tenantSlotString);

            var owner = Players.Players.Instance.FindPlayer(userToFind);
            if (owner == null || !owner.IsValid()) return;

            HouseRent houseRent = HouseRentModule.Instance.houseRents.Where(hr => hr.HouseId == owner.ownHouse[0] && hr.SlotId == tenantSlot).FirstOrDefault();

            if (houseRent == null) return;

            houseRent.PlayerId = dbPlayer.Id;
            houseRent.Save();

            owner.SendNewNotification($"Sie haben einen Mietvertrag mit { dbPlayer.GetName() } für Ihre Immobilie ({ houseRent.HouseId } | Mietslot { houseRent.SlotId }) erstellt! ");
            dbPlayer.SendNewNotification($"Sie haben einen Mietvertrag mit { owner.GetName() } erstellt! ");

            owner.Container.RemoveItem(HouseRentModule.ItemHouseRentContract);
            dbPlayer.ResetData("HouseSeller");
            dbPlayer.ResetData("TenantSlot");
        }
    }
}
