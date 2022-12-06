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

namespace VMP_CNR.Module.Einreiseamt
{
    public class EinreiseAmtMenuBuilder : MenuBuilder
    {
        public EinreiseAmtMenuBuilder() : base(PlayerMenu.EinreiseAmtMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsEinreiseAmt() || !dbPlayer.HasData("einreiseamtp")) return null;

            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(dbPlayer.GetData("einreiseamtp"));
            if (foundPlayer == null || !foundPlayer.IsValid() || !foundPlayer.IsNewbie()) return null;


            var menu = new Menu.Menu(Menu, "Einreise " + foundPlayer.GetName());

            menu.Add($"Schließen");
            menu.Add($"Level " + foundPlayer.Level + " | GB: " + foundPlayer.birthday[0]);
            menu.Add($"Spieler ablehnen (Permbann)");
            menu.Add($"Spieler annehmen (Perso)");

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
                if (!dbPlayer.IsEinreiseAmt() || !dbPlayer.HasData("einreiseamtp")) return false;

                DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(dbPlayer.GetData("einreiseamtp"));
                if (foundPlayer == null || !foundPlayer.IsValid() || !foundPlayer.IsNewbie()) return false;

                switch(index)
                {
                    case 2: // Ablehnen

                        foundPlayer.SendNewNotification("Ihnen wurde die Einreise nicht gestattet!");
                        foundPlayer.SendNewNotification("Bitte melden Sie sich bei Fragen im Support!");
                        dbPlayer.SendNewNotification($"Sie haben {foundPlayer.GetName()} die Einreise verweigert!");

                        DatabaseLogging.Instance.LogAdminAction(dbPlayer.Player, foundPlayer.GetName(), adminLogTypes.perm, "Einreiseamt", 0, Configurations.Configuration.Instance.DevMode);
                        foundPlayer.warns[0] = 3;

                        Logging.Logger.AddToEinreiseLog(dbPlayer.Id, foundPlayer.Id, false);

                        foundPlayer.Player.Kick("Einreise abgelehnt!");
                        MenuManager.DismissCurrent(dbPlayer);

                        dbPlayer.ResetData("einreiseamtp");

                        if (dbPlayer.IsInDuty())
                        {
                            dbPlayer.GiveMoney(5000);
                        }
                        else dbPlayer.GiveMoney(8000);
                        return true;

                    case 3: // Annehmen
                        foundPlayer.HasPerso[0] = 1;
                        foundPlayer.Save();

                        foundPlayer.SendNewNotification("Ihnen wurde die Einreise gestattet! Viel Spaß auf PARADOX!");
                        dbPlayer.SendNewNotification($"Sie haben {foundPlayer.GetName()} die Einreise gestattet!");
                        MenuManager.DismissCurrent(dbPlayer);

                        Logging.Logger.AddToEinreiseLog(dbPlayer.Id, foundPlayer.Id, true);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Einreiseamt-Formular", Callback = "EinreiseAmtPlayerBirthday", Message = "Geben Sie das Geburtsdatum ein : XX.XX.XXXX Beispiel : 09.12.1997 " });

                        if (dbPlayer.IsInDuty())
                        {
                            dbPlayer.GiveMoney(5000);
                        }
                        else dbPlayer.GiveMoney(8000);
                        return true;

                    default:
                        break;
                }
                MenuManager.DismissCurrent(dbPlayer);
                return false;
            }
        }
    }
}