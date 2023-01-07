using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Clothes.Outfits
{
    public class OutfitsSubMenuBuilder : MenuBuilder
    {
        public OutfitsSubMenuBuilder() : base(PlayerMenu.OutfitsSubMenu)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasData("outfit")) return null;
            var menu = new Menu.NativeMenu(Menu, "Outfits");
            menu.Add(GlobalMessages.General.Close());
            menu.Add("Anlegen");
            menu.Add("Löschen");
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
                if (!dbPlayer.HasData("outfit")) return false;
                if (index == 1)
                {
                    Outfit outfit = null;
                    try {
                        outfit = dbPlayer.GetData("outfit");
                    }
                    catch(Exception e)
                    {
                        Logging.Logger.Crash(e);
                        return false;
                    }
                    if (outfit == null) return false;

                    // Resettall
                    dbPlayer.Character.Clothes.Clear();
                    dbPlayer.Character.EquipedProps.Clear();

                    // Clothes
                    foreach (KeyValuePair<int, uint> kvp in outfit.Clothes.ToList())
                    {
                        
                        Cloth cloth = ClothModule.Instance.Get(kvp.Value);
                        if (cloth == null) continue;

                        if (!dbPlayer.Character.Wardrobe.Contains(kvp.Value) && !cloth.Teams.Contains(dbPlayer.TeamId))
                        {
                            dbPlayer.SendNewNotification("Dieses Kleidungsstück befindet sich nicht mehr in deinem Kleiderschrank!");
                            continue;
                        }

                        // Put on this ...
                        if (dbPlayer.Character.Clothes.ContainsKey(kvp.Key))
                        {
                            dbPlayer.Character.Clothes[kvp.Key] = kvp.Value;
                        }
                        else
                        {
                            dbPlayer.Character.Clothes.Add(kvp.Key, kvp.Value);
                        }
                    }

                    // Props
                    foreach (KeyValuePair<int, uint> kvp in outfit.Props.ToList())
                    {
                        Prop prop = PropModule.Instance.Get(kvp.Value);
                        if (prop == null) continue;

                        if (prop.TeamId != 0 && prop.TeamId != dbPlayer.TeamId) continue;

                        if (!dbPlayer.Character.Props.Contains(kvp.Value) && prop.TeamId == 0)
                        {
                            dbPlayer.SendNewNotification("Dieses Kleidungsstück befindet sich nicht mehr in deinem Kleiderschrank!");
                            continue;
                        }

                        // Put on this ...
                        if (dbPlayer.Character.EquipedProps.ContainsKey(prop.Slot))
                        {
                            dbPlayer.Character.EquipedProps[prop.Slot] = prop.Id;
                        }
                        else
                        {
                            dbPlayer.Character.EquipedProps.Add(prop.Slot, prop.Id);
                        }
                    }


                    ClothModule.Instance.RefreshPlayerClothes(dbPlayer);
                    ClothModule.SaveCharacter(dbPlayer);

                    dbPlayer.SendNewNotification($"Outfit {outfit.Name} angelegt!");
                    return true;
                }
                else if (index == 2)
                {
                    Outfit outfit = null;
                    try
                    {
                        outfit = dbPlayer.GetData("outfit");
                    }
                    catch (Exception e)
                    {
                        Logging.Logger.Crash(e);
                        return false;
                    }
                    if (outfit == null) return false;

                    dbPlayer.SendNewNotification($"Outfit {outfit.Name} gelöscht!");
                    OutfitsModule.Instance.DeleteOutfit(dbPlayer, outfit);
                    return true;
                }
                else
                {
                    MenuManager.DismissMenu(dbPlayer.Player, (uint)PlayerMenu.OutfitsMenu);
                    return false;
                }
            }
        }
    }
}