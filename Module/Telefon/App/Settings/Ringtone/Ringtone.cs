using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace VMP_CNR.Module.Telefon.App.Settings.Ringtone
{
    public class Ringtone : Loadable<uint>
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        public Ringtone(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
