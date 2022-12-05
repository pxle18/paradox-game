using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Forum
{
    public sealed class TeamForumSync : SqlModule<TeamForumSync, TeamForumSyncItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `team_forumsync`;";
        }

        public override void OnPlayerLoggedIn(DbPlayer dbPlayer)
        {
            // Setze User Forum Rechte
            if (dbPlayer.ForumId == 0) return;
            if (!ServerFeatures.IsActive("forumsync")) return;
            if (!Configuration.Instance.DevMode)
            {
                dbPlayer.SynchronizeForum();
            }
        }
    }
}