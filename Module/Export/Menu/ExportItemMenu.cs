using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Export.Menu
{
    public class ExportItemMenu : MenuBuilder
    {
        public ExportItemMenu() : base(PlayerMenu.ItemExportsMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            ItemExportNpc itemExportNpc = ItemExportNpcModule.Instance.GetAll().Values.FirstOrDefault(ie => ie.Position.DistanceTo(dbPlayer.Player.Position) < 3.0f);


            if (itemExportNpc == null) return null;

            var l_Menu = new Module.Menu.NativeMenu(Menu, "Export Händler");

            l_Menu.Add(GlobalMessages.General.Close(), "");
            foreach (var kvp in itemExportNpc.ItemExportList)
            {
                l_Menu.Add(kvp.Item.Name + " - $" + kvp.Price, "");
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
                ItemExportNpc itemExportNpc = ItemExportNpcModule.Instance.GetAll().Values.FirstOrDefault(ie => ie.Position.DistanceTo(dbPlayer.Player.Position) < 3.0f);
                
                if (itemExportNpc == null) return true;

                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else
                {
                    int idx = 1;
                    
                    foreach (var kvp in itemExportNpc.ItemExportList)
                    {
                        if (index == idx)
                        {
                            int count = dbPlayer.Container.GetItemAmount(kvp.Item);
                            if (count >= 1)
                            {
                                dbPlayer.Container.RemoveItem(kvp.Item, count);
                                if (itemExportNpc.Illegal) dbPlayer.GiveBlackMoney(kvp.Price * count);
                                else dbPlayer.GiveMoney(kvp.Price * count);
                                dbPlayer.SendNewNotification(count + " " + kvp.Item.Name + " verkauft fuer $" + kvp.Price * count);

                                if(dbPlayer.Team.IsBadOrga() && itemExportNpc.Id == 12) // Kleinkaliber Export
                                {
                                    int WeaponSellPriceFbank = Convert.ToInt32((kvp.Price * count) * 0.05f);
                                    TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(WeaponSellPriceFbank);

                                    dbPlayer.SendNewNotification("+5% gehen in die Fraktionskasse ($" + WeaponSellPriceFbank + ")");
                                }


                                kvp.TempCountSaving += count;

                                if(kvp.TempCountSaving >= 50)
                                {
                                    kvp.TempCountSaving = 0;
                                    itemExportNpc.CalculateNewPrices(kvp);
                                }

                                // Logging
                                Logging.Logger.SaveToItemExportSellLog(dbPlayer.Id, itemExportNpc.Id, kvp.Item.Id, kvp.Price, count);

                                return true;
                            }
                            MenuManager.DismissCurrent(dbPlayer);
                            return false;
                        }
                        idx++;
                    }
                }
                return true;
            }
        }
    }
}
