using System.Collections.Generic;
using Newtonsoft.Json;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.BigDataSender;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Menu
{
    public class NativeMenu
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public PlayerMenu MenuId { get; set; }

        public class Item
        {
            public string Label { get; set; }

            public string Description { get; set; }

            public Item(string label, string description)
            {
                Label = label;
                Description = description;
            }
        }

        public List<Item> Items = new List<Item>();

        public NativeMenu(PlayerMenu menuId, string title = "", string description = "")
        {
            MenuId = menuId;
            Title = title;
            Description = description;
        }

        public void Add(string label, string description = "")
        {
            Items.Add(new Item(label, description));
        }

        public void Show(DbPlayer dbPlayer, bool freeze = false)
        {
            if(dbPlayer == null || !dbPlayer.IsValid()) return;
            dbPlayer.WatchMenu = (uint) MenuId;
            dbPlayer.Player.TriggerNewClientBig("componentServerEvent", "NativeMenu", "showNativeMenu", JsonConvert.SerializeObject(this), (uint) MenuId);
        }
    }

    public interface IMenuEventHandler
    {
        bool OnSelect(int index, DbPlayer dbPlayer);
    }

    public abstract class MenuBuilder
    {
        public PlayerMenu Menu { get; set; }

        public MenuBuilder(PlayerMenu menu)
        {
            Menu = menu;
        }

        public abstract NativeMenu Build(DbPlayer dbPlayer);

        public abstract IMenuEventHandler GetEventHandler();
    }
}