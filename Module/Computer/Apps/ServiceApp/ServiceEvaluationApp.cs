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
    public class ServiceEvaluationApp : SimpleApp
    {
        public ServiceEvaluationApp() : base("ServiceEvaluationApp") { }

        [RemoteEvent]
        public void requestEvalutionServices(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)TeamTypes.TEAM_MEDIC && dbPlayer.TeamId != (int)TeamTypes.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)TeamTypes.TEAM_DPOS && dbPlayer.TeamId != (int)TeamTypes.TEAM_NEWS && dbPlayer.TeamId != (int)TeamTypes.TEAM_LSC && dbPlayer.TeamId != (int) TeamTypes.TEAM_GOV && dbPlayer.TeamId != (int)TeamTypes.TEAM_AUCTION) return;

            var teamRankPermission = dbPlayer.TeamRankPermission;
            if (teamRankPermission.Manage < 1) return;


            if (!ServiceModule.Instance.evaluations.ContainsKey(dbPlayer.TeamId)) return;

            List<ServiceEvaluation> evaluations = ServiceModule.Instance.evaluations[dbPlayer.TeamId].ToList();

            List<ServiceEvaluationJson> jsonData = new List<ServiceEvaluationJson>();

            foreach(ServiceEvaluation eval in evaluations)
            {
                jsonData.Add(new ServiceEvaluationJson()
                {
                    id = eval.id,
                    amount = eval.amount,
                    name = eval.name,
                    timestr = eval.timestr.ToString("yyyy-MM-dd H:mm:ss")
                });
            }

            var serviceJson = NAPI.Util.ToJson(jsonData);
            TriggerNewClient(client, "responseEvaluationService", serviceJson);
        }

    }
}
