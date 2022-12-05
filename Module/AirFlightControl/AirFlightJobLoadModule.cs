using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.NpcSpawner;

namespace VMP_CNR.Module.AirFlightControl
{
    public class AirFlightAirportModule : SqlModule<AirFlightAirportModule, AirFlightAirport, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `airflight_airports`;";
        }

        public AirFlightAirport GetByPosition(Vector3 Position)
        {
            return GetAll().Values.Where(airport => airport.Position.DistanceTo(Position) < 3.0f).FirstOrDefault();
        }
        public AirFlightAirport GetByLoadingPosition(Vector3 Position)
        {
            return GetAll().Values.Where(airport => airport.LoadingPoint.DistanceTo(Position) < 8.0f).FirstOrDefault();
        }
    }

    public class AirFlightAirport : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public Vector3 LoadingPoint { get; set; }

        public AirFlightAirport(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            LoadingPoint = new Vector3(reader.GetFloat("loading_pos_x"), reader.GetFloat("loading_pos_y"), reader.GetFloat("loading_pos_z"));

            // NPC
            new Npc(PedHash.AirworkerSMY, Position, Heading, 0);

            // Blip
            Spawners.Blips.Create(Position, "", 578, 1.0f, true, 4);

            // Markers
            Spawners.Markers.Create(7, LoadingPoint, new Vector3(), new Vector3(), 1.0f, 255, 255, 0, 0);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class AirFlightAirportQuestsModule : SqlModule<AirFlightAirportQuestsModule, AirFlightAirportQuests, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `airflight_airports_quests`;";
        }
    }

    public class AirFlightAirportQuests : Loadable<uint>
    {
        public uint Id { get; set; }

        public int SourceAirport { get; set; }
        public int DestinationAirport { get; set; }

        public int DelayMin { get; set; }

        public int MinReward { get; set; }

        public int MaxReward { get; set; }

        public DateTime avaiableAt { get; set; }

        public AirFlightAirportQuests(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            SourceAirport = reader.GetInt32("source_airport_id");
            DestinationAirport = reader.GetInt32("destination_airport_id");
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
