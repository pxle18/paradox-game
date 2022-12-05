using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Staatsgefaengnis.Menu
{
    public class SGJobChooseMenu : MenuBuilder
    {
        public SGJobChooseMenu() : base(PlayerMenu.SGJobChooseMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
         
            var l_Menu = new Module.Menu.Menu(Menu, "Job Auswahl");

            l_Menu.Add($"Schließen");
            l_Menu.Add($"Aktuellen Job beenden");

            l_Menu.Add($"Wäscherei {StaatsgefaengnisModule.Instance.GetJobAmounts(SGJobs.WASHING)}/{StaatsgefaengnisModule.SGWashingJobMax}");
            //l_Menu.Add($"Handwerk {StaatsgefaengnisModule.Instance.GetJobAmounts(SGJobs.WORKBENCH)}/{StaatsgefaengnisModule.SGWorkbenchJobMax}");


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
                else if (index == 1)
                {
                    if (!dbPlayer.HasPlayerSGJob())
                    {
                        dbPlayer.SendNewNotification("Du hast keinen Job aktiv!");
                        MenuManager.DismissCurrent(dbPlayer);
                        return true;
                    }

                    StaatsgefaengnisModule.Instance.SGJobPlayers.Remove(dbPlayer);

                    // Remove Job Items
                    foreach (uint itemId in StaatsgefaengnisModule.Instance.removeSGItemsOnNormalUnjail)
                    {
                        if (dbPlayer.Container.GetItemAmount(itemId) > 0)
                        {
                            dbPlayer.Container.RemoveItemAll(itemId);
                        }
                    }

                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else if(index == 2)
                {
                    if(dbPlayer.HasPlayerSGJob())
                    {
                        dbPlayer.SendNewNotification("Du hast bereits einen Job aktiv!");
                        MenuManager.DismissCurrent(dbPlayer);
                        return true;
                    }

                    if(!dbPlayer.Container.CanInventoryItemAdded(LaundryModule.LaundryKeyItemId))
                    {
                        dbPlayer.SendNewNotification("Du hast nicht genug Platz im Inventar!");
                        return true;
                    }

                    if(StaatsgefaengnisModule.Instance.GetJobAmounts(SGJobs.WASHING) < StaatsgefaengnisModule.SGWashingJobMax)
                    {

                        StaatsgefaengnisModule.Instance.SGJobPlayers.Add(dbPlayer, SGJobs.WASHING);

                        dbPlayer.SendNewNotification("Du hast nun den Wäscherei Job angenommen, besorg dir einen Wäschekorb und sammel die Bettlaken ein!");
                        dbPlayer.SendNewNotification("Schlüssel für die Wäscherei und Zellen erhalten!");
                        dbPlayer.Container.AddItem(LaundryModule.LaundryKeyItemId);
                    }
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else if (index == 3)
                {
                    if (dbPlayer.HasPlayerSGJob())
                    {
                        dbPlayer.SendNewNotification("Du hast bereits einen Job aktiv!");
                        MenuManager.DismissCurrent(dbPlayer);
                        return true;
                    }
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
