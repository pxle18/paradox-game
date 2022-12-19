using VMP_CNR.Module.GTAN;
using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Character;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Clothes.Shops;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.Export;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Jobs;
using VMP_CNR.Module.Jobs.Bus;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerTask;
using VMP_CNR.Module.Robbery;
using VMP_CNR.Module.Tasks;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Permission;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;
using VMP_CNR.Module.Vehicles.Garages;
using VMP_CNR.Module.Vehicles.Shops;
using VMP_CNR.Module.Players.Events;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Shops;
using VMP_CNR.Module.Farming;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Helper;
using VMP_CNR.Module.Teams.Shelter;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Clothes.Slots;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Warrants;
using VMP_CNR.Module.Staatskasse;
using VMP_CNR.Module.Vehicles.RegistrationOffice;
using VMP_CNR.Module.Dealer;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Zone;
using VMP_CNR.Module.NSA;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.FIB;
using VMP_CNR.Module.Payment;

namespace VMP_CNR
{
    public class MenuCallbackEventHandler : Script
    {
        public List<DbPlayer> Users = Players.Instance.GetValidPlayers();

        [RemoteEvent]
        public void m(Player player, int index, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            try
            {
                var isClosing = index < 0;
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                // Menu is closed, reset menu id
                if (isClosing)
                {
                    /*if (dbPlayer.Freezed == false)
                    {
                        player.FreezePosition = false;
                    }*/

                    if (-index == dbPlayer.WatchMenu)
                    {
                        dbPlayer.WatchMenu = 0;
                    }

                    return;
                }

                var menuid = dbPlayer.WatchMenu;

                if (Enum.IsDefined(typeof(PlayerMenu), menuid) && Enum.TryParse(menuid.ToString(), out PlayerMenu menu))
                {
                    MenuManager.Instance.OnSelect(menu, index, dbPlayer);
                    return;
                }

                if (menuid == Dialogs.menu_fmembers)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmembers);
                }
                else if (menuid == Dialogs.menu_fmanager)
                {
                    DbPlayer targetPlayer = dbPlayer.GetData("temp_indx");
                    if (!targetPlayer.IsValid()) return;
                    switch (index)
                    {
                        case 0:
                            if (targetPlayer.TeamId == dbPlayer.TeamId &&
                                dbPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Manage == 2)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        "Sie können keinen Mainleader uninviten!", title: "", notificationType: PlayerNotification.NotificationType.ERROR);
                                    return;
                                }

                                targetPlayer.RemoveParamedicLicense();

                                targetPlayer.SetTeam(0);
                                targetPlayer.SendNewNotification(
                                    dbPlayer.GetName() + " hat Sie aus der Fraktion entlassen!");
                                dbPlayer.SendNewNotification($"Sie haben {targetPlayer.GetName()} entlassen!");
                                
                                PlayerSpawn.OnPlayerSpawn(targetPlayer.Player);
                                targetPlayer.SetTeamRankPermission(false, 0, false, "");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 1:
                            if (targetPlayer.TeamId == dbPlayer.TeamId &&
                                dbPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRank + 1 >= 12)
                                {
                                    dbPlayer.SendNewNotification(
                                         "maximaler Rang erreicht");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.TeamRank = targetPlayer.TeamRank + 1;
                                targetPlayer.SendNewNotification(
                                    dbPlayer.GetName() + " hat Sie befoerdert!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                dbPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} befoerdert!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 2:
                            if (targetPlayer.TeamId == dbPlayer.TeamId &&
                                dbPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Manage == 2)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        "Sie können keinen Mainleader degradieren!");
                                    return;
                                }

                                if (targetPlayer.TeamRank == 0)
                                {
                                    dbPlayer.SendNewNotification(
                                         "minimaler Rang erreicht!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.TeamRank = targetPlayer.TeamRank - 1;
                                targetPlayer.SendNewNotification(
                                    dbPlayer.GetName() + " hat Sie degradiert!");
                                dbPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} degradiert!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 3:
                            if (targetPlayer.TeamId == dbPlayer.TeamId &&
                                dbPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Bank)
                                {
                                    dbPlayer.SendNewNotification(
                                         "Spieler hat bereits Rechte zur Bank!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.SetTeamRankPermission(true, targetPlayer.TeamRankPermission.Manage,
                                    targetPlayer.TeamRankPermission.Inventory, targetPlayer.TeamRankPermission.Title);
                                targetPlayer.SendNewNotification(
                                    dbPlayer.GetName() + " hat Ihnen Rechte zur Fraktionsbank gegeben!");
                                dbPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} Rechte zur Fraktionsbank gegeben!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 4:
                            if (targetPlayer.TeamId == dbPlayer.TeamId &&
                                dbPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Inventory)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        "Spieler hat bereits Rechte zum Inventar!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.SetTeamRankPermission(targetPlayer.TeamRankPermission.Bank,
                                    targetPlayer.TeamRankPermission.Manage, true, targetPlayer.TeamRankPermission.Title);
                                targetPlayer.SendNewNotification(
                                    dbPlayer.GetName() + " hat Ihnen Rechte zum Fraktionsinventar gegeben!");
                                dbPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} Rechte zum Fraktionsinventar gegeben!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 5:
                            if (targetPlayer.TeamId == dbPlayer.TeamId &&
                                dbPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Manage == 1)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        "Spieler hat bereits Rechte zur Fraktionsverwaltung!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.SetTeamRankPermission(targetPlayer.TeamRankPermission.Bank, 1,
                                    targetPlayer.TeamRankPermission.Inventory, targetPlayer.TeamRankPermission.Title);
                                targetPlayer.SendNewNotification(
                                    dbPlayer.GetName() + " hat Ihnen Rechte zur Fraktionsverwaltung gegeben!");
                                dbPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} Rechte zur Fraktionsverwaltung gegeben!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 6:
                            if (targetPlayer.TeamId == dbPlayer.TeamId &&
                                dbPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Manage == 2)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        "Sie können keinem Mainleader die Rechte entziehen!");
                                    return;
                                }

                                targetPlayer.SetTeamRankPermission(false, 0, false, "");
                                targetPlayer.SendNewNotification(
                                    dbPlayer.GetName() + " hat Ihnen alle Fraktionsrechte entzogen!");
                                dbPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} alle Fraktionsrechte entzogen!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_player)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_player);
                }
                else if (menuid == Dialogs.menu_vehinventory)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_vehinventory);
                }
                else if (menuid == Dialogs.menu_shop_clothes)
                {
                    int idx = 0;
                    uint shopId = 0;

                    if (dbPlayer.HasData("clothShopId"))
                    {
                        shopId = dbPlayer.GetData("clothShopId");
                    }

                    ClothesShop clothesShop = ClothesShopModule.Instance.GetShopById(shopId);
                    if (clothesShop == null) return;

                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes);
                        return;
                    }
                    // Buy
                    else if (index == 1)
                    {
                        int price = ClothesShopModule.Instance.GetActualClothesPrice(dbPlayer);


                        if (price > 0)
                        {
                            int couponPercent = 0;
                            uint whatCoupon = 0;

                            if (clothesShop.CouponUsable)
                            {
                                foreach (KeyValuePair<int, Item> kvp in dbPlayer.Container.Slots)
                                {
                                    if (kvp.Value.Model == null) continue;
                                    if (kvp.Value.Model.Script == null) continue;
                                    if (kvp.Value.Model.Script.Contains("discount_cloth_"))
                                    {
                                        try
                                        {
                                            couponPercent = Int32.Parse(kvp.Value.Model.Script.Replace("discount_cloth_", ""));
                                            double temp = couponPercent / 100.0d;
                                            price -= (int)(price * temp);
                                            whatCoupon = kvp.Value.Id;
                                        }
                                        catch (Exception)
                                        {
                                            dbPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");
                                            return;
                                        }
                                        break;
                                    }
                                }
                            }

                            if(clothesShop.CWSId > 0)
                            {
                                if (!dbPlayer.TakeCWS((CWSTypes)clothesShop.CWSId, price))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughCW(price, (CWSTypes)clothesShop.CWSId));
                                    return;
                                }
                                else
                                {
                                    dbPlayer.SendNewNotification(
                                     "Sie haben diese Kleidung fuer " + price +
                                    " " + ((CWSTypes)clothesShop.CWSId).ToString() + " erworben!");
                                }
                            }
                            else
                            {
                                if (!dbPlayer.TakeMoney(price))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                                    return;
                                }
                                else
                                {
                                    dbPlayer.SendNewNotification(
                                     "Sie haben diese Kleidung fuer $" + price +
                                    " erworben!");
                                }
                            }

                            if (whatCoupon != 0 && clothesShop.CouponUsable)
                            {
                                dbPlayer.SendNewNotification("- " + couponPercent + " % Rabatt", title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                dbPlayer.Container.RemoveItem(whatCoupon);
                            }

                            shopId = 0;
                            if (dbPlayer.HasData("clothShopId"))
                            {
                                shopId = dbPlayer.GetData("clothShopId");
                            }
                            
                            if (shopId != 0)
                            {
                                Logger.SaveClothesShopBuyAction(shopId, price);
                            }
                        }
                        
                        ClothesShopModule.Instance.Buy(dbPlayer, null);

                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes);
                        return;
                    }

                    var character = dbPlayer.Character;

                    var clothesSlots = clothesShop.GetClothesSlotsForPlayer(dbPlayer);

                    var propsSlots = clothesShop.GetPropsSlotsForPlayer(dbPlayer);

                    foreach (KeyValuePair<int, ClothesSlot> kvp in clothesSlots)
                    {
                        if (kvp.Value == null) continue;
                        // found
                        if (idx == index - 2)
                        {
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_shop_clothes_selection, kvp.Value.Name, "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes_selection, "Zurueck", "");

                            foreach (Cloth cloth in clothesShop.GetClothesBySlotForPlayer(kvp.Key, dbPlayer))
                            {
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes_selection, cloth.Name + " $" + cloth.Price, "");
                            }

                            dbPlayer.ResetData("propsActualSlot");
                            dbPlayer.SetData("clothesActualSlot", kvp.Key);
                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_shop_clothes_selection);
                            return;
                        }

                        idx++;
                    }

                    foreach (KeyValuePair<int, PropsSlot> kvp in propsSlots)
                    {
                        if (kvp.Value == null) continue;
                        // found
                        if (idx == index - 2)
                        {
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_shop_clothes_selection, kvp.Value.Name, "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes_selection, "Zurueck", "");

                            foreach (Prop prop in clothesShop.GetPropsBySlotForPlayer(kvp.Key, dbPlayer))
                            {
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes_selection, prop.Name + " $" + prop.Price, "");
                            }

                            dbPlayer.ResetData("clothesActualSlot");
                            dbPlayer.SetData("propsActualSlot", kvp.Key);
                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_shop_clothes_selection);
                            return;
                        }

                        idx++;
                    }
                }
                else if (menuid == Dialogs.menu_shop_clothes_selection)
                {
                    uint shopId = 0;
                    if (dbPlayer.HasData("clothShopId"))
                    {
                        shopId = dbPlayer.GetData("clothShopId");
                    }
                    else
                    {
                        return;
                    }

                    var shop = ClothesShopModule.Instance.GetShopById(shopId);

                    if (shop == null) return;

                    // Zurueck
                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes_selection, true);

                        DialogMigrator.CreateMenu(player, Dialogs.menu_shop_clothes, "Kleiderladen", "");

                        DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes, GlobalMessages.General.Close(), "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes, "Kaufen: $" + ClothesShopModule.Instance.GetActualClothesPrice(dbPlayer), "");

                        var clothesSlots = shop.GetClothesSlotsForPlayer(dbPlayer);

                        var propsSlots = shop.GetPropsSlotsForPlayer(dbPlayer);

                        foreach (KeyValuePair<int, ClothesSlot> kvp in clothesSlots)
                        {
                            if (kvp.Value == null) continue;
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes, kvp.Value.Name, kvp.Value.Name);
                        }

                        foreach (KeyValuePair<int, PropsSlot> kvp in propsSlots)
                        {
                            if (kvp.Value == null) continue;
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes, kvp.Value.Name, kvp.Value.Name);
                        }

                        DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_shop_clothes);
                        return;
                    }

                    if (dbPlayer.HasData("clothesActualSlot") || dbPlayer.HasData("propsActualSlot"))
                    {
                        var character = dbPlayer.Character;
                        int slot = -1;
                        if (dbPlayer.HasData("clothesActualSlot"))
                        {
                            slot = (int)dbPlayer.GetData("clothesActualSlot");
                            List<Cloth> clothesList = shop.GetClothesBySlotForPlayer(slot, dbPlayer);

                            if (index - 1 < 0) return;
                            if (index - 1 >= clothesList.Count) return;

                            var selectedCloth = clothesList[index - 1];

                            if (selectedCloth != null)
                            {
                                dbPlayer.SetClothes(clothesList[index - 1].Slot, clothesList[index - 1].Variation, clothesList[index - 1].Texture);

                                dbPlayer.SetData("clothesActualItem-" + slot,
                                    selectedCloth.Id);
                                return;
                            }

                            return;
                        }
                        else
                        {
                            slot = (int)dbPlayer.GetData("propsActualSlot");
                            List<Prop> propsList = shop.GetPropsBySlotForPlayer(slot, dbPlayer);

                            int newIndex = index - 1;
                            if (propsList.Count > newIndex && newIndex > 0)
                            {
                                var selectedProp = propsList[index - 1];

                                if (selectedProp != null)
                                {
                                    ClothModule.Instance.SetPlayerAccessories(dbPlayer, propsList[index - 1].Slot, propsList[index - 1].Variation, propsList[index - 1].Texture);

                                    dbPlayer.SetData("propsActualItem-" + slot,
                                        selectedProp.Id);
                                    return;
                                }
                            }
                        }

                        return;
                    }
                    else
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes_selection);
                        return;
                    }
                }
                else if (menuid == Dialogs.menu_wardrobe)
                {
                    int idx = 3;

                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_wardrobe);
                        return;
                    }
                    
                    if (index == 1)
                    {
                        MenuManager.Instance.Build(PlayerMenu.OutfitsMenu, dbPlayer).Show(dbPlayer);
                        return;
                    }

                    if (index == 2) // Altkleider
                    {
                        if (dbPlayer.HasData("teamWardrobe")) return;
                        MenuManager.Instance.Build(PlayerMenu.Altkleider, dbPlayer).Show(dbPlayer);
                        return;
                    }

                    var character = dbPlayer.Character;

                    foreach (KeyValuePair<int, ClothesSlot> kvp in ClothesShopModule.Instance.GetClothesSlots())
                    {
                        // found
                        if (idx == index)
                        {
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_wardrobe, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_wardrobe_selection, kvp.Value.Name, "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, "Zurück", "");
                            if (dbPlayer.HasData("teamWardrobe"))
                            {
                                foreach (Cloth cloth in ClothModule.Instance.GetTeamWarerobe(dbPlayer, kvp.Key))
                                {
                                    if (cloth == null) continue;
                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, cloth.Name,
                                        cloth.Name);
                                }
                            }
                            else
                            {
                                foreach (Cloth cloth in ClothModule.Instance.GetWardrobeBySlot(dbPlayer, character,
                                    kvp.Key))
                                {
                                    if (cloth == null) continue;
                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, cloth.Name,
                                        cloth.Name);
                                }
                            }

                            dbPlayer.SetData("clothesWardrobeActualSlot", kvp.Key);
                            dbPlayer.ResetData("propsWardrobeActualSlot");
                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_wardrobe_selection);
                            return;
                        }

                        idx++;
                    }

                    foreach (KeyValuePair<int, PropsSlot> kvp in ClothesShopModule.Instance.GetPropsSlots())
                    {
                        // found
                        if (idx == index - 1)
                        {
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_wardrobe, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_wardrobe_selection, kvp.Value.Name, "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, "Zurück", "");
                            if (dbPlayer.HasData("teamWardrobe"))
                            {
                                foreach (Prop prop in PropModule.Instance.GetTeamWarerobe(dbPlayer, kvp.Key))
                                {
                                    if (prop == null) continue;
                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, prop.Name,
                                        prop.Name);
                                }
                            }
                            else
                            {
                                foreach (Prop prop in PropModule.Instance.GetWardrobeBySlot(dbPlayer, kvp.Key))
                                {
                                    if (prop == null) continue;
                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, prop.Name,
                                        prop.Name);
                                }
                            }

                            dbPlayer.SetData("propsWardrobeActualSlot", kvp.Key);
                            dbPlayer.ResetData("clothesWardrobeActualSlot");
                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_wardrobe_selection);
                            return;
                        }

                        idx++;
                    }
                }
                else if (menuid == Dialogs.menu_wardrobe_selection)
                {
                    if (index == 0)
                    {
                        Character playerCharacter = dbPlayer.Character;
                        var playerWardrobe = playerCharacter.Wardrobe;
                        DialogMigrator.CreateMenu(player, Dialogs.menu_wardrobe, "Kleiderschrank", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, GlobalMessages.General.Close(), "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, "Outfits", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, "Altkleider packen", "");

                        foreach (KeyValuePair<int, ClothesSlot> kvp in ClothesShopModule.Instance.GetClothesSlots())
                        {
                            if (kvp.Value == null) continue;
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, kvp.Value.Name, kvp.Value.Name);
                        }

                        foreach (KeyValuePair<int, PropsSlot> kvp in ClothesShopModule.Instance.GetPropsSlots())
                        {
                            if (kvp.Value == null) continue;
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, kvp.Value.Name, kvp.Value.Name);
                        }

                        DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_wardrobe);

                        return;
                    }

                    if (dbPlayer.HasData("clothesWardrobeActualSlot"))
                    {
                        Character playerCharacter = dbPlayer.Character;

                        int slot = dbPlayer.GetData("clothesWardrobeActualSlot");
                        List<Cloth> wardrobeClothesForSlot;
                        if (dbPlayer.HasData("teamWardrobe"))
                        {
                            wardrobeClothesForSlot =
                                ClothModule.Instance.GetTeamWarerobe(dbPlayer, slot);
                        }
                        else
                        {
                            wardrobeClothesForSlot =
                                ClothModule.Instance.GetWardrobeBySlot(dbPlayer, playerCharacter, slot);
                        }

                        if (wardrobeClothesForSlot.Count > index - 1)
                        {
                            var cloth = wardrobeClothesForSlot[index - 1];

                            playerCharacter.Clothes[cloth.Slot] = cloth.Id;

                            ClothModule.Instance.RefreshPlayerClothes(dbPlayer);
                            ClothModule.SaveCharacter(dbPlayer);
                        }
                    }

                    if (dbPlayer.HasData("propsWardrobeActualSlot"))
                    {
                        Character playerCharacter = dbPlayer.Character;

                        int slot = dbPlayer.GetData("propsWardrobeActualSlot");
                        List<Prop> wardrobeClothesForSlot;
                        if (dbPlayer.HasData("teamWardrobe"))
                        {
                            wardrobeClothesForSlot =
                                PropModule.Instance.GetTeamWarerobe(dbPlayer, slot);
                        }
                        else
                        {
                            wardrobeClothesForSlot =
                                PropModule.Instance.GetWardrobeBySlot(dbPlayer, slot);
                        }

                        if (wardrobeClothesForSlot.Count > index - 1)
                        {
                            var cloth = wardrobeClothesForSlot[index - 1];
                            playerCharacter.EquipedProps[cloth.Slot] = cloth.Id;

                            ClothModule.Instance.RefreshPlayerClothes(dbPlayer);
                            ClothModule.SaveCharacter(dbPlayer);
                        }
                    }
                }
                else if (menuid == Dialogs.menu_shop_mechanic)
                {
                    // Close
                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_mechanic);
                        return;
                    }

                    if (index == 1)
                    {
                        if (!dbPlayer.Container.CanInventoryItemAdded(26, 1))
                        {
                            dbPlayer.SendNewNotification("Nicht genug Platz im Inventar!");
                            return;
                        }

                        if (!dbPlayer.TakeMoney(300))
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genug Geld!" + GlobalMessages.Money.NotEnoughMoney(300));
                            return;
                        }

                        dbPlayer.Container.AddItem(26, 1);
                        return;
                    }

                    if (index == 2)
                    {
                        if (!dbPlayer.Container.CanInventoryItemAdded(297, 1))
                        {
                            dbPlayer.SendNewNotification("Nicht genug Platz im Inventar!");
                            return;
                        }

                        if (!dbPlayer.TakeMoney(1000))
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genug Geld!" + GlobalMessages.Money.NotEnoughMoney(1000));
                            return;
                        }

                        dbPlayer.Container.AddItem(297, 1);
                        return;
                    }
                    if (index == 3)
                    {
                        if (!dbPlayer.Container.CanInventoryItemAdded(591, 1))
                        {
                            dbPlayer.SendNewNotification("Nicht genug Platz im Inventar!");
                            return;
                        }

                        if (!dbPlayer.TakeMoney(4000))
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genug Geld!" + GlobalMessages.Money.NotEnoughMoney(4000));
                            return;
                        }

                        dbPlayer.Container.AddItem(591, 1);
                        return;
                    }
                    if (index == 4)
                    {
                        if (!dbPlayer.Container.CanInventoryItemAdded(245, 1))
                        {
                            dbPlayer.SendNewNotification("Nicht genug Platz im Inventar!");
                            return;
                        }

                        if (!dbPlayer.TakeMoney(2000))
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genug Geld!" + GlobalMessages.Money.NotEnoughMoney(2000));
                            return;
                        }

                        dbPlayer.Container.AddItem(245, 1);
                        return;
                    }

                    //TODO

                    /*
                    if (idx == index)
                    {
                        if (dbPlayer.Container.CanInventoryItemAdded()
                        {
                            dbPlayer.SendNewNotification(
                                 "Inventar ist voll!");
                            return;
                        }

                        if (!dbPlayer.TakeMoney(item.BuyPrice))
                        {
                            dbPlayer.SendNewNotification(
                                
                                MSG.Money.NotEnoughMoney(item.BuyPrice));
                            return;
                        }

                        dbPlayer.Container.AddItem(item, 1);
                        dbPlayer.SendNewNotification(
                                "Sie haben " + item.Name + " fuer $" +
                            item.BuyPrice + " gekauft!");
                    }*/
                }
                else if (menuid == Dialogs.menu_adminObject)
                {
                    if (dbPlayer.AdminObject == null)
                    {
                        DialogMigrator.CloseUserDialog(player, Dialogs.menu_adminObject);
                        return;
                    }

                    /*switch (index)
                    {
                        case 0:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Position.X + dbPlayer.adminObjectSpeed,
                                    dbPlayer.adminObject.Position.Y, dbPlayer.adminObject.Position.Z), 1);
                            break;
                        case 1:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Position.X - dbPlayer.adminObjectSpeed,
                                    dbPlayer.adminObject.Position.Y, dbPlayer.adminObject.Position.Z), 1);
                            break;
                        case 2:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Position.X,
                                    dbPlayer.adminObject.Position.Y + dbPlayer.adminObjectSpeed,
                                    dbPlayer.adminObject.Position.Z), 1);
                            break;
                        case 3:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Position.X,
                                    dbPlayer.adminObject.Position.Y - dbPlayer.adminObjectSpeed,
                                    dbPlayer.adminObject.Position.Z), 1);
                            break;
                        case 4:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Position.X, dbPlayer.adminObject.Position.Y,
                                    dbPlayer.adminObject.Position.Z + dbPlayer.adminObjectSpeed), 1);
                            break;
                        case 5:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Position.X, dbPlayer.adminObject.Position.Y,
                                    dbPlayer.adminObject.Position.Z - dbPlayer.adminObjectSpeed), 1);
                            break;
                        case 6:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Rotation.X, dbPlayer.adminObject.Rotation.Y,
                                    dbPlayer.adminObject.Rotation.Z + dbPlayer.adminObjectSpeed * 8), 1);
                            break;
                        case 7:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Rotation.X, dbPlayer.adminObject.Rotation.Y,
                                    dbPlayer.adminObject.Rotation.Z - dbPlayer.adminObjectSpeed * 8), 1);
                            break;
                        case 8:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Rotation.X + dbPlayer.adminObjectSpeed * 8,
                                    dbPlayer.adminObject.Rotation.Y,
                                    dbPlayer.adminObject.Rotation.Z), 1);
                            break;
                        case 9:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Rotation.X - dbPlayer.adminObjectSpeed * 8,
                                    dbPlayer.adminObject.Rotation.Y,
                                    dbPlayer.adminObject.Rotation.Z), 1);
                            break;
                        case 10:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Rotation.X,
                                    dbPlayer.adminObject.Rotation.Y + dbPlayer.adminObjectSpeed * 8,
                                    dbPlayer.adminObject.Rotation.Z), 1);
                            break;
                        case 11:
                            dbPlayer.adminObject.MovePosition(
                                new Vector3(dbPlayer.adminObject.Rotation.X,
                                    dbPlayer.adminObject.Rotation.Y - dbPlayer.adminObjectSpeed * 8,
                                    dbPlayer.adminObject.Rotation.Z), 1);
                            break;
                        case 12:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_adminObject);
                            break;
                    }*/
                }
                else if (menuid == Dialogs.menu_house_main)
                {
                    if (!dbPlayer.HasData("houseId")) return;
                    uint houseId = dbPlayer.GetData("houseId");
                    House iHouse = HouseModule.Instance[houseId];
                    if (iHouse == null) return;
                    switch (index)
                    {
                        case 0:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                            break;
                        case 1:
                            HouseModule.Instance.PlayerEnterHouse(dbPlayer, iHouse);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                            break;
                        case 2:
                            if (iHouse.Locked == true)
                            {
                                dbPlayer.SendNewNotification("Bitte schließe dein Haus zunaechst auf 'L'", title: "Haus", notificationType: PlayerNotification.NotificationType.HOUSE);
                                return;
                            }

                            // Keller
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_house_keller, "Hauskeller", "");

                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, GlobalMessages.General.Close(),
                                "");

                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, "Keller betreten",
                                "Betretet den Keller");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, "Geldwaesche betreten",
                                "Betretet den Keller");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, "Keller ausbauen",
                                "Baut oder upgraded einen Keller");

                            if (iHouse.Type == 3)
                            {
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, "Geldwaesche ausbauen",
                                    "Baut oder upgraded einen Geldwaesche Keller");
                            }

                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_house_keller);
                            break;
                        case 3:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                            MenuManager.Instance.Build(PlayerMenu.AnimalAssignMenu, dbPlayer).Show(dbPlayer);
                            break;
                        case 4:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main, true);

                            if (dbPlayer.OwnHouse[0] != iHouse.Id)
                            {
                                dbPlayer.SendNewNotification(

                                    "Sie müssen der Besitzer des Hauses sein!");
                                return;
                            }
                            
                            if(iHouse.ShowPhoneNumber.Length > 0)
                            {
                                dbPlayer.SendNewNotification($"Die Telefonnummer wird nun nicht mehr angezeigt!");
                                iHouse.ShowPhoneNumber = "";
                                iHouse.SaveShowPhoneNumber();
                            }
                            else
                            {
                                dbPlayer.SendNewNotification($"Die Telefonnummer wird nun angezeigt!");
                                iHouse.ShowPhoneNumber = "" + dbPlayer.handy[0];
                                iHouse.SaveShowPhoneNumber();
                            }

                            break;
                        case 5: // Garage
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main, true);

                            // Wenn Garage
                            if (iHouse.GarageId != 0)
                            {
                                Garage garage = GarageModule.Instance.GetHouseGarage(iHouse.Id);
                                if (garage == null) return;
                                if (garage.IsTeamGarage()) return;
                                DialogMigrator.CreateMenu(player, Dialogs.menu_garage_overlay, "Fahrzeug-Garage", "");
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, GlobalMessages.General.Close(), "");
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, "Fahrzeug entnehmen", "");
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, "Fahrzeug einlagern", "");
                                

                                DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_garage_overlay);
                                dbPlayer.SetData("GarageId", garage.Id);
                                return;
                            }
                            else // Garage Ausbau
                            {
                                int cost = iHouse.Price / 4;

                                if (!iHouse.CanGarageBuild())
                                {
                                    dbPlayer.SendNewNotification(
                                        $"Fuer den Ausbau benötigen Sie mindestens 50 {ItemModelModule.Instance.Get(312).Name} und 100 {ItemModelModule.Instance.Get(310).Name}!");
                                    return;
                                }
                                else
                                {
                                    if (dbPlayer.OwnHouse[0] != iHouse.Id)
                                    {
                                        dbPlayer.SendNewNotification(
                                            
                                            "Um den Keller auszubauen muessen Sie der Besitzer des Hauses sein!");
                                        return;
                                    }

                                    if (!dbPlayer.TakeMoney(cost))
                                    {
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.Money.NotEnoughMoney(cost));
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                                        return;
                                    }

                                    iHouse.BuildGarage();

                                    dbPlayer.SendNewNotification(
                                         "Garage fuer $" + cost +
                                        " ausgebaut!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                                    iHouse.SaveGarage();
                                    return;
                                }
                            }
                            
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_house_keller)
                {
                    if (!dbPlayer.HasData("houseId")) return;
                    uint houseId = dbPlayer.GetData("houseId");
                    House iHouse = HouseModule.Instance[houseId];
                    if (iHouse == null) return;
                    switch (index)
                    {
                        case 0:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                            break;
                        case 1:
                            if (iHouse.Keller > 0)
                            {
                                //dbPlayer.Player.FreezePosition = true;
                                
                                player.SetPosition(new Vector3(1138.25f, -3198.88f, -39.6657f));
                                player.SetRotation(357.87f);
                                
                                if (iHouse.Keller == 2)
                                {
                                    dbPlayer.DimensionType[0] = DimensionType.Labor;
                                }
                                else
                                {
                                    dbPlayer.DimensionType[0] = DimensionType.Basement;
                                }

                                dbPlayer.SetData("inHouse", iHouse.Id);
                                dbPlayer.SetDimension(iHouse.Id);

                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie haben keinen Keller!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                return;
                            }
                        case 2:
                            if (iHouse.MoneyKeller > 0)
                            {
                                if (iHouse.Type != 3)
                                {
                                    dbPlayer.SendNewNotification("Sie haben keinen Geldwäsche Keller!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }

                                if (!ServerFeatures.IsActive("blackmoney"))
                                {
                                    dbPlayer.SendNewNotification("Die Geldwäsche ist aufgrund von Problemen deaktiviert. Wir arbeiten an der Lösung des Problems. Sobald dieses Feature wieder verfügbar ist," +
                                        "geben wir im Forum und/oder GVMP-Launcher Bescheid.");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }

                                //dbPlayer.Player.FreezePosition = true;

                                player.SetPosition(new Vector3(1138.25f, -3198.88f, -39.6657f));
                                player.SetRotation(357.87f);
                                dbPlayer.DimensionType[0] = DimensionType.MoneyKeller;
                                dbPlayer.Player.TriggerNewClient("loadblackmoneyInterior");
                                dbPlayer.SetData("inHouse", iHouse.Id);

                                dbPlayer.SetDimension(iHouse.Id);

                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(

                                    "Sie haben keinen Geldwäsche Keller!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                return;
                            }
                        case 3:
                            if (iHouse.Keller == 1) // Ausbau auf Labor
                            {
                                int cost = 100000;

                                if (dbPlayer.CheckTaskExists(PlayerTaskTypeId.KellerAusbau) ||
                                    dbPlayer.CheckTaskExists(PlayerTaskTypeId.LaborAusbau) ||
                                    dbPlayer.CheckTaskExists(PlayerTaskTypeId.MoneyKellerAusbau))
                                {
                                    dbPlayer.SendNewNotification("Ein Ausbau ist bereits in Arbeit!");
                                    return;
                                }

                                if (!iHouse.CanKellerUpgraded())
                                {
                                    dbPlayer.SendNewNotification(
                                        $"Fuer den Ausbau benötigen Sie mindestens 40 {ItemModelModule.Instance.Get(312).Name} und 100 {ItemModelModule.Instance.Get(310).Name}!");
                                    return;
                                }
                                else
                                {
                                    if (dbPlayer.OwnHouse[0] != iHouse.Id)
                                    {
                                        dbPlayer.SendNewNotification(
                                            
                                            "Um den Keller auszubauen muessen Sie der Besitzer des Hauses sein!");
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                        return;
                                    }

                                    if (!dbPlayer.TakeMoney(cost))
                                    {
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.Money.NotEnoughMoney(cost));
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                        return;
                                    }

                                    iHouse.UpgradeKeller(dbPlayer);

                                    dbPlayer.SendNewNotification(
                                         "Ausbauvorgang fuer $" + cost +
                                        " gestartet!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }
                            }
                            else if (iHouse.Keller == 0) // Normaler ausbau
                            {
                                int cost = 50000;

                                if (dbPlayer.CheckTaskExists(PlayerTaskTypeId.KellerAusbau) ||
                                    dbPlayer.CheckTaskExists(PlayerTaskTypeId.LaborAusbau) ||
                                    dbPlayer.CheckTaskExists(PlayerTaskTypeId.MoneyKellerAusbau))
                                {
                                    dbPlayer.SendNewNotification("Ein Ausbau ist bereits in Arbeit!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }

                                if (!iHouse.CanKellerBuild())
                                {
                                    dbPlayer.SendNewNotification(
                                        $"Fuer den Ausbau benötigen Sie mindestens 25 {ItemModelModule.Instance.Get(312).Name} und 60 {ItemModelModule.Instance.Get(310).Name}!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }
                                else
                                {
                                    if (dbPlayer.OwnHouse[0] != iHouse.Id)
                                    {
                                        dbPlayer.SendNewNotification(
                                            
                                            "Um den Keller auszubauen muessen Sie der Besitzer des Hauses sein!");
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                        return;
                                    }

                                    if (!dbPlayer.TakeMoney(cost))
                                    {
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.Money.NotEnoughMoney(cost));
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                        return;
                                    }

                                    iHouse.BuildKeller(dbPlayer);

                                    dbPlayer.SendNewNotification(
                                         "Ausbauvorgang fuer $" + cost +
                                        " gestartet!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Keller bereits ausgebaut!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                return;
                            }
                            break;
                        case 4:
                            if (iHouse.MoneyKeller != 1) // Ausbau auf Geldwäsche
                            {
                                if(iHouse.Type != 3)
                                {
                                    dbPlayer.SendNewNotification("Bei diesem Typ ist ein Ausbau nicht möglich!");
                                    return;
                                }

                                int cost = 250000;

                                if (dbPlayer.CheckTaskExists(PlayerTaskTypeId.KellerAusbau) ||
                                    dbPlayer.CheckTaskExists(PlayerTaskTypeId.LaborAusbau) ||
                                    dbPlayer.CheckTaskExists(PlayerTaskTypeId.MoneyKellerAusbau))
                                {
                                    dbPlayer.SendNewNotification("Ein Ausbau ist bereits in Arbeit!");
                                    return;
                                }

                                if (!iHouse.CanMoneyKellerBuild())
                                {
                                    dbPlayer.SendNewNotification(
                                        $"Fuer den Ausbau benötigen Sie mindestens 10 {ItemModelModule.Instance.Get(312).Name} und 30 {ItemModelModule.Instance.Get(310).Name}!");
                                    return;
                                }
                                else
                                {
                                    if (dbPlayer.OwnHouse[0] != iHouse.Id)
                                    {
                                        dbPlayer.SendNewNotification(

                                            "Um den Geldwäsche Keller auszubauen muessen Sie der Besitzer des Hauses sein!");
                                        return;
                                    }

                                    if (!dbPlayer.TakeMoney(cost))
                                    {
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.Money.NotEnoughMoney(cost));
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                        return;
                                    }

                                    iHouse.BuildMoneyKeller(dbPlayer);

                                    dbPlayer.SendNewNotification(
                                         "Geldwäsche Keller fuer $" + cost +
                                        " gestartet!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }
                            }
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_shop_changecar)
                {
                    switch (index)
                    {
                        case 0:
                            if (!dbPlayer.RageExtension.IsInVehicle) return;
                            SxVehicle playerVeh = player.Vehicle.GetVehicle();
                            if (playerVeh == null || !dbPlayer.IsOwner(playerVeh) || !playerVeh.IsValid()) return;
                            VehicleHash model = playerVeh.Entity.GetModel();
                            int price = (VehicleShopModule.Instance.GetVehiclePriceFromHash(playerVeh.Data) / 100) * 20;

                            string newmodel = "";

                            if (model == VehicleHash.Banshee)
                                newmodel = Convert.ToString(VehicleHash.Banshee2);
                            else if (model == VehicleHash.Chino)
                                newmodel = Convert.ToString(VehicleHash.Chino2);
                            else if (model == VehicleHash.Diablous)
                                newmodel = Convert.ToString(VehicleHash.Diablous2);
                            else if (model == VehicleHash.Comet2)
                                newmodel = Convert.ToString(VehicleHash.Comet3);
                            else if (model == VehicleHash.Elegy2)
                                newmodel = Convert.ToString(VehicleHash.Elegy);
                            else if (model == VehicleHash.Faction)
                                newmodel = Convert.ToString(VehicleHash.Faction2);
                            else if (model == VehicleHash.Faction2)
                                newmodel = Convert.ToString(VehicleHash.Faction3);
                            else if (model == VehicleHash.Minivan)
                                newmodel = Convert.ToString(VehicleHash.Minivan2);
                            else if (model == VehicleHash.Moonbeam)
                                newmodel = Convert.ToString(VehicleHash.Moonbeam2);
                            else if (model == VehicleHash.Nero)
                                newmodel = Convert.ToString(VehicleHash.Nero2);
                            else if (model == VehicleHash.Primo)
                                newmodel = Convert.ToString(VehicleHash.Primo2);
                            else if (model == VehicleHash.Sabregt)
                                newmodel = Convert.ToString(VehicleHash.Sabregt2);
                            else if (model == VehicleHash.Slamvan)
                                newmodel = Convert.ToString(VehicleHash.Slamvan3);
                            else if (model == VehicleHash.Specter)
                                newmodel = Convert.ToString(VehicleHash.Specter2);
                            else if (model == VehicleHash.Sultan)
                                newmodel = Convert.ToString(VehicleHash.Sultanrs);
                            else if (model == VehicleHash.Tornado)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            else if (model == VehicleHash.Virgo)
                                newmodel = Convert.ToString(VehicleHash.Virgo2);
                            else if (model == VehicleHash.Voodoo2)
                                newmodel = Convert.ToString(VehicleHash.Voodoo);
                            else if (model == VehicleHash.Buffalo)
                                newmodel = Convert.ToString(VehicleHash.Buffalo2);
                            else if (model == VehicleHash.Rapidgt)
                                newmodel = Convert.ToString(VehicleHash.Rapidgt2);
                            else if (model == VehicleHash.Schafter2)
                                newmodel = Convert.ToString(VehicleHash.Schafter4);
                            else if (model == VehicleHash.Sentinel)
                                newmodel = Convert.ToString(VehicleHash.Sentinel2);
                            else if (model == VehicleHash.Rebel)
                                newmodel = Convert.ToString(VehicleHash.Rebel2);
                            else if (model == VehicleHash.Ninef)
                                newmodel = Convert.ToString(VehicleHash.Ninef2);
                            else if (model == VehicleHash.Banshee2)
                                newmodel = Convert.ToString(VehicleHash.Banshee);
                            else if (model == VehicleHash.Chino2)
                                newmodel = Convert.ToString(VehicleHash.Chino);
                            else if (model == VehicleHash.Diablous2)
                                newmodel = Convert.ToString(VehicleHash.Diablous);
                            else if (model == VehicleHash.Comet3)
                                newmodel = Convert.ToString(VehicleHash.Comet2);
                            else if (model == VehicleHash.Elegy)
                                newmodel = Convert.ToString(VehicleHash.Elegy2);
                            else if (model == VehicleHash.Faction3)
                                newmodel = Convert.ToString(VehicleHash.Faction);
                            else if (model == VehicleHash.Minivan2)
                                newmodel = Convert.ToString(VehicleHash.Minivan);
                            else if (model == VehicleHash.Moonbeam2)
                                newmodel = Convert.ToString(VehicleHash.Moonbeam);
                            else if (model == VehicleHash.Nero2)
                                newmodel = Convert.ToString(VehicleHash.Nero);
                            else if (model == VehicleHash.Primo2)
                                newmodel = Convert.ToString(VehicleHash.Primo);
                            else if (model == VehicleHash.Sabregt2)
                                newmodel = Convert.ToString(VehicleHash.Sabregt);
                            else if (model == VehicleHash.Slamvan3)
                                newmodel = Convert.ToString(VehicleHash.Slamvan);
                            else if (model == VehicleHash.Specter2)
                                newmodel = Convert.ToString(VehicleHash.Specter);
                            else if (model == VehicleHash.Sultanrs)
                                newmodel = Convert.ToString(VehicleHash.Sultan);
                            else if (model == VehicleHash.Tornado5)
                                newmodel = Convert.ToString(VehicleHash.Tornado);
                            else if (model == VehicleHash.Virgo2)
                                newmodel = Convert.ToString(VehicleHash.Virgo);
                            else if (model == VehicleHash.Voodoo)
                                newmodel = Convert.ToString(VehicleHash.Voodoo2);
                            else if (model == VehicleHash.Buffalo2)
                                newmodel = Convert.ToString(VehicleHash.Buffalo);
                            else if (model == VehicleHash.Rapidgt2)
                                newmodel = Convert.ToString(VehicleHash.Rapidgt);
                            else if (model == VehicleHash.Schafter4)
                                newmodel = Convert.ToString(VehicleHash.Schafter2);
                            else if (model == VehicleHash.Sentinel2)
                                newmodel = Convert.ToString(VehicleHash.Sentinel);
                            else if (model == VehicleHash.Rebel2)
                                newmodel = Convert.ToString(VehicleHash.Rebel);
                            else if (model == VehicleHash.Ninef2)
                                newmodel = Convert.ToString(VehicleHash.Ninef);
                            else if (model == VehicleHash.Tornado)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            else if (model == VehicleHash.Tornado2)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            else if (model == VehicleHash.Tornado3)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            else if (model == VehicleHash.Tornado4)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            //else if (model == VehicleHash.Fcr2)
                            //    newmodel = Convert.ToString(VehicleHash.Fcr);
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Leider koennen wir dein Auto nicht aufruesten. Komm bitte mit einem Modell wieder, das wir umbauen koennen.");
                                return;
                            }

                            if (newmodel != "")
                            {
                                if (!dbPlayer.TakeMoney(price))
                                {
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.Money.NotEnoughMoney(price));
                                    return;
                                }

                                //UpdateDB
                                string x = player.Position.X.ToString().Replace(",", ".");
                                string y = player.Position.Y.ToString().Replace(",", ".");
                                string z = player.Position.Z.ToString().Replace(",", ".");
                                string heading = player.Rotation.Z.ToString().Replace(",", ".");

                                if (!Enum.TryParse(newmodel, true, out VehicleHash newModelHash)) return;

                                if (VehicleDataModule.Instance.GetData((uint)newModelHash) != null) return;

                                string query = String.Format(
                                    "UPDATE `vehicles` SET `pos_x` = '{0}', `pos_y` = '{1}', `pos_z` = '{2}', `heading` = '{3}', model = '{4}', tuning = '' WHERE id = '{5}';",
                                    x, y, z, heading, VehicleDataModule.Instance.GetData((uint)newModelHash).Id.ToString(),
                                    playerVeh.databaseId);

                                MySQLHandler.Execute(query);

                                // set Car New
                                // ResetMods && clear
                                uint ownerid = playerVeh.databaseId;

                                // Spawn new Vehicle XX
                                VehicleHandler.Instance.DeleteVehicleByEntity(playerVeh.Entity);

                                try
                                {
                                    query = string.Format(
                                        "SELECT * FROM `vehicles` WHERE id = '{0}' ORDER BY id", ownerid);
                                    using (var conn =
                                        new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        conn.Open();
                                        cmd.CommandText = @query;
                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(
                                                        reader.GetUInt32("model"), reader.GetInt32("registered") == 1,
                                                        new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                                                            (reader.GetFloat("pos_z") + 0.3f)),
                                                        reader.GetFloat("heading"), reader.GetInt32("color1"),
                                                        reader.GetInt32("color2"), 0, reader.GetUInt32("gps_tracker") == 1, true, true, 0,
                                                        "",
                                                        reader.GetUInt32("id"), 0, reader.GetUInt32("owner"),
                                                        reader.GetInt32("fuel"), reader.GetInt32("zustand"),
                                                        reader.GetString("tuning"), reader.GetString("neon"),
                                                        reader.GetFloat("km"), null, "", false, reader.GetInt32("TuningState") == 1, WheelClamp: reader.GetInt32("WheelClamp"), AlarmSystem: reader.GetInt32("alarm_system") == 1);

                                                    Task.Run(async () =>
                                                    {
                                                        while (xVeh.Entity == null)
                                                        {
                                                            await Task.Delay(500);
                                                        }
                                                    });

                                                    NAPI.Task.Run(() =>
                                                    {
                                                        xVeh.Entity.NumberPlate = reader.GetString("plate");
                                                    });
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                }

                                dbPlayer.SendNewNotification(

                                    "Ihr Fahrzeug wurde erfolgreich entwickelt, bitte parke es erneut!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_changecar);
                                return;
                            }


                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_changecar);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_changecar);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_shop_stores)
                {
                    /*Shop shop = Shops.Instance.GetThisShop(player.Position);
                    if (shop == null) return;

                    int idx = 0;
                    if (index == 1)
                    {
                        // Guthaben
                        if (dbPlayer.guthaben[0] >= 900)
                        {
                            dbPlayer.SendNewNotification(
                                
                                "Sie haben das maximale Limit an Guthaben erreicht!");
                            return;
                        }

                        if (!dbPlayer.TakeMoney(100))
                        {
                            dbPlayer.SendNewNotification(
                                 MSG.Money.NotEnoughMoney(100));
                            return;
                        }

                        dbPlayer.guthaben[0] = dbPlayer.guthaben[0] + 100;
                        dbPlayer.SendNewNotification(
                             "Sie haben $100 Guthaben gekauft!");
                        return;
                    }

                    if (index == 2)
                    {
                        if (dbPlayer.Container.CanInventoryItemAdded(12))
                        {
                            dbPlayer.SendNewNotification( "Inventar ist voll!");
                            return;
                        }

                        if (!dbPlayer.TakeMoney(12).BuyPrice))
                        {
                            dbPlayer.SendNewNotification(
                                
                                MSG.Money.NotEnoughMoney(12).BuyPrice));
                            return;
                        }

                        dbPlayer.Container.AddItem(12), 1);
                        dbPlayer.SendNewNotification(
                             "Sie haben ein " +
                            12).Name +
                            " fuer $" + 12).BuyPrice + " gekauft!");
                        return;
                    }

                    if (index > 2)
                    {
                        List<ItemData> shopitems =
                            ItemHandler.Instance.GetItemsByMenu((int) Dialogs.menu_shop_stores);
                        foreach (ItemData kvp in shopitems)
                        {
                            if (kvp == null) continue;
                            if (idx == index - 3)
                            {
                                dbPlayer.SetData("sBuyItem", kvp.Id);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_stores);

                                player.CreateUserDialog(Dialogs.menu_shop_input, "input");
                                return;
                            }

                            idx++;
                        }
                    }

                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_stores);*/
                }
                else if (menuid == Dialogs.menu_shop_rebel_weapons)
                {
                    /*if (dbPlayer.IsInventoryMax())
                    {
                        dbPlayer.SendNewNotification( "Inventar ist voll!");
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_rebel_weapons);
                        return;
                    }

                    if (!dbPlayer.IsAGangster()) return;

                    int idx = 0;

                    if (index > 0)
                    {
                        List<ItemData> shopitems =
                            ItemHandler.Instance.GetItemsByMenu(Convert.ToInt32(Dialogs.menu_shop_rebel_weapons));
                        foreach (ItemData kvp in shopitems)
                        {
                            if (kvp == null) continue;
                            if (idx == index - 1)
                            {
                                if (dbPlayer.IsInventoryMax() ||
                                    !dbPlayer.Container.CanInventoryItemAdded(kvp))
                                {
                                    dbPlayer.SendNewNotification(
                                         "Inventar ist voll!");
                                    return;
                                }

                                int price = kvp.BuyPrice;
                                if (!dbPlayer.TakeMoney(price))
                                {
                                    dbPlayer.SendNewNotification(
                                         MSG.Money.NotEnoughMoney(price));
                                    return;
                                }

                                dbPlayer.Container.AddItem(kvp, 1);
                                dbPlayer.SendNewNotification(
                                     "Sie haben sich " + kvp.Name +
                                    " fuer $" +
                                    price + " gekauft!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_rebel_weapons);
                                return;
                            }

                            idx++;
                        }
                    }

                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_rebel_weapons);*/
                }
                else if (menuid == Dialogs.menu_garage_overlay)
                {
                    if (!dbPlayer.HasData("GarageId")) return;
                    Garage garage = GarageModule.Instance[dbPlayer.GetData("GarageId")];
                    switch (index)
                    {
                        case 1: // Getlist
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_overlay, true);
                            DialogMigrator.CreateMenu(player, Dialogs.menu_garage_getlist, "Fahrzeug-Garage", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_getlist, GlobalMessages.General.Close(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_getlist, GlobalMessages.General.Back(), "");

                            if (garage != null)
                            {
                                // Fraktionsgarage
                                if (garage.IsTeamGarage() && garage.Teams.Contains(dbPlayer.TeamId))
                                {
                                    // Exclude GWD
                                    if (dbPlayer.Team.Id == (int)TeamTypes.TEAM_ARMY && dbPlayer.TeamRank == 0) return;

                                    dbPlayer.SetData("garage_getlist", Main.getTeamGarageVehicleList(
                                        dbPlayer.TeamId, garage));

                                    // List Fahrzeuge
                                    foreach (KeyValuePair<uint, string> kvp in dbPlayer.GetData("garage_getlist"))
                                    {
                                        DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_getlist, kvp.Value,
                                            Convert.ToString(kvp.Key));
                                    }
                                }
                                else
                                {
                                    dbPlayer.SetData("garage_getlist", Main.getPlayerGarageVehicleList(
                                        dbPlayer, garage));

                                    // List Fahrzeuge
                                    foreach (KeyValuePair<uint, string> kvp in dbPlayer.GetData("garage_getlist"))
                                    {
                                        DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_getlist, kvp.Value + " - " + Convert.ToString(kvp.Key), "");
                                    }
                                }
                            }

                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_garage_getlist);
                            break;
                        case 2: // setList

                            // Verwahrplatz kein SET
                            if (garage.Type == GarageType.VehicleCollection) return;
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_overlay, true);
                            DialogMigrator.CreateMenu(player, Dialogs.menu_garage_setlist, "Fahrzeug-Garage", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_setlist, GlobalMessages.General.Close(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_setlist, GlobalMessages.General.Back(), "");

                            if (garage != null)
                            {
                                if (garage.HouseId > 0 &&
                                    (dbPlayer.OwnHouse[0] != garage.HouseId && !dbPlayer.HouseKeys.Contains(garage.HouseId) &&
                                    (!dbPlayer.IsTenant() || dbPlayer.GetTenant().HouseId != garage.HouseId))) return;

                                // Fraktionsgarage
                                if (garage.IsTeamGarage() && garage.Teams.Contains(dbPlayer.TeamId))
                                {
                                    // List Fahrzeuge
                                    foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetAllVehicles())
                                    {
                                        if (Vehicle == null) continue;

                                        if (Vehicle.teamid == dbPlayer.TeamId &&
                                            Utils.IsPointNearPoint(25.0f, player.Position,
                                                Vehicle.Entity.Position))
                                        {
                                            string l_Name = "";
                                            if (Vehicle.Data.IsModdedCar == 1)
                                                l_Name = Vehicle.Data.mod_car_name;
                                            else
                                                l_Name = Vehicle.Data.Model;
                                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_setlist,
                                                l_Name,
                                                "");
                                        }
                                    }
                                }
                                else
                                {
                                    if (garage.Spawns.Count <= 0) return;

                                    // List Fahrzeuge
                                    foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetAllVehicles())
                                    {
                                        if (Vehicle == null) continue;
                                        if (Vehicle.databaseId == 0) continue;
                                        if (dbPlayer.CanControl(Vehicle) &&
                                            Utils.IsPointNearPoint(15.0f, garage.Spawns.First().Position,
                                                Vehicle.Entity.Position))
                                        {
                                            string l_Name = "";
                                            if (Vehicle.Data.IsModdedCar == 1)
                                                l_Name = Vehicle.Data.mod_car_name;
                                            else
                                                l_Name = Vehicle.Data.Model;
                                            if (garage.Classifications.Contains(Vehicle.Data.ClassificationId))
                                            {
                                                if (!Vehicle.IsTeamVehicle())
                                                {
                                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_setlist, l_Name + " - " + Vehicle.databaseId, "");
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_garage_setlist);
                            break;
                        
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_overlay);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_garage_setlist)
                {
                    if (!dbPlayer.HasData("GarageId")) return;
                    Garage garage = GarageModule.Instance[dbPlayer.GetData("GarageId")];
                    if (garage == null) return;
                    if (index == 0) // Close
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_setlist);
                        return;
                    }
                    else if (index == 1) // Return
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_setlist, true);
                        CreateUserMenuFahrzeugGarage(player, dbPlayer, garage);
                    }
                    else
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_setlist);
                        
                        if (garage != null)
                        {
                            
                            if (!garage.IsTeamGarage())
                            {
                                if(garage.HouseId > 0 && !garage.CanVehiclePutIntoHouseGarage())
                                {
                                    dbPlayer.SendNewNotification("Hausgarage ist voll!");
                                    return;
                                }

                                if (garage.Spawns.Count <= 0) return;

                                int idx = 0;
                                foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetAllVehicles())
                                {
                                    if (Vehicle == null) continue;
                                    if (Vehicle.databaseId == 0) continue;
                                    if (Vehicle.IsTeamVehicle()) continue;
                                    if (!garage.Classifications.Contains(Vehicle.Data.ClassificationId)) continue;
                                    if (garage.Classifications.Contains(Vehicle.Data.ClassificationId) &&
                                        dbPlayer.CanControl(Vehicle) &&
                                        garage.Spawns.First().Position.DistanceTo(Vehicle.Entity.Position) <= 15.0f)
                                    {
                                            if (idx == index - 2)
                                            {
                                                Vehicle.SetPrivateCarGarage(1, garage.Id);
                                                dbPlayer.SendNewNotification("Fahrzeug wurde in die Garage geparkt!");
                                                return;
                                            }
                                        idx++;
                                    }
                                }
                                return;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("derzeit deaktiviert!");
                                return;
                            }
                        }
                        return;
                    }
                }
                else if (menuid == Dialogs.menu_garage_getlist)
                {
                    if (!dbPlayer.HasData("GarageId")) return;
                    Garage garage = GarageModule.Instance[dbPlayer.GetData("GarageId")];
                    if (garage == null) return;
                    if (index == 0) // Close
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_getlist);
                        return;
                    }
                    else if (index == 1) // Return
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_getlist, true);
                        CreateUserMenuFahrzeugGarage(player, dbPlayer, garage);
                    }
                    else
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_getlist);

                        if (garage.Type == GarageType.VehicleCollection)
                        {
                            if (!dbPlayer.TakeMoney(2500))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Um ein Fahrzeug freizukaufen benötigst du mindestens $2500 fuer eine Kaution!");
                            }

                            return;
                        }

                        if (garage.Id == 0)
                        {
                            if (!dbPlayer.TakeMoney(500))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Dein Fahrzeug wurde zerstört um es zu reparieren benötigst du mindestens 500$!");
                                return;
                            }
                        }

                        if (garage.Rang > 0 && dbPlayer.TeamRank < garage.Rang)
                        {
                            dbPlayer.SendNewNotification(
                                 "Sie haben nicht den benötigten Rang!");
                            return;
                        }

                        var spawnPos = garage.GetFreeSpawnPosition();

                        if (spawnPos == null)
                        {
                            dbPlayer.SendNewNotification(

                                "Jemand anderes hat gerade sein Fahrzeug ausgeparkt, bitte warte kurz!");
                            return;
                        }

                        if (garage != null)
                        {
                            // Fraktionsgarage
                            if (garage.IsTeamGarage() && garage.Teams.Contains(dbPlayer.TeamId))
                            {
                                int idx = index - 2;

                                NAPI.Task.Run(async () =>
                                {
                                    NetHandle xveh = await Main.LoadTeamVehicle(dbPlayer.TeamId, idx, garage, spawnPos);
                                    if (xveh != null)
                                    {
                                        dbPlayer.SendNewNotification(

                                            "Sie haben Ihr Fraktions Fahrzeug erfolgreich aus der Garage entnommen!");
                                        return;
                                    }
                                });
                                return;
                            }
                            else
                            {

                                Dictionary<uint, string> VehicleList = dbPlayer.GetData("garage_getlist");

                                int idx = 0;
                                foreach (KeyValuePair<uint, string> kvp in VehicleList)
                                {
                                    if (idx == index - 2)
                                    {
                                        SynchronizedTaskManager.Instance.Add(
                                            new GaragePlayerVehicleTakeOutTask(garage, kvp.Key, dbPlayer, spawnPos));
                                        dbPlayer.SendNewNotification(

                                            $"Ihr Fahrzeug {kvp.Value} ({kvp.Key}) wurde aus der Garage entnommen!");
                                        return;
                                    }

                                    idx++;
                                }

                                return;
                            }
                        }
                    }
                }
                else if (menuid == Dialogs.menu_shop_ammunation_main)
                {
                    switch (index)
                    {
                        case 0: // weapons

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation_main, true);
                            //Waffenshop
                            DialogMigrator.CreateMenu(player, Dialogs.menu_shop_ammunation, "Ammunation", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, GlobalMessages.General.Close(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, GlobalMessages.General.Back(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, "Pistole (12000$)", "");
                            //DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, "Pistole 50 (8000$)", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, "Schwere Pistole (15000$)", "");
                            
                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_shop_ammunation);
                            break;
                        case 1: // ammo

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation_main, true);
                            //ammo
                            DialogMigrator.CreateMenu(player, Dialogs.menu_shop_ammunation_ammo, "Ammunation", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, GlobalMessages.General.Close(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, GlobalMessages.General.Back(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, "Pistole Ammo (500$)", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, "Pistole 50 Ammo (1000$)", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, "Schwere Pistole Ammo (800$)", "");
                            
                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_shop_ammunation_ammo);
                            break;
                        case 2: // components

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation_main, true);
                            //ammo
                            DialogMigrator.CreateMenu(player, Dialogs.menu_shop_ammunation_components, "Ammunation", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_components, GlobalMessages.General.Close(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_components, GlobalMessages.General.Back(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_components, "Pistole Schalldämpfer ($5000)", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_components, "Pistole Licht ($3000)", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_components, "Schwere Pistole Schalldämpfer ($5000)", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_components, "Schwere Pistole Licht ($3000)", "");

                            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_shop_ammunation_components);
                            break;
                        case 3: // Fallschirm
                            if (!dbPlayer.Container.CanInventoryItemAdded(116, 1))
                            {
                                dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                return;
                            }

                            if (!dbPlayer.TakeMoney(1000))
                            {
                                dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                return;
                            }

                            dbPlayer.Container.AddItem(116, 1);

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation_main, true);

                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation_main);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_shop_ammunation)
                {
                    switch (index)
                    {
                        case 0:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 1:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 2:
                            {
                                if (!dbPlayer.Container.CanInventoryItemAdded(59, 1))
                                {
                                    dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(12000))
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                dbPlayer.Container.AddItem(59, 1);
                                break;
                            }
                        case 3:
                            {
                                if (!dbPlayer.Container.CanInventoryItemAdded(63, 1))
                                {
                                    dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(15000))
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                dbPlayer.Container.AddItem(63, 1);
                                break;
                            }
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_weapondealer)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealer);
                }
                else if (menuid == Dialogs.menu_shop_ammunation_ammo)
                {
                    switch (index)
                    {
                        case 0:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 1:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 2:
                            {
                                if (!dbPlayer.Container.CanInventoryItemAdded(200, 1))
                                {
                                    dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(500))
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                dbPlayer.Container.AddItem(200, 1);
                                break;
                            }
                        case 3:
                            {
                                if (!dbPlayer.Container.CanInventoryItemAdded(202, 1))
                                {
                                    dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(1000))
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                dbPlayer.Container.AddItem(202, 1);
                                break;
                            }
                        case 4:
                            {
                                if (!dbPlayer.Container.CanInventoryItemAdded(204, 1))
                                {
                                    dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(800))
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                dbPlayer.Container.AddItem(204, 1);
                                break;
                            }
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_shop_ammunation_components)
                {
                    switch (index)
                    {
                        case 0:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 1:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 2:
                            {
                                // pistol schall
                                if (!dbPlayer.Container.CanInventoryItemAdded(1201, 1))
                                {
                                    dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(5000))
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                dbPlayer.Container.AddItem(1201, 1);
                                break;
                            }
                        case 3:
                            {
                                // pistol light
                                if (!dbPlayer.Container.CanInventoryItemAdded(1221, 1))
                                {
                                    dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(3000))
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                dbPlayer.Container.AddItem(1221, 1);
                                break;
                            }
                        case 4:
                            {
                                // swpistol schall
                                if (!dbPlayer.Container.CanInventoryItemAdded(1204, 1))
                                {
                                    dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(5000))
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                dbPlayer.Container.AddItem(1204, 1);
                                break;
                            }
                        case 5:
                            {
                                // swpistol light
                                if (!dbPlayer.Container.CanInventoryItemAdded(1222, 1))
                                {
                                    dbPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(3000))
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                dbPlayer.Container.AddItem(1222, 1);
                                break;
                            }
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_taxi)
                {
                    switch (index)
                    {
                        case 0: // Taxi Lic
                            if (dbPlayer.Lic_Taxi[0] == 1)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie besitzen bereits eine Taxilizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                                break;
                            }

                            if (!dbPlayer.TakeMoney(4300))
                            {
                                dbPlayer.SendNewNotification(
                                     GlobalMessages.Money.NotEnoughMoney(4300));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                                break;
                            }

                            dbPlayer.Lic_Taxi[0] = 1;
                            dbPlayer.SendNewNotification(

                                "Sie haben eine Taxi Lizenz fuer $4300 erworben!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                            break;
                        case 1: // Aus Dienst
                            if (dbPlayer.HasData("taxi"))
                            {
                                dbPlayer.ResetData("taxi");
                                dbPlayer.SendNewNotification(

                                    "Sie haben nun den Dienst verlassen und sind fuer keine Kunden mehr sichtbar!");
                                dbPlayer.SendNewNotification(

                                    "Um erneut sichtbar fuer Kunden zu werden muessen Sie die Fahrtkosten setzen! (/taxi ($200-$999))");
                                break;
                            }
                            else
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                                break;
                            }
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_givelicenses)
                {
                    if (!dbPlayer.HasData("giveLic")) return;
                    Player xplayer = dbPlayer.GetData("giveLic");

                    DbPlayer xPlayer = xplayer.GetPlayer();
                    if (xPlayer == null || !xPlayer.IsValid()) return;

                    dbPlayer.ResetData("giveLic");
                    if (xPlayer == null) return;

                    int bonus = 0;
                    var licence = "";

                    switch (index)
                    {
                        case 0: // Fuehrerschein
                            if (xPlayer.Lic_Car[0] < 0)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_Car[0] == 1)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.License.PlayerAlreadyOwnLic(Content.License.Car));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Car))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.Money.PlayerNotEnoughMoney(Price.License.Car));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_Car[0] = 1;
                            xPlayer.SendNewNotification(
                                 GlobalMessages.License.HasGiveYouLicense(
                                    dbPlayer.GetName(),
                                    Price.License.Car, Content.License.Car));
                            xPlayer.SendNewNotification(
                                GlobalMessages.License.HaveGetLicense(Content.License.Car));
                            dbPlayer.SendNewNotification(
                                 GlobalMessages.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Car));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Car / 10) + 200;
                            dbPlayer.GiveMoney(bonus);
                            dbPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.Car;
                            break;
                        case 1: // LKW
                            if (xPlayer.Lic_LKW[0] < 0)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                dbPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_LKW[0] == 1)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.License.PlayerAlreadyOwnLic(Content.License.Lkw));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Lkw))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.Money.PlayerNotEnoughMoney(Price.License.Lkw));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_LKW[0] = 1;
                            xPlayer.SendNewNotification(
                                 GlobalMessages.License.HasGiveYouLicense(
                                    dbPlayer.GetName(),
                                    Price.License.Lkw, Content.License.Lkw));
                            xPlayer.SendNewNotification(
                                GlobalMessages.License.HaveGetLicense(Content.License.Lkw));
                            dbPlayer.SendNewNotification(
                                 GlobalMessages.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Lkw));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Lkw / 10) + 200;
                            dbPlayer.GiveMoney(bonus);
                            dbPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.Lkw;
                            break;
                        case 2: // Motorrad
                            if (xPlayer.Lic_Bike[0] < 0)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                dbPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_Bike[0] == 1)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.License.PlayerAlreadyOwnLic(Content.License.Bike));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Bike))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.Money.PlayerNotEnoughMoney(Price.License.Bike));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_Bike[0] = 1;
                            xPlayer.SendNewNotification(
                                 GlobalMessages.License.HasGiveYouLicense(
                                    dbPlayer.GetName(),
                                    Price.License.Bike, Content.License.Bike));
                            xPlayer.SendNewNotification(
                                GlobalMessages.License.HaveGetLicense(Content.License.Bike));
                            dbPlayer.SendNewNotification(
                                 GlobalMessages.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Bike));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Bike / 10) + 200;
                            dbPlayer.GiveMoney(bonus);
                            dbPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.Bike;
                            break;
                        case 3: // Bootsschein
                            if (xPlayer.Lic_Boot[0] < 0)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                dbPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_Boot[0] == 1)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.License.PlayerAlreadyOwnLic(Content.License.Boot));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Boot))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.Money.PlayerNotEnoughMoney(Price.License.Boot));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_Boot[0] = 1;
                            xPlayer.SendNewNotification(
                                 GlobalMessages.License.HasGiveYouLicense(
                                    dbPlayer.GetName(),
                                    Price.License.Boot, Content.License.Boot));
                            xPlayer.SendNewNotification(
                                GlobalMessages.License.HaveGetLicense(Content.License.Boot));
                            dbPlayer.SendNewNotification(
                                 GlobalMessages.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Boot));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Boot / 10) + 200;
                            dbPlayer.GiveMoney(bonus);
                            dbPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.Boot;
                            break;
                        case 4: // Flugschein A
                            if (xPlayer.Lic_PlaneA[0] < 0)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                dbPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_PlaneA[0] == 1)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.License.PlayerAlreadyOwnLic(Content.License.PlaneA));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.PlaneA))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.Money.PlayerNotEnoughMoney(Price.License.PlaneA));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_PlaneA[0] = 1;
                            xPlayer.SendNewNotification(
                                 GlobalMessages.License.HasGiveYouLicense(
                                    dbPlayer.GetName(),
                                    Price.License.PlaneA, Content.License.PlaneA));
                            xPlayer.SendNewNotification(
                                GlobalMessages.License.HaveGetLicense(Content.License.PlaneA));
                            dbPlayer.SendNewNotification(
                                 GlobalMessages.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.PlaneA));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.PlaneA / 10) + 200;
                            dbPlayer.GiveMoney(bonus);
                            dbPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.PlaneA;
                            break;
                        case 5: // Flugschein B
                            if (xPlayer.Lic_PlaneB[0] < 0)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                dbPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_PlaneB[0] == 1)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.License.PlayerAlreadyOwnLic(Content.License.PlaneB));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.PlaneB))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.Money.PlayerNotEnoughMoney(Price.License.PlaneB));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_PlaneB[0] = 1;
                            xPlayer.SendNewNotification(
                                 GlobalMessages.License.HasGiveYouLicense(
                                    dbPlayer.GetName(),
                                    Price.License.PlaneB, Content.License.PlaneB));
                            xPlayer.SendNewNotification(
                                GlobalMessages.License.HaveGetLicense(Content.License.PlaneB));
                            dbPlayer.SendNewNotification(
                                 GlobalMessages.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.PlaneB));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.PlaneB / 10) + 200;
                            dbPlayer.GiveMoney(bonus);
                            dbPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                            licence = Content.License.PlaneB;
                            break;
                        case 6: // Transport
                            if (xPlayer.Lic_Transfer[0] < 0)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                dbPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_Transfer[0] == 1)
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.License.PlayerAlreadyOwnLic(Content.License.Transfer));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Transfer))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.Money.PlayerNotEnoughMoney(Price.License.Transfer));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_Transfer[0] = 1;
                            xPlayer.SendNewNotification(
                                 GlobalMessages.License.HasGiveYouLicense(
                                    dbPlayer.GetName(),
                                    Price.License.Transfer, Content.License.Transfer));
                            xPlayer.SendNewNotification(
                                GlobalMessages.License.HaveGetLicense(Content.License.Transfer));
                            dbPlayer.SendNewNotification(
                                 GlobalMessages.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Transfer));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Transfer / 10) + 200;
                            dbPlayer.GiveMoney(bonus);
                            dbPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                            licence = Content.License.Transfer;
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                            break;
                    }

                    if (licence != "")
                    {
                        TeamModule.Instance.SendMessageToTeam($"{dbPlayer.GetName()} hat einen {licence} an {xPlayer.GetName()} vergeben", TeamTypes.TEAM_DRIVINGSCHOOL);
                    }

                    return;
                }
                else if (menuid == Dialogs.menu_job_createlicenses)
                {
                    if (dbPlayer.job[0] != (int)JobTypes.JOB_PLAGIAT)
                    {
                        return;
                    }

                    switch (index)
                    {
                        case 0: // Fuehrerschein
                            if (dbPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Car)
                            {
                                if (dbPlayer.JobSkill[0] < JobContent.Plagiat.Requiredskill.Car)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        GlobalMessages.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Car));
                                    return;
                                }

                                ItemModel itemModel = ItemModelModule.Instance.Get(1450);
                                if (itemModel == null) return;

                                if(!dbPlayer.Container.CanInventoryItemAdded(itemModel.Id))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                    return;
                                }

                                dbPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Car);
                                dbPlayer.JobSkillsIncrease();
                                dbPlayer.Container.AddItem(itemModel.Id);
                               
                                dbPlayer.SendNewNotification($"Du hast einen {itemModel.Name} hergestellt!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Car + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 1: // LKW
                            if (dbPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Lkw)
                            {
                                if (dbPlayer.JobSkill[0] < JobContent.Plagiat.Requiredskill.Lkw)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        GlobalMessages.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Lkw));
                                    return;
                                }

                                ItemModel itemModel = ItemModelModule.Instance.Get(1452);
                                if (itemModel == null) return;

                                if (!dbPlayer.Container.CanInventoryItemAdded(itemModel.Id))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                    return;
                                }

                                dbPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Lkw);
                                dbPlayer.JobSkillsIncrease();
                                dbPlayer.Container.AddItem(itemModel.Id);

                                dbPlayer.SendNewNotification($"Du hast einen {itemModel.Name} hergestellt!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Lkw + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 2: // Motorrad
                            if (dbPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Bike)
                            {
                                if (dbPlayer.JobSkill[0] < JobContent.Plagiat.Requiredskill.Bike)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        GlobalMessages.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Bike));
                                    return;
                                }

                                ItemModel itemModel = ItemModelModule.Instance.Get(1455);
                                if (itemModel == null) return;

                                if (!dbPlayer.Container.CanInventoryItemAdded(itemModel.Id))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                    return;
                                }

                                dbPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Bike);
                                dbPlayer.JobSkillsIncrease();
                                dbPlayer.Container.AddItem(itemModel.Id);

                                dbPlayer.SendNewNotification($"Du hast einen {itemModel.Name} hergestellt!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Bike + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 3: // Boot
                            if (dbPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Boot)
                            {
                                if (dbPlayer.JobSkill[0] < JobContent.Plagiat.Requiredskill.Boot)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        GlobalMessages.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Boot));
                                    return;
                                }

                                ItemModel itemModel = ItemModelModule.Instance.Get(1454);
                                if (itemModel == null) return;

                                if (!dbPlayer.Container.CanInventoryItemAdded(itemModel.Id))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                    return;
                                }

                                dbPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Boot);
                                dbPlayer.JobSkillsIncrease();
                                dbPlayer.Container.AddItem(itemModel.Id);

                                dbPlayer.SendNewNotification($"Du hast einen {itemModel.Name} hergestellt!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Boot + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 4: // PlaneA
                            if (dbPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.PlaneA)
                            {
                                if (dbPlayer.JobSkill[0] < JobContent.Plagiat.Requiredskill.PlaneA)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        GlobalMessages.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.PlaneA));
                                    return;
                                }

                                ItemModel itemModel = ItemModelModule.Instance.Get(1451);
                                if (itemModel == null) return;

                                if (!dbPlayer.Container.CanInventoryItemAdded(itemModel.Id))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                    return;
                                }

                                dbPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.PlaneA);
                                dbPlayer.JobSkillsIncrease();
                                dbPlayer.Container.AddItem(itemModel.Id);

                                dbPlayer.SendNewNotification($"Du hast einen {itemModel.Name} hergestellt!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.PlaneA + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 5: // PlaneB
                            if (dbPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.PlaneB)
                            {
                                if (dbPlayer.JobSkill[0] < JobContent.Plagiat.Requiredskill.PlaneB)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        GlobalMessages.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.PlaneB));
                                    return;
                                }

                                ItemModel itemModel = ItemModelModule.Instance.Get(1453);
                                if (itemModel == null) return;

                                if (!dbPlayer.Container.CanInventoryItemAdded(itemModel.Id))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                    return;
                                }

                                dbPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.PlaneB);
                                dbPlayer.JobSkillsIncrease();
                                dbPlayer.Container.AddItem(itemModel.Id);

                                dbPlayer.SendNewNotification($"Du hast einen {itemModel.Name} hergestellt!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.PlaneB + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 6: // Gun
                            if (dbPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Gun)
                            {
                                if (dbPlayer.JobSkill[0] < JobContent.Plagiat.Requiredskill.Gun)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        GlobalMessages.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Gun));
                                    return;
                                }

                                ItemModel itemModel = ItemModelModule.Instance.Get(1461);
                                if (itemModel == null) return;

                                if (!dbPlayer.Container.CanInventoryItemAdded(itemModel.Id))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                    return;
                                }

                                dbPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Gun);
                                dbPlayer.JobSkillsIncrease();
                                dbPlayer.Container.AddItem(itemModel.Id);

                                dbPlayer.SendNewNotification($"Du hast einen {itemModel.Name} hergestellt!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Gun + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 7: // Transfer
                            if (dbPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Transfer)
                            {
                                if (dbPlayer.JobSkill[0] < JobContent.Plagiat.Requiredskill.Transfer)
                                {
                                    dbPlayer.SendNewNotification(
                                        
                                        GlobalMessages.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Transfer));
                                    return;
                                }

                                ItemModel itemModel = ItemModelModule.Instance.Get(1456);
                                if (itemModel == null) return;

                                if (!dbPlayer.Container.CanInventoryItemAdded(itemModel.Id))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                    return;
                                }

                                dbPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Transfer);
                                dbPlayer.JobSkillsIncrease();
                                dbPlayer.Container.AddItem(itemModel.Id);

                                dbPlayer.SendNewNotification($"Du hast einen {itemModel.Name} hergestellt!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Transfer + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 8: // Hunting
                            if (dbPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Hunting) {
                                if (dbPlayer.JobSkill[0] < JobContent.Plagiat.Requiredskill.Hunting) {
                                    dbPlayer.SendNewNotification(

                                        GlobalMessages.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Hunting));
                                    return;
                                }

                                ItemModel itemModel = ItemModelModule.Instance.Get(1462);
                                if (itemModel == null) return;

                                if (!dbPlayer.Container.CanInventoryItemAdded(itemModel.Id))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                    return;
                                }

                                dbPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Hunting);
                                dbPlayer.JobSkillsIncrease();
                                dbPlayer.Container.AddItem(itemModel.Id);

                                dbPlayer.SendNewNotification($"Du hast einen {itemModel.Name} hergestellt!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            } else {
                                dbPlayer.SendNewNotification(

                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Hunting + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_takelic)
                {
                    if (!dbPlayer.HasData("takeLic")) return;
                    DbPlayer xPlayer = dbPlayer.GetData("takeLic");

                    dbPlayer.ResetData("takeLic");

                    if (xPlayer == null || xPlayer.Player.Position.DistanceTo(player.Position) > 5.0f)
                    {
                        dbPlayer.SendNewNotification(
                            
                            "Spieler muss in Ihrer Naehe sein");
                        return;
                    }

                    // Fuehrerscheinsperre
                    int sperre = -720;
                    switch (index)
                    {
                        case 0: // Fuehrerschein
                            xPlayer.Lic_Car[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Car + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Car + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 1: // LKW
                            xPlayer.Lic_LKW[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Lkw + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Lkw + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 2: // Motorrad
                            xPlayer.Lic_Bike[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Bike + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Bike + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 3: // Boot
                            xPlayer.Lic_Boot[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Boot + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Boot + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 4: // PlaneA
                            xPlayer.Lic_PlaneA[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.PlaneA + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.PlaneA + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 5: // PlaneB
                            xPlayer.Lic_PlaneB[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.PlaneB + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.PlaneB + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 6: // Biz
                            xPlayer.Lic_Biz[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Biz + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Biz + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 7: // Gun
                            xPlayer.Lic_Gun[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Gun + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Gun + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 8: // Hunting
                            xPlayer.Lic_Hunting[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Hunting + " entzogen!");
                            xPlayer.SendNewNotification(
                                "Ein Beamter hat Ihnen den " +
                                Content.License.Hunting + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 9: // Transfer
                            xPlayer.Lic_Transfer[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(dbPlayer,
                                dbPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Transfer + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Transfer + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                    }

                    xPlayer.Save();
                }
                else if (menuid == Dialogs.menu_bizacceptinvite)
                {
                    if (!dbPlayer.HasData("bizinv"))
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_bizacceptinvite);
                        return;
                    }

                    uint bizid = dbPlayer.GetData("bizinv");
                    switch (index)
                    {
                        case 0: // Accept
                            dbPlayer.SendNewNotification("Einladung angenommen!");

                            dbPlayer.AddBusinessMembership(BusinessModule.Instance.GetById(bizid));

                            dbPlayer.GetActiveBusiness()?.SendMessageToMembers(
                                $"{dbPlayer.GetName()} ist {dbPlayer.GetActiveBusiness()?.Name} beigetreten!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_bizacceptinvite);
                            break;
                        case 1: // Cancel
                            dbPlayer.SendNewNotification("Einladung abgelehnt!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_bizacceptinvite);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_bizacceptinvite);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_invited)
                {
                    uint teamInviteId;
                    switch (index)
                    {
                        case 0:
                            if (dbPlayer.TryData("teamInvite", out teamInviteId))
                            {
                                var team = TeamModule.Instance[teamInviteId];
                                if (team != null)
                                {
                                    dbPlayer.SetTeamRankPermission(false, 0, false, "");
                                    dbPlayer.SetTeam(teamInviteId);
                                    dbPlayer.TeamRank = 0;
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                                    dbPlayer.SendNewNotification(
                                        "Sie wurden erfolgreich in die Fraktion eingeladen!", title: $"{dbPlayer.Team.Name}", notificationType: PlayerNotification.NotificationType.FRAKTION);

                                    if (dbPlayer.TeamId != 0)
                                    {
                                        dbPlayer.Team.AddMember(dbPlayer);
                                    }

                                    dbPlayer.ResetData("teamInvite");
                                    dbPlayer.ResetData("teamInvite");

                                    PlayerSpawn.OnPlayerSpawn(player);
                                }

                                break;
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                            break;
                        case 1:
                            if (dbPlayer.TryData("teamInvite", out teamInviteId))
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                                dbPlayer.SendNewNotification(
                                    "Sie haben den Fraktions-Invite abgelehnt!");
                                dbPlayer.ResetData("teamInvite");
                                PlayerSpawn.OnPlayerSpawn(player);
                                break;
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                            break;
                        default:
                            if (dbPlayer.HasData("teamInvite"))
                            {
                                dbPlayer.ResetData("teamInvite");
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_show_wanteds)
                {
                    if(index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_show_wanteds);
                        return;
                    }
                    else
                    {
                        if (dbPlayer.TeamId != (int)TeamTypes.TEAM_FIB || !dbPlayer.IsInDuty())
                        {
                            dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                            return;
                        }
                        int idx = 1;
                        foreach (DbPlayer xPlayer in Players.Instance.GetValidPlayers())
                        {
                            if (CrimeModule.Instance.CalcJailTime(xPlayer.Crimes) > 0)
                            {
                                if (index == idx)
                                {
                                    if (xPlayer.IsOrtable(dbPlayer))
                                    {
                                        NSAModule.Instance.HandleFind(dbPlayer, xPlayer);

                                        xPlayer.SetData("isOrted_" + dbPlayer.TeamId, DateTime.Now.AddMinutes(1));

                                        dbPlayer.SendNewNotification("Gesuchte Person " + xPlayer.GetName() + " wurde geortet!");
                                        dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat die Person {xPlayer.GetName()} geortet!", 5000, 10);

                                        if (dbPlayer.IsNSADuty || dbPlayer.FindFlags.HasFlag(FindFlags.Continuous))
                                        {
                                            dbPlayer.SetData("nsaOrtung", xPlayer.Id);
                                        }

                                        Logger.AddFindLog(dbPlayer.Id, xPlayer.Id);
                                        return;
                                    }
                                    else
                                    {
                                        dbPlayer.SendNewNotification("Smartphone konnte nicht geortet werden... ");
                                        return;
                                    }
                                }
                                idx++;
                            }
                        }
                    }
                    return;
                }
                else if (menuid == Dialogs.menu_academic)
                {
                    switch (index)
                    {
                        case 0:

                            if (dbPlayer.uni_points[0] == 0)
                            {
                                dbPlayer.SendNewNotification(
                                     "Keine Academie Punkte verfuegbar!");
                                return;
                            }

                            if (dbPlayer.uni_business[0] >= 10)
                            {
                                dbPlayer.SendNewNotification(
                                     "Maximales Academiclevel erreicht!");
                                return;
                            }

                            dbPlayer.uni_points[0] = dbPlayer.uni_points[0] - 1;
                            dbPlayer.uni_business[0] = dbPlayer.uni_business[0] + 1;

                            dbPlayer.SendNewNotification("Sie sind nun Geschaeftsmann Stufe " +
                                dbPlayer.uni_business[0] + ". Preisnachlass: " +
                                (dbPlayer.uni_business[0] * 2) + "%");
                            dbPlayer.SendNewNotification(
                                "Sie sind nun Geschaeftsmann Stufe " + dbPlayer.uni_business[0], title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            break;
                        case 1:

                            if (dbPlayer.uni_points[0] == 0)
                            {
                                dbPlayer.SendNewNotification(
                                     "Keine Academie Punkte verfuegbar!");
                                return;
                            }

                            if (dbPlayer.uni_economy[0] >= 10)
                            {
                                dbPlayer.SendNewNotification(
                                     "Maximales Academiclevel erreicht!");
                                return;
                            }

                            dbPlayer.uni_points[0] = dbPlayer.uni_points[0] - 1;
                            dbPlayer.uni_economy[0] = dbPlayer.uni_economy[0] + 1;

                            dbPlayer.SendNewNotification( "Sie sind nun Sparfuchs Stufe " +
                                dbPlayer.uni_economy[0] + ". Sparrate: " + (dbPlayer.uni_economy[0] * 2) +
                                "%");
                            dbPlayer.SendNewNotification(
                                "Sie sind nun Sparfuchs Stufe " + dbPlayer.uni_economy[0], title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            break;
                        case 2:

                            if (dbPlayer.uni_points[0] == 0)
                            {
                                dbPlayer.SendNewNotification(
                                     "Keine Academie Punkte verfuegbar!");
                                return;
                            }

                            if (dbPlayer.uni_workaholic[0] >= 10)
                            {
                                dbPlayer.SendNewNotification(
                                     "Maximales Academiclevel erreicht!");
                                return;
                            }

                            dbPlayer.uni_points[0] = dbPlayer.uni_points[0] - 1;
                            dbPlayer.uni_workaholic[0] = dbPlayer.uni_workaholic[0] + 1;

                            dbPlayer.SendNewNotification("Sie sind nun Workaholic Stufe " +
                                dbPlayer.uni_workaholic[0] + ". Job Erfahrung: +" +
                                (dbPlayer.uni_workaholic[0] * 2) + "%");
                            dbPlayer.SendNewNotification(
                                "Sie sind nun Workaholic Stufe " + dbPlayer.uni_workaholic[0], title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            break;
                        case 3:

                            if (dbPlayer.Level == 0) return;

                            int academicpoints = (dbPlayer.Level - 1);

                            if (!dbPlayer.TakeMoney(5000 * academicpoints))
                            {
                                dbPlayer.SendNewNotification(
                                    
                                    GlobalMessages.Money.NotEnoughMoney(5000 * academicpoints));
                                return;
                            }

                            dbPlayer.uni_economy[0] = 0;
                            dbPlayer.uni_business[0] = 0;
                            dbPlayer.uni_workaholic[0] = 0;
                            dbPlayer.SendNewNotification(
                                "Sie haben Ihre academic Punkte erfolgreich resettet! (" + academicpoints +
                                " verfuegbar)");
                            dbPlayer.uni_points[0] = academicpoints;
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_academic);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_taxilist)
                {
                    // close
                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxilist);
                        return;
                    }
                    else
                    {
                        int idx = 0;
                        for (int ix = 0; ix < Users.Count; ix++)
                        {
                            if (!Users[ix].IsValid()) continue;
                            if (Users[ix].HasData("taxi") &&
                                Users[ix].Lic_Taxi[0] == 1)
                            {
                                if (idx == index - 1)
                                {
                                    string ort = dbPlayer.GetData("taxi_ort");
                                    // taxifahrer gefunden yay
                                    Users[ix].SendNewNotification(
                                         "Sie haben eine Taxianfrage von " +
                                        dbPlayer.GetName() + " (" + dbPlayer.ForumId + ") Ort: " + ort +
                                        ", benutzen Sie die TaxiAPP um diese anzunehmen!");
                                    dbPlayer.SendNewNotification(

                                        "Anfrage an den Taxifahrer wurde gestellt!");
                                    dbPlayer.SetData("taxi_request", Users[ix].GetName());
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxilist);
                                    return;
                                }

                                idx++;
                            }
                        }
                    }
                }
                else if (menuid == Dialogs.menu_servicelist)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_servicelist);
                }
                else if (menuid == Dialogs.menu_plain)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_plain);
                }
                else if (menuid == Dialogs.menu_pd_su)
                {
                    switch (index)
                    {
                        case 0: // Waffenschein
                            int price = Price.License.Gun;
                            string Lic = Content.License.Gun;
                            if (dbPlayer.Lic_Gun[0] == 0)
                            {
                                if (dbPlayer.IsHomeless())
                                {
                                    dbPlayer.SendNewNotification("Ohne einen Wohnsitz können Sie keinen Waffenschein erwerben!");
                                    return;
                                }
                                
                                if (!dbPlayer.TakeMoney(price))
                                {
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.Money.NotEnoughMoney(price));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                                    return;
                                }

                                dbPlayer.Lic_Gun[0] = 1;
                                dbPlayer.SendNewNotification(GlobalMessages.License.HaveGetLicense(Lic), title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                                KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, price);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                                return;
                            }

                            dbPlayer.SendNewNotification(
                                 GlobalMessages.License.AlreadyOwnLic(Lic));
                            break;
                        case 1: // Waffenschein
                            int h_price = Price.License.Hunting;
                            string Lic_Hunting = Content.License.Hunting;
                            if (dbPlayer.Lic_Hunting[0] == 0) {
                                if (dbPlayer.IsHomeless()) {
                                    dbPlayer.SendNewNotification("Ohne einen Wohnsitz können Sie keinen " + Lic_Hunting + " erwerben!");
                                    return;
                                }

                                if (!dbPlayer.TakeMoney(h_price)) {
                                    dbPlayer.SendNewNotification(
                                        GlobalMessages.Money.NotEnoughMoney(h_price));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                                    return;
                                }

                                dbPlayer.Lic_Hunting[0] = 1;
                                dbPlayer.SendNewNotification(GlobalMessages.License.HaveGetLicense(Lic_Hunting), title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                                KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, h_price);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                                return;
                            }

                            dbPlayer.SendNewNotification(
                                GlobalMessages.License.AlreadyOwnLic(Lic_Hunting));
                            break;
                        case 2: // Ticket

                            int l_Price = 0;

                            if (dbPlayer.Crimes.Count > 0)
                            {
                                if (CrimeModule.Instance.CalcWantedStars(dbPlayer.Crimes) == 0 && CrimeModule.Instance.CalcJailCosts(dbPlayer.Crimes, dbPlayer.EconomyIndex) > 0)
                                {
                                    l_Price = CrimeModule.Instance.CalcJailCosts(dbPlayer.Crimes, dbPlayer.EconomyIndex);

                                    Task.Run(async () =>
                                    {

                                        PaymentStatus pStatus = await PaymentModule.Instance.AskForPayment(dbPlayer, l_Price);

                                        if (pStatus == PaymentStatus.Wallet)
                                        {
                                            if (!dbPlayer.TakeMoney(l_Price))
                                            {
                                                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(l_Price));
                                                return;
                                            }
                                        }
                                        else if (pStatus == PaymentStatus.Bank)
                                        {
                                            if (!dbPlayer.TakeBankMoney(l_Price, "Ihre Zahlung im LSPD ($" + l_Price + ")"))
                                            {
                                                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(l_Price));
                                                return;
                                            }
                                        }
                                        else return;

                                        KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, l_Price);

                                        dbPlayer.RemoveAllCrimes();
                                        dbPlayer.SendNewNotification(
                                        "Sie haben ihr Ticket bezahlt, Ihre Delikte sind erloschen.");
                                    });
                                }
                            }
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                            break;
                    }
                }

                if (menuid == Dialogs.menu_ressourcemap)
                {
                    if (index == 0)
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_ressourcemap);

                    int idx = 1;

                    foreach (var xFarm in FarmSpotModule.Instance.GetAll())
                    {
                        if (xFarm.Value.RessourceName != "")
                        {
                            if (idx == index)
                            {
                                dbPlayer.Player.TriggerNewClient("setPlayerGpsMarker", xFarm.Value.Positions[0].X, xFarm.Value.Positions[0].Y);
                                dbPlayer.SendNewNotification(
                                    "GPS fuer Ressource: " + xFarm.Value.RessourceName +
                                    " wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_ressourcemap);
                                return;
                            }

                            idx++;
                        }
                    }

                    foreach (var farmProcess in FarmProcessModule.Instance.GetAll())
                    {
                        if (farmProcess.Value.ProcessName != "")
                        {
                            if (idx == index)
                            {
                                dbPlayer.Player.TriggerNewClient("setPlayerGpsMarker", farmProcess.Value.NpcPosition.X, farmProcess.Value.NpcPosition.Y);
                                dbPlayer.SendNewNotification(
                                    "GPS fuer " + farmProcess.Value.ProcessName + " wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_ressourcemap);
                                return;
                            }

                            idx++;
                        }
                    }
                }
                else if (menuid == Dialogs.menu_weapondealergps)
                {
                    switch (index)
                    {
                        case 0: // 
                            dbPlayer.SetWaypoint(582.3424f, -2723.283f);
                            dbPlayer.SendNewNotification("GPS fuer Fabrik 1 wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealergps);
                            break;
                        case 1: // 
                            dbPlayer.SetWaypoint(32.56189f, -627.6917f);
                            dbPlayer.SendNewNotification("GPS fuer Fabrik 2 wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealergps);
                            break;
                        case 2: // 
                            dbPlayer.SetWaypoint(2709.886f, 4316.729f);
                            dbPlayer.SendNewNotification("GPS fuer Fabrik 3 wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealergps);
                            break;
                        case 3: // 
                            dbPlayer.SetWaypoint(-121.5611f, 6204.626f);
                            dbPlayer.SendNewNotification("GPS fuer Fabrik 4 wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealergps);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_findrob)
                {
                    int interval = 0;
                    foreach (Rob rob in RobberyModule.Instance.GetActiveRobs(true))
                    {
                        if (index == interval)
                        {
                            Zone zone = ZoneModule.Instance.GetZone(rob.Player.Player.Position);
                            dbPlayer.SendNewNotification($"Der Raubüberfall wurde in {zone.Name} gemeldet!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_findrob);
                            return;
                        }

                        interval++;
                    }
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_findrob);
                }
                else if (menuid == Dialogs.menu_dealerhint)
                {
                    int interval = 0;
                    foreach (Dealer dealer in DealerModule.Instance.GetAll().Values.Where(dealer => dealer.Alert == true))
                    {
                        if ((index-1) == interval)
                        {
                            Vector3 position = dealer.Position;
                            position = Utils.GenerateRandomPosition(position, 400);
                            string message = "";
                            dbPlayer.SendNewNotification("Der ungefähre Standort des Drogendeals wurde im GPS markiert!", duration: 10000);
                            dbPlayer.SetWaypoint(position.X, position.Y);
                            dbPlayer.Team.SendNotification($"Der Dealertipp wurde dem Agenten {dbPlayer.GetName()} mitgeteilt.", 10000, 3);
                            if (dealer.LastSeller != null)
                            {
                                bool found = false;
                                if(dealer.LastSeller.IsFemale())
                                {
                                    message = "Die Verkäuferin trug folgende Kleidung: ";
                                }
                                else
                                {
                                    message = "Der Verkäufer trug folgende Kleidung: ";
                                }
                                foreach (uint clothId in dealer.LastSeller.Character.Clothes.Values)
                                {
                                    if (ClothModule.Instance.Get(clothId).Name == "Leer" || ClothModule.Instance.Get(clothId).Slot == 3)
                                        continue;
                                    if (Utils.RandomNumber(1, 8) <= 4)
                                    {
                                        message += ClothModule.Instance.Get(clothId).Name + ", ";
                                        found = true;
                                    }
                                }
                                if (found)
                                {
                                    message = message.Substring(0, message.Length - 2);
                                    dbPlayer.SendNewNotification(message, duration: 30000);
                                }
                                else
                                {
                                    dbPlayer.SendNewNotification("Leider gibt es keine Beschreibung der Personen.");
                                }
                            }
                            dealer.Alert = false;
                            string messageToDB = "FindDealer: " + message;
                            MySQLHandler.ExecuteAsync($"INSERT INTO `log_bucket` (`player_id`, `message`) VALUES ('{dbPlayer.Id}', '{messageToDB}');");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_dealerhint);
                            return;
                        }
                        interval++;
                    }
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_dealerhint);
                }
                else if (menuid == Dialogs.menu_quitjob)
                {
                    switch (index)
                    {
                        case 0: // job quit

                            dbPlayer.SendNewNotification(

                                "Sie haben Ihren Job erfolgreich gekuendigt!");
                            dbPlayer.job[0] = 0;
                            dbPlayer.JobSkill[0] = 0;
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_quitjob);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_quitjob);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_jobaccept)
                {
                    switch (index)
                    {
                        case 0: // job annehmen
                            Job xJob;
                            if ((xJob = JobsModule.Instance.GetThisJob(player.Position)) != null)
                            {
                                dbPlayer.job[0] = xJob.Id;
                                dbPlayer.SetPlayerCurrentJobSkill();
                                dbPlayer.SendNewNotification(
                                    "Sie sind nun " + xJob.Name + ", /help fuer weitere Hilfe.", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_jobaccept);
                                break;
                            }

                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_jobaccept);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_mdc)
                {
                    if (!dbPlayer.HasData("db")) return;
                    DbPlayer xPlayer = dbPlayer.GetData("db");
                    dbPlayer.ResetData("db");
                    if (xPlayer == null) return;

                    switch (index)
                    {
                        case 0: // Player Infos
                            dbPlayer.SendNewNotification(
                                 "Person " + xPlayer.GetName() + " | Level " +
                                xPlayer.Level);
                            if (xPlayer.Wanteds[0] > 0)
                                dbPlayer.SendNewNotification(
                                     "Gesucht mit " + xPlayer.Wanteds[0]);
                            dbPlayer.SendNewNotification(
                                 "Wanteds " + xPlayer.Wanteds[0]);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            break;
                        case 1: // Car Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Car[0] == 1)
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        case 2: // LKW Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_LKW[0] == 1)
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        case 3: // Bike Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Bike[0] == 1)
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        case 4: // FlyA Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_PlaneA[0] == 1)
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        case 5: // FlyB Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_PlaneB[0] == 1)
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        case 6: // Boot Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Boot[0] == 1)
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        case 7: // Gun Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Gun[0] == 1)
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        case 8: // Biz Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Biz[0] == 1)
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        case 9: // Transfer
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Transfer[0] == 1)
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        case 10: // Hunting
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Hunting[0] == 1) {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(true));
                            } else {
                                dbPlayer.SendNewNotification(GlobalMessages.MDC_LicCheck(false));
                            }

                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_help)
                {
                    switch (index)
                    {
                        case 0: // Allgemeine Hilfe
                            dbPlayer.SendNewNotification("Allgemeine Befehle: /support /report /tognews", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                            dbPlayer.SendNewNotification("Allgemeine Befehle: /dropguns /spende /ooc (Out of Character)", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 1: // Fraktions Hilfe

                            if (dbPlayer.TeamId == (int)TeamTypes.TEAM_CIVILIAN)
                            {
                                dbPlayer.SendNewNotification( GlobalMessages.Error.NoTeam());
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                                break;
                            }

                            switch (dbPlayer.TeamId)
                            {
                                case (int)TeamTypes.TEAM_POLICE:
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.HelpPolice());
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.HelpPolice2());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_ARMY:
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.HelpPolice());
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.HelpPolice2());
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpArmy());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_DPOS:
                                    dbPlayer.SendNewNotification(

                                        "/m(egaphone) /r(adio) /d(epartment)");
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_GOV:
                                    dbPlayer.SendNewNotification("/gov");
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_FIB:
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.HelpPolice());
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.HelpPolice2());
                                    dbPlayer.SendNewNotification(
                                        "FIB Befehle: /find (Person) R3+: /fakename Neuer_Name /resetfakename R5+ /findhouse (Hausnummer)");
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_BALLAS:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_LOST:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_NEWS:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpNews());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_DRIVINGSCHOOL:
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.HelpDrivingSchool());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_MEDIC:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpMedic());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_IRISHMOB:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_LCN:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_YAKUZA:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_HUSTLER:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_BRATWA:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_NNM:
                                    dbPlayer.Player.SendNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.Player.SendNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_GROVE:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                case (int)TeamTypes.TEAM_ICA:
                                    dbPlayer.SendNewNotification(GlobalMessages.HelpGang());
                                    if (dbPlayer.TeamRankPermission.Manage >= 1)
                                        dbPlayer.SendNewNotification(
                                             GlobalMessages.HelpLeader());
                                    break;
                                default:
                                    dbPlayer.SendNewNotification(
                                         GlobalMessages.Error.NoTeam());
                                    break;
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 2: // Job Hilfe
                            if (dbPlayer.job[0] > 0)
                            {
                                Job xJob;
                                xJob = JobsModule.Instance.GetJob(dbPlayer.job[0]);
                                if (xJob == null) return;
                                dbPlayer.SendNewNotification(
                                     xJob.Name + " Befehle: " + xJob.Helps);
                            }
                            else
                            {
                                dbPlayer.SendNewNotification( GlobalMessages.Error.NoJob());
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 3: // Haus Hilfe
                            dbPlayer.SendNewNotification("Haus Befehle: /buyinterior (Interior kaufen)");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 4: // Fahrzeug Hilfe
                            dbPlayer.SendNewNotification("Fahrzeug Befehle: /grab [Name]");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 5: // Inventar Hilfe
                            dbPlayer.SendNewNotification("Inventar Befehle: /dropguns");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 6:
                            /*// Biz Hilfe
                            dbPlayer.SendNewNotification(

                                MSG.HelpBusiness());
                                */
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 7: // Admin Hilfe
                            if (dbPlayer.RankId > 0)
                            {
                                var commands = "";
                                int curr = 0;
                                foreach (var command in dbPlayer.Rank.Commands)
                                {
                                    commands += " /" + command + " ";
                                    if (curr >= 5)
                                    {
                                        dbPlayer.SendNewNotification(commands);
                                        commands = "";
                                        curr = 0;
                                    }

                                    curr++;
                                }

                                if (!string.IsNullOrEmpty(commands))
                                {
                                    dbPlayer.SendNewNotification(commands);
                                }
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_shop_interior)
                {
                    switch (index)
                    {
                        case 0: //Schließen
                            House xHouse = HouseModule.Instance[dbPlayer.OwnHouse[0]];
                            if (xHouse == null) return;
                            player.SetPosition(xHouse.Position);
                            dbPlayer.SetDimension(0);
                            dbPlayer.DimensionType[0] = DimensionType.World;
                            dbPlayer.ResetData("tempInt");
                            dbPlayer.ResetData("lastPosition");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_interior);
                            break;

                        case 1: //Kaufen
                            if (dbPlayer.HasData("tempInt"))
                            {
                                uint i = dbPlayer.GetData("tempInt");
                                if (i > 0)
                                {
                                    var interior = InteriorModule.Instance[i];
                                    if (interior == null) return;
                                    int price = interior.Price;
                                    if (!dbPlayer.TakeMoney(price))
                                    {
                                        dbPlayer.SendNewNotification(
                                            
                                            GlobalMessages.Money.NotEnoughMoney(price));
                                        return;
                                    }

                                    dbPlayer.SendNewNotification(
                                         "Interior gekauft $" + price);
                                    House mHouse = HouseModule.Instance[dbPlayer.OwnHouse[0]];

                                    mHouse.Interior = interior;
                                    player.SetPosition(mHouse.Position);
                                    dbPlayer.SetDimension(0);
                                    dbPlayer.DimensionType[0] = DimensionType.World;
                                    dbPlayer.ResetData("tempInt");
                                    dbPlayer.ResetData("lastPosition");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_interior);
                                    mHouse.SaveInterior();
                                }

                                return;
                            }

                            break;
                        default:
                            int idx = 0;
                            House iHouse;
                            if ((iHouse = HouseModule.Instance[dbPlayer.OwnHouse[0]]) != null)
                            {
                                foreach (KeyValuePair<uint, Interior> kvp in InteriorModule.Instance.GetAll())
                                {
                                    if (kvp.Value.Type == iHouse.Type)
                                    {
                                        if (index - 2 == idx)
                                        {
                                            dbPlayer.SetData("lastPosition", dbPlayer.Player.Position);
                                            player.SetPosition(kvp.Value.Position);
                                            dbPlayer.DimensionType[0] = DimensionType.House_Shop_Interior;
                                            dbPlayer.SetData("tempInt", kvp.Key);
                                            break;
                                        }
                                        else
                                        {
                                            idx++;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_carshop)
                {
                    VehicleShop cShop = VehicleShopModule.Instance.GetThisShop(player.Position);
                    if (cShop == null)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_carshop);
                        return;
                    }
                    if (dbPlayer == null || !dbPlayer.IsValid()) return;
                    int idx = 0;
                    List<ShopVehicle> VehsOfShop = VehicleShopModule.Instance.GetVehsFromCarShop(cShop.Id);
                    foreach (ShopVehicle Vehicle in VehsOfShop)
                    {
                        if (idx == index)
                        {
                            var price = Vehicle.Price;
                            var discount = 0;

                            
                            
                            if (price < 0) return;

                            if(!Vehicle.CanPurchased())
                            {
                                dbPlayer.SendNewNotification("Dieses Fahrzeug ist limitiert und derzeit nicht erhältlich!");
                                return;
                            }

                            if(!cShop.TeamCarShop)
                            {
                                int couponPercent = 0;
                                uint whatCoupon = 0;

                                if(cShop.Id == 1001 && Vehicle.IsSpecialCar)
                                {
                                    if(cShop.PlayerIds.Contains((int)dbPlayer.Id))
                                    {
                                        dbPlayer.SendNewNotification("Du hast bei mir bereits ein Fahrzeug gekauft.");
                                        return;
                                    }
                                }

                                if (!Vehicle.IsSpecialCar)
                                {
                                    foreach (KeyValuePair<int, Item> kvp in dbPlayer.Container.Slots)
                                    {
                                        if (kvp.Value.Model == null) continue;
                                        if (kvp.Value.Model.Script == null) continue;
                                        if (kvp.Value.Model.Script.Contains("discount_car_"))
                                        {
                                            try
                                            {
                                                couponPercent = Int32.Parse(kvp.Value.Model.Script.Replace("discount_car_", ""));
                                                double temp = couponPercent / 100.0d;
                                                price -= (int)(price * temp);
                                                whatCoupon = kvp.Value.Id;
                                            }
                                            catch (Exception)
                                            {
                                                dbPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");
                                                return;
                                            }
                                            break;
                                        }
                                    }

                                    if (dbPlayer.uni_business[0] > 0 && couponPercent == 0)
                                    {
                                        discount = 100 - (dbPlayer.uni_business[0] * 2);
                                        price = Convert.ToInt32((discount / 100.0) * price);
                                    }
                                }

                                Task.Run(async () =>
                                {
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_carshop);
                                    PaymentStatus pStatus = await PaymentModule.Instance.AskForPayment(dbPlayer, price);

                                    if (pStatus == PaymentStatus.Wallet)
                                    {
                                        if (!dbPlayer.TakeMoney(price))
                                        {
                                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                                            return;
                                        }
                                    }
                                    else if (pStatus == PaymentStatus.Bank)
                                    {
                                        if (!dbPlayer.TakeBankMoney(price, "Ihre Zahlung im Autohaus für " + Vehicle.Name +" ($" + price + ")"))
                                        {
                                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                                            return;
                                        }
                                    }
                                    else return;

                                    if (whatCoupon != 0)
                                    {
                                        dbPlayer.SendNewNotification("- " + couponPercent + " % Rabatt", title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                        dbPlayer.Container.RemoveItem(whatCoupon);
                                    }

                                    if (cShop.Id == 1001 && Vehicle.IsSpecialCar)
                                    {
                                        cShop.PlayerIds.Add((int)dbPlayer.Id);
                                        string PlayerIds = "";
                                        cShop.PlayerIds.ForEach(playerid => PlayerIds += playerid + ",");
                                        if (PlayerIds.Length > 0)
                                            PlayerIds = PlayerIds.Substring(0, PlayerIds.Length - 1);
                                        MySQLHandler.ExecuteAsync($"UPDATE `carshop_shops` SET `player_ids` = '{PlayerIds}' WHERE `id` = '{cShop.Id}'");
                                    }

                                    dbPlayer.SendNewNotification("Sie haben einen " + Vehicle.Name +
                                    " fuer $" + price +
                                    " gekauft!");

                                    if (discount > 0)
                                    {
                                        dbPlayer.SendNewNotification(
                                            "Sie haben fuer dieses Fahrzeug nur " + discount +
                                            "% des Kaufpreises bezahlt!");
                                    }

                                    string x = cShop.SpawnPosition.X.ToString().Replace(",", ".");
                                    string y = cShop.SpawnPosition.Y.ToString().Replace(",", ".");
                                    string z = cShop.SpawnPosition.Z.ToString().Replace(",", ".");
                                    string heading2 = cShop.SpawnHeading.ToString().Replace(",", ".");

                                    var crumbs = dbPlayer.GetName().Split('_');

                                    var firstLetter = "";
                                    var secondLetter = "";

                                    if (crumbs.Length == 2)
                                    {
                                        firstLetter = crumbs[0][0].ToString();
                                        secondLetter = crumbs[1][0].ToString();
                                    }

                                    int defaultgarage = 1;
                                    if (Vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Boot) defaultgarage = 34;
                                    else if (Vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Flugzeug) defaultgarage = 35;
                                    else if (Vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Helikopter) defaultgarage = 466;

                                    var registered = 0;
                                    bool GpsSender = false;
                                    var plate = "";

                                    //register veh if noob shoh
                                    //Noobshop, damit die Noobs direkt einen GPS Sender eingebaut haben
                                    if (cShop.Id == 12)
                                    {
                                        GpsSender = true;
                                        registered = 1;
                                        plate = RegistrationOfficeFunctions.GetRandomPlate(true);
                                    }

                                    string query = String.Format(
                                            "INSERT INTO `vehicles` (`owner`, `pos_x`, `pos_y`, `pos_z`, `heading`, `color1`, `color2`, `plate`, `model`, `vehiclehash`, `garage_id`,`gps_tracker`,`registered`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}');",
                                            dbPlayer.Id, x, y, z, heading2,
                                            Vehicle.PrimaryColor,
                                            Vehicle.SecondaryColor,
                                            plate, Vehicle.Data.Id, Vehicle.Data.IsModdedCar == 1 ? Vehicle.Data.mod_car_name : Vehicle.Data.Model, defaultgarage, GpsSender ? 1 : 0, registered);
                                    MySQLHandler.Execute(query);
                                    Logger.AddVehiclePurchaseLog(dbPlayer.Id, cShop.Id, Vehicle.Data.Id, price, 0);

                                    query = string.Format(
                                        "SELECT * FROM `vehicles` WHERE `owner` = '{0}' AND `model` LIKE '{1}' ORDER BY id DESC LIMIT 1;",
                                        dbPlayer.Id,
                                        Vehicle.Data.Id);

                                    // Update Special Cars Stuff
                                    if (Vehicle.IsSpecialCar) Vehicle.IncreaseLimitedAmount();

                                    uint id = 0;

                                    using (var conn =
                                        new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        conn.Open();
                                        cmd.CommandText = @query;
                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    id = reader.GetUInt32(0);
                                                    break;
                                                }
                                            }
                                        }
                                        conn.Close();
                                    }
                                    NAPI.Task.Run(async () =>
                                    {
                                        SxVehicle sxVehicle = VehicleHandler.Instance.CreateServerVehicle(Vehicle.Data.Id, false,
                                        cShop.SpawnPosition, cShop.SpawnHeading, Vehicle.PrimaryColor,
                                        Vehicle.SecondaryColor, 0, GpsSender, true,
                                        false, 0,
                                        "", id, 0, dbPlayer.Id, 100, VehicleHandler.MaxVehicleHealth,
                                        "", "", 0, ContainerManager.LoadContainer(id, ContainerTypes.VEHICLE, Vehicle.Data.InventorySize, Vehicle.Data.InventoryWeight),
                                        plate, container2: ContainerManager.LoadContainer(id, ContainerTypes.VEHICLE2));

                                        while (sxVehicle.Entity == null)
                                        {
                                            await NAPI.Task.WaitForMainThread(100);
                                        }

                                        dbPlayer.OwnVehicles.Add(id, Vehicle.Data.IsModdedCar == 1 ? Vehicle.Data.mod_car_name : Vehicle.Data.Model);

                                        if (cShop.Id != 12)
                                        {
                                            RegistrationOfficeFunctions.GiveVehicleContract(dbPlayer, sxVehicle, "Fahrzeugshop " + cShop.Description);
                                        }
                                        else
                                        {
                                            RegistrationOfficeFunctions.UpdateVehicleRegistrationToDb(sxVehicle, dbPlayer, dbPlayer, plate, true);
                                        }
                                    });

                                    dbPlayer.Save();
                                });

                                return;
                            }
                            else
                            {
                                if (dbPlayer.TeamId == 0 || !cShop.RestrictedTeams.Contains((int)dbPlayer.TeamId))
                                {
                                    dbPlayer.SendNewNotification("Dieser Shop ist nicht fuer Ihr Team geeignet!");
                                    DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_carshop);
                                    return;
                                }

                                if (!Vehicle.IsSpecialCar)
                                {
                                    // Gangwar Discount
                                    discount = 100 - (GangwarTownModule.Instance.GetCarShopDiscount(dbPlayer.Team));
                                    price = Convert.ToInt32((discount / 100.0) * price);
                                }

                                // Check FBank & Rights
                                if (dbPlayer.TeamRankPermission == null || dbPlayer.TeamRankPermission.Manage == 0)
                                {
                                    dbPlayer.SendNewNotification("Keine Berechtigung!");
                                    DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_carshop);
                                    return;
                                }

                                var teamShelter = TeamShelterModule.Instance.GetAll().FirstOrDefault(s => s.Value.Team.Id == dbPlayer.Team.Id).Value;
                                if (teamShelter == null || teamShelter.Team == null || dbPlayer.TeamId != teamShelter.Team.Id) return;
                                if (teamShelter.Money < price)
                                {
                                    dbPlayer.SendNewNotification($"Kosten {price}$ nicht in der FBank vorhanden!");
                                    DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_carshop);
                                    return;
                                }

                                teamShelter.TakeMoney(price);
                                string query = String.Format(
                                    "INSERT INTO `fvehicles` (`vehiclehash`, `team`, `color1`, `color2`, `inGarage`, `model`, `fuel`) VALUES ('{0}', '{1}', '{2}', '{3}', '1', '{4}', '100');",
                                    Vehicle.Data.IsModdedCar == 1 ? Vehicle.Data.mod_car_name : Vehicle.Data.Model, dbPlayer.TeamId, dbPlayer.Team.ColorId, dbPlayer.Team.ColorId,
                                    Vehicle.Data.Id);
                                MySQLHandler.Execute(query);
                                Logger.AddVehiclePurchaseLog(dbPlayer.Id, cShop.Id, Vehicle.Data.Id, price, dbPlayer.TeamId);


                                query = string.Format(
                                    "SELECT * FROM `fvehicles` WHERE `team` = '{0}' AND `model` LIKE '{1}' ORDER BY id DESC LIMIT 1;",
                                    dbPlayer.Team.Id,
                                    Vehicle.Data.Id);

                                uint id = 0;

                                using (var conn =
                                    new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                                using (var cmd = conn.CreateCommand())
                                {
                                    conn.Open();
                                    cmd.CommandText = @query;
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            while (reader.Read())
                                            {
                                                id = reader.GetUInt32("id");
                                                break;
                                            }
                                        }
                                    }
                                    conn.Close();
                                }

                                dbPlayer.SendNewNotification(
                                    $"Sie haben {Vehicle.Data.Model} fuer Ihre Fraktion gekauft!");

                                SxVehicle sxVehicle = new SxVehicle()
                                {
                                    Data = Vehicle.Data,
                                    databaseId = 0
                                };
                                RegistrationOfficeFunctions.GiveVehicleContract(dbPlayer, sxVehicle, "Fahrzeugshop " + cShop.Description);
                            }
                            
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_carshop);
                            dbPlayer.Save();
                            return;
                        }

                        idx++;
                    }

                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_carshop);
                }
                else if (menuid == Dialogs.menu_jailinhabits)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_jailinhabits);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        private static void CreateUserMenuFahrzeugGarage(Player player, DbPlayer dbPlayer, Garage garage)
        {
            DialogMigrator.CreateMenu(player, Dialogs.menu_garage_overlay, "Fahrzeug-Garage", "");
            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, GlobalMessages.General.Close(), "");
            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, "Fahrzeug entnehmen", "");
            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, "Fahrzeug einlagern", "");
         
            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_garage_overlay);
            dbPlayer.SetData("GarageId", garage.Id);
            return;
        }
    }
}