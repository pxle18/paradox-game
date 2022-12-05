using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Clothes.Altkleider
{
    public class AltkleiderMenuBuilder : MenuBuilder
    {
        public AltkleiderMenuBuilder() : base(PlayerMenu.Altkleider)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.Menu(Menu, "Altkleider");
            menu.Add(MSG.General.Close());

            foreach (var clothId in dbPlayer.Character.Wardrobe.ToList())
            {
                var cloth = ClothModule.Instance.Get(clothId);
                if (cloth == null || !cloth.Teams.Contains((int)teams.TEAM_CIVILIAN) || cloth.Gender != dbPlayer.Customization.Gender) continue;
                if (cloth.Slot == 3) continue; //Körper
                menu.Add($"{cloth.Name}");
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
                    MenuManager.DismissMenu(dbPlayer.Player, (uint)PlayerMenu.Altkleider);
                    ClothModule.SaveCharacter(dbPlayer);
                    return false;
                }
                
                int idx = 1;

                foreach (var clothId in dbPlayer.Character.Wardrobe.ToList())
                {
                    var cloth = ClothModule.Instance.Get(clothId);
                    if (cloth == null || !cloth.Teams.Contains((int)teams.TEAM_CIVILIAN) || cloth.Gender != dbPlayer.Customization.Gender) continue;
                    if (cloth.Slot == 3) continue; //Körper

                    if (idx == index)
                    {
                        if(!dbPlayer.Container.CanInventoryItemAdded(AltkleiderModule.AltkleiderSackId, 1))
                        {
                            dbPlayer.SendNewNotification("Du hast kein Platz im Inventar!");
                            return false;
                        }

                        Logging.Logger.AddToAltkleiderLog(dbPlayer.Id, "Cloth " + cloth.Id);

                        dbPlayer.Character.Wardrobe.Remove(cloth.Id);
                        MySQLHandler.ExecuteAsync($"DELETE FROM `player_ownedclothes` WHERE player_id = '{dbPlayer.Id}' AND clothes_id = '{cloth.Id}'");

                        MenuManager.DismissMenu(dbPlayer.Player, (uint)PlayerMenu.Altkleider);
                        dbPlayer.SendNewNotification($"Du hast {cloth.Name} in einen Altkleidersack verstaut!");

                        Dictionary<string, dynamic> ItemData = new Dictionary<string, dynamic>();
                        ItemData.Add(AltkleiderModule.PriceInfoString, (int)Convert.ToInt32(cloth.Price / 5));

                        dbPlayer.Container.AddItem(AltkleiderModule.AltkleiderSackId, 1, ItemData);
                        return true;
                    }
                    idx++;
                }
                return false;
            }
        }
    }
}