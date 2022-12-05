

using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Animal;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerPosition
    {
        public static async void SetRotation(this Player client, float rotation)
        {
            await NAPI.Task.WaitForMainThread(0);
            client.Rotation = new Vector3(0, 0, rotation);
            await Task.Delay(5); // Thread Wechsel
        }

        public static void SetPosition(this Player player, Vector3 pos)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer != null && dbPlayer.IsValid())
            {
                dbPlayer.SetData("Teleport", 5);
                dbPlayer.SetData("ac_lastPos", pos);
            }

            NAPI.Task.Run(() =>
            {
                player.Position = pos;
            });

            if (dbPlayer != null && dbPlayer.IsValid())
            {
                // Fix GodMode if Aduty
                if (dbPlayer.IsInAdminDuty())
                {
                    dbPlayer.Player.TriggerNewClient("setPlayerAduty", true);
                }

                if (dbPlayer.IsAnimalActiv())
                {
                    dbPlayer.PlayerPed.Ped.Position = pos;
                }
            }
        }
        
        public static void SetWaypoint(this DbPlayer dbPlayer, float x, float y)
        {
            dbPlayer.SetData("waypoint_x", x);
            dbPlayer.SetData("waypoint_y", y);
            dbPlayer.Player.SendWayPoint(x, y);
        }

        public static Vector3 GetWaypoint(this DbPlayer dbPlayer)
        {
            float x = dbPlayer.GetData("waypoint_x");
            float y = dbPlayer.GetData("waypoint_y");
            float z = 0.0f;
            return new Vector3(x, y, z);
        }

        public static void SendWayPoint(this Player player, float x, float y)
        {
            player.TriggerNewClient("setPlayerGpsMarker", x, y);
        }
    }
}