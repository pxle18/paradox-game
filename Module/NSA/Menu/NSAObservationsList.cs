using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;

namespace VMP_CNR.Module.NSA.Menu
{
    public class NSAObservationsListMenuBuilder : MenuBuilder
    {
        public NSAObservationsListMenuBuilder() : base(PlayerMenu.NSAObservationsList)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "NSA Observationen");
            l_Menu.Add($"Schließen");
            l_Menu.Add($"Observation hinzufügen");

            foreach (NSAObservation nSAObservation in NSAObservationModule.ObservationList.Values.ToList())
            {
                DbPlayer targetOne = Players.Players.Instance.FindPlayerById(nSAObservation.PlayerId);
                if (targetOne == null || !targetOne.IsValid()) continue;

                l_Menu.Add($"{targetOne.Id} {targetOne.GetName()}");
            }
            
            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else if(index == 1)
                {
                    if(!dbPlayer.IsNSADuty)
                    {
                        return false;
                    }
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Obersvation hinzufügen", Callback = "AddObservationPlayer", Message = "Geben Sie einen Namen ein:" });
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else
                {
                    int idx = 2;
                    foreach (NSAObservation nSAObservation in NSAObservationModule.ObservationList.Values.ToList())
                    {
                        DbPlayer targetOne = Players.Players.Instance.FindPlayerById(nSAObservation.PlayerId);
                        if (targetOne == null || !targetOne.IsValid()) continue;

                        if (idx == index)
                        {
                            // Targetplayer Submenu...
                            dbPlayer.SetData("nsa_target_player_id", targetOne.Id);
                            Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.NSAObservationsSubMenu, dbPlayer).Show(dbPlayer);
                            return false;
                        }
                        idx++;
                    }
                }
                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
