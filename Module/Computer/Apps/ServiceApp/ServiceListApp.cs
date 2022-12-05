using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Service;

namespace VMP_CNR.Module.Computer.Apps.ServiceApp
{
    public class ServiceListApp : SimpleApp
    {
        public ServiceListApp() : base("ServiceListApp") { }

        [RemoteEvent]
        public async void requestOpenServices(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)teams.TEAM_MEDIC && dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)teams.TEAM_DPOS && dbPlayer.TeamId != (int)teams.TEAM_NEWS && dbPlayer.TeamId != (int)teams.TEAM_LSC && dbPlayer.TeamId != (int) teams.TEAM_GOV && dbPlayer.TeamId != (int) teams.TEAM_AUCTION) return;

            await NAPI.Task.WaitForMainThread(0);

            List<serviceObject> serviceList = new List<serviceObject>();
            var teamServices = ServiceModule.Instance.GetAvailableServices(dbPlayer);

            foreach (var service in teamServices)
            {
                string accepted = string.Join(',', service.Accepted);

                string varname = service.Player.GetName();

                if(dbPlayer.TeamId == (int)teams.TEAM_MEDIC)
                {
                    if (service.Player.GovLevel.ToLower() == "a" || service.Player.GovLevel.ToLower() == "b" || service.Player.GovLevel.ToLower() == "c")
                    {
                        varname = "[PRIORISIERT]";
                    }
                    else if (service.Player.TeamId == (int)teams.TEAM_MEDIC)
                    {
                        varname = "[LSMC]";
                    }
                    else varname = "Verletzte Person";

                    if(LeitstellenPhone.LeitstellenPhoneModule.Instance.IsLeiststelle(dbPlayer))
                    {
                        varname = varname + " (" +service.Player.GetName() + ")";
                    }
                }

                serviceList.Add(new serviceObject() { id = (int)service.Player.Id, name = varname, message = ServiceModule.Instance.GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
            }

            var serviceJson = NAPI.Util.ToJson(serviceList);
            TriggerNewClient(client, "responseOpenServiceList", serviceJson);
        }

        [RemoteEvent]
        public async void acceptOpenService(Player client, int playerId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)teams.TEAM_MEDIC && dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)teams.TEAM_DPOS && dbPlayer.TeamId != (int)teams.TEAM_NEWS && dbPlayer.TeamId != (int)teams.TEAM_LSC && dbPlayer.TeamId != (int)teams.TEAM_GOV && dbPlayer.TeamId != (int)teams.TEAM_AUCTION) return;

            var findplayer = Players.Players.Instance.FindPlayerById(playerId);
            if (findplayer == null || !findplayer.IsValid()) return;

            await NAPI.Task.WaitForMainThread(0);

            bool response = ServiceModule.Instance.Accept(dbPlayer, findplayer);

            dbPlayer.SendNewNotification(response ? "Sie haben einen Service entgegengenommen!" : "Der Service konnte nicht entgegengenommen werden!");
            findplayer.SendNewNotification("Ihr Service wurde entgegen genommen!");

            if(dbPlayer.TeamId == (int)teams.TEAM_MEDIC)
            {
                string optional = "";

                if (findplayer.GovLevel.ToLower() == "a" || findplayer.GovLevel.ToLower() == "b" || findplayer.GovLevel.ToLower() == "c")
                {
                    optional = "priorisierten";
                }


                dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat einen {optional} Notruf angenommen");
            }
            else dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat den Notruf von {findplayer.GetName()} angenommen");
        }

        public class serviceObject
        {
            [JsonProperty(PropertyName = "id")]
            public int id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string message { get; set; }
            
            [JsonProperty(PropertyName = "posX")]
            public float posX { get; set; }

            [JsonProperty(PropertyName = "posY")]
            public float posY { get; set; }

            [JsonProperty(PropertyName = "posZ")]
            public float posZ { get; set; }

            [JsonProperty(PropertyName = "accepted")]
            public string accepted { get; set; }
            [JsonProperty(PropertyName ="telnr")]
            public string telnr { get; set; }
        }
    }
}
