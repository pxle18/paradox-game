using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps
{
    public class DesktopApp : App<Func<DbPlayer, List<ComputerAppClientObject>, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "apps")] private List<ComputerAppClientObject> Apps { get; }

            public ShowEvent(DbPlayer dbPlayer, List<ComputerAppClientObject> computer) : base(dbPlayer)
            {
                Apps = Apps;
            }
        }

        public DesktopApp() : base("DesktopApp", "DesktopApp")
        {
        }

        public override Func<DbPlayer, List<ComputerAppClientObject>, bool> Show()
        {
            return (dbPlayer, apps) => OnShow(new ShowEvent(dbPlayer, apps));
        }

        [RemoteEvent]
        public void requestComputerApps(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (player.IsReloading) return;
            if (!dbPlayer.CanInteract()) return;

            List<ComputerAppClientObject> computerAppClientObjects = new List<ComputerAppClientObject>();

            foreach (KeyValuePair<uint, ComputerApp> kvp in ComputerAppModule.Instance.GetAll())
            {
                if (dbPlayer.CanAccessComputerApp(kvp.Value) && kvp.Value.Type == ComputerTypes.Computer) computerAppClientObjects.Add(new ComputerAppClientObject(kvp.Value));
            }

            TriggerNewClient(player, "responseComputerApps", NAPI.Util.ToJson(computerAppClientObjects));
        }
    }
}
