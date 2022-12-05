using System;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Module.Injury
{
    public class InjuryTypeModule : SqlModule<InjuryTypeModule, InjuryType, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(InjuryModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `injury_types`;";
        }

        protected override void OnItemLoaded(InjuryType injury)
        {
            return;
        }
    }
}