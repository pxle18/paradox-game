using System.Runtime.CompilerServices;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Commands
{
    public static class PlayerCommandPermissions
    {
        public static bool CanAccessMethod(this DbPlayer dbPlayer, [CallerMemberName] string callerName = "")
        {
            if (!dbPlayer.IsValid()) return false;
            var methodName = callerName.ToLower();
            var commandPermissions = CommandPermissions.Instance[methodName];
            if (commandPermissions == null) return true;
            if (Configurations.Configuration.Instance.DevMode) return true;
            if (dbPlayer.Rank.Id == 6) return true;
            if (commandPermissions.TeamId != null && dbPlayer.TeamId != commandPermissions.TeamId) return false;
            if (commandPermissions.PlayerRankPermission && !dbPlayer.Rank.Commands.Contains(methodName)) return false;
            if (!commandPermissions.AllowedDeath && dbPlayer.IsInjured() && !dbPlayer.Rank.CanAccessFeature("deathCommands"))
                return false;
            if (!commandPermissions.AllowedOnCuff && dbPlayer.IsCuffed) return false;
            if (!commandPermissions.AllowedOnTied && dbPlayer.IsTied) return false;
            return true;
        }
    }
}