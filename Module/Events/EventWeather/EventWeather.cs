using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VMP_CNR.Module.MapParser;
namespace VMP_CNR.Module.Events.EventWeather
{
    public class EventWeather : Loadable<uint>
    {
        public uint Id { get; }
        public uint WeatherId { get; }
        public uint EventId { get; }

        public EventWeather(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            WeatherId = reader.GetUInt32("weather_id");
            EventId = reader.GetUInt32("event_id");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
