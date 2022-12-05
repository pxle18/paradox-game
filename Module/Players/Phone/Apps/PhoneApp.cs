using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.Phone.Apps
{
    //TODO: rename to PlayerApp because it is used in computer as well
    public class PhoneApp : Loadable<string>
    {
        [JsonProperty(PropertyName = "id")] public string Id { get; }
        [JsonProperty(PropertyName = "name")] public string Name { get; }
        [JsonProperty(PropertyName = "icon")] public string Icon { get; }

        public PhoneApp(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetString("id");
            Name = reader.GetString("name");
            Icon = reader.GetString("icon");
        }

        public PhoneApp(string id, string name, string icon) : base(null)
        {
            Id = id;
            Name = name;
            Icon = icon;
        }

        public override string GetIdentifier()
        {
            return Id;
        }
    }
    
    public class HomeApp : SimpleApp
    {
        public HomeApp() : base("HomeApp")
        {
        }

        [RemoteEvent]
        public void requestApps(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            Logger.Print(dbPlayer.PhoneApps?.GetJson());
            TriggerNewClient(player, "responseApps", dbPlayer.PhoneApps?.GetJson());
        }
        [RemoteEvent]
        public void requestPhoneWallpaper(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            Logger.Print("Wallpaper: " + dbPlayer.wallpaper.File);
            TriggerNewClient(player, "responsePhoneWallpaper", dbPlayer.wallpaper.File);
        }

    }
}