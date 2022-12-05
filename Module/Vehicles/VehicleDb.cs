using GTANetworkAPI;
using VMP_CNR.Handler;

namespace VMP_CNR.Module.Vehicles
{
    public static class VehicleDb
    {
        public static SxVehicle GetVehicle(this Vehicle vehicle)
        {
            if (vehicle == null) return null;
            if (!vehicle.HasData("vehicle"))
            {
                vehicle.DeleteVehicle();
                return null;
            }

            var dbVehicleData = vehicle.GetData<SxVehicle>("vehicle");
            if (dbVehicleData is SxVehicle dbVehicle)
            {
                return dbVehicle;
            }
            return null;
        }

        public static bool IsValid(this SxVehicle sxVehicle)
        {
            return sxVehicle != null && sxVehicle.entity != null && sxVehicle.Data != null && sxVehicle.entity.Handle != null && VehicleHandler.SxVehicles.ContainsKey(sxVehicle.uniqueServerId);
        }
    }
}