using GTANetworkMethods;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.NSA;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Computer(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.IsValid()) return false;
            if (dbPlayer.CanNSADuty())
            {
                Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.NSAComputerMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            if (dbPlayer.IsInDuty() && dbPlayer.TeamId == (uint)teams.TEAM_FIB)
            {
                Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.NSAComputerMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            if ((dbPlayer.IsInDuty() && dbPlayer.Team.Id == (int)teams.TEAM_GOV))
            {
                Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.GOVComputerMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }
    }
}