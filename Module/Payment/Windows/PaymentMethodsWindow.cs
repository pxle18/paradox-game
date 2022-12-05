using System;
using VMP_CNR.Module.Players.Db;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Windows;
using Newtonsoft.Json;
using VMP_CNR.Module.Players;

namespace VMP_CNR.Module.Payment.Windows
{
    public class PaymentMethods : Window<Func<DbPlayer, int, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "price")] private int price { get; }

            public ShowEvent(DbPlayer dbPlayer, int lprice) :
                base(dbPlayer)
            {
                price = lprice;
            }
        }

        public PaymentMethods() : base("PaymentMethods")
        {
        }

        public override Func<DbPlayer, int, bool> Show()
        {
            return (player, price) =>
                OnShow(new ShowEvent(player, price));
        }

        [RemoteEvent]
        public void selectPaymentMethod(Player player, string method, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if(method == "bank")
            {
                dbPlayer.ChoosenPaymentState = PaymentStatus.Bank;
            }
            else
            {
                dbPlayer.ChoosenPaymentState = PaymentStatus.Wallet;
            }
            return;
        }
    }
}