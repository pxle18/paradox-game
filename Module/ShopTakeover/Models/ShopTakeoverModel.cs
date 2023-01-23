using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Maps;
using VMP_CNR.Module.Maps.Models;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Shops;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.ShopTakeover.Models
{
    public class ShopTakeoverModel : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Shop Shop { get; set; }
        public Team Team { get; set; }
        public LoadableScriptMapModel ScriptMap { get; set; }
        public int Money { get; set; }
        public DateTime LastRob { get; set; }

        public Marker Marker { get; set; }
        public ColShape ColShape { get; set; }

        public Team Attacker { get; set; }
        public bool IsInTakeover { get; set; } = false;

        public Dictionary<uint, DbPlayer> Players { get; set; } = new Dictionary<uint, DbPlayer>();
        
        /**
         * Key: TeamId
         * Value: Player
         * - Um dann zu schauen: Wurden alle Personen in der jeweiligen Fraktion schon getötet?
         * - Ob es sinn macht?
         */
        public Dictionary<uint, DbPlayer> Deaths { get; set; } = new Dictionary<uint, DbPlayer>();

        public ShopTakeoverModel(MySql.Data.MySqlClient.MySqlDataReader reader) : base(reader)
        {
            Name = reader.GetString("name");
            Shop = ShopModule.Instance[reader.GetUInt32("shop_id")];
            Team = TeamModule.Instance[reader.GetUInt32("ownerTeam")];
            Money = reader.GetInt32("money");
            LastRob = reader.GetDateTime("lastRob");

            Map = LoadableScriptMapModule.Instance[
                reader.GetUInt32("script_map_id")
            ];
        }

        public override uint GetIdentifier() => Id;
        public bool CanAttacked() => Configurations.Configuration.Instance.DevMode || LastRob.AddHours(8) <= DateTime.Now;

        public void AddMoney(int moneyAmount)
        {
            Money += moneyAmount;

            string query = $"UPDATE `shop_takeovers` SET `money` = '{Money}' WHERE `id` = '{Id}'";
            MySQLHandler.ExecuteAsync(query);
        }

        public void ClearMoney()
        {
            Money = 0;

            string query = $"UPDATE `shop_takeovers` SET `money` = '{Money}' WHERE `id` = '{Id}'";
            MySQLHandler.ExecuteAsync(query);
        }

        public void SetOwner(Team team)
        {
            Team = TeamModule.Instance[team.Id];

            string query = $"UPDATE `shop_takeovers` SET `ownerTeam` = '{team.Id}' WHERE `id` = '{Id}'";
            MySQLHandler.ExecuteAsync(query);
        }

        public void UpdateLastRob()
        {
            LastRob = DateTime.Now;
            ShopTakeoverModule.Instance.Update(Id, this, "shop_takeovers", $"id = {Id}", "lastRob", DateTime.Now);
        }
    }
}
