using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Keys;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Storage;

namespace VMP_CNR.Handler
{
    public class StorageKeyHandler
    {
        public static StorageKeyHandler Instance { get; } = new StorageKeyHandler();

        private StorageKeyHandler()
        {

        }

        private string tableName = "player_to_storage";

        public void AddStorageKey(DbPlayer dbPlayer, StorageRoom storageRoom)
        {
            if (dbPlayer.StorageKeys.Contains(storageRoom.Id)) return;
            dbPlayer.StorageKeys.Add(storageRoom.Id);
            MySQLHandler.ExecuteAsync(
                    $"INSERT INTO `{tableName}` (`player_id`, `storage_id`) VALUES ('{dbPlayer.Id}', '{storageRoom.Id}');");
        }

        public void DeleteStorageKey(DbPlayer dbPlayer, StorageRoom storageRoom)
        {
            if (!dbPlayer.StorageKeys.Contains(storageRoom.Id)) return;
            dbPlayer.StorageKeys.Remove(storageRoom.Id);
            MySQLHandler.ExecuteAsync(
                $"DELETE FROM `{tableName}` WHERE `storage_id` = '{storageRoom.Id}' AND `player_id` = '{dbPlayer.Id}';");
        }

        public void DeleteAllStorageKeys(StorageRoom storageRoom)
        {
            foreach (DbPlayer dbPlayer in Players.Instance.GetValidPlayers())
            {
                if (dbPlayer?.StorageKeys == null) continue;
                if (dbPlayer.StorageKeys.Contains(storageRoom.Id))
                {
                    dbPlayer.StorageKeys.Remove(storageRoom.Id);
                }
            }
            MySQLHandler.ExecuteAsync($"DELETE FROM `{tableName}` WHERE `storage_id` = '{storageRoom.Id}';");
        }

        public void GetAllStorageKeys(StorageRoom storageRoom)
        {
            foreach (DbPlayer dbPlayer in Players.Instance.GetValidPlayers())
            {
                if (dbPlayer?.StorageKeys == null) continue;
                if (dbPlayer.StorageKeys.Contains(storageRoom.Id))
                {
                    dbPlayer.StorageKeys.Remove(storageRoom.Id);
                }
            }
            MySQLHandler.ExecuteAsync($"DELETE FROM `{tableName}` WHERE `storage_id` = '{storageRoom.Id}';");
        }

        public async Task LoadStorageKeys(DbPlayer dbPlayer)
        {
            
                using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var keyCmd = keyConn.CreateCommand())
                {
                    await keyConn.OpenAsync();
                    keyCmd.CommandText =
                        $"SELECT storage_id FROM `{tableName}` WHERE player_id = '{dbPlayer.Id}';";
                    using (var keyReader = keyCmd.ExecuteReader())
                    {
                        if (keyReader.HasRows)
                        {
                            while (keyReader.Read())
                            {
                                var keyId = (uint)keyReader.GetInt32(0);
                                if (!dbPlayer.StorageKeys.Contains(keyId))
                                {
                                    dbPlayer.StorageKeys.Add(keyId);
                                }
                            }
                        }
                    }
                    await keyConn.CloseAsync();
                }
            
        }

        public List<VHKey> GetAllKeysPlayerHas(DbPlayer dbPlayer)
        {
            List<VHKey> storages = new List<VHKey>();
            foreach (uint storage in dbPlayer.StorageKeys)
            {
                storages.Add(new VHKey("" + storage, storage));
            }
            foreach (KeyValuePair<uint, StorageRoom> kvp in StorageRoomModule.Instance.GetAll().Where(st => st.Value.OwnerId == dbPlayer.Id))
            {
                storages.Add(new VHKey("" + kvp.Key, kvp.Key));
            }
            return storages;
        }

        public List<VHKey> GetOwnStorageKey(DbPlayer dbPlayer)
        {
            List<VHKey> storages = new List<VHKey>();
            foreach(KeyValuePair<uint, StorageRoom> kvp in StorageRoomModule.Instance.GetAll().Where(st => st.Value.OwnerId == dbPlayer.Id))
            {
                storages.Add(new VHKey("" + kvp.Key, kvp.Key));
            }
            return storages;
        }

    }
}