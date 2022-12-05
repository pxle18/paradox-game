using System;
using MySql.Data.MySqlClient;

namespace VMP_CNR.Module.Events
{
    public class EventData : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int ObjectGroup { get; }
        public bool IsActive { get; set; }

        public EventData(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            Name = reader.GetString(1);
            StartDate = reader.GetDateTime(2);
            EndDate = reader.GetDateTime(3);

            UpdateActive();
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void UpdateActive()
        {
            IsActive = DateTime.Now >= StartDate && DateTime.Now <= EndDate;
        }
    }
}