using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Clothes.Props;

namespace VMP_CNR.Module.Outfits
{
    public class OutfitPropModule : SqlModule<OutfitPropModule, OutfitProp, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(PropModule) };
        }
        protected override string GetQuery()
        {
            return "SELECT * FROM `outfits_props`;";
        }

    }
}
