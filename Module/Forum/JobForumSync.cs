using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Forum
{
    public sealed class JobForumSync : SqlModule<JobForumSync, JobForumSyncItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `job_forumsync`;";
        }

        /*public override void OnPlayerLoggedIn(DbPlayer dbPlayer)
        {
            if (dbPlayer.ForumId == 0) return;
            if (!Configuration.Instance.DevMode)
            {
                dbPlayer.SynchronizeJobForum();
            }
        }*/
    }
}