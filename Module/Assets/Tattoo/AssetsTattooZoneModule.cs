
namespace VMP_CNR.Module.Assets.Tattoo
{
    public class AssetsTattooZoneModule : SqlModule<AssetsTattooZoneModule, AssetsTattooZone, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `assets_tattoo_zone`;";
        }
    }
}
