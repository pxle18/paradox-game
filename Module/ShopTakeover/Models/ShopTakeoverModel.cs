using System;
using System.Collections.Generic;
using System.Text;
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

        public ShopTakeoverModel(MySql.Data.MySqlClient.MySqlDataReader reader) : base(reader)
        {
            Name = reader.GetString("name");
            Shop = ShopModule.Instance[reader.GetUInt32("shop_id")];
            Team = TeamModule.Instance[reader.GetUInt32("ownerTeam")];
            Money = reader.GetInt32("money");
            LastRob = reader.GetDateTime("lastRob");

            TeamsCanAccess = GetHashSet<Team>(reader.GetString("teamsCanAccess"), (teamId, hashSet) =>
            {
                var targetTeam = TeamModule.Instance[teamId];
                if (targetTeam == null) return;

                if (!TeamsCanAccess.Contains(targetTeam))
                    hashSet.Add(targetTeam);
            });
        }

        public override uint GetIdentifier() => Id;
    }
}
