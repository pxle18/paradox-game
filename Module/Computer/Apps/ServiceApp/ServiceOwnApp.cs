using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Service;
using static VMP_CNR.Module.Computer.Apps.ServiceApp.ServiceListApp;

namespace VMP_CNR.Module.Computer.Apps.ServiceApp
{
    public class ServiceOwnApp : SimpleApp
    {
        public ServiceOwnApp() : base("ServiceOwnApp") { }

        [RemoteEvent]
        public void requestOwnServices(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)teams.TEAM_MEDIC && dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)teams.TEAM_DPOS && dbPlayer.TeamId != (int)teams.TEAM_NEWS && dbPlayer.TeamId != (int)teams.TEAM_LSC && dbPlayer.TeamId != (int) teams.TEAM_GOV && dbPlayer.TeamId != (int)teams.TEAM_AUCTION) return;

            List<serviceObject> serviceList = new List<serviceObject>();
            var acceptedServices = ServiceModule.Instance.GetAcceptedServices(dbPlayer);

            foreach (var service in acceptedServices)
            {
                string accepted = string.Join(',', service.Accepted);

                string varname = service.Player.GetName();

                if (dbPlayer.TeamId == (int)teams.TEAM_MEDIC)
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

                    if (LeitstellenPhone.LeitstellenPhoneModule.Instance.IsLeiststelle(dbPlayer))
                    {
                        varname = varname + " (" + service.Player.GetName() + ")";
                    }
                }

                serviceList.Add(new serviceObject() { id = (int)service.Player.Id, name = varname, message = ServiceModule.Instance.GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
            }

            var serviceJson = NAPI.Util.ToJson(serviceList);
            TriggerNewClient(client, "responseOwnServiceList", serviceJson);
        }

        [RemoteEvent]
        public void finishOwnAcceptedService(Player client, int creatorId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)teams.TEAM_MEDIC && dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)teams.TEAM_DPOS && dbPlayer.TeamId != (int)teams.TEAM_NEWS && dbPlayer.TeamId != (int)teams.TEAM_LSC && dbPlayer.TeamId != (int) teams.TEAM_GOV && dbPlayer.TeamId != (int)teams.TEAM_AUCTION) return;

            var findplayer = Players.Players.Instance.FindPlayerById(creatorId);
            if (findplayer == null || !findplayer.IsValid()) return;

            bool response = ServiceModule.Instance.Cancel(dbPlayer, findplayer, dbPlayer.TeamId);

            if (response)
            {
                findplayer.ResetData("service");

                findplayer.SendNewNotification("Ihr Service wurde bearbeitet und somit geschlossen!");
                dbPlayer.SendNewNotification($"Sie haben den Service von beendet!");
            }
            else
            {
                dbPlayer.SendNewNotification("Der Service konnte nicht beendet werden!");
            }
        }
    }
}
