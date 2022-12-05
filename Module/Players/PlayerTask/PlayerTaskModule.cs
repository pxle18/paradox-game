using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.PlayerTask
{
    public sealed class PlayerTaskModule : Module<PlayerTaskModule>
    {
        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            dbPlayer.LoadTasks();
        }

        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            //TODO: ConcurrentBag<>
            if (dbPlayer?.AccountStatus != AccountStatus.LoggedIn) return;
            if (dbPlayer.PlayerTasks.Count > 0) dbPlayer.CheckTasks();
        }
    }
}