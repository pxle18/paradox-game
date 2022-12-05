using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerCommunications
    {
        public static bool CanCommunicate(this DbPlayer dbPlayer)
        {
            return !(dbPlayer.IsInjured() || dbPlayer.JailTime[0] > 0 ||
                     dbPlayer.IsCuffed || dbPlayer.IsTied);
        }

        public static void BlockCommunications(this DbPlayer dbPlayer)
        {
        }
    }
}