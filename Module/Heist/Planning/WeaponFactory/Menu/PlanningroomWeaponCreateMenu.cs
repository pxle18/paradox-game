using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Heist.Planning.WeaponFactory;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Laboratories.Menu
{
    public class PlanningroomWeaponCreateMenuBuilder : MenuBuilder
    {
        public PlanningroomWeaponCreateMenuBuilder() : base(PlayerMenu.PlanningroomWeaponCreateMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Module.Menu.Menu(Menu, "Herstellung");

            menu.Add($"Schließen");
            foreach (PlanningWeaponFactoryItem planningWeaponFactoryItem in PlanningWeaponFactoryItemModule.Instance.GetAll().Values)
            {
                ItemModel buildItem = ItemModelModule.Instance.Get(planningWeaponFactoryItem.ResultItemId);

                menu.Add($"{buildItem.Name} - ${planningWeaponFactoryItem.RequiredBlackMoney}");
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else
                {
                    int idx = 1;
                    foreach (PlanningWeaponFactoryItem planningWeaponFactoryItem in PlanningWeaponFactoryItemModule.Instance.GetAll().Values)
                    {
                        if (index == idx)
                        {

                            foreach (KeyValuePair<uint, int> kvp in planningWeaponFactoryItem.RequiredItems)
                            {
                                if (dbPlayer.Container.GetItemAmount(kvp.Key) < kvp.Value)
                                {
                                    dbPlayer.SendNewNotification($"Sie benötigen {kvp.Value} {ItemModelModule.Instance.GetById(kvp.Key).Name}!");
                                    return false;
                                }
                            }

                            if (!dbPlayer.TakeBlackMoney(planningWeaponFactoryItem.RequiredBlackMoney))
                            {
                                dbPlayer.SendNewNotification(MSG.Money.NotEnoughSWMoney(planningWeaponFactoryItem.RequiredBlackMoney));
                                return false;
                            }

                            foreach (KeyValuePair<uint, int> kvp in planningWeaponFactoryItem.RequiredItems)
                            {
                                dbPlayer.Container.RemoveItem(kvp.Key, kvp.Value);
                            }

                            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                            {
                                int time = 10000; // 10 sek
                                Chats.sendProgressBar(dbPlayer, time);

                                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                                dbPlayer.SetData("userCannotInterrupt", true);

                                await NAPI.Task.WaitForMainThread(time);

                                dbPlayer.SetData("userCannotInterrupt", false);
                                dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                                dbPlayer.Container.AddItem(planningWeaponFactoryItem.ResultItemId, 1);
                                dbPlayer.SendNewNotification($"Sie haben {ItemModelModule.Instance.Get(planningWeaponFactoryItem.ResultItemId).Name} hergestellt!");

                            }));
                            return true;
                        }
                        idx++;
                    }

                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }

            }
        }
    }
}
