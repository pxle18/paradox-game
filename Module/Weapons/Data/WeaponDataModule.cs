namespace VMP_CNR.Module.Weapons.Data
{
    public class WeaponDataModule : SqlModule<WeaponDataModule, WeaponData, int>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `weapon_data`;";
        }
    }
}