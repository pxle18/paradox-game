using System;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.TeamSubgroups.Models;

namespace VMP_CNR.Module.TeamSubgroups
{
    public sealed class TeamSubgroupModule : SqlModule<TeamSubgroupModule, TeamSubgroup, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM 'teamsubgroups';";
        }
    }

    public static class TeamSubgroupExtension
    {
        public static void SaveTeamSubgroupFight(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE teamsubgroups SET `lastFight` = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `id` = '{dbPlayer.TeamSubgroupId}'");
        }
    }
}
