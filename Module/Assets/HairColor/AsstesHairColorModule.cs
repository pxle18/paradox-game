using System;
using VMP_CNR.Module.Barber;

namespace VMP_CNR.Module.Assets.HairColor
{
    public class AssetsHairColorModule : SqlModule<AssetsHairColorModule, AssetsHairColor, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `assets_hair_color`;";
        }
    }
}
