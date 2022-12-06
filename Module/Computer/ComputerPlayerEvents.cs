using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer
{
    public static class ComputerPlayerEvents
    {
        public static bool CanAccessComputerApp(this DbPlayer dbPlayer, ComputerApp computerApp)
        {
            if (computerApp.Type == ComputerTypes.Computer)
            {
                // Wenn nicht im benötigten Team
                if (computerApp.Teams.Count > 0 && !computerApp.Teams.Contains(dbPlayer.TeamId)) return false;

                // Wenn nicht benötigter Rang
                if (computerApp.Rang > 0 && dbPlayer.TeamRank < computerApp.Rang) return false;

                // Wenn Duty vorrausgesetzt wird und nicht duty ist
                if (computerApp.Duty && !dbPlayer.Duty && dbPlayer.TeamId != (int)TeamTypes.TEAM_LSC) return false;

                return true;
            }
            else if (computerApp.Type == ComputerTypes.AdminTablet)
            {
                return (dbPlayer.Rank.CanAccessFeature(computerApp.AppName) || dbPlayer.Rank.CanAccessFeature("allApps"));
            }

            return false;
        }
    }
}
