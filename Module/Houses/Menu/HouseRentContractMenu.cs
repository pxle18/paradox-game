using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Farming;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Houses
{
    public class HouseRentContractMenuBuilder : MenuBuilder
    {
        public HouseRentContractMenuBuilder() : base(PlayerMenu.HouseRentContract)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Module.Menu.Menu(Menu, "Mietslot wählen");

            menu.Add($"Schließen");
            foreach (HouseRent houseRent in HouseRentModule.Instance.houseRents.ToList().Where(hr => hr.HouseId == dbPlayer.ownHouse[0] && hr.PlayerId == 0))
            {
                menu.Add($"Freier Slot {houseRent.SlotId} | ${houseRent.RentPrice}");
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
                
                
                int count = 1;
                foreach (HouseRent houseRent in HouseRentModule.Instance.houseRents.ToList().Where(hr => hr.HouseId == dbPlayer.ownHouse[0] && hr.PlayerId == 0))
                {
                    if (index == count)
                    {
                        dbPlayer.SetData("TenantSlot", houseRent.SlotId);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Mietvertrag erstellen", Callback = "HouseRentAskTenant", Message = "Hiermit schließen Sie einen Mietvertrag auf dem Mietplatz " + houseRent.SlotId + ". Geben Sie den Namen des Mieters ein:" });

                        MenuManager.DismissCurrent(dbPlayer);
                        return true;
                    }
                    else count++;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}