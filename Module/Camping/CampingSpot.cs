using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Camper
{
    public class CampingPlace : Loadable<uint>
    {
        public uint Id { get; set; }
        public int PlayerId { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 WaterBarrelPosition { get; set; }
        public Vector3 FuelBarrelPosition { get; set; }
        public Vector3 GrillPosition { get; set; }

        public float GrillHeading { get; set; }

        public int Fuel { get; set; }
        public int Water { get; set; }
        public ColShape Shape { get; set; }

        public List<DbPlayer> CookingPlayers { get; set; }

        public int FireStateTent { get; set; }

        public int FireStateBed { get; set; }

        public int FireStateTable { get; set; }

        public int SmokingState { get; set; }

        public bool IsCocain { get; set; }
        public CampingPlace(MySqlDataReader reader) : base(reader)
        {
            
            Id = reader.GetUInt32("id");
            PlayerId = reader.GetInt32("player_id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            WaterBarrelPosition = new Vector3(reader.GetFloat("waterbarrel_x"), reader.GetFloat("waterbarrel_y"), reader.GetFloat("waterbarrel_z"));
            FuelBarrelPosition = new Vector3(reader.GetFloat("fuelbarrel_x"), reader.GetFloat("fuelbarrel_y"), reader.GetFloat("fuelbarrel_z"));
            GrillPosition = new Vector3(reader.GetFloat("grill_x"), reader.GetFloat("grill_y"), reader.GetFloat("grill_z"));
            GrillHeading = reader.GetFloat("grill_heading");
            Fuel = reader.GetInt32("fuel");
            Water = reader.GetInt32("water");
            IsCocain = reader.GetInt32("iscocain") == 1;
            CookingPlayers = new List<DbPlayer>();
            FireStateBed = 0;
            FireStateTable = 0;
            FireStateTent = 0;
            SmokingState = 0;
        }

        public CampingPlace(int playerId, Vector3 Pos, bool cocain)
        {
            PlayerId = playerId;
            Position = Pos;
            Fuel = 0;
            Water = 0;
            WaterBarrelPosition = new Vector3();
            FuelBarrelPosition = new Vector3();
            CookingPlayers = new List<DbPlayer>();
            GrillPosition = new Vector3(0, 0, 0);
            GrillHeading = 0.0f;
            IsCocain = cocain;
            FireStateBed = 0;
            FireStateTable = 0;
            FireStateTent = 0;
            SmokingState = 0;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void Create()
        {
            NAPI.Task.Run(() =>
            {
                Shape = Spawners.ColShapes.Create(Position, CampingModule.StreamingCampingRange, 0);
                Shape.SetData("campingPlace", PlayerId);
            });
        }

        public void UpdateWaterAndFuel()
        {
            MySQLHandler.ExecuteAsync($"UPDATE camping_places SET fuel = '{Fuel}', water = '{Water}' WHERE player_id = '{PlayerId}';");
        }

        public void SaveFuelBarrelPosition()
        {
            MySQLHandler.ExecuteAsync($"UPDATE camping_places SET " +
                $"fuelbarrel_x = '{FuelBarrelPosition.X.ToString().Replace(",", ".")}', fuelbarrel_y = '{FuelBarrelPosition.Y.ToString().Replace(",", ".")}', " +
                $"fuelbarrel_z = '{FuelBarrelPosition.Z.ToString().Replace(",", ".")}' WHERE player_id = '{PlayerId}';");
        }

        public void SaveWaterBarrelPosition()
        {
            MySQLHandler.ExecuteAsync($"UPDATE camping_places SET " +
                $"waterbarrel_x = '{WaterBarrelPosition.X.ToString().Replace(",", ".")}', waterbarrel_y = '{WaterBarrelPosition.Y.ToString().Replace(",", ".")}', " +
                $"waterbarrel_z = '{WaterBarrelPosition.Z.ToString().Replace(",", ".")}' WHERE player_id = '{PlayerId}';");
        }
        public void SaveGrillPosition()
        {
            MySQLHandler.ExecuteAsync($"UPDATE camping_places SET " +
                $"grill_x = '{GrillPosition.X.ToString().Replace(",", ".")}', grill_y = '{GrillPosition.Y.ToString().Replace(",", ".")}', " +
                $"grill_z = '{GrillPosition.Z.ToString().Replace(",", ".")}', grill_heading = '{GrillHeading.ToString().Replace(",", ".")}' WHERE player_id = '{PlayerId}';");
        }
        public void SetLastUsedNow()
        {
            MySQLHandler.ExecuteAsync($"UPDATE camping_places SET lastused = NOW() WHERE player_id = '{PlayerId}';");
        }

        public List<CampingClientObj> GetCampingSpotObjects(float distance)
        {
            List<CampingClientObj> campingClientObjs = new List<CampingClientObj>();

            campingClientObjs.Add(new CampingClientObj(CampingModule.TentObjectId, Position.Add(CampingModule.AdjustmentTent), CampingModule.AdjustmentRotTent));
            campingClientObjs.Add(new CampingClientObj(CampingModule.BedObjectId, Position.Add(CampingModule.AdjustmentBed), CampingModule.AdjustmentRotBed));
            if(IsCocain) campingClientObjs.Add(new CampingClientObj(CampingModule.TableObjectId, Position.Add(CampingModule.AdjustmentTable), CampingModule.AdjustmentRotTable));

            if(Fuel > 0)
            {
                campingClientObjs.Add(new CampingClientObj(-1344435013, FuelBarrelPosition, new Vector3()));
            }
            if(Water > 0)
            {
                campingClientObjs.Add(new CampingClientObj(1298403575, WaterBarrelPosition, new Vector3()));
            }

            if (GrillPosition != new Vector3(0, 0, 0))
            {
                campingClientObjs.Add(new CampingClientObj(1903501406, GrillPosition, new Vector3(0, 0, GrillHeading)));
            }

            if (FireStateTent > 0)
            {
                Vector3 pos = Position.Add(CampingModule.AdjustmentTent);
                pos.Z += 0.7f;
                campingClientObjs.Add(new CampingClientObj(-1065766299, pos, CampingModule.AdjustmentRotTent));
            }

            if(FireStateTable > 0 && IsCocain)
            {
                Vector3 pos = Position.Add(CampingModule.AdjustmentTable);
                pos.Z += 0.8f;
                campingClientObjs.Add(new CampingClientObj(-1065766299, pos, CampingModule.AdjustmentRotBed));
            }

            if(FireStateBed > 0)
            {
                Vector3 pos = Position.Add(CampingModule.AdjustmentBed);
                pos.Z += 0.6f;
                campingClientObjs.Add(new CampingClientObj(-1065766299, pos, CampingModule.AdjustmentRotBed));
            }

            if (SmokingState > 0)
            {
                Vector3 pos = new Vector3(Position.X, Position.Y, Position.Z);
                pos.Z -= 6.5f;
                campingClientObjs.Add(new CampingClientObj(-1903396261, pos, CampingModule.AdjustmentRotBed));
            }
            return campingClientObjs;
        }

        public void RefreshObjectsForPlayerInRange()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (DbPlayer dbPlayer in Players.Players.Instance.GetPlayersInRange(Position, CampingModule.StreamingCampingRange).Where(p => p.Dimension[0] == 0))
                {
                    dbPlayer.Player.TriggerNewClient("createCustomObjects", "cp_" + PlayerId, NAPI.Util.ToJson(GetCampingSpotObjects(dbPlayer.Player.Position.DistanceTo(Position))));
                }
            }));
        }


        public void RemoveAllObjectsForPlayerInRange()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (DbPlayer dbPlayer in Players.Players.Instance.GetPlayersInRange(Position, CampingModule.StreamingCampingRange).Where(p => p.Dimension[0] == 0))
                {
                    dbPlayer.Player.TriggerNewClient("removeCustomObjects", "cp_" + PlayerId);
                }
            }));
        }
    }

    public class CampingClientObj
    {
        [JsonProperty(PropertyName = "objectid")]
        public int ObjectId { get; set; }

        [JsonProperty(PropertyName = "pos")]
        public Vector3 Pos { get; set; }

        [JsonProperty(PropertyName = "rot")]
        public Vector3 Rot { get; set; }

        public CampingClientObj(int objid, Vector3 pos, Vector3 rot)
        {
            ObjectId = objid;
            Pos = pos;
            Rot = rot;
        }
    }
}
