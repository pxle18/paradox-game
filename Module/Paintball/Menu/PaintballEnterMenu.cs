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

namespace VMP_CNR.Module.Paintball.Menu
{
    public class PaintballEnterMenuBuilder : MenuBuilder
    {
        public PaintballEnterMenuBuilder() : base(PlayerMenu.PaintballEnterMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "Paintball Arenen");
            l_Menu.Add($"Schließen");
            foreach (KeyValuePair<uint, PaintballArea> pba in PaintballAreaModule.Instance.GetAll())
            {
                l_Menu.Add($"{pba.Value.Name} ${pba.Value.LobbyEnterPrice} {pba.Value.pbPlayers.Count}/{pba.Value.MaxLobbyPlayers}");
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
                else
                {
                    int idx = 1;

                    foreach (KeyValuePair<uint, PaintballArea> pba in PaintballAreaModule.Instance.GetAll())
                    {

                        if (idx == index)
                        {
                            if (pba.Value.pbPlayers.Count >= pba.Value.MaxLobbyPlayers)
                            {
                                dbPlayer.SendNewNotification("Diese Lobby ist bereits voll!");
                                return false;
                            }
                            if (pba.Value.Password.Length >= 1)
                            {
                                dbPlayer.SetData("pba_choose", pba.Value.Id);
                                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = $"Beitritt Event Paintball", Callback = "PbaConfirmPassword", CustomData = new { id = pba.Value.Id.ToString() } });
                            }
                            else
                            {
                                ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Beitritt Paintball ${pba.Value.LobbyEnterPrice}", $"ACHTUNG: Sämtliche Waffen werden Dir vom Sicherheitspersonal abgenommen, es gibt keine Garantie, dass du deine Waffen wiederbekommst. Hinweis: Das Paintball-Team entlässt Dich sofort mit einem /quit.", "PbaConfirm", pba.Value.Id.ToString(), ""));
                            }
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
