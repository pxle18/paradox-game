using VMP_CNR.Module.ClientUI.Apps;
using GTANetworkAPI;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone.Apps;

namespace VMP_CNR.Module.Teams.Apps
{
    public class TeamEditApp : SimpleApp
    {
        public TeamEditApp() : base("TeamEditApp")
        {
        }

        [RemoteEvent]
        public void leaveTeam(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.SetTeam((uint) TeamList.Zivilist);
        }
    }
}