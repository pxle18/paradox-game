using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;

namespace VMP_CNR.Module.Gangwar
{
    public class GangwarGarage : Loadable<uint> {

        public uint Id { get; set; }
        public List<GangwarGarageSpawn> spawns { get; set; }

        public GangwarGarage(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");

            GangwarGarageSpawn spawn1 = new GangwarGarageSpawn(
                new Vector3(
                    reader.GetFloat("pos_1_x"),
                    reader.GetFloat("pos_1_y"),
                    reader.GetFloat("pos_1_z")
                ),
                reader.GetFloat("heading_1")
            );
            GangwarGarageSpawn spawn2 = new GangwarGarageSpawn(
                new Vector3(
                    reader.GetFloat("pos_2_x"),
                    reader.GetFloat("pos_2_y"),
                    reader.GetFloat("pos_2_z")
                ),
                reader.GetFloat("heading_2")
            );
            GangwarGarageSpawn spawn3 = new GangwarGarageSpawn(
                new Vector3(
                    reader.GetFloat("pos_3_x"),
                    reader.GetFloat("pos_3_y"),
                    reader.GetFloat("pos_3_z")
                ),
                reader.GetFloat("heading_3")
            );
            spawns = new List<GangwarGarageSpawn> {spawn1, spawn2, spawn3};
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public static class GangwarGarageFunctions
    {
        public static GangwarGarageSpawn GetFreeSpawnPosition(this GangwarGarage gangwarGarage) {
            foreach (GangwarGarageSpawn spawnPoint in gangwarGarage.spawns)
            {
                if(spawnPoint.LastUsed.AddSeconds(10) > DateTime.Now) continue;

                bool found = false;
                foreach (SxVehicle vehicle in VehicleHandler.Instance.GetAllVehicles()) {
                    if (vehicle?.Entity.Position.DistanceTo(spawnPoint.Position) <= 2.0f) {
                        found = true;
                    }
                }

                if (!found) {
                    return spawnPoint;
                }
            }
            return null;
        }
    }

    public class GangwarGarageSpawn
    {
        public Vector3 Position { get; }
        public float Heading { get; }
        public DateTime LastUsed { get; }

        public GangwarGarageSpawn(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
            LastUsed = DateTime.Now;
        }
    }
}