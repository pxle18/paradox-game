using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Extensions;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Kasino.Windows;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.RemoteEvents;
using VMP_CNR.Module.Sync;

namespace VMP_CNR.Module.Christmas
{
    /**
     * This is part of the PARADOX Game-Rework.
     * Made by module@jabber.ru
     */

    public sealed class ChristmasPresentModule : Module<ChristmasPresentModule>
    {
        private readonly Vector3 _presentLocation;

        public ChristmasPresentModule()
        {
            _presentLocation = new Vector3(-416.655, 1160.048, 325.858);
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemModelModule) };
        }

        protected override bool OnLoad()
        {
            PlayerNotifications.Instance.Add(
                _presentLocation,
                "PARADOX Roleplay", "Nehmt eure gesammelten Geschenke mit. I think it’s time, isn’t it?"
            );

            return true;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;
            if (dbPlayer.Player.Position.DistanceTo(_presentLocation) > 10) return false;

            /*
             * TODO: 
             */

            return true;
        }
    }
}
