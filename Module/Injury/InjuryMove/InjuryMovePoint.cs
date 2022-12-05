using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Injury.InjuryMove
{
    public enum InjuryMoveGroup
    {
        KH1 = 1
    }

    public class InjuryMovePoint : Loadable<uint>
    {
        public uint Id { get; }
        public uint Grouping { get; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }

        public string Name { get; set; }

        public uint Dimension { get; set; }

        public InjuryMovePoint(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Grouping = reader.GetUInt32("grouping");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Name = reader.GetString("name");
            Dimension = reader.GetUInt32("dimension");

            ColShapes.Create(Position, 2.5f, Dimension).SetData("InjuryMovePointID", Id);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
