namespace VMP_CNR.Module.PointsOfInterest
{
    public class PointOfInterestCategoryModule : SqlModule<PointOfInterestCategoryModule, PointOfInterest.Category, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `point_of_interest_category`;";
        }
    }
}