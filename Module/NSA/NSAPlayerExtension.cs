using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.NSA
{

    public enum NSARangs
    {
        NONE = 0,
        LIGHT = 1,
        NORMAL = 2,
        LEAD = 3,
    }
    public static class NSAPlayerExtension
    {
        public static bool HasMoneyTransferWantedStatus(this DbPlayer dbPlayer)
        {
            return CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes) > 30;
        }

        public static bool IsMasked(this DbPlayer dbPlayer)
        {
            return (dbPlayer != null && dbPlayer.IsValid() && dbPlayer.Character != null && dbPlayer.Character.ActiveClothes != null && dbPlayer.Character.ActiveClothes.ContainsKey(1) && dbPlayer.Character.ActiveClothes[1]);
        }

        public static void AddTransferHistory(string Description, Vector3 position)
        {
            NSAModule.TransactionHistory.Add(new TransactionHistoryObject() { Description = Description, Added = DateTime.Now, Position = position, TransactionType = TransactionType.MONEY });
        }
        
        public static void AddEnergyHistory(string Description, Vector3 position)
        {
            NSAModule.TransactionHistory.Add(new TransactionHistoryObject() { Description = Description, Added = DateTime.Now, Position = position, TransactionType = TransactionType.ENERGY });
        }
        public static void SetGovLevel(this DbPlayer dbPlayer, string Level)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            MySQLHandler.ExecuteAsync($"UPDATE player SET gov_level = '{Level}' WHERE id = '{dbPlayer.Id}';");

            dbPlayer.GovLevel = Level;
            return;
        }


        public static void RemoveGovLevel(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            MySQLHandler.ExecuteAsync($"UPDATE player SET gov_level = '' WHERE id = '{dbPlayer.Id}'");

            dbPlayer.GovLevel = "";
            return;
        }

        public static bool CanNSADuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.IsNSAState != (int)NSARangs.NONE;
        }
    }
}
