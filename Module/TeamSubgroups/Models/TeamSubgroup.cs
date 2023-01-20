using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.TeamSubgroups.Models
{
    public class TeamSubgroup : DbTeamSubgroup
    {
        public Dictionary<uint, DbPlayer> Members { get; }

        public TeamSubgroup(MySqlDataReader reader) : base(reader)
        {

        }

        public void AddMember(DbPlayer dbPlayer)
        {
            Members[dbPlayer.Id] = dbPlayer;
        }

        public void RemoveMember(DbPlayer dbPlayer)
        {
            if (Members.ContainsKey(dbPlayer.Id))
                Members.Remove(dbPlayer.Id);
        }

        public bool IsMember(DbPlayer dbPlayer)
        {
            return Members.ContainsKey(dbPlayer.Id);
        }

        public List<DbPlayer> GetTeamSubgroupMembers()
        {
            List<DbPlayer> tmpMembers = new List<DbPlayer>();
            foreach (DbPlayer dbPlayer in Members.Values.ToList())
            {
                if (dbPlayer != null && dbPlayer.IsValid() && dbPlayer.TeamSubgroupId == this.Id)
                {
                    tmpMembers.Add(dbPlayer);
                }
            }
            return tmpMembers;
        }
    }
}
