using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.FahrzeugUebersichtApp;
using VMP_CNR.Module.Email;
using VMP_CNR.Module.Export;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.VehicleRent;

namespace VMP_CNR.Module.Computer.Apps.ExportApp.Apps
{
    public class Export
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "desc")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "items")]
        public List<ExportItem> exportItems { get; set; }

        public Export(uint id, string name, string desc)
        {
            Id = id;
            Name = name;
            Description = desc;
            exportItems = new List<ExportItem>();
        }
    }

    public class ExportItem
    {
        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "png")]
        public string Image { get; set; }
    }

    public class ExportApp : SimpleApp
    {
        public ExportApp() : base("ExportApp") { }

        [RemoteEvent]
        public void requestExports(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer p_DbPlayer = client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            List<Export> exports = new List<Export>();

            foreach(ItemExportNpc npc in ItemExportNpcModule.Instance.GetAll().Values.Where(npc => !npc.Illegal && npc.Teams.Count == 0).ToList())
            {
                Export expItem = new Export(npc.Id, npc.Name, "");

                List<ExportItem> exportItems = new List<ExportItem>();

                foreach(ItemExport itemExport in ItemExportModule.Instance.GetAll().Values.Where(ie => ie.NpcId == npc.Id).ToList())
                {
                    if (itemExport != null && itemExport.Item != null)
                    {
                        exportItems.Add(new ExportItem() { Name = itemExport.Item.Name, Price = itemExport.Price, Image = itemExport.Item.ImagePath });
                    }
                }

                expItem.Description = npc.LocationDescription;
                expItem.exportItems = exportItems;
                exports.Add(expItem);
            }


            Logging.Logger.Debug(NAPI.Util.ToJson(exports));
            TriggerNewClient(client, "responseExports", NAPI.Util.ToJson(exports));
        }

        [RemoteEvent]
        public void findExport(Player client, uint exportId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer p_DbPlayer = client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            ItemExportNpc itemExportNpc = ItemExportNpcModule.Instance.GetAll().Values.Where(npc => npc.Id == exportId).ToList().FirstOrDefault();

            if (itemExportNpc == null) return;

            client.TriggerNewClient("setPlayerGpsMarker", itemExportNpc.Position.X, itemExportNpc.Position.Y);

            p_DbPlayer.SendNewNotification($"GPS zu {itemExportNpc.Name} wurde gesetzt!");
        }
    }
}
