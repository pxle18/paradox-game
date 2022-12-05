using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tattoo;

namespace VMP_CNR
{
    public class TattooLaseringMenuBuilder : MenuBuilder
    {
        public TattooLaseringMenuBuilder() : base(PlayerMenu.TattooLaseringMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Tattoo Lasering");
            
            foreach(uint id in dbPlayer.Customization.Tattoos)
            {
                int price = 200 * dbPlayer.Level;

                AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get(id);
                if (assetsTattoo == null) continue;
                menu.Add($"{assetsTattoo.Name} {price}$");
            }

            menu.Add(MSG.General.Close());
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
                int idx = 0;

                
                foreach (uint id in dbPlayer.Customization.Tattoos)
                {
                    if (AssetsTattooModule.Instance.GetAll().ContainsKey(id))
                    {
                        if(idx == index)
                        {
                            int price = 200 * dbPlayer.Level;

                            if(!dbPlayer.TakeMoney(price))
                            {
                                dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(price));
                                return false;
                            }

                            dbPlayer.SendNewNotification($"Tattoo {AssetsTattooModule.Instance.Get(id).Name} entfernt, kosten: {price}$");
                            dbPlayer.LaserTattoo(id);
                            return true;
                        }
                        idx++;
                    }
                }

                MenuManager.DismissCurrent(dbPlayer);
                return false;
            }
        }
    }
}