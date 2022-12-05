using GTANetworkAPI;

namespace VMP_CNR
{
    public static class NetHandlePlayer
    {
        public static Player ToPlayer(this NetHandle netHandle)
        {
            return NAPI.Player.GetPlayerFromHandle(netHandle);
        }
    }
}