using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Business.FuelStations
{
    public class FuelStationGasModule : SqlModule<FuelStationGasModule, FuelStationGas, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `business_fuelstations_gas`;";
        }
    }
}
