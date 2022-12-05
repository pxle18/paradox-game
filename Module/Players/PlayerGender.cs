using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerGender
    {
        public static bool IsFemale(this DbPlayer dbPlayer)
        {
            if (dbPlayer.AccountStatus != AccountStatus.LoggedIn) return false;
            return dbPlayer.Customization?.Gender == 1;
        }

        public static bool IsMale(this DbPlayer dbPlayer)
        {
            if (dbPlayer.AccountStatus != AccountStatus.LoggedIn) return false;
            return dbPlayer.Customization?.Gender == 0;
        }
    }
}