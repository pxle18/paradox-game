using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Zone;

namespace VMP_CNR.Module.Shops
{
    public class Shop : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public ColShape Shape { get; set; }
        public float Heading { get; set; }
        public List<ShopItem> ShopItems { get; set; }
        public bool Robbed { get; set; }
        public PedHash Ped { get; set; }
        public Vector3 DeliveryPosition { get; set; }
        public Vector3 RobPosition { get; set; }
        public HashSet<Team> Teams { get; set; }

        public uint OwnerTeam { get; set; }

        public DateTime LastOwned { get; set; }

        public List<DbPlayer> ShoppingRangePlayers { get; set; }

        public bool ShopCanOwned { get; set; }

        public int ShopOwningState { get; set; }

        public uint ActingOwningTeam { get; set; }

        public bool SchwarzgeldUse { get; }

        public bool Marker { get; }
        public uint CWSId { get; }
        public uint EventId { get; }
        public uint DriveIn { get; }

        public Shop(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");

            Shape = Spawners.ColShapes.Create(Position, 3.0f);
            Shape.SetData("shopId", Id);

            Ped = Enum.TryParse(reader.GetString("ped_hash"), true, out PedHash skin) ? skin : PedHash.Michael;

            DeliveryPosition = new Vector3(reader.GetFloat("deliver_pos_x"), reader.GetFloat("deliver_pos_y"),
                reader.GetFloat("deliver_pos_z"));
            Robbed = false;
            ShopItems = new List<ShopItem>();
            SchwarzgeldUse = reader.GetInt32("schwarzgelduse") == 1;
            Marker = reader.GetInt32("marker") == 1;

            var teamString = reader.GetString("team");
            Teams = new HashSet<Team>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId) || Teams.Contains(TeamModule.Instance[teamId])) continue;
                    Teams.Add(TeamModule.Instance[teamId]);
                }
            }

            ShopCanOwned = false;

            RobPosition = new Vector3(reader.GetFloat("rob_pos_x"), reader.GetFloat("rob_pos_y"),
                reader.GetFloat("rob_pos_z"));

            // Set to Default
            OwnerTeam = 0;
            LastOwned = DateTime.Now.AddHours(-1);
            ShoppingRangePlayers = new List<DbPlayer>();
            ShopOwningState = 0;
            ActingOwningTeam = 0;

            CWSId = reader.GetUInt32("cws_id");
            EventId = reader.GetUInt32("event_id");
            DriveIn = reader.GetUInt32("drivein");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void ActiveOwningState(uint teamid)
        {
            if(ActingOwningTeam == 0 || ActingOwningTeam != teamid) //neues team
            {
                ShopOwningState = 0;
                ActingOwningTeam = teamid;


                Zone.Zone zone = ZoneModule.Instance.GetZone(Position);
                TeamModule.Instance.SendMessageToTeam($"Ihre Fraktion beginnt nun damit {Name} {zone.Name} einzuschüchtern!", teamid);
            }
            

            if(ActingOwningTeam == teamid)
            {
                ShopOwningState++;

                int timeToOwned = ShopsModule.TimeToGetShopOwned;
                if(Configurations.Configuration.Instance.DevMode)
                {
                    timeToOwned = 6;
                }

                // Shop wird eingenommen
                if (ShopOwningState >= timeToOwned)
                {
                    ShopOwningState = 0;
                    ActingOwningTeam = 0;
                    OwnerTeam = teamid;
                    LastOwned = DateTime.Now;
                    Zone.Zone zone = ZoneModule.Instance.GetZone(Position);
                    TeamModule.Instance.SendMessageToTeam($"Ihre Fraktion hat erfolgreich {Name} {zone.Name} eingeschüchtert! (10% des Umsatzes gehen in die Fbank) ", teamid);

                    int reward = ShopsModule.RewardShopOwning;

                    reward = Convert.ToInt32(reward / ShoppingRangePlayers.ToList().Where(s => s != null && s.IsValid() && s.TeamId == teamid).Count());

                    // Reward
                    foreach (DbPlayer dbPlayer in ShoppingRangePlayers.ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.TeamId != teamid) continue;
                        dbPlayer.SendNewNotification($"Sie konnten ${reward} aus der Kasse mitgehen lassen!");
                        dbPlayer.GiveMoney(reward);
                    }
                }
            }
        }
    }

    public class ShopItem : Loadable<uint>
    {
        public uint Id { get; set; }
        public uint ShopId { get; set; }
        public uint ItemId { get; set; }
        public int Price { get; set; }
        public string Name { get; set; }

        public int Stored { get; set; }
        public int StoredMax { get; set; }
        public bool IsStoredItem { get; set; }
        public int EKPrice { get; }
        public int RequiredChestItemId { get; }

        public ShopItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            ShopId = reader.GetUInt32("shop_id");
            ItemId = reader.GetUInt32("item_id");
            Name = ItemModelModule.Instance.Get(ItemId).Name;
            Price = reader.GetInt32("price");
            Stored = reader.GetInt32("stored");
            StoredMax = reader.GetInt32("max_stored");
            EKPrice = reader.GetInt32("ek_price");
            RequiredChestItemId = reader.GetInt32("required_chest_item_id");
            IsStoredItem = StoredMax > 0 && EKPrice > 0;
        }
        
        public override uint GetIdentifier()
        {
            return Id;
        }

        public int GetRequiredAmount()
        {
            return StoredMax - Stored;
        }

        public void SaveStoreds()
        {
            MySQLHandler.ExecuteAsync($"UPDATE shops_items SET stored = '{Stored}' WHERE id = '{Id}';");
        }
    }
}