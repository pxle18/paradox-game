using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Telefon.App.Settings.Wallpaper;

namespace VMP_CNR.Module.Telefon.App.Settings
{
    public class SettingsEditWallpaper : SimpleApp
    {
        public SettingsEditWallpaper() : base("SettingsEditWallpaperApp") { }

        [RemoteEvent]
        public void requestWallpaperList(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            TriggerNewClient(player, "responseWallpaperList", WallpaperModule.Instance.getJsonWallpapersForPlayer(dbPlayer));

        }

        [RemoteEvent]
        public void saveWallpaper(Player player, int wallpaperId, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            dbPlayer.wallpaper = WallpaperModule.Instance.Get((uint)wallpaperId);
            dbPlayer.SaveWallpaper();
        }
        
    }

}
