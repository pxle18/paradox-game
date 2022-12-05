using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Module.Banks.BankHistory;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Players
{
    //Todo: new module like player team rank
    public class PlayerBankHistoryModule : Module<PlayerBankHistoryModule>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long ConvertToTimestamp(DateTime value)
        {
            var elapsedTime = value - Epoch;
            return (long) elapsedTime.TotalSeconds;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {

            dbPlayer.BankHistory = new List<Banks.BankHistory.BankHistory>();

            // Load Player Bank
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT * FROM `player_bankhistory` WHERE player_id = '{dbPlayer.Id}' ORDER BY date DESC LIMIT 10;";
                using (var reader2 = cmd.ExecuteReader())
                {
                    if (reader2.HasRows)
                    {
                        while (reader2.Read())
                        {
                            var bankHistory = new Banks.BankHistory.BankHistory
                            {
                                PlayerId = reader2.GetUInt32(1),
                                Name = reader2.GetString(2),
                                Value = reader2.GetInt32(3),
                                Date = reader2.GetDateTime(4)
                            };

                            dbPlayer.BankHistory.Add(bankHistory);
                        }
                    }
                }
            }

            return;
        }
        
    }

    public static class BankhistoryExtensions
    {

        public static void AddPlayerBankHistory(this DbPlayer dbPlayer, int value, string description)
        {
            var bankHistory = new Banks.BankHistory.BankHistory
            {
                PlayerId = dbPlayer.Id,
                Name = description,
                Value = value,
                Date = DateTime.Now
            };

            dbPlayer.BankHistory.Insert(0, bankHistory);

            var query =
                $"INSERT INTO `player_bankhistory` (`player_id`, `description`, `value`) VALUES ('{dbPlayer.Id}', '{MySqlHelper.EscapeString(description)}', '{value}')";

            MySQLHandler.ExecuteAsync(query);
        }

        public static void AddPlayerBankHistories(this DbPlayer dbPlayer, List<Banks.BankHistory.BankHistory> bankHistories)
        {
            var query = "";
            foreach (var bankHistory in bankHistories)
            {
                var tmpBankHistory = new Banks.BankHistory.BankHistory
                {
                    PlayerId = dbPlayer.Id,
                    Name = bankHistory.Name,
                    Value = bankHistory.Value,
                    Date = DateTime.Now
                };

                dbPlayer.BankHistory.Insert(0, tmpBankHistory);

                query +=
                    $"INSERT INTO `player_bankhistory` (`player_id`, `description`, `value`) VALUES ('{dbPlayer.Id}', '{MySqlHelper.EscapeString(bankHistory.Name)}', '{bankHistory.Value}');";
            }
            MySQLHandler.ExecuteAsync(query);
        }

        public static void AddBankHistory(this Team team, int value, string description)
        {
            var bankHistory = new Banks.BankHistory.BankHistory
            {
                PlayerId = team.Id,
                Name = description,
                Value = value,
                Date = DateTime.Now
            };

            team.BankHistory.Insert(0, bankHistory);

            var query =
                $"INSERT INTO `team_bankhistory` (`team_id`, `description`, `value`) VALUES ('{team.Id}', '{MySqlHelper.EscapeString(description)}', '{value}')";

            MySQLHandler.ExecuteAsync(query);
        }
        public static void AddBankHistory(this Business.Business business, int value, string description)
        {
            var bankHistory = new Banks.BankHistory.BankHistory
            {
                PlayerId = business.Id,
                Name = description,
                Value = value,
                Date = DateTime.Now
            };

            business.BankHistory.Insert(0, bankHistory);

            var query =
                $"INSERT INTO `business_bankhistory` (`business_id`, `description`, `value`) VALUES ('{business.Id}', '{MySqlHelper.EscapeString(description)}', '{value}')";

            MySQLHandler.ExecuteAsync(query);
        }

        public static void LogVermoegen(this DbPlayer dbPlayer)
        {
            int moneyvm = dbPlayer.Money[0] + dbPlayer.BankMoney[0];
            int vehiclevm = 0;

            int housevm = 0;

            if(dbPlayer.OwnHouse[0] != 0)
            {
                House xHouse = HouseModule.Instance.GetByOwner(dbPlayer.Id);
                if(xHouse != null)
                {
                    housevm = xHouse.InventoryCash;
                }
            }

            try
            {
                // Load vehicle sum 
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText =
                        $"SELECT IFNULL(SUM(vm.price), 0) as vehiclevm FROM vehicles LEFT JOIN vehicledata vm ON vm.id = vehicles.model WHERE vehicles.Owner = '{dbPlayer.Id}';";
                    using (var reader2 = cmd.ExecuteReader())
                    {
                        if (reader2 != null && reader2.HasRows)
                        {
                            while (reader2.Read())
                            {
                                vehiclevm = reader2.GetInt32("vehiclevm");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
            }
            int businessvm = 0;

            if(dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Owner)
            {
                businessvm = dbPlayer.GetActiveBusiness().Money;
            }

            MySQLHandler.ExecuteAsync($"INSERT INTO log_player_vermoegen (`player_id`, `moneyvm`, `vehiclevm`, `housevm`, `businessvm`) VALUES ('{dbPlayer.Id}', '{moneyvm}', '{vehiclevm}', '{housevm}', '{businessvm}')");
        }
    }
}