using System;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.NpcSpawner;

namespace VMP_CNR.Module.Freiberuf.Trucker
{
    public class TruckerDepotLoadModule : SqlModule<TruckerDepotLoadModule, TruckerDepot, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `trucker_depots`;";
        }

        public TruckerDepot GetByPosition(Vector3 Position)
        {
            return GetAll().Values.FirstOrDefault(point => point.Position.DistanceTo(Position) < 3.0f);
        }

        public TruckerDepot GetByLoadingPosition(Vector3 Position)
        {
            return GetAll().Values.FirstOrDefault(point => point.LoadingPoint.DistanceTo(Position) < 8.0f);
        }
    }

    public class TruckerDepot : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public Vector3 LoadingPoint { get; set; }
        public float LoadingHeading { get; set; }

        public TruckerDepot(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(
                reader.GetFloat("pos_x"),
                reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z")
            );
            Heading = reader.GetFloat("heading");
            LoadingPoint = new Vector3(
                reader.GetFloat("loading_pos_x"),
                reader.GetFloat("loading_pos_y"),
                reader.GetFloat("loading_pos_z")
            );
            LoadingHeading = reader.GetFloat("loading_heading");

            // NPC
            new Npc(PedHash.Trucker01SMM, Position, Heading, 0);

            // Blip
            Spawners.Blips.Create(Position, "Trucker", 67, 1.0f, true, 4);

            // Markers
            Spawners.Markers.Create(39, LoadingPoint, new Vector3(), new Vector3(), 1.0f, 255, 0, 255, 0);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class TruckerQuestLoadModule : SqlModule<TruckerQuestLoadModule, TruckerQuest, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `trucker_depot_quests`;";
        }
    }

    public class TruckerQuest : Loadable<uint>
    {
        public uint Id { get; set; }

        public int SourceTruckerDepot { get; set; }
        public int DestinationTruckerDepot { get; set; }

        public int DelayMin { get; set; }

        public int MinReward { get; set; }

        public int MaxReward { get; set; }

        public DateTime avaiableAt { get; set; }

        public TruckerQuest(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            SourceTruckerDepot = reader.GetInt32("source_depot_id");
            DestinationTruckerDepot = reader.GetInt32("destination_depot_id");
            DelayMin = reader.GetInt32("delay_in_min");
            MinReward = reader.GetInt32("min_reward");
            MaxReward = reader.GetInt32("max_reward");
            avaiableAt = DateTime.Now;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}