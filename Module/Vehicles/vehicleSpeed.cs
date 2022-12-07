using System;
using GTANetworkAPI;
using VMP_CNR.Handler;

namespace VMP_CNR.Module.Vehicles
{
    public static class VehicleSpeed
    {
        public static double fakeMulti = 1.20f;
        public static double fakeMultiReverse = 0.80f;

        public static int GetSpeed(this SxVehicle sxVeh)
        {
            var velocity = NAPI.Entity.GetEntityVelocity(sxVeh.Entity);
            var speed = Math.Sqrt(
                velocity.X * velocity.X +
                velocity.Y * velocity.Y +
                velocity.Z * velocity.Z
            );
            double kmh = speed * 3.6;
            double fakeKmh = kmh * fakeMulti; // 20% higher...
            return Convert.ToInt32(fakeKmh); // from m/s to km/h
        }
    }
}