using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Paintball
{

 

    public class PaintballSpawn : Loadable<uint>
    {
        public uint Id { get; }
        public uint paintball_id;
        public float x;
        public float y;
        public float z;
        public float h;


        public PaintballSpawn(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            paintball_id = reader.GetUInt32("paintball_id");
            x = reader.GetFloat("x");
            y = reader.GetFloat("y");
            z = reader.GetFloat("z");
            h = reader.GetFloat("h");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }




}
