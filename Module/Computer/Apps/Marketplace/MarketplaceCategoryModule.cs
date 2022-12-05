namespace VMP_CNR.Module.Computer.Apps.Marketplace
{
    public class MarketplaceCategoryModule : SqlModule<MarketplaceCategoryModule, Category, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `marketplace_categorys`;";
        }
    }
}
