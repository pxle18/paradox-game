
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;

namespace VMP_CNR.Module.Vehicles.Garages
{
    public class GarageSpawn : Loadable<uint>
    {
        public uint Id { get; }
        public uint GarageId { get; }
        public Vector3 Position { get; }
        public float Heading { get; }
        public uint Dimension { get; }

        public DateTime LastUsed { get; set; }

        public GarageSpawn(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            GarageId = reader.GetUInt32(1);
            Position = new Vector3(reader.GetFloat(2), reader.GetFloat(3), reader.GetFloat(4));
            Heading = reader.GetFloat(5);
            Dimension = reader.GetUInt32(6);

            LastUsed = DateTime.Now;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}