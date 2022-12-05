using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Heist.Planning.WeaponFactory
{
    public class PlanningWeaponFactoryItemModule : SqlModule<PlanningWeaponFactoryItemModule, PlanningWeaponFactoryItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `planningroom_weaponfactory_items`;";
        }
    }
}
