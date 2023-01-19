using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Logging;
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
        public HashSet<Team> TeamsCanAccess { get; set; }
        public int Money { get; set; }
        public DateTime LastRob { get; set; }

        public Marker Marker { get; set; }
        public ColShape ColShape { get; set; }

        public Team Attacker { get; set; }
        public bool IsInTakeover { get; set; } = false;

        public Dictionary<uint, DbPlayer> Players { get; set; } = new Dictionary<uint, DbPlayer>();

        public ShopTakeoverModel(MySql.Data.MySqlClient.MySqlDataReader reader) : base(reader)
        {
            Name = reader.GetString("name");
            Shop = ShopModule.Instance[reader.GetUInt32("shop_id")];
            Team = TeamModule.Instance[reader.GetUInt32("ownerTeam")];
            Money = reader.GetInt32("money");
            LastRob = reader.GetDateTime("lastRob");

            TeamsCanAccess = new HashSet<Team>();

            var teamString = reader.GetString("teamsCanAccess");
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId) || TeamsCanAccess.Contains(TeamModule.Instance[teamId])) continue;
                    TeamsCanAccess.Add(TeamModule.Instance[teamId]);
                }
            }
        }

        public override uint GetIdentifier() => Id;
        public bool CanAttacked() => Configurations.Configuration.Instance.DevMode || LastRob.AddHours(72) <= DateTime.Now;

        public void AddMoney(int moneyAmount)
        {
            Money += moneyAmount;
            ShopTakeoverModule.Instance.Update(Id, this, "shop_takeovers", $"id = {Id}", "money", Money);
        }
    }
}
