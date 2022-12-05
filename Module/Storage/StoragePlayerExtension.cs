using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Storage
{
    public static class StoragePlayerExtension
    {
        public static Dictionary<uint, StorageRoom> GetStoragesOwned(this DbPlayer dbPlayer)
        {
            Dictionary<uint, StorageRoom> result = new Dictionary<uint, StorageRoom>();
            foreach(KeyValuePair<uint, StorageRoom> kvp in StorageRoomModule.Instance.GetAll().Where(st => st.Value.OwnerId == dbPlayer.Id))
            {
                if (kvp.Value == null) continue;
                result.Add(kvp.Key, kvp.Value);
            }
            
            return result;
        }
    }
}
