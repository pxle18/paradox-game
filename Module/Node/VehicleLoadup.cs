using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Tuning;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Node
{
    public class VehicleLoadup : Module<VehicleLoadup>
    {
        public static List<SxVehicle> afterLoadVehicles = new List<SxVehicle>();

        public override bool Load(bool reload = false)
        {
            afterLoadVehicles = new List<SxVehicle>();
            return base.Load(reload);
        }

        public void StartResyncVehicleBridges()
        {
            Task.Run(async () =>
            {
                Configurations.Configuration.Instance.CanBridgeUsed = true;
                foreach (SxVehicle sxVehicle in afterLoadVehicles)
                {
                    if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.entity == null) continue;
                    await NAPI.Task.WaitForMainThread(200);// Workaround for floods
                }
            });
        }
    }
}
