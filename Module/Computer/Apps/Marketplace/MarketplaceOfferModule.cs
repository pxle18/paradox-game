using System.Collections.Generic;
using static VMP_CNR.Module.Computer.Apps.MarketplaceApp.MarketplaceCategoryApp;

namespace VMP_CNR.Module.Computer.Apps.Marketplace
{
    public class MarketplaceOfferModule : SqlModule<MarketplaceOfferModule, Marketplace, uint>
    {
        public Dictionary<int, OfferObject> OfferObjects = new Dictionary<int, OfferObject>();


        protected override string GetQuery()
        {
            return "SELECT * FROM `marketplace_offers`;";
        }

        protected override void OnLoaded()
        {

            foreach (var item in this.GetAll().Values)
            {
                var OfferObject = new OfferObject();
                OfferObject.id = (int)item.Id;
                OfferObject.name = item.Name;
                OfferObject.phone = item.Phone;
                OfferObject.price = item.Price;
                OfferObject.description = item.Description;
                OfferObject.CategoryId = (int)item.CategoryId;
                OfferObject.PlayerId = (uint)item.PlayerId;
                OfferObject.Search = item.Search;

                OfferObjects.Add(OfferObject.id, OfferObject);
            }

            base.OnLoaded();
        }
    }
}