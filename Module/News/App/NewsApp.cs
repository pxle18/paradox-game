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
            if (!player.CheckRemoteEventKey(key))
            {
                return;
            }
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.TeamId != 4) return;

            // Add News
            title = title.Replace("\"", "");
            content = content.Replace("\"", "");

            title = title.Replace("\"\"", "");
            content = content.Replace("\"\"", "");

            uint newsId = (uint)Main.newsList.Count + 1;
            Main.newsList.Add(new NewsFound(newsId, $"{title} ({newsId})", content, newsType));

            Main.newsList.Sort((x, y) => y.Id.CompareTo(x.Id));

            // Update NewsList
            SendNewsList(player);

            string notificationText = newsType == 0 ?
                "[NEWS] Es wurde ein Wetterbericht veröffentlicht. Check die News App!" :
                "[NEWS] Es wurde eine News veröffentlicht. Check die News App!";
            Main.sendNotificationToPlayersWhoCanReceive(notificationText, "Weazel News");
           
        }

        private void SendNewsList(Player player)
        {
            TriggerNewClient(player, "updateNews", NAPI.Util.ToJson(Main.newsList));
        }

        public void deleteNews(uint newsId)
        {
            if (Main.newsList.Exists(n => n.Id == newsId))
            {
                NewsFound news = Main.newsList.Find(n => n.Id == newsId);
                Main.newsList.Remove(news);
            }
        }

        [RemoteEvent]
        public void RemoveNews(Player player, uint newsId, string key)
        {
            if (!player.CheckRemoteEventKey(key))
            {
                return;
            }

            deleteNews(newsId);
        }
    }
}