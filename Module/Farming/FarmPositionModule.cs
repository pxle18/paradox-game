using System;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Module.Farming
{
    public class FarmPositionModule : SqlModule<FarmPositionModule, FarmPosition, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] {typeof(FarmSpotModule)};
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `farm_positions`;";
        }

        protected override void OnItemLoaded(FarmPosition farmPosition)
        {
            var farmSpot = FarmSpotModule.Instance[farmPosition.FarmSpotId];
            if (farmSpot == null)
            {
                Logger.Debug($"Void FarmSpotId {farmPosition.FarmSpotId}");
                return;
            }

            farmSpot.Positions.Add(farmPosition.Position);
        }
    }
}