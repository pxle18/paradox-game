using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Keys;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Handler
{
    public class HouseKeyHandler
    {
        public static HouseKeyHandler Instance { get; } = new HouseKeyHandler();

        private HouseKeyHandler()
        {

        }

        public void AddHouseKey(DbPlayer dbPlayer, House house)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                if (dbPlayer.HouseKeys.Contains(house.Id)) return;
                dbPlayer.HouseKeys.Add(house.Id);
                MySQLHandler.ExecuteAsync($"INSERT INTO `house_keys` (`player_id`, `house_id`) VALUES ('{dbPlayer.Id}', '{house.Id}');");
            }));
        }

        public void DeleteHouseKey(DbPlayer dbPlayer, House house)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.HouseKeys.Contains(house.Id)) return;
                dbPlayer.HouseKeys.Remove(house.Id);
                MySQLHandler.ExecuteAsync($"DELETE FROM `house_keys` WHERE `house_id` = '{house.Id}' AND `player_id` = '{dbPlayer.Id}';");
            }));
        }

        public void DeleteAllHouseKeys(House house)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (DbPlayer dbPlayer in Players.Instance.GetValidPlayers())
                {
                    if (dbPlayer?.HouseKeys == null) continue;
                    if (dbPlayer.HouseKeys.Contains(house.Id))
                    {
                        dbPlayer.HouseKeys.Remove(house.Id);
                    }
                }
                MySQLHandler.ExecuteAsync($"DELETE FROM `house_keys` WHERE `house_id` = '{house.Id}';");
            }));
        }

        public async Task LoadHouseKeys(DbPlayer dbPlayer)
        {
            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                await keyConn.OpenAsync();
                keyCmd.CommandText =
                    $"SELECT house_id FROM `house_keys` WHERE player_id = '{dbPlayer.Id}';";
                using (var keyReader = await keyCmd.ExecuteReaderAsync())
                {
                    if (keyReader.HasRows)
                    {
                        while (await keyReader.ReadAsync())
                        {
                            var keyId = (uint)keyReader.GetInt32(0);
                            if (!dbPlayer.HouseKeys.Contains(keyId))
                            {
                                dbPlayer.HouseKeys.Add(keyId);
                            }
                        }
                    }
                }
                await keyConn.CloseAsync();
            }
        }

        public async Task<bool> CanHouseKeyGiven(House house)
        {
            bool keyExists = false;

            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                await keyConn.OpenAsync();
                keyCmd.CommandText =
                    $"SELECT `player_id` FROM `house_keys` WHERE house_id = '{house.Id}';";
                using (var keyReader = keyCmd.ExecuteReader())
                {
                    if (keyReader.HasRows)
                        keyExists = true;
                }

                await keyConn.CloseAsync();
            }

            return !keyExists;
        }

        public List<VHKey> GetAllKeysPlayerHas(DbPlayer dbPlayer)
        {
            List<VHKey> houses = new List<VHKey>();
            foreach (uint house in dbPlayer.HouseKeys.ToList())
            {
                houses.Add(new VHKey("" + house, house));
            }

            if (dbPlayer.OwnHouse[0] != 0) houses.Add(new VHKey("" + dbPlayer.OwnHouse[0], dbPlayer.OwnHouse[0]));

            return houses;
        }

        public List<VHKey> GetOwnHouseKey(DbPlayer dbPlayer)
        {
            List<VHKey> houses = new List<VHKey>();
            if (dbPlayer.OwnHouse[0] != 0) houses.Add(new VHKey("" + dbPlayer.OwnHouse[0], dbPlayer.OwnHouse[0]));
            return houses;
        }

    }
}