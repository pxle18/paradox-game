using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Tattoo;

namespace VMP_CNR.Module.Crime
{
    public class CrimeJailMenuBuilder : MenuBuilder
    {
        public CrimeJailMenuBuilder() : base(PlayerMenu.CrimeJailMenu)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsACop() || !dbPlayer.Duty) return null;
            
            var menu = new Menu.NativeMenu(Menu, "Gefaengnisuebersicht");

            menu.Add($"Schließen");

            foreach (DbPlayer jailPlayer in Players.Players.Instance.GetValidPlayers().Where(x => x.JailTime[0] > 0).ToList())
            {
                menu.Add($"{jailPlayer.GetName()} | {jailPlayer.JailTime[0]} Hafteinheiten");
            }
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                MenuManager.DismissCurrent(dbPlayer);
                return false;
            }
        }
    }
}