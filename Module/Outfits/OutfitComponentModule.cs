using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Clothes;

namespace VMP_CNR.Module.Outfits
{
    public class OutfitComponentModule : SqlModule<OutfitComponentModule, OutfitComponent, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ClothModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `outfits_components`;";
        }

    }
}
