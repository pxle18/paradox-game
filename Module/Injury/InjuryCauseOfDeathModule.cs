using System;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Module.Injury
{
    public class InjuryCauseOfDeathModule : SqlModule<InjuryCauseOfDeathModule, InjuryCauseOfDeath, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(InjuryTypeModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `injury_causes_of_death`;";
        }
    }
}