using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Assets.Tattoo
{
    public class AssetsTattoo : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public string HashMale { get; }
        public string HashFemale { get; }
        public string Collection { get; }
        public int ZoneId { get; }
        public int Price { get; }
        
        public AssetsTattoo(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.IsDBNull(reader.GetOrdinal("name")) ? "" : reader.GetString("name");
            HashMale = reader.IsDBNull(reader.GetOrdinal("hash_male")) ? "" : reader.GetString("hash_male");
            HashFemale = reader.IsDBNull(reader.GetOrdinal("hash_female")) ? "" : reader.GetString("hash_female");
            Collection = reader.IsDBNull(reader.GetOrdinal("collection")) ? "" : reader.GetString("collection");
            ZoneId = reader.IsDBNull(reader.GetOrdinal("zone_id")) ? 0 : reader.GetInt32("zone_id");
            Price = reader.IsDBNull(reader.GetOrdinal("price")) ? 0 : reader.GetInt32("price");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public string GetHashForPlayer(DbPlayer dbPlayer)
        {
            return dbPlayer.Customization.Gender == 0 ? HashMale : HashFemale;
        }
    }
}