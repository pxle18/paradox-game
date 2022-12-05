using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace VMP_CNR.Module.Weapons.Component
{
    public class WeaponComponent : Loadable<int>
    {

        [JsonProperty(PropertyName = "Id")]
        public int Id { get; }


        [JsonProperty(PropertyName = "Hash")]
        public uint Hash { get; }


        [JsonProperty(PropertyName = "Name")]
        public string Name { get; }


        [JsonProperty(PropertyName = "WeaponDataId")]

        public uint WeaponDataId { get; }

        public bool DisablePacking { get; }

        public WeaponComponent(MySqlDataReader reader): base(reader)
        {
            Id = reader.GetInt32("id");
            Hash = reader.GetUInt32("hash");
            Name = reader.GetString("name");
            WeaponDataId = reader.GetUInt32("weapon_data_id");
            DisablePacking = reader.GetInt32("disablepacking") == 1;
        }

        public override int GetIdentifier()
        {
            return Id;
        }
    }
}