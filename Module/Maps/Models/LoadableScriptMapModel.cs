using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Maps.Models
{
    public class LoadableScriptMapModel : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public LoadableMapModel Map { get; set; }

        public LoadableScriptMapModel(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");

            Map = JsonConvert.DeserializeObject<LoadableMapModel>(
                reader.GetString("map")
            );
        }

        public override uint GetIdentifier() => Id;
    }
}
