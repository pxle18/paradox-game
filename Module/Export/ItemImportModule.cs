using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Export.Menu;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Payment;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Export
{
    public sealed class ItemImportEvents : Script
    {
        [RemoteEvent]
        public async void ImportBatteries(Player player, string batterieBoughtAmountString, string remoteKey)
        {
            if (!player.CheckRemoteEventKey(remoteKey)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!int.TryParse(batterieBoughtAmountString, out int batterieBoughtAmount)) return;

            var _itemImportModule = ItemImportModule.Instance;
            var _paymentModule = PaymentModule.Instance;

            if (!_itemImportModule.BoughtBatteries.TryGetValue(dbPlayer.Id, out int boughtBatteries))
                boughtBatteries = 0;

            int batteryCapDifference = Math.Max(0, (_itemImportModule.DailyBatteryCap - boughtBatteries) - batterieBoughtAmount);
            if (batteryCapDifference <= 0)
            {
                dbPlayer.SendNewNotification($"So viele Batterien darfst du heute nicht mehr kaufen. #FiNdEsRaUs");
                return;
            }

            var batteriesPrice = batterieBoughtAmount * _itemImportModule.BatteryPrice;
            if (batteriesPrice <= 0) return;

            if (!dbPlayer.Container.CanInventoryItemAdded(_itemImportModule.BatteryItemId, batterieBoughtAmount))
            {
                dbPlayer.SendNewNotification($"Du kannst soviele Batterien nicht tragen.");
                return;
            }

            PaymentStatus paymentStatus = await _paymentModule.AskForPayment(dbPlayer, batteriesPrice);
            if (paymentStatus == PaymentStatus.None) return;

            switch (paymentStatus)
            {
                case PaymentStatus.Wallet:
                    if (!dbPlayer.TakeMoney(batteriesPrice))
                    {
                        dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(batteriesPrice));
                        return;
                    }
                    break;
                case PaymentStatus.Bank:
                    if (!dbPlayer.TakeBankMoney(batteriesPrice, "Ihre Zahlung beim Importhandel ($" + batteriesPrice + ")"))
                    {
                        dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(batteriesPrice));
                        return;
                    }
                    break;
            }

            _itemImportModule.BoughtBatteries[dbPlayer.Id] = batterieBoughtAmount + boughtBatteries;

            dbPlayer.Container.AddItem(_itemImportModule.BatteryItemId, batterieBoughtAmount);
            dbPlayer.SendNewNotification($"Du hast {batterieBoughtAmount}x Batterien für ${batteriesPrice} gekauft.");
        }
    }

    public class ItemImportModule : Module<ItemImportModule>
    {
        private readonly Vector3 _importPosition = new Vector3(2707.3198, 2776.9255, 37.877968);

        public int DailyBatteryCap = 196;
        public int BatteryPrice = 125;

        public uint BatteryItemId = 15;

        public Dictionary<uint, int> BoughtBatteries = new Dictionary<uint, int>();

        protected override bool OnLoad()
        {
            new Npc(PedHash.GarbageSMY, _importPosition, 34f, 0);

            PlayerNotifications.Instance.Add(
                _importPosition,
                "Importhandel", "Drücke E um mit dem Importhandel zu interagieren."
            );

            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer == null || !dbPlayer.IsValid() || key != Key.E) return false;
            if (dbPlayer.Player.Position.DistanceTo(_importPosition) > 10.0f) return false;

            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Batterie-Import", Callback = "ImportBatteries", Message = "Gebe die Anzahl an Batterien an die du erwerben möchtest: ($125 - 165x pro Wende)." });

            return true;
        }
    }
}