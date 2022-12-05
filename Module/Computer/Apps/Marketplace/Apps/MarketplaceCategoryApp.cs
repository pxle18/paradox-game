using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.Marketplace;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.MarketplaceApp
{
    public class MarketplaceCategoryApp : SimpleApp
    {
        public MarketplaceCategoryApp() : base("MarketplaceCategory") { }

        public class OfferObject
        {
            [JsonProperty(PropertyName = "id")]
            public int id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string description { get; set; }

            [JsonProperty(PropertyName = "price")]
            public int price { get; set; }

            [JsonProperty(PropertyName = "phone")]
            public int phone { get; set; }

            [JsonIgnore]
            public int CategoryId { get; set; }

            [JsonIgnore]
            public uint PlayerId { get; set; }

            [JsonProperty(PropertyName = "search")]
            public bool Search { get; set; }

        }
    }
}
