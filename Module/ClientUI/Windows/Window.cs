using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.ClientUI.Windows
{
    public abstract class Window<T> : Component
    {
        public class Event
        {
            [JsonIgnore] public DbPlayer DbPlayer { get; }

            public Event(DbPlayer dbPlayer)
            {
                DbPlayer = dbPlayer;
            }
        }

        public Window(string name) : base(name)
        {
        }

        public bool OnShow(Event @event)
        {
            string json;
            try
            {
                json = NAPI.Util.ToJson(@event);
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                json = null;
            }

            if (string.IsNullOrEmpty(json)) return false;

            Open(@event.DbPlayer.Player, json);
            return true;
        }

        public virtual void Open(Player player, string json)
        {
            player.TriggerNewClient("openWindow", Name, json);
        }

        public virtual void Close(Player player)
        {
            player.TriggerNewClient("closeWindow", Name);
        }

        public abstract T Show();
    }
}