using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Banks.Windows;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.RemoteEvents;

namespace VMP_CNR.Module.Banks.App
{
    public class BankingApp : SimpleApp
    {
        public BankingApp() : base("BankingApp")
        {
        }
    }

    public class BankingAppOverview : SimpleApp
    {
        public BankingAppOverview() : base("BankingAppOverview")
        {
        }

        [RemoteEvent]
        public void requestBankingAppOverview(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent() || !dbPlayer.IsValid()) return;
                TriggerNewClient(player, "responseBankingAppOverview", dbPlayer.bank_money[0], NAPI.Util.ToJson(dbPlayer.BankHistory));
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return;
            }
        }
    }

    public class BankingAppTransfer : SimpleApp
    {
        int bankingmaxcap = 1000000;
        int bankingmincap = 500;
        
        public BankingAppTransfer() : base("BankingAppTransfer")
        {
        }

        [RemoteEvent]
        public void requestBankingCap(Player player, string key)
        {   // Achtung - BankingCap wird auch im Client abgefragt
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent() || !dbPlayer.IsValid()) return;
            TriggerNewClient(player, "responseBankingCap", bankingmaxcap, bankingmincap);
        }

        [RemoteEvent]
        public void bankingAppTransfer(Player player,String toPlayer,int amount, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent() || !dbPlayer.IsValid()) return;
            if (amount > bankingmaxcap) { return; }
            if (amount < bankingmincap) { return; }
            var bankwindow = new BankWindow();
            bankwindow.bankTransfer(player,amount,toPlayer, "", key);
        }

    }

}
