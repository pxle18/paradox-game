using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Service;
using static VMP_CNR.Module.Computer.Apps.ServiceApp.ServiceListApp;

namespace VMP_CNR.Module.Computer.Apps.ServiceApp
{
    public class ServiceAcceptedApp : SimpleApp
    {
        public ServiceAcceptedApp() : base("ServiceAcceptedApp") { }

        [RemoteEvent]
        public void RequestTeamServiceList(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)TeamTypes.TEAM_MEDIC && dbPlayer.TeamId != (int)TeamTypes.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)TeamTypes.TEAM_DPOS && dbPlayer.TeamId != (int)TeamTypes.TEAM_NEWS && dbPlayer.TeamId != (int)TeamTypes.TEAM_LSC && dbPlayer.TeamId != (int) TeamTypes.TEAM_GOV && dbPlayer.TeamId != (int)TeamTypes.TEAM_AUCTION) return;

            List<serviceObject> serviceList = new List<serviceObject>();
            var teamServices = ServiceModule.Instance.GetAcceptedTeamServices(dbPlayer);

            foreach (var service in teamServices)
            {
                string accepted = string.Join(',', service.Accepted);

                string varname = service.Player.GetName();

                if (dbPlayer.TeamId == (int)TeamTypes.TEAM_MEDIC)
                {
                    if (service.Player.GovLevel.ToLower() == "a" || service.Player.GovLevel.ToLower() == "b" || service.Player.GovLevel.ToLower() == "c")
                    {
                        varname = "[PRIORISIERT]";
                    }
                    else if (service.Player.TeamId == (int)TeamTypes.TEAM_MEDIC)
                    {
                        varname = "[LSMC]";
                    }
                    else varname = "Verletzte Person";

                    if (LeitstellenPhone.LeitstellenPhoneModule.Instance.IsLeiststelle(dbPlayer))
                    {
                        varname = varname + " (" + service.Player.GetName() + ")";
                    }
                }

                serviceList.Add(new serviceObject() { id = (int)service.Player.Id, name = varname, message = ServiceModule.Instance.GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
            }

            var serviceJson = NAPI.Util.ToJson(serviceList);
            TriggerNewClient(client, "responseTeamServiceList", serviceJson);
        }
    }
}
