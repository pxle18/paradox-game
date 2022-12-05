using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using Newtonsoft.Json;

namespace VMP_CNR.Module.News.App
{
    public class NewsListApp : SimpleApp
    {
        public NewsListApp() : base("NewsListApp")
        {
        }

        public class NewsFound
        {
            [JsonProperty(PropertyName = "id")] public uint Id { get; }
            [JsonProperty(PropertyName = "title")] public string Title { get; }
            [JsonProperty(PropertyName = "content")] public string Content { get; }
            [JsonProperty(PropertyName = "typeId")] public int TypeId { get; }

            public NewsFound(uint id, string title, string content, int typeId)
            {
                Id = id;
                Title = title;
                Content = content;
                TypeId = typeId;
            }
        }

        [RemoteEvent]
        public void requestNews(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            SendNewsList(player);
        }

        [RemoteEvent]
        public void addNews(Player player, int newsType, string title, string content, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.TeamId != 4) return;

            // Add News
            title = title.Replace("\"", "");
            content = content.Replace("\"", "");

            title = title.Replace("\"\"", "");
            content = content.Replace("\"\"", "");

            var l_NewsID = (uint)Main.newsList.Count + 1;
            Main.newsList.Add(new NewsFound(l_NewsID, title + " (" + (l_NewsID.ToString()) + ")", content, newsType));

            Main.newsList.Sort(delegate (NewsFound x, NewsFound y)
            {
                return y.Id.CompareTo(x.Id);
            });

            // Update NewsList
            this.SendNewsList(player);

            if (newsType == 0)
                Main.sendNotificationToPlayersWhoCanReceive("[NEWS] Es wurde ein Wetterbericht veroeffentlicht. Check die News App!", "Weazel News");
            else
                Main.sendNotificationToPlayersWhoCanReceive("[NEWS] Es wurde eine News veroeffentlicht. Check die News App!", "Weazel News");
        }

        private void SendNewsList(Player player)
        {
            TriggerNewClient(player, "updateNews", NAPI.Util.ToJson(Main.newsList));
        }

        public void deleteNews(uint p_NewsID)
        {
            if (Main.newsList.Exists(n => n.Id == p_NewsID))
            {
                NewsFound l_News = Main.newsList.Find(n => n.Id == p_NewsID);
                Main.newsList.Remove(l_News);
            }
        }


        [RemoteEvent]
        public void removeNews(Player player, uint p_NewsID, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            deleteNews(p_NewsID);
        }
    }
}