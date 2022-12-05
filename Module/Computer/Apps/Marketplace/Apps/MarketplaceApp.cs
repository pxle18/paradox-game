using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.Marketplace;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using static VMP_CNR.Module.Computer.Apps.MarketplaceApp.MarketplaceCategoryApp;

namespace VMP_CNR.Module.Computer.Apps.MarketplaceApp
{
    public class MarketplaceApp : SimpleApp
    {
        public MarketplaceApp() : base("MarketplaceApp") { }

        [RemoteEvent]
        public void requestMarketPlaceOffers(Player client, int category, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                var offerList = new List<OfferObject>();
                var offers = MarketplaceOfferModule.Instance.OfferObjects.Where(x => x.Value.CategoryId == (uint)category);

                // Only online players marketoffers...
                foreach (KeyValuePair<int, OfferObject> item in offers.ToList().Where(o => Players.Players.Instance.GetValidPlayers().Where(dp => dp.Id == o.Value.PlayerId).Count() > 0))
                {
                    offerList.Add(item.Value);
                }
                var offerJson = NAPI.Util.ToJson(offerList);
                offerJson = Regex.Escape(offerJson);
                TriggerNewClient(client, "responseMarketPlaceOffers", offerJson, dbPlayer.Rank.CanAccessFeature("admingebay"));
            }
            catch (System.Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void deleteOffer(Player client, int offerId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MarketplaceOfferModule.Instance.OfferObjects.ContainsKey(offerId)) return;

            OfferObject offerObject = MarketplaceOfferModule.Instance.OfferObjects[offerId];
            string pl_id;
            if (dbPlayer.Rank.CanAccessFeature("admingebay"))
            {
                pl_id = $"player_id = '{offerObject.PlayerId}'";
            }
            else
            {
                pl_id = $"player_id = '{dbPlayer.Id}'";
            }
            MySQLHandler.ExecuteAsync($"DELETE FROM `marketplace_offers` WHERE {pl_id} AND name = '{offerObject.name}' AND category_id = '{offerObject.CategoryId}'");
            MarketplaceOfferModule.Instance.OfferObjects.Remove(offerId);

            dbPlayer.SendNewNotification("Angebot gelöscht.");

        }

        [RemoteEvent]
        public void requestMyOffers(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var offerList = new List<OfferObject>();
            var offers = MarketplaceOfferModule.Instance.OfferObjects.Where(x => x.Value.phone == dbPlayer.handy[0]);

            foreach (KeyValuePair<int, OfferObject> item in offers)
            {
                offerList.Add(item.Value);
            }
            var offerJson = NAPI.Util.ToJson(offerList);
            offerJson = Regex.Escape(offerJson);
            TriggerNewClient(client, "responseMyOffers", offerJson);
        }

        [RemoteEvent]
        public void requestMarketplaceCategories(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var marketplaceCategorys = MarketplaceCategoryModule.Instance.GetAll();
            var categoryList = new List<CategoryObject>();

            foreach (var category in marketplaceCategorys.Values)
            {
                CategoryObject categoryObject = new CategoryObject()
                {
                    id = (int)category.Id,
                    name = category.Name,
                    icon_path = category.IconPath
                };

                categoryList.Add(categoryObject);
            }

            var categoryJson = NAPI.Util.ToJson(categoryList);
            TriggerNewClient(client, "responseMarketPlaceCategories", categoryJson);
        }

        [RemoteEvent]
        public void addOffer(Player client, int category, string name, int price, string description, bool search, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, name)) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, description)) return;

            if (MarketplaceOfferModule.Instance.OfferObjects.Where(x => x.Value.phone == (int)dbPlayer.handy[0]).Count() == 3)
            {
                dbPlayer.SendNewNotification("Du hast bereits 3 Anzeigen aufgegeben.", title: "Werbung", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (!dbPlayer.TakeBankMoney(1500))
            {
                dbPlayer.SendNewNotification("Du hast nicht genug Geld auf dem Konto ($ 1500", title: "Werbung", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }



            string offerName = replaceContent(name);
            string offerDescription = replaceContent(description);

            int newId = 0;
            if (MarketplaceOfferModule.Instance.OfferObjects.Count != 0)
            {
                while (MarketplaceOfferModule.Instance.OfferObjects.ContainsKey(newId))
                {
                    newId++;
                }
            }

            // Lol hier könnte man hart DB ficken
            name = MySqlHelper.EscapeString(name);
            description = MySqlHelper.EscapeString(description);

            // Add to current list
            OfferObject OfferObject = new OfferObject()
            {
                id = newId,
                name = offerName,
                description = offerDescription,
                price = price,
                phone = (int)dbPlayer.handy[0],
                CategoryId = category,
                PlayerId = dbPlayer.Id,
                Search = search,
            };

            MarketplaceOfferModule.Instance.OfferObjects.Add(newId, OfferObject);

            // Add to db
            MySQLHandler.ExecuteAsync($"INSERT INTO `marketplace_offers` (`name`, `player_id`, `description`, `category_id`, `price`, `phone`, `search`, `date`) VALUES ('{ offerName }', '{dbPlayer.Id}', '{ offerDescription }', '{ category }', '{ price }', '{ dbPlayer.handy[0] }', '{(search ? 1 : 0)}', '{ DateTime.Now:yyyy-MM-dd H:mm:ss}');");
            dbPlayer.SendNewNotification("Angebot erfolgreich erstellt.", title: "Werbung", notificationType: PlayerNotification.NotificationType.SUCCESS);
        }

        public string replaceContent(string input)
        {
            //return Regex.Replace(input, @"^[a-zA-Z0-9\s]+$", "");
            return Regex.Replace(input, @"[^a-zA-Z0-9äÄöÖüÜß\s]", "");

        }

        public class CategoryObject
        {
            [JsonProperty(PropertyName = "id")]
            public int id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "icon_path")]
            public string icon_path { get; set; }
        }
    }
}
