using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Blitzer
{
    public class Blitzer : Loadable<uint>
    {
        public uint Id { get; set; }
        public float Range { get; set; }
        public ColShape Shape { get; set; }
        public int SpeedLimit { get; set; }

        public Vector3 Position { get; set; }
        public int Tolleranz { get; set; }

        public bool Active { get; set; }

        public Blitzer(MySqlDataReader reader) : base(reader)
        {
            if(BlitzerModule.Instance.BlitzerLoaded == null)
            {
                BlitzerModule.Instance.BlitzerLoaded = new Dictionary<int, int>();
            }

            Id = reader.GetUInt32("id");
            Range = reader.GetInt32("range");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Shape = Spawners.ColShapes.Create(Position, Range);
            Shape.SetData("blitzer", Id);
            SpeedLimit = reader.GetInt32("speed_limit");
            Tolleranz = Convert.ToInt32(SpeedLimit/5); // 20%
            if (Tolleranz < 15) Tolleranz = 15; // minimal 15 km/h

            Active = true;

            int groupId = reader.GetInt32("group");
            if (groupId != 0) 
            {
                if (BlitzerModule.Instance.BlitzerLoaded.ContainsKey(groupId))
                {
                    if (BlitzerModule.Instance.BlitzerLoaded[groupId] >= 1)
                    {
                        Active = false;
                    }
                    BlitzerModule.Instance.BlitzerLoaded[groupId]++;
                }
                else
                {
                    BlitzerModule.Instance.BlitzerLoaded.Add(groupId, 1);
                }
            }

            if (Active) Spawners.ObjectSpawn.Create(-6978462, new Vector3(reader.GetFloat("obj_pos_x"), reader.GetFloat("obj_pos_y"), reader.GetFloat("obj_pos_z") - 1.0f), new Vector3(0, 0, reader.GetFloat("obj_heading")));
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
