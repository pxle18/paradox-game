using System;
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace VMP_CNR.Module.Players.Ragdoll
{
    public class PlayerRagdoll : Loadable<uint>
    {
        public uint Id { get; private set; }
        public Vector3 Position { get; private set; }
        public ColShape colShape { get; private set; }

        public PlayerRagdoll(MySqlDataReader reader)
        {
            Id = reader.GetUInt32("id");

            Position = new Vector3();
            Position.X = reader.GetFloat("x");
            Position.Y = reader.GetFloat("y");
            Position.Z = reader.GetFloat("z");

            colShape = Spawners.ColShapes.Create(Position, 1.5f);
            colShape.SetData("ragdoll_id", Id);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}