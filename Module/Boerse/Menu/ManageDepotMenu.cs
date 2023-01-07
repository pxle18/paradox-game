using System;
using System.Collections.Generic;
using VMP_CNR.Module.Banks;
using VMP_CNR.Module.Banks.BankHistory;
using VMP_CNR.Module.Banks.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Boerse.Menu
{
    public class ManageDepotMenu : MenuBuilder
    {
        public ManageDepotMenu() : base(PlayerMenu.ManageDepotMenu)
        {
        }

        public override Module.Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            Module.Menu.NativeMenu menu = new Module.Menu.NativeMenu(PlayerMenu.ManageDepotMenu, "Depot-Management", "");
            menu.Add("Schließen", "");
            menu.Add("Depot erstellen", "");
            
            if (dbPlayer.HasDepot())
                menu.Add("Depot verwalten", "");

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
                switch (index)
                {
                    case 1: // Depot erstellen
                        if (dbPlayer.HasDepot())
                            dbPlayer.SendNewNotification("Du hast bereits ein Depot!", PlayerNotification.NotificationType.ADMIN, "Fehler!", 5000);
                        else
                            dbPlayer.CreateDepot();
                        break;
                    case 2: // Depot ein- und auszahlen
                        if (!dbPlayer.HasDepot())
                            break;
                        
                        ComponentManager.Get<BankWindow>().Show()(dbPlayer, "Aktien-Depot", dbPlayer.GetName(), dbPlayer.Money[0], (int)dbPlayer.Depot.Amount, 0, new List<BankHistory>());
                        break;
                    default: // Wird aufgerufen, wenn Schließen ausgewählt wurde
                        break;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}