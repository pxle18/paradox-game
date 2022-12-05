
using GTANetworkAPI;
using System;
using VMP_CNR.Module.Doors;

namespace VMP_CNR.Module.Players
{
    public static class PlayerDoor
    {
        public static void SetDoorState(this Player player, Door door)
        {
            //TriggerNewClient does not accept int64 -> Workaround. Range between -2147483648 and 4294967295 allowed.
            if (door.Model >= 0)
                player.TriggerNewClient("setStateOfClosestDoorOfType", (uint)door.Model, door.Position.X, door.Position.Y, door.Position.Z, door.Locked, 0, false);
            else
                player.TriggerNewClient("setStateOfClosestDoorOfType", (int)door.Model, door.Position.X, door.Position.Y, door.Position.Z, door.Locked, 0, false);

        }
    }
}