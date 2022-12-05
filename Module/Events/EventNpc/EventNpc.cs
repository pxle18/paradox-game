using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.NpcSpawner;

namespace VMP_CNR.Module.Events.EventNpc
{
    public class EventNpc : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public uint Dimension { get; set; }
        public uint EventId { get; set; }
        public PedHash PedHash { get; set; }

        public EventNpc(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Dimension = reader.GetUInt32("dimension");
            EventId = reader.GetUInt32("event_id");
            PedHash = Enum.TryParse(reader.GetString("ped_hash"), true, out PedHash skin) ? skin : PedHash.Trucker01SMM;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
