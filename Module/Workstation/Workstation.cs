using GTANetworkAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VMP_CNR.Module.NpcSpawner;

namespace VMP_CNR.Module.Workstation
{
    public enum WorkstationSpecialType
    {
        Default = 0,
        PlanningRoomStahlpatronen = 1,
    }

    public class Workstation : Loadable<uint>
    {

        [JsonProperty(PropertyName = "Id")]
        public uint Id { get; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; }

        [JsonIgnore]
        public Vector3 NpcPosition { get; }

        [JsonIgnore]
        public float NpcHeading { get; }

        [JsonIgnore]
        public PedHash PedHash { get; }

        [JsonIgnore]
        public Vector3 SourcePosition { get; }

        [JsonIgnore]
        public Vector3 FuelPosition { get; }

        [JsonIgnore]
        public Vector3 EndPosition { get; }

        [JsonIgnore]

        public HashSet<uint> LimitTeams { get; set; }

        [JsonIgnore]
        public Dictionary<uint, int> SourceConvertItems { get; set; }

        [JsonIgnore]
        public int Fuel5MinAmount { get; }

        [JsonIgnore]
        public uint FuelItemId { get; }

        [JsonIgnore]
        public int End5MinAmount { get; }

        [JsonIgnore]
        public uint EndItemId { get; }

        [JsonIgnore]

        public int LimitedSourceSize { get; }

        [JsonProperty(PropertyName = "RequiredLevel")]
        public int RequiredLevel { get; }

        [JsonIgnore]
        public uint Dimension { get; }

        [JsonIgnore]

        public bool Interval15 { get; set; }

        [JsonIgnore]
        public WorkstationSpecialType SpecialType { get; set; }

        public Workstation(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            NpcPosition = new Vector3(reader.GetFloat("npc_pos_x"), reader.GetFloat("npc_pos_y"), reader.GetFloat("npc_pos_z"));
            NpcHeading = reader.GetFloat("npc_heading");
            PedHash = Enum.TryParse(reader.GetString("npc_model"), true, out PedHash skin) ? skin : PedHash.ShopKeep01;
            SourcePosition = new Vector3(reader.GetFloat("source_pos_x"), reader.GetFloat("source_pos_y"), reader.GetFloat("source_pos_z"));
            FuelPosition = new Vector3(reader.GetFloat("fuel_pos_x"), reader.GetFloat("fuel_pos_y"), reader.GetFloat("fuel_pos_z"));
            EndPosition = new Vector3(reader.GetFloat("end_pos_x"), reader.GetFloat("end_pos_y"), reader.GetFloat("end_pos_z"));

            LimitedSourceSize = reader.GetInt32("limited_source_amount");

            SourceConvertItems = new Dictionary<uint, int>();

            string sourceItemString = reader.GetString("source_items");

            if (!string.IsNullOrEmpty(sourceItemString))
            {
                var splittedItemsSeperated = sourceItemString.Split(',');
                foreach (var splittedItemContainer in splittedItemsSeperated)
                {
                    if (splittedItemContainer == null || splittedItemContainer.Length <= 0 || !splittedItemContainer.Contains(":")) continue;
                    var splittedItemContainerParts = splittedItemContainer.Split(':');
                    if (splittedItemContainerParts.Length < 2) continue;
                    if (!UInt32.TryParse(splittedItemContainerParts[0], out uint splittedItemId)) continue;
                    if (!Int32.TryParse(splittedItemContainerParts[1], out int splittedItemAmount)) continue;

                    SourceConvertItems.Add(splittedItemId, splittedItemAmount);
                }
            }

            var teamString = reader.GetString("limited_teams");
            LimitTeams = new HashSet<uint>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId)) continue;
                    LimitTeams.Add(teamId);
                }
            }

            FuelItemId = reader.GetUInt32("fuel_item_id");
            Fuel5MinAmount = reader.GetInt32("fuel_convert_amount");
            EndItemId = reader.GetUInt32("end_item_id");
            End5MinAmount = reader.GetInt32("end_convert_amount");

            Dimension = reader.GetUInt32("dimension");
            RequiredLevel = reader.GetInt32("required_level");

            Interval15 = reader.GetInt32("interval_15") == 1;

            SpecialType = (WorkstationSpecialType)reader.GetInt32("specialtype");

            // NPC
            if (NpcPosition.X != 0 && NpcPosition.Y != 0)
            { 
                ColShape shape = Spawners.ColShapes.Create(NpcPosition, 1.5f, 0);
                shape.SetData("workstation", Id);
                new Npc(PedHash, NpcPosition, NpcHeading, 0);
            }

            // Markers
            NAPI.Marker.CreateMarker(25, (EndPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, Dimension);
            NAPI.Marker.CreateMarker(25, (SourcePosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, Dimension);

            // Optional
            if (FuelItemId != 0) NAPI.Marker.CreateMarker(25, (FuelPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, Dimension);
        }
        
        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
