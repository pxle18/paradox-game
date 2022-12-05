using GTANetworkAPI;

namespace VMP_CNR
{
    public static class NetHandleType
    {
        public static EntityType GetEntityType(this NetHandle netHandle)
        {
            return netHandle.Type;
        }
    }
}