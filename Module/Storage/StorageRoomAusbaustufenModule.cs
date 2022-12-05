using System;
using System.Linq;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Storage
{
    public class StorageRoomAusbaustufenModule : SqlModule<StorageRoomAusbaustufenModule, StorageRoomAusbaustufe, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `storage_rooms_ausbaustufen`;";
        }
    }
}