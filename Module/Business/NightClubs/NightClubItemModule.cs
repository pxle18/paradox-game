using System;
using VMP_CNR.Module.Items;

namespace VMP_CNR.Module.Business.NightClubs
{
    class NightClubItemModule : SqlModule<NightClubItemModule, NightClubItem, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemModelModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `business_nightclubs_items`;";
        }
    }
}
