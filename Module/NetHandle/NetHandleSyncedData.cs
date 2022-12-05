using GTANetworkAPI;

namespace VMP_CNR
{
    public static class NetHandleSyncedData
    {   
        public static void RemoveEntityDataWhenExists(this Entity entity, string key)
        {
            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(0);
                if (entity.HasSharedData(key))
                {
                    entity.ResetSharedData(key);
                }
            });
        }
        
        public static void RemoveEntityDataWhenExists(this Player client, string key)
        {
            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(0);
                if (client.HasSharedData(key))
                {
                    client.ResetSharedData(key);
                }
            });
        }
    }
}