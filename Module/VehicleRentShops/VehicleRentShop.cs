using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.VehicleRentShops
{
    public class VehicleRentShopItem : Loadable<uint>
    {
        [JsonProperty(PropertyName = "Id")]
        public uint Id { get; set; }

        [JsonIgnore]
        public uint VehicleRentShopId { get; set; }

        [JsonProperty(PropertyName = "ModelId")]
        public uint VehicleModelId { get; set; }

        [JsonProperty(PropertyName = "Price")]
        public int Price { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }
        public VehicleRentShopItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            VehicleRentShopId = reader.GetUInt32("vehiclerent_shop_id");
            VehicleModelId = reader.GetUInt32("model_id");
            Price = reader.GetInt32("price");
            Name = reader.GetString("name");
        }
        public override uint GetIdentifier()
        {
            return Id;
        }
    }


    public class VehicleRentShopSpawn : Loadable<uint>
    {
        public uint Id { get; set; }

        public uint VehicleRentShopId { get; set; }

        public Vector3 Position { get; set; }

        public float Heading { get; set; }

        public VehicleRentShopSpawn(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            VehicleRentShopId = reader.GetUInt32("vehiclerent_shop_id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
        }
        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class VehicleRentShop : Loadable<uint>
    {
        [JsonProperty(PropertyName = "Id")]
        public uint Id { get; set; }

        [JsonIgnore]
        public Vector3 Position { get; set; }

        [JsonIgnore]
        public float Heading { get; set; }

        [JsonIgnore]
        public int MaxRentAmount { get; set; }

        [JsonProperty(PropertyName = "FreeToRent")]
        public int FreeToRent { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Items")]
        public List<VehicleRentShopItem> ShopItems { get; set; }

        [JsonIgnore]
        public List<VehicleRentShopSpawn> ShopSpawns { get; set; }

        public VehicleRentShop(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            MaxRentAmount = reader.GetInt32("max_rent_amount");
            Name = reader.GetString("name");

            PedHash Ped = Enum.TryParse(reader.GetString("pedhash"), true, out PedHash skin) ? skin : PedHash.Michael;
            new Npc(Ped, Position, Heading, 0);

            FreeToRent = MaxRentAmount;
            ShopItems = new List<VehicleRentShopItem>();
            ShopSpawns = new List<VehicleRentShopSpawn>();
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void ActualizeLeftAmount()
        {
            int actualUsed = VehicleHandler.Instance.GetJobVehicles().ToList().Where(v => v.jobid == (VehicleRentShopModule.FakeJobVehicleRentShopId + Id)).Count();
            int leftAmount = MaxRentAmount - actualUsed;
            FreeToRent = leftAmount > 0 ? leftAmount : 0;

            return;
        }

        public VehicleRentShopSpawn GetFreeSpawnPosition()
        {
            foreach (var spawnPoint in this.ShopSpawns)
            {
                var found = false;
                foreach (var vehicle in VehicleHandler.Instance.GetAllVehicles())
                {
                    if (vehicle?.entity.Position.DistanceTo(spawnPoint.Position) <= 2.0f)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    return spawnPoint;
                }
            }

            return null;
        }
    }
}
