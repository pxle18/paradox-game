using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Boerse
{
    public class PlayerDepot
    {
        public uint PlayerID { get; private set; }
        public uint Amount { get; private set; }
        public uint UsedWithdraw { get; private set; }
        public uint UsedDeposit { get; private set; }

        public PlayerDepot(uint playerID, uint amount, uint usedWithdraw, uint usedDeposit)
        {
            PlayerID = playerID;
            Amount = amount;
            UsedWithdraw = usedWithdraw;
            UsedDeposit = usedDeposit;
        }

        /// <summary>
        /// Speichert den Depot-Kontostand
        /// </summary>
        private void Save()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"UPDATE `player_depots` SET amount = {Amount}, used_withdraw = {UsedWithdraw}, used_deposit = {UsedDeposit} WHERE player_id = '{PlayerID}';";

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Holt sich den aktuellen Depot-Kontostand um Glitches/Dupes zu vermeiden
        /// </summary>
        private void Refresh()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT `amount`, `used_withdraw`, `used_deposit` FROM `player_depots` WHERE `player_id` = '{PlayerID}';";
                
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            uint amount         = reader.GetUInt32("amount");
                            uint usedWithdraw   = reader.GetUInt32("used_withdraw");
                            uint usedDeposit    = reader.GetUInt32("used_deposit");
                            Amount              = amount;
                            UsedWithdraw        = usedWithdraw;
                            UsedDeposit         = usedDeposit;
                        }
                    }
                }
            }
        }

        public void Add(uint amount)
        {
            Refresh();
            Amount += amount;
            UsedDeposit += amount;
            Save();
        }

        public void Subtract(uint amount)
        {
            Refresh();
            
            if (Amount - amount < 0)
                Amount = 0;
            else
                Amount -= amount;

            UsedWithdraw += amount;
            
            Save();
        }
    }
    
    public static class PlayerDepotExtensions
    {
        public enum DepotOperation : uint
        {
            Add         = 0,
            Subtract    = 1,
            Both        = 2
        }

        /// <summary>
        /// Zieht sich das aktuelle Depot des Spielers falls vorhanden
        /// </summary>
        /// <param name="dbPlayer"></param>
        public static void LoadPlayerDepot(this DbPlayer dbPlayer)
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT * FROM `player_depots` WHERE `player_id` = '{dbPlayer.Id}';";
                
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            uint PlayerID = dbPlayer.Id;
                            uint Amount = reader.GetUInt32("amount");
                            uint usedWithdraw = reader.GetUInt32("used_withdraw");
                            uint usedDeposit = reader.GetUInt32("used_deposit");

                            PlayerDepot playerDepot = new PlayerDepot(PlayerID, Amount, usedWithdraw, usedDeposit);
                            dbPlayer.Depot = playerDepot;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Erstellt dem Spieler ein neues Depot
        /// </summary>
        /// <param name="dbPlayer"></param>
        public static void CreateDepot(this DbPlayer dbPlayer)
        {
            dbPlayer.SendNewNotification($"Du hast erfolgreich ein Depot für {PlayerDepotModule.DepotKosten} eröffnet! Du kannst nun in der Forum-App mit Aktien handeln und am Schalter dein Depot auffüllen.");
            dbPlayer.TakeMoney((int)PlayerDepotModule.DepotKosten);
            
            MySQLHandler.ExecuteAsync($"INSERT INTO `player_depots` (`player_id`, `amount`) VALUES ('{dbPlayer.Id}', '0');");
            
            dbPlayer.Depot = new PlayerDepot(dbPlayer.Id, 0, 0, 0);
        }

        public static bool HasDepot(this DbPlayer dbPlayer)
        {
            if (dbPlayer.Depot == null)
                return false;

            return true;
        }

        public static bool HasDepotLimitReached(this DbPlayer dbPlayer, DepotOperation operation, uint amountToCalculateIn = 0)
        {
            switch (operation)
            {
                case DepotOperation.Add:
                    if (dbPlayer.Depot.UsedDeposit + amountToCalculateIn > 50000)
                        return true;
                    break;
                case DepotOperation.Subtract:
                    if (dbPlayer.Depot.UsedWithdraw + amountToCalculateIn > 50000)
                        return true;
                    break;
                case DepotOperation.Both:
                    if (dbPlayer.Depot.UsedDeposit + amountToCalculateIn > 50000 || dbPlayer.Depot.UsedWithdraw + amountToCalculateIn > 50000)
                        return true;
                    break;
                default:
                    break;
            }

            return false;
        }
    }
}