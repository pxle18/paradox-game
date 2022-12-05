using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Payment
{
    public enum PaymentStatus
    {
        None,
        Wallet,
        Bank
    }

    public class PaymentModule : Module<PaymentModule>
    {

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.ChoosenPaymentState = PaymentStatus.None;
        }

        public async Task<PaymentStatus> AskForPayment(DbPlayer dbPlayer, int price)
        {
            int i = 0;
            // Open Window
            ComponentManager.Get<Payment.Windows.PaymentMethods>().Show()(dbPlayer, price);

            // Wait for Stats Change trough Window Callback Event
            while (dbPlayer.ChoosenPaymentState == PaymentStatus.None && i < 100)
            {
                i++;
                await Task.Delay(500);
            }

            PaymentStatus lPaymentStatus = dbPlayer.ChoosenPaymentState;

            dbPlayer.ChoosenPaymentState = PaymentStatus.None;
            return lPaymentStatus;
        }

    }
}
