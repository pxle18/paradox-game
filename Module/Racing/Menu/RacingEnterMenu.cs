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

namespace VMP_CNR.Module.Racing.Menu
{
    public class RacingEnterMenuBuilder : MenuBuilder
    {
        public RacingEnterMenuBuilder() : base(PlayerMenu.RacingEnterMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.NativeMenu(Menu, "Racing Arena");

            l_Menu.Add($"Schließen");
            foreach (RacingLobby racingLobby in RacingModule.Lobbies)
            {
                l_Menu.Add($"Lobby {racingLobby.LobbyId} {racingLobby.Desc} | {racingLobby.RacingPlayers.Count}/{RacingModule.MaxLobbyPlayers}");
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
                if(index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else
                {
                    int idx = 1;
                    
                    foreach (RacingLobby racingLobby in RacingModule.Lobbies)
                    {
                        if(idx == index)
                        {
                            if(racingLobby.RacingPlayers.Count >= RacingModule.MaxLobbyPlayers)
                            {
                                dbPlayer.SendNewNotification("Diese Lobby ist bereits voll!");
                                return false;
                            }
                            
                            if(!dbPlayer.TakeMoney(RacingModule.LobbyEnterPrice))
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(RacingModule.LobbyEnterPrice));
                                return false;
                            }

                            dbPlayer.SetPlayerIntoRacing(racingLobby);
                            dbPlayer.SendNewNotification("Sie sind dem Rennen beigetreten. Fahren Sie ihre Bestzeit!");
                            return true;
                        }
                        idx++;
                    }
                }

                return true;
            }
        }
    }
}
