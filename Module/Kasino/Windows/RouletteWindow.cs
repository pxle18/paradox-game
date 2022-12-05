using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Kasino.Windows
{
    public class RouletteWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            public ShowEvent(DbPlayer dbPlayer) : base(dbPlayer) { }
        }

        public RouletteWindow() : base("RouletteTable") { }

        public override Func<DbPlayer, bool> Show()
        {
            return player => OnShow(
                new ShowEvent(player)
            );
        }

        public void ResponseResultNextRound(DbPlayer player, int deltaFullMinute)
        {
            if (player == null || !player.IsValid()) return;

            TriggerNewClient(player.Player, "resultNextRound", (int)deltaFullMinute);
        }

        public void ResponseResultEndGame(DbPlayer player, int resultFieldNumber)
        {
            if (player == null || !player.IsValid()) return;

            TriggerNewClient(player.Player, "resultEndGame", (int)resultFieldNumber);
        }
    }
}