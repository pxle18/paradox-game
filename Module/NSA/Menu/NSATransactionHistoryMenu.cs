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
    public class NSATransactionHistoryMenuBuilder : MenuBuilder
    {
        public NSATransactionHistoryMenuBuilder() : base(PlayerMenu.NSATransactionHistory)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "IAA Transfer History");
            l_Menu.Add($"Schließen");

            foreach (TransactionHistoryObject transactionHistoryObject in NSAModule.TransactionHistory.ToList().Where(t => t.TransactionType == TransactionType.MONEY))
            {
                l_Menu.Add($"{transactionHistoryObject.Description} - {transactionHistoryObject.Added.ToShortTimeString()}");
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
                    foreach (TransactionHistoryObject transactionHistoryObject in NSAModule.TransactionHistory.ToList().Where(t => t.TransactionType == TransactionType.MONEY))
                    {
                        if (idx == index)
                        {
                            dbPlayer.Player.TriggerNewClient("setPlayerGpsMarker", transactionHistoryObject.Position.X, transactionHistoryObject.Position.Y);
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
