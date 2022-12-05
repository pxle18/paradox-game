using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VMP_CNR.Module.MapParser;

namespace VMP_CNR.Module.Events.EventMaps
{
    public class EventMap : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public uint EventId { get; }

        public EventMap(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("map_name");
            EventId = reader.GetUInt32("event_id");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
