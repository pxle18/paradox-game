using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using VMP_CNR.Handler;

namespace VMP_CNR.Module.Vehicles
{
    public static class VehicleSeat
    {
        public static int GetNextFreeSeat(this Vehicle vehicle, int offset = 0)
        {
            var seats = new bool[(int) Math.Round((double)vehicle.MaxOccupants)];

            var unavailableSeats = new HashSet<int>();

            foreach (Player player in vehicle.Occupants)
            {
                unavailableSeats.Add(player.VehicleSeat);
            }

            for (int i = offset, length = (int) Math.Round((double)vehicle.MaxOccupants); i < length; i++)
            {
                if (!unavailableSeats.Contains(i))
                {
                    return i;
                }
            }

            return -2;
        }

        public static bool IsSeatFree(this Vehicle vehicle, int seat)
        {
            SxVehicle sxVeh = vehicle.GetVehicle();
            if (sxVeh == null || !sxVeh.IsValid()) return false;

            return vehicle.IsValidSeat(seat) && sxVeh.Occupants.IsSeatFree(seat);
        }

        public static bool IsValidSeat(this Vehicle vehicle, int seat)
        {
            return seat > -2 && seat < vehicle.MaxOccupants - 1;
        }
    }
}