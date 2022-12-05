using System;
using System.Linq;
using System.Reflection;
using VMP_CNR.Module.Players.Db;
using GTANetworkAPI;
using VMP_CNR.Module.Admin;
using VMP_CNR.Module.ClientUI.Windows;

namespace VMP_CNR.Module.Players.Windows
{
    public class DeathWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            public ShowEvent(DbPlayer dbPlayer) : base(dbPlayer)
            {
            }
        }

        public DeathWindow() : base("Death")
        {
        }

        public override Func<DbPlayer, bool> Show()
        {
            return player => OnShow(new ShowEvent(player));
        }

        public void closeDeathWindowS(Player client)
        {
            TriggerNewClient(client, "closeDeathScreen");
        }
    }
}