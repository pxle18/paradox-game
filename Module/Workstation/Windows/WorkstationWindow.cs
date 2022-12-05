using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Business.NightClubs;
using System.Threading.Tasks;
using VMP_CNR.Module.Schwarzgeld;
using VMP_CNR.Module.NSA;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Workstation.Windows
{
    
    public class WorkStationSendClientItem
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
    }

    public class WorkstationWindow : Window<Func<DbPlayer, Workstation, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "workstation")] private Workstation workstationData { get; }
            [JsonProperty(PropertyName = "sourceItems")] private List<WorkStationSendClientItem> sourceItemsData { get; }
            [JsonProperty(PropertyName = "endItems")] private List<WorkStationSendClientItem> endItemsData { get; }
            [JsonProperty(PropertyName = "price")] private int price { get; }

            public ShowEvent(DbPlayer dbPlayer, Workstation Workstation) : base(dbPlayer)
            {
                workstationData = Workstation;

                List<WorkStationSendClientItem> sourceItems = new List<WorkStationSendClientItem>();
                List<WorkStationSendClientItem> endItems = new List<WorkStationSendClientItem>();

                foreach (KeyValuePair<uint, int> kvp in Workstation.SourceConvertItems)
                {
                    ItemModel xItem = ItemModelModule.Instance.Get(kvp.Key);
                    if(xItem != null)
                    {
                        sourceItems.Add(new WorkStationSendClientItem() { Id = kvp.Key, Name = xItem.Name, Amount = kvp.Value });
                    }
                }

                ItemModel xEndItem = ItemModelModule.Instance.Get(Workstation.EndItemId);
                if (xEndItem != null)
                {
                    endItems.Add(new WorkStationSendClientItem() { Id = Workstation.EndItemId, Name = xEndItem.Name, Amount = Workstation.End5MinAmount });
                }

                endItemsData = endItems;
                sourceItemsData = sourceItems;
                price = 2500;
            }
        }

        public WorkstationWindow() : base("Workstation")
        {
        }

        public override Func<DbPlayer, Workstation, bool> Show()
        {
            return (player, workstation) => OnShow(new ShowEvent(player, workstation));
        }

        [RemoteEvent]
        public void RentWorkstationEvent(Player client, int workstationId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            Workstation workstation = WorkstationModule.Instance.GetAll().Values.ToList().Where(w => w.Id == workstationId && w.NpcPosition.DistanceTo(dbPlayer.Player.Position) < 2.0f).FirstOrDefault();

            if(workstation != null)
            {
                if (!workstation.LimitTeams.Contains(dbPlayer.TeamId))
                {
                    dbPlayer.SendNewNotification($"Du scheinst mir zu unseriös zu sein... Arbeitest du schon etwas anderes?");
                    return;
                }
                if (dbPlayer.WorkstationId == workstation.Id)
                {
                    dbPlayer.SendNewNotification($"Sie sind hier bereits eingemietet!");
                    return;
                }
                if (workstation.RequiredLevel > 0 && workstation.RequiredLevel > dbPlayer.Level)
                {
                    dbPlayer.SendNewNotification($"Für diese Workstation benötigen Sie mind Level {workstation.RequiredLevel}!");
                    return;
                }
                if (!dbPlayer.TakeMoney(2500))
                {
                    dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(2500));
                    return;
                }

                dbPlayer.WorkstationEndContainer.ClearInventory();
                dbPlayer.WorkstationFuelContainer.ClearInventory();
                dbPlayer.WorkstationSourceContainer.ClearInventory();

                dbPlayer.SendNewNotification($"Sie haben sich in {workstation.Name} eingemietet und können diese nun benutzen!");
                dbPlayer.WorkstationId = workstation.Id;
                dbPlayer.SaveWorkstation();
            }
            
        }
    }
}
