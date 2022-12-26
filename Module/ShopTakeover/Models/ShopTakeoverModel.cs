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

        public ShopTakeoverModel(MySql.Data.MySqlClient.MySqlDataReader reader) : base(reader)
        {
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
