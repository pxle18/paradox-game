using System;

namespace VMP_CNR.Module.Weapons.Tint
{
    public class WeaponTintModule : SqlModule<WeaponTintModule, WeaponTint, Tuple<uint, uint>>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `weapon_tint`;";
        }
    }
}