using GTANetworkAPI;

namespace VMP_CNR.Module.Players
{
    public static class PlayerNative
    {
        public static void SendNative(this Player player, ulong longHash, params object[] args)
        {
            //API.Shared.SendNativeToPlayer(player, longHash, args);
        }
    }
}