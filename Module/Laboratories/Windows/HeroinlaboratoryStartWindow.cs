using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Laboratories.Windows
{
    public class HeroinlaboratoryStartWindow : Window<Func<DbPlayer, Heroinlaboratory, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "status")]
            private bool Status { get; }

            public ShowEvent(DbPlayer dbPlayer, Heroinlaboratory cannabislaboratory) : base(dbPlayer)
            {
                Status = cannabislaboratory.ActingPlayers.Contains(dbPlayer);
            }
        }

        public HeroinlaboratoryStartWindow() : base("HeroinLabor")
        {

        }

        public override Func<DbPlayer, Heroinlaboratory, bool> Show()
        {
            return (player, methlaboratory) => OnShow(new ShowEvent(player, methlaboratory));
        }

        [RemoteEvent]
        public void toggleHerionLabor(Player client, bool result, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            Heroinlaboratory heroinlaboratory = HeroinlaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (heroinlaboratory == null)
            {
                return;
            }

            if (result)
            {
                heroinlaboratory.StartProcess(dbPlayer);
            }
            else
            {
                heroinlaboratory.StopProcess(dbPlayer);
            }
        }
    }
}