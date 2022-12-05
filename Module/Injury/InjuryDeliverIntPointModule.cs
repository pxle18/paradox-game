using System;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Injury
{
    public class InjuryDeliverIntPointModule : SqlModule<InjuryDeliverIntPointModule, InjuryDeliverIntPoint, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `injury_deliver_int_points`;";
        }
    }
}