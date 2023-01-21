using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Laboratories.Windows
{
    public class TeamSubgroupStartWindow : Window<Func<DbPlayer, TeamSubgroupLaboratory, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "status")]
            private bool Status { get; }

            public ShowEvent(DbPlayer dbPlayer, TeamSubgroupLaboratory teamSubgroupLaboratory) : base(dbPlayer)
            {
                Status = teamSubgroupLaboratory.ActingPlayers.Contains(dbPlayer);
            }
        }

        public TeamSubgroupStartWindow() : base("UgLabor")
        {

        }

        public override Func<DbPlayer, TeamSubgroupLaboratory, bool> Show()
        {
            return (player, teamSubgroupLaboratory) => OnShow(new ShowEvent(player, teamSubgroupLaboratory));
        }

        [RemoteEvent]
        public void toggleTeamSubgroupLabor(Player client, bool result, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            TeamSubgroupLaboratory teamSubgroupLaboratory = TeamSubgroupLaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (teamSubgroupLaboratory == null)
            {
                return;
            }

            if (result)
            {
                teamSubgroupLaboratory.StartProcess(dbPlayer);
            }
            else
            {
                teamSubgroupLaboratory.StopProcess(dbPlayer);
            }
        }
    }
}