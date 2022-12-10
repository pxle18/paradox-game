using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Handler;
using VMP_CNR.Module.AnimationMenu;
using VMP_CNR.Module.Attachments;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Mobile;
using VMP_CNR.Module.Clothes.Outfits;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Clothes.Team;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Injury.InjuryMove;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Kasino;
using VMP_CNR.Module.Keys;
using VMP_CNR.Module.Keys.Windows;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.RemoteEvents;
using VMP_CNR.Module.Schwarzgeld;
using VMP_CNR.Module.Staatskasse;
using VMP_CNR.Module.Swat;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.Weapons.Data;
using static VMP_CNR.AnimationContent;

namespace VMP_CNR.Module.Players
{
    public class PlayerEventHandler : Script
    {
        [RemoteEvent]
        public void LscConfirmVehRequest(Player player, string pb_map, string none, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;
            var pcom = new PlayerCommands();
            pcom.acceptlsc(player);
        }

        [RemoteEvent]
        public void LscConfirmPayRequest(Player player, string pb_map, string none, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;
            var pcom = new PlayerCommands();
            pcom.lscpay(player);
        }

        [RemoteEvent]
        public void UpdatePlayerWaterState(Player player, bool isInWater, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            // TODO: Modul-Event einfügen
            if (isInWater && dbPlayer.PlayingAnimation)
                dbPlayer.StopAnimation();
        }

        //TODO: Fix clientside
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_GIVEMONEY_DIALOG(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (destinationDbPlayer == null || !destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 20f) return;

            dbPlayer.SetData("sInteraction", destinationDbPlayer);
            ComponentManager.Get<GiveMoneyWindow>().Show()(dbPlayer, destinationDbPlayer);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void showOwnLicenses(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            
            if (dbPlayer.HasPerso[0] == 0)
            {
                dbPlayer.SendNewNotification("Du besitzt keinen Personalausweis!");
                return;
            }
            dbPlayer.ShowIdCard(dbPlayer);
            dbPlayer.ShowLicenses(dbPlayer);
        }

        //TODO: Fix clientside
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_SHOW_PERSO(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 20f) return;
            if (destinationDbPlayer.IsInjured()) return;

            if (dbPlayer.HasPerso[0] == 0)
            {
                dbPlayer.SendNewNotification("Du besitzt keinen Personalausweis!");
                return;
            }

            dbPlayer.SendNewNotification("Sie haben Ihre Personalien gezeigt!");
            dbPlayer.ShowIdCard(destinationPlayer);
        }

        //TODO: Fix clientside
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_GETPERSO(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.5f) return;


            if (destinationDbPlayer.IsInjured() || destinationDbPlayer.IsTied || destinationDbPlayer.IsCuffed)
            {
                if (destinationDbPlayer.HasPerso[0] == 0 || destinationDbPlayer.IsSwatDuty())
                {
                    dbPlayer.SendNewNotification("Spieler hat keinen Perso!");
                }
                else
                {
                    dbPlayer.SendNewNotification("Sie haben den Personalausweis genommen!");
                    destinationDbPlayer.ShowIdCard(client);
                }
            }


        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_GIVEKEY(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.5f) return;

            Dictionary<string, List<VHKey>> keys = new Dictionary<string, List<VHKey>>();

            List<VHKey> houses = HouseKeyHandler.Instance.GetOwnHouseKey(dbPlayer);
            List<VHKey> vehicles = VehicleKeyHandler.Instance.GetOwnVehicleKeys(dbPlayer);
            List<VHKey> storages = StorageKeyHandler.Instance.GetOwnStorageKey(dbPlayer);

            keys.Add("Häuser", houses);
            keys.Add("Fahrzeuge", vehicles);
            keys.Add("Lagerräume", storages);

            ComponentManager.Get<KeyWindow>().Show()(dbPlayer, destinationDbPlayer.GetName(), keys);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_PEDS_PLAYER_GIVEITEM(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            if (!dbPlayer.CanAccessRemoteEvent())
            {
                dbPlayer.SendNewNotification( GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.InventoryActivated)
            {
                dbPlayer.SendNewNotification("Das Inventarsystem ist aus Performance-Gründen deaktiviert.");
                dbPlayer.SendNewNotification("Es ist in wenigen Minuten wieder erreichbar!");
                return;
            }

            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.5f) return;
            
            dbPlayer.SetData("giveitem", destinationDbPlayer.Id);
            await ItemsModuleEvents.RequestInventory(dbPlayer);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void packArmor(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid() || !dbPlayer.CanAccessMethod() || dbPlayer.IsTied || dbPlayer.IsCuffed || dbPlayer.HasData("paintball_map")) return;

            if (dbPlayer.RageExtension.IsInVehicle) return;

            if (dbPlayer.DimensionType[0] == DimensionType.Gangwar) return;

            if (dbPlayer.HasData("blockArmorCheat")) return;

            if (dbPlayer.HasData("lastArmorPacked"))
            {
                DateTime date = dbPlayer.GetData("lastArmorPacked");
                if (date.AddSeconds(5) > DateTime.Now)
                {
                    return;
                }
                else dbPlayer.ResetData("lastArmorPacked");
            }

            if (dbPlayer.Player.Armor < 25)
            {
                dbPlayer.SendNewNotification("Die Weste ist zu kaputt zum packen!");
                return;
            }

            uint itemId = 1142;
            if(dbPlayer.IsCopPackGun() && dbPlayer.IsInDuty())
            {
                itemId = 1141;
            }

            if(!dbPlayer.Container.CanInventoryItemAdded(itemId))
            {
                dbPlayer.SendNewNotification("Ihr Inventar reicht nicht aus!");
                return;
            }

            if (!dbPlayer.CanInteract()) return;

            dbPlayer.SetData("lastArmorPacked", DateTime.Now);
            dbPlayer.SetCannotInteract(true);
            Chats.sendProgressBar(dbPlayer, 4000);
            dbPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            await Task.Delay(4000);
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();

            if (dbPlayer.Player.Armor < 25)
            {
                dbPlayer.SendNewNotification("Die Weste ist zu kaputt zum packen!");
                dbPlayer.SetCannotInteract(false);
                return;
            }

            Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();
            Data.Add("armorvalue", dbPlayer.Player.Armor);
            Data.Add("Desc", "Haltbarkeit: " + dbPlayer.Player.Armor + "%");

            dbPlayer.Container.AddItem(itemId, 1, Data);
            Logging.Logger.AddToArmorPackLog(dbPlayer.Id, dbPlayer.Player.Armor);
            dbPlayer.SendNewNotification($"Schutzweste mit {dbPlayer.Player.Armor} gepackt!");
            dbPlayer.SetArmor(0);
            dbPlayer.SetCannotInteract(false);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void packGun(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid() || !dbPlayer.CanAccessMethod() || dbPlayer.IsTied || dbPlayer.IsCuffed || dbPlayer.HasData("paintball_map")) return;

            if (dbPlayer.DimensionType[0] == DimensionType.Gangwar) return;

            if (dbPlayer.RageExtension.IsInVehicle) return;
                        if (dbPlayer.HasData("no-packgun"))
            {
                if (dbPlayer.GetData("no-packgun") == true)
                {
                    dbPlayer.SendNewNotification("Du kannst während eines Nachladevorgangs die Waffe nicht verstauen");
                    return;
                }
            }

            if (dbPlayer.HasData("do-packgun"))
            {
                if (dbPlayer.GetData("do-packgun") == true)
                {
                    dbPlayer.SendNewNotification("Du packst deine Waffe bereits.");
                    return;
                }
            }

            if (dbPlayer.HasData("lastWeaponPacked"))
            {
                DateTime date = dbPlayer.GetData("lastWeaponPacked");
                if (date.AddSeconds(5) > DateTime.Now)
                {
                    return;
                }
                else dbPlayer.ResetData("lastWeaponPacked");
            }

            var gun = dbPlayer.Player.CurrentWeapon;
            if (gun == 0) return;
            var ammo = 0;
            var l_WeaponID = 0;

            var l_WeaponDatas = WeaponDataModule.Instance.GetAll();

            var l_Weapon = l_WeaponDatas.Values.FirstOrDefault(data => data.Hash == (int)gun);
            if (l_Weapon == null) return;
            l_WeaponID = l_Weapon.Id;


            var l_playerWeapon = dbPlayer.Weapons.FirstOrDefault(detail => detail.WeaponDataId == l_WeaponID);
            if (l_playerWeapon == null) return;
            ammo = l_playerWeapon.Ammo;

            var weapon = dbPlayer.IsCopPackGun() ? ItemModelModule.Instance.GetByScript($"bw_{l_Weapon.Name}") : ItemModelModule.Instance.GetByScript($"w_{l_Weapon.Name}");
            if (weapon == null || l_WeaponID == 0) return;
            if (weapon.Name.ToLower().Contains("unarmed")) return;


            if (dbPlayer.Container.CanInventoryItemAdded(weapon))
            {
                Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

                if(dbPlayer.HasWeaponComponentsForWeapon((uint)l_Weapon.Hash) && dbPlayer.WeaponComponents.ContainsKey((uint)l_Weapon.Hash))
                {
                    int compCount = 0;
                    List<uint> weaponComps = new List<uint>();
                    foreach(Weapons.Component.WeaponComponent comp in dbPlayer.WeaponComponents[(uint)l_Weapon.Hash].ToList())
                    {
                        if (comp == null || comp.DisablePacking) continue;
                        weaponComps.Add((uint)comp.Id);
                        compCount++;
                    }
                    if (compCount > 0)
                    {
                        data.Add("components", NAPI.Util.ToJson(weaponComps));
                        data.Add("Desc", compCount + " Modifizierungen angebracht. ");
                    }
                }

                if (ammo > 0)
                {
                    ItemModel magazin;
                    int magazinAmmo;
                    if (l_Weapon.Name.ToLower() == "pistol")    //Sonst beim Packen von Pistol -> Munition wird Pistol50
                    {
                        magazin = dbPlayer.IsCopPackGun() ? ItemModelModule.Instance.GetByScript($"bammo_{l_Weapon.Name}_12") : ItemModelModule.Instance.GetByScript($"ammo_{l_Weapon.Name}_12");
                        if (magazin == null) return;
                        magazinAmmo = dbPlayer.IsCopPackGun() ? Convert.ToInt32(magazin.Script.ToLower().Replace("bammo_", "").Split('_')[1]) : Convert.ToInt32(magazin.Script.ToLower().Replace("ammo_", "").Split('_')[1]);
                    }
                    else
                    {
                        magazin = dbPlayer.IsCopPackGun() ? ItemModelModule.Instance.GetByScript($"bammo_{l_Weapon.Name}") : ItemModelModule.Instance.GetByScript($"ammo_{l_Weapon.Name}");
                        if (magazin == null) return;
                        magazinAmmo = dbPlayer.IsCopPackGun() ? Convert.ToInt32(magazin.Script.ToLower().Replace("bammo_", "").Split('_')[1]) : Convert.ToInt32(magazin.Script.ToLower().Replace("ammo_", "").Split('_')[1]);
                    }

                    dbPlayer.SetData("lastWeaponPacked", DateTime.Now);
                    dbPlayer.SetData("do-packgun", true);
                    dbPlayer.ResyncWeaponAmmo();

                    Chats.sendProgressBar(dbPlayer, 4000);
                    dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    await Task.Delay(4000);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.StopAnimation();

                    l_playerWeapon = dbPlayer.Weapons.FirstOrDefault(detail => detail.WeaponDataId == l_WeaponID);
                    if (l_playerWeapon == null) return;
                    ammo = l_playerWeapon.Ammo;

                    var magazines = ammo / magazinAmmo;

                    dbPlayer.SetData("packgun-timestamp", DateTime.Now);
                    if (magazines > 0)
                    {
                        if (!dbPlayer.Container.CanInventoryWeaponAndAmmoAdded(weapon, magazin, magazines))
                        {
                            dbPlayer.SendNewNotification("Deine Waffe würde passen, aber deine Munition nicht, alla!");
                            dbPlayer.ResetData("do-packgun");
                            return;
                        }
                        dbPlayer.RemoveWeapon(gun);

                        if (dbPlayer.IsValid())
                        {
                            dbPlayer.Container.AddItem(magazin, magazines);
                            dbPlayer.Container.AddItem(weapon, 1, data);
                            Logger.AddPackgunLog(dbPlayer.Id, weapon.Id, 1);
                            Logger.SaveToItemLog(dbPlayer.Id, dbPlayer.GetName(), weapon.Id, 1, "PACKGUN - WEAPON", (int)dbPlayer.Id);
                            Logger.SaveToItemLog(dbPlayer.Id, dbPlayer.GetName(), magazin.Id, magazines, "PACKGUN - MAGAZINES", (int)dbPlayer.Id);
                        }

                    }
                    else
                    {
                        dbPlayer.RemoveWeapon(gun);

                        if (dbPlayer.IsValid())
                        {
                            Logger.AddPackgunLog(dbPlayer.Id, weapon.Id, 1);
                            Logger.SaveToItemLog(dbPlayer.Id, dbPlayer.GetName(), weapon.Id, 1, "PACKGUN - WEAPON", (int)dbPlayer.Id);
                            dbPlayer.Container.AddItem(weapon, 1, data);
                        }
                    }
                }
                else
                {
                    dbPlayer.RemoveWeapon(gun);

                    if (dbPlayer.IsValid())
                    {
                        Logger.AddPackgunLog(dbPlayer.Id, weapon.Id, 1);
                        Logger.SaveToItemLog(dbPlayer.Id, dbPlayer.GetName(), weapon.Id, 1, "PACKGUN - WEAPON", (int)dbPlayer.Id);
                        dbPlayer.Container.AddItem(weapon, 1, data);
                    }
                }


            }
            else
            {
                dbPlayer.SendNewNotification("Du hast nicht mal genug Platz für die Waffe... Und dann auch noch Munition?!?!?");
                return;
            }

            dbPlayer.SendNewNotification(
                "Sie haben Ihre " + weapon.Name +
                " in Ihr Inventar verstaut!");
            await dbPlayer.PlayInventoryInteractAnimation();
            dbPlayer.ResetData("do-packgun");
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void requestPlayerKeys(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            Dictionary<string, List<VHKey>> keys = new Dictionary<string, List<VHKey>>();

            List<VHKey> houses = HouseKeyHandler.Instance.GetAllKeysPlayerHas(dbPlayer);
            List<VHKey> vehicles = VehicleKeyHandler.Instance.GetAllKeysPlayerHas(dbPlayer);
            List<VHKey> storages = StorageKeyHandler.Instance.GetAllKeysPlayerHas(dbPlayer);

            keys.Add("Häuser", houses);
            keys.Add("Fahrzeuge", vehicles);
            keys.Add("Lagerräume", storages);

            ComponentManager.Get<KeyWindow>().Show()(dbPlayer, null, keys);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void packblackmoney(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsTied || dbPlayer.IsCuffed || !dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return;

            if (dbPlayer.BlackMoney[0] > 0)
            {
                int blAmount = dbPlayer.BlackMoney[0];
                int maxStackSize = ItemModelModule.Instance.GetById(SchwarzgeldModule.SchwarzgeldId).MaximumStacksize;

                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetData("userCannotInterrupt", true);
                dbPlayer.SetData("packBlackMoney", blAmount);
                dbPlayer.SetData("blockKeyPress", true);

                while (blAmount > 0)
                {
                    Console.WriteLine("### SCHWARZGELD WHILE LOOP START ###");
                    // Prüfe ob überhaupt Platz für 1 Schwarzgeld ist. Wenn ja, ist auch für ein Stack Platz weil weight == 0
                    if (!dbPlayer.Container.CanInventoryItemAdded(SchwarzgeldModule.SchwarzgeldId, 1))
                    {
                        dbPlayer.SendNewNotification($"Kein Platz im Inventar!");
                        break;
                    }

                    int amountForCurrentItr = blAmount < maxStackSize ? blAmount : maxStackSize;

                    Chats.sendProgressBar(dbPlayer, 5000);
                    
                    dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);

                    dbPlayer.TakeBlackMoney(amountForCurrentItr);

                    await Task.Delay(5000);

                    dbPlayer.Container.AddItem(SchwarzgeldModule.SchwarzgeldId, amountForCurrentItr);
                    dbPlayer.SendNewNotification($"Du hast Schwarzgeld gepackt (${amountForCurrentItr})!");

                    blAmount -= amountForCurrentItr;
                    Console.WriteLine("### SCHWARZGELD WHILE LOOP END ###");
                    await Task.Delay(1000);
                }
                
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.ResetData("userCannotInterrupt");
                dbPlayer.ResetData("packBlackMoney");
                dbPlayer.ResetData("blockKeyPress");
                dbPlayer.StopAnimation();
            }
        }

        [RemoteEvent]
        public void SwitchSeat(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            /*
            try
            {
                DbPlayer dbPlayer = player.GetPlayer();
                SxVehicle sxVeh = player.Vehicle.GetVehicle();
                if (dbPlayer == null || !dbPlayer.IsValid() || sxVeh == null || !sxVeh.IsValid() || !dbPlayer.CanInteract()) return;
                if (!dbPlayer.RageExtension.IsInVehicle) return;

                if (dbPlayer.HasData("SwitchSeats"))
                {
                    if (dbPlayer.GetData("SwitchSeats").AddSeconds(5) > DateTime.Now) { return; }
                }

                int akt_seat = player.VehicleSeat;
                int newseat = 1;

                if (sxVeh.Data != null && akt_seat == 0 && (sxVeh.Data.ClassificationId == 8 || sxVeh.Data.ClassificationId == 9))
                {
                    return;
                }

                bool free = false;
                if (sxVeh.Occupants.Where(x => x.Value != null && x.Value.IsValid() && x.Value.Player.VehicleSeat == 1).Count() == 0 && akt_seat == 0 && !free)
                {
                    free = true;
                    newseat = 1;
                }
                if (sxVeh.Occupants.Where(x => x.Value != null && x.Value.IsValid() && x.Value.Player.VehicleSeat == 0).Count() == 0 && akt_seat == 1 && !free)
                {
                    free = true;
                    newseat = 0;
                }
                if (sxVeh.Occupants.Where(x => x.Value != null && x.Value.IsValid() && x.Value.Player.VehicleSeat == 3).Count() == 0 && akt_seat == 2 && !free)
                {
                    free = true;
                    newseat = 3;
                }
                if (sxVeh.Occupants.Where(x => x.Value != null && x.Value.IsValid() && x.Value.Player.VehicleSeat == 3).Count() == 0 && akt_seat == 2 && !free)
                {
                    free = true;
                    newseat = 3;
                }

                if (free)
                {
                    
                    dbPlayer.SetData("SwitchSeats", DateTime.Now);
                    Players.Instance.GetPlayersInRange(dbPlayer.Player.Position).TriggerNewClient("shuffleseat", dbPlayer.Player, newseat);

                    Task.Run(async () =>
                    {
                        await NAPI.Task.WaitForMainThread(1550);

                        if (dbPlayer == null || !dbPlayer.IsValid() || sxVeh == null || !sxVeh.IsValid() || !dbPlayer.CanInteract()) return;
                        if (!dbPlayer.RageExtension.IsInVehicle) return;

                        Logger.Debug("actual Seat " + dbPlayer.Player.VehicleSeat);

                        if (sxVeh == null || sxVeh.entity == null || !sxVeh.IsValid()) return;

                        SxVehicle sxVeh2 = dbPlayer.Player.Vehicle.GetVehicle();
                        if (sxVeh2 == null || !sxVeh2.IsValid() || sxVeh2 != sxVeh) return;

                        dbPlayer.WarpOutOfVehicle();

                        dbPlayer.Player.SetIntoVehicle(sxVeh.entity, newseat);
                    });
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }*/
        }


        //TODO: Fix clientside / missing Seil
        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false)]
        [RemoteEvent]
        public async Task REQUEST_PEDS_PLAYER_TIE(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.0f) return;

            float distance = Math.Abs(destinationPlayer.Heading - dbPlayer.Player.Heading);

            if (distance > 45) return;

            if (destinationDbPlayer.IsInjured() || destinationDbPlayer.IsCuffed ||  destinationDbPlayer.GetData("lastCuffedTied") == "cuffed")
            {
                return;
            }

            if (destinationDbPlayer.HasData("follow"))  // Untied if follow but not if cuffed
            {
                destinationDbPlayer.IsTied = true;
                destinationDbPlayer.ResetData("follow");
                destinationDbPlayer.Player.TriggerNewClient("toggleShooting", false);
            }
            // tie or untie the player
            if (destinationDbPlayer.IsTied)
            {
                // Animation init for cuffing
                destinationPlayer.SetRotation(dbPlayer.Player.Heading);
                await Task.Delay(500);

                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetData("userCannotInterrupt", true);
                destinationDbPlayer.Player.TriggerNewClient("freezePlayer", true);
                destinationDbPlayer.SetData("userCannotInterrupt", true);

                destinationDbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "b_uncuff");
                dbPlayer.PlayAnimation((int)(AnimationFlags.StopOnLastFrame | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "a_uncuff");

                await Task.Delay(5000);

                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.SetData("userCannotInterrupt", false);
                destinationDbPlayer.Player.TriggerNewClient("freezePlayer", false);
                destinationDbPlayer.SetData("userCannotInterrupt", false);

                dbPlayer.StopAnimation();

                await Task.Delay(500);

                // is already tied - everybody can untie this person
                destinationDbPlayer.SetTied(false);
                destinationDbPlayer.ResetData("lastCuffedTied");
                // send messages and /me animations
                dbPlayer.SendNewNotification("Sie haben jemanden entfesselt!");
                destinationDbPlayer.SendNewNotification("Jemand hat Sie entfesselt!");
                return;
            }
            else
            {
                // is not tied                                    
                // verify has requiered item in inventory
                if (dbPlayer.Container.GetItemAmount(17) == 0)
                {
                    dbPlayer.SendNewNotification("Sie benoetigen ein Seil um einen Spieler zu fesseln!");
                    return;
                }

                // remove one item from iunventory
                dbPlayer.Container.RemoveItem(17, 1);


                // Animation init for cuffing
                NAPI.Player.SetPlayerCurrentWeapon(destinationPlayer, WeaponHash.Unarmed);


                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetData("userCannotInterrupt", true);
                destinationDbPlayer.Player.TriggerNewClient("freezePlayer", true);
                destinationDbPlayer.SetData("userCannotInterrupt", true);

                destinationDbPlayer.StopAnimation();

                destinationPlayer.SetRotation(dbPlayer.Player.Heading);
                await Task.Delay(500);

                destinationDbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arrest_paired", "crook_p2_back_right");
                dbPlayer.PlayAnimation((int)(AnimationFlags.StopOnLastFrame | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arrest_paired", "cop_p2_back_right");
                await Task.Delay(5000);

                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.SetData("userCannotInterrupt", false);
                destinationDbPlayer.Player.TriggerNewClient("freezePlayer", false);
                destinationDbPlayer.SetData("userCannotInterrupt", false);

                dbPlayer.StopAnimation();
                destinationDbPlayer.StopAnimation();
                await Task.Delay(500);


                // is already tied - everybody can untie this person
                destinationDbPlayer.SetTied(true);
                destinationDbPlayer.SetData("lastCuffedTied", "tied");
                // Cancel phone call when tied
                if (PhoneCall.IsPlayerInCall(destinationDbPlayer.Player))
                {
                    destinationDbPlayer.CancelPhoneCall();
                }

                // send messages and /me animations
                dbPlayer.SendNewNotification("Sie haben jemanden gefesselt!");
                destinationDbPlayer.SendNewNotification("Jemand hat Sie gefesselt!");
            }
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_CASINO(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            if (!dbPlayer.IsInCasinoDuty()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 5.0f) return;

            if (KasinoModule.Instance.CasinoGuests.Contains(destinationDbPlayer))
            {
                dbPlayer.SendNewNotification($"Casino Zugang entzogen für Kunde {destinationDbPlayer.GetName()}", PlayerNotification.NotificationType.ERROR);
                destinationDbPlayer.SendNewNotification($"Casino Zugang entzogen", PlayerNotification.NotificationType.ERROR);
                KasinoModule.Instance.CasinoGuests.Remove(destinationDbPlayer);
            }
            else
            {
                dbPlayer.SendNewNotification($"Casino Zugang gewährt für Kunde {destinationDbPlayer.GetName()}", PlayerNotification.NotificationType.SUCCESS);
                destinationDbPlayer.SendNewNotification($"Casino Zugang gewährt", PlayerNotification.NotificationType.SUCCESS);
                KasinoModule.Instance.CasinoGuests.Add(destinationDbPlayer);
            }


        }




        //TODO: Fix serverside
        [RemoteEventPermission]
        [RemoteEvent]
        public async Task REQUEST_PEDS_PLAYER_CUFF(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            if (!dbPlayer.IsInCasinoDuty())
            {
                if ((!dbPlayer.IsACop() && !dbPlayer.IsGoverment() && !dbPlayer.IsAMedic()) || !dbPlayer.IsInDuty() || dbPlayer.IsCuffed || dbPlayer.IsTied) return;
            }

            // Main Thread
            await NAPI.Task.WaitForMainThread(0);
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.0f) return;

            // Worker Thread
            await Task.Delay(5);
            float distance = Math.Abs(destinationPlayer.Heading - client.Heading);

            if (distance > 45 && !destinationDbPlayer.HasData("SMGkilledPos")) return;

            if (destinationDbPlayer.IsInjured()) return;
            if (!destinationDbPlayer.RageExtension.IsInVehicle)
            {
                if (destinationDbPlayer.RageExtension.IsInVehicle) return;
                if (destinationDbPlayer.HasData("follow")) // Prevent Cuffs Again and bug
                {
                    destinationDbPlayer.IsCuffed = true;
                    destinationDbPlayer.ResetData("follow");
                    destinationDbPlayer.Player.TriggerNewClient("toggleShooting", false);
                }
                if ((destinationDbPlayer.IsCuffed || destinationDbPlayer.IsTied) && !destinationDbPlayer.HasData("SMGkilledPos"))
                {

                    // Animation init for cuffing
                    destinationPlayer.SetRotation(client.Heading);
                    await Task.Delay(500);

                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetData("userCannotInterrupt", true);
                    destinationDbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    destinationDbPlayer.SetData("userCannotInterrupt", true);

                    destinationDbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "b_uncuff");
                    dbPlayer.PlayAnimation((int)(AnimationFlags.StopOnLastFrame | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "a_uncuff");
                    await Task.Delay(5000);

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.SetData("userCannotInterrupt", false);
                    destinationDbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    destinationDbPlayer.SetData("userCannotInterrupt", false);

                    dbPlayer.StopAnimation();

                    await Task.Delay(500);
                    destinationDbPlayer.SetCuffed(false);
                    destinationDbPlayer.SetTied(false);
                    destinationDbPlayer.ResetData("lastCuffedTied");
                    dbPlayer.SendNewNotification("Sie haben jemanden die Handschellen abgenommen!");
                    destinationDbPlayer.SendNewNotification("Ein Beamter hat Ihnen die Handschellen abgenommen!");
                    return;
                }
                else
                {
                    // Cancel phone call when arrested
                    if (PhoneCall.IsPlayerInCall(destinationDbPlayer.Player))
                    {
                        destinationDbPlayer.CancelPhoneCall();
                    }


                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetData("userCannotInterrupt", true);
                    destinationDbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    destinationDbPlayer.SetData("userCannotInterrupt", true);

                    destinationDbPlayer.StopAnimation();

                    destinationPlayer.SetRotation(client.Heading); 
                    await Task.Delay(500);

                    destinationDbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arrest_paired", "crook_p2_back_right");
                    dbPlayer.PlayAnimation((int)(AnimationFlags.StopOnLastFrame | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arrest_paired", "cop_p2_back_right");
                    await Task.Delay(5000);

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.SetData("userCannotInterrupt", false);
                    destinationDbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    destinationDbPlayer.SetData("userCannotInterrupt", false);

                    dbPlayer.StopAnimation();
                    destinationDbPlayer.StopAnimation();
                    await Task.Delay(500);

                    destinationDbPlayer.SetCuffed(true);
                    destinationDbPlayer.SetData("lastCuffedTied", "cuffed");
                    dbPlayer.SendNewNotification("Sie haben jemanden die Handschellen angelegt!");
                    destinationDbPlayer.SendNewNotification("Ein Beamter hat Ihnen die Handschellen angelegt!");
                }
            }
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async Task REQUEST_PEDS_PLAYER_FRISK(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            if (dbPlayer.IsCuffed || dbPlayer.IsTied) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.2f) return;

            ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
            ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);
            
            if (!destinationDbPlayer.IsCuffed && !destinationDbPlayer.IsTied && !destinationDbPlayer.IsInjured())
            {
                dbPlayer.SendNewNotification("Person ist nicht gefesselt", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (!dbPlayer.HasData("lastfriskperson") || dbPlayer.GetData("lastfriskperson") != destinationDbPlayer.Id)
            {
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                Chats.sendProgressBar(dbPlayer, 8000);
                await Task.Delay(8000);

                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();
            }

            dbPlayer.SetData("lastfriskperson", destinationDbPlayer.Id);

            var lWeapons = destinationDbPlayer.Weapons;
            if (lWeapons.Count > 0)
            {
                var lWeaponListContainer = new List<WeaponListContainer>();
                foreach (var lWeapon in lWeapons)
                {
                    var lData = WeaponDataModule.Instance.Get(lWeapon.WeaponDataId);
                    var weapon = ItemModelModule.Instance.GetByScript("w_" + Convert.ToString(lData.Name.ToLower()));
                    if (weapon == null) continue;
                    lWeaponListContainer.Add(new WeaponListContainer(lData.Name, lWeapon.Ammo, weapon.ImagePath));
                }

                if (dbPlayer.IsACop() && dbPlayer.Duty)
                {
                    dbPlayer.SetData("disableFriskInv", true);
                    dbPlayer.SetData("friskInvUserID", destinationDbPlayer.Id);
                    dbPlayer.SetData("friskInvUserName", destinationDbPlayer.GetName());
                }
                else
                {
                    dbPlayer.SetData("disableinv", true);
                }

                var lWeaponListObject = new WeaponListObject(destinationDbPlayer.GetName(), dbPlayer.IsACop(), lWeaponListContainer);
                ComponentManager.Get<FriskWindow>().Show()(dbPlayer, lWeaponListObject);
                return;
            }


            ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
            ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);
            
            dbPlayer.SetData("friskInvUserID", destinationDbPlayer.Id);
            destinationDbPlayer.Container.ShowFriskInventory(dbPlayer, destinationDbPlayer, "Spieler", (destinationDbPlayer.Money[0] + destinationDbPlayer.BlackMoney[0]));

            if (destinationDbPlayer.BlackMoney[0] > 0 && dbPlayer.TeamId == (int)TeamTypes.TEAM_FIB)
            {
                dbPlayer.SendNewNotification($"Sie konnten von ${(destinationDbPlayer.Money[0] + destinationDbPlayer.BlackMoney[0])} insgesamt ${destinationDbPlayer.BlackMoney[0]} Schwarzgeld feststellen! (/takebm zum entfernen)");
            }
        }

        //TODO: Fix serverside
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_TAKEPERSON(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.5f) return;
            
            if (dbPlayer.RageExtension.IsInVehicle || destinationDbPlayer.RageExtension.IsInVehicle)
            {
                dbPlayer.SendNewNotification(
                    
                    "Sie oder die Person duerfen nicht in einem Fahrzeug sein!");
                return;
            }
            //if cuffed no other person than duty should uncuff
            if (destinationDbPlayer.GetData("lastCuffedTied") == "cuffed")
            {
                if (!dbPlayer.IsInCasinoDuty())
                {
                    if ((!dbPlayer.IsACop() && !dbPlayer.IsGoverment() && !dbPlayer.IsAMedic()) || !dbPlayer.IsInDuty() || dbPlayer.IsCuffed || dbPlayer.IsTied) return;
                }
            }
          

            if (!destinationDbPlayer.HasData("follow"))
            {
                if (!destinationDbPlayer.IsCuffed && !destinationDbPlayer.IsTied)
                {
                    dbPlayer.SendNewNotification(
                         "Spieler ist nicht gefesselt/gecuffed!");
                    return;
                }

                    dbPlayer.SendNewNotification(
              "Sie haben jemanden gepackt!");
                destinationDbPlayer.SendNewNotification(
              "Jemand hat Sie gepackt!");
                if (destinationDbPlayer.GetData("lastCuffedTied") == "cuffed")
                {
                    destinationDbPlayer.SetCuffed(false);
                }
                else
                {
                    destinationDbPlayer.SetTied(false);
                }
                destinationDbPlayer.SetData("follow", client.Name);
                destinationDbPlayer.Player.TriggerNewClient("toggleShooting", true);
                destinationDbPlayer.PlayAnimation(AnimationScenarioType.Animation,"anim@move_m@prisoner_cuffed_rc","aim_low_loop", -1, true, AnimationLevels.UserCop,
                    (int) (AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.Loop |
                           AnimationFlags.AllowPlayerControl), true);
            }
            else
            {
                destinationDbPlayer.StopAnimation();
                dbPlayer.SendNewNotification(
              "Sie haben jemanden losgelassen!");
                destinationDbPlayer.SendNewNotification(
              "Jemand hat Sie losgelassen!");
                if (destinationDbPlayer.GetData("lastCuffedTied") == "cuffed")
                {
                    destinationDbPlayer.SetCuffed(true);
                }
                else
                {
                    destinationDbPlayer.SetTied(true);
                }
                destinationDbPlayer.ResetData("follow");
                destinationDbPlayer.Player.TriggerNewClient("toggleShooting", false);
            }
        }
        
        //TODO: Check
        [RemoteEventPermission]
        [RemoteEvent]
        public async Task REQUEST_PEDS_PLAYER_STABALIZE(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            var destinationDbPlayer = destinationPlayer.GetPlayer();

            if (!destinationDbPlayer.IsValid()) return;

            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 5.0f) return;
            if (!destinationDbPlayer.IsInjured()) return;

            if (!dbPlayer.RageExtension.IsInVehicle)
            {
                uint stabilizeItemId = destinationDbPlayer.Injury.ItemToStabilizeId;

                if (destinationDbPlayer.Injury.Id == InjuryModule.Instance.InjuryDeathScreenId)
                {
                    dbPlayer.SendNewNotification("Es scheint als wäre die Person ins Koma gefallen!");
                    return;
                }

                if (stabilizeItemId != 0 && destinationDbPlayer.Player.Dimension == 0)
                {
                    await InjuryPlayerExtension.Medicate(destinationDbPlayer, dbPlayer);
                }
                else
                {

                    if ((destinationDbPlayer.Injury.Id == InjuryModule.Instance.InjuryKrankentransport || destinationDbPlayer.Player.Dimension != 0) && 
                    (dbPlayer.IsAMedic() && dbPlayer.IsInDuty() || dbPlayer.ParamedicLicense))
                    {
                        if ((dbPlayer.IsAGangster() || dbPlayer.IsBadOrga()) && dbPlayer.ParamedicLicense && !dbPlayer.InParamedicDuty)
                        {
                            dbPlayer.SendNewNotification("Du bist nicht im Medic Dienst deiner Fraktion!");
                            return;
                        }

                        // Anpassung für Revive in KH1
                        InjuryDeliverIntPoint injuryDeliveryPoint = InjuryDeliverIntPointModule.Instance.GetAll().FirstOrDefault(dlp => dlp.Value.Position.DistanceTo(dbPlayer.Player.Position) < 3.0f).Value;

                        // Anpassung für KH1 wegen tragenwechselzeug
                        InjuryMovePoint injuryMovePoint = InjuryMoveModule.Instance.GetAll().Values.Where(ip => ip.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f).FirstOrDefault();

                        if (injuryDeliveryPoint != null || injuryMovePoint != null || destinationDbPlayer.Player.Dimension != 0)
                        {
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl),
                                                                Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            if (dbPlayer.IsAGangster())
                            {
                                Chats.sendProgressBar(dbPlayer, 15000);
                                await Task.Delay(15000);
                            }
                            else
                            {
                                Chats.sendProgressBar(dbPlayer, 9000);
                                await Task.Delay(9000);
                            }
                            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured())
                            {
                                dbPlayer.SendNewNotification("Stabilisierung fehlgeschlagen!");
                                return;
                            }
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.StopAnimation();
                            destinationDbPlayer.Revive();
                            destinationDbPlayer.SendNewNotification($"Du wurdest vom Medic behandelt!");

                            //Prevent Weapon-Switch after Treatment
                            destinationDbPlayer.TimeSinceTreatment = DateTime.Now;
                            destinationDbPlayer.RecentlyInjured = true;

                            dbPlayer.SendNewNotification($"Du hast den Patienten behandelt!");

                            // keine Behandlungskosten für Badmedics
                            if (dbPlayer.IsAMedic() || dbPlayer.TeamId == (uint)TeamTypes.TEAM_ARMY)
                            {
                                int khcosts = 0;

                                switch (destinationDbPlayer.EconomyIndex)
                                {
                                    case EconomyIndex.Low:
                                        khcosts = 50;
                                        break;
                                    case EconomyIndex.Mid:
                                        khcosts = 500;
                                        break;
                                    case EconomyIndex.Rich:
                                        khcosts = 1000;
                                        break;
                                    case EconomyIndex.Superrich:
                                        khcosts = 2000;
                                        break;
                                    case EconomyIndex.Jeff:
                                        khcosts = 4000;
                                        break;
                                }

                                Random random = new Random();

                                khcosts = random.Next((Convert.ToInt32(khcosts * 0.9)), (Convert.ToInt32(khcosts * 1.5)));

                                if (destinationDbPlayer.IsACop() && destinationDbPlayer.IsInDuty())
                                {
                                    khcosts = khcosts / 4; // wegen Beamter im Dienst weil Steuern etc...
                                }

                                if (destinationDbPlayer.InsuranceType > 0 || destinationDbPlayer.HasCopInsurance())
                                {
                                    // 1 wenn hat oder wenn nicht privat und copinsurance
                                    if (destinationDbPlayer.InsuranceType == 1 || (destinationDbPlayer.InsuranceType != 2 && destinationDbPlayer.HasCopInsurance()))
                                    {
                                        khcosts = khcosts / 2;
                                        destinationDbPlayer.SendNewNotification("Durch ihre Krankenversicherung wurden 50% der Behandlungskosten übernommen!");
                                    }
                                    else if (destinationDbPlayer.InsuranceType == 2)
                                    {
                                        khcosts = 0;
                                        destinationDbPlayer.SendNewNotification("Durch ihre private Krankenversicherung wurden 100% der Behandlungskosten übernommen!");
                                    }
                                }

                                if (khcosts > 0)
                                {
                                    destinationDbPlayer.SendNewNotification($"Für Ihre Behandlung wurden Krankenhauskosten von ${khcosts} berechnet und von ihrem Konto abgebucht!");
                                    destinationDbPlayer.TakeBankMoney(khcosts, "Behandlungskosten", true);
                                }
                            }

                            return;
                        }
                        else if(destinationDbPlayer.Injury.Id == InjuryModule.Instance.InjuryKrankentransport) // Krankentransport Bug
                        {
                            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicleFromTeamFilter(dbPlayer.Player.Position, (int)dbPlayer.TeamId, 15.0f, 4);

                            if (sxVehicle == null)
                            {
                                dbPlayer.SendNewNotification($"Kein Krankenwagen zum Transport in der naehe!");
                                return;
                            }

                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl),
                                    Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            if (dbPlayer.IsAGangster())
                            {
                                Chats.sendProgressBar(dbPlayer, 15000);
                                await Task.Delay(15000);
                            }
                            else
                            {
                                Chats.sendProgressBar(dbPlayer, 9000);
                                await Task.Delay(9000);
                            }
                            VehicleHandler.Instance.TrySetPlayerIntoVehicleOccupants(sxVehicle, destinationDbPlayer, 2);

                            await Task.Delay(1500);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.StopAnimation();
                            destinationDbPlayer.SetWayToKH();
                            destinationDbPlayer.Freeze(true);
                            destinationDbPlayer.Player.TriggerNewClient("noweaponsoninjury", true);
                            destinationDbPlayer.SendNewNotification($"Du wurdest transportbereit gemacht!");
                            dbPlayer.SendNewNotification($"Du hast den Patienten transportbereit gemacht!");

                            return;
                        }
                    }

                    // Normales Stabilisieren

                    uint Verbandskasten = 39;
                    //no first aid licence
                    if (dbPlayer.Lic_FirstAID[0] == 0)
                    {
                        dbPlayer.SendNewNotification("Dafür benötigst du eine Erste-Hilfe-Ausbildung!", title: "Erste Hilfe Schein");
                        return;
                    }

                    if (dbPlayer.Container.GetItemAmount(Verbandskasten) > 0)
                    {
                        if(destinationDbPlayer.Injury.StabilizedInjuryId == 0)
                        {
                            dbPlayer.SendNewNotification($"Person ist bereits stabilisiert!");

                            if (dbPlayer.Team.Id == (int)TeamTypes.TEAM_MEDIC || dbPlayer.ParamedicLicense) dbPlayer.SendNewNotification($"Veletzung: {destinationDbPlayer.Injury.Name}!");
                            return;
                        }
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            if (destinationDbPlayer.Injury.Id != 17)
                            {
                                Chats.sendProgressBar(dbPlayer, 25000);
                                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                                await Task.Delay(25000);
                                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured())
                                {
                                    dbPlayer.SendNewNotification("Stabilisierung fehlgeschlagen!");
                                    return;
                                }
                                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                                dbPlayer.StopAnimation();
                            }
                            if(dbPlayer.Team.Id == (int)TeamTypes.TEAM_MEDIC || dbPlayer.ParamedicLicense) dbPlayer.SendNewNotification($"Du hast {destinationDbPlayer.Injury.Name} stabilisiert!");
                            else dbPlayer.SendNewNotification($"Du hast die Verletzung stabilisiert!");
                            dbPlayer.Container.RemoveItem(Verbandskasten);

                            if (destinationDbPlayer.Injury.Id == InjuryModule.Instance.InjuryDeathScreenId)
                            {
                                destinationDbPlayer.SendNewNotification("Du kannst nichts mehr für diese Person tun...");
                                return;
                            }

                            destinationDbPlayer.Stabilize();
                        }));
                    }
                    else
                    {
                        dbPlayer.SendNewNotification($"Kein Verbandskoffer!");
                    }
                }
            }
        }

        [RemoteEventPermission(AllowedDeath = false, AllowedOnCuff = false, AllowedOnTied = false)]
        [RemoteEvent]
        public void computerCheck(Player player, uint type, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CanInteract()) return;
            if (dbPlayer.IsInAnimation()) return;

            // Kein Cop kein Reg Rang 9 und kein Laptop
            if (type == 1 && dbPlayer.Container.GetItemAmount(173) < 1)
            {
                if (!dbPlayer.IsACop() && !dbPlayer.IsAMedic() && !dbPlayer.IsGoverment() && !dbPlayer.Team.IsDpos()) return;
                if (dbPlayer.RageExtension.IsInVehicle && !player.Vehicle.GetVehicle().Team.IsCops() && !player.Vehicle.GetVehicle().Team.IsMedics() && !player.Vehicle.GetVehicle().Team.IsDpos()) return;
                else if (!dbPlayer.RageExtension.IsInVehicle
                    && player.Position.DistanceTo(new Vector3(440.971, -978.654, 31.690)) > 5.0f            // LSPD Oben
                    && player.Position.DistanceTo(new Vector3(-2347.05, 3269.65, 32.8107)) > 5.0f           // Army Tower
                    && player.Position.DistanceTo(new Vector3(-787.619, -710.327, 35.7604)) > 5.0f          // Justiz
                    && player.Position.DistanceTo(new Vector3(461.575, -988.992, 24.9149)) > 5.0f           // LSPD unten
                    && player.Position.DistanceTo(new Vector3(2107.16, 2929.5, -61.9019)) > 5.0f) return;   // FIB Computer
            }

            if (type == 1)
            {
                if (!dbPlayer.RageExtension.IsInVehicle)
                {
                    AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachment.TABLET);
                    NAPI.Player.PlayPlayerAnimation(player, (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@code_human_in_bus_passenger_idles@female@tablet@idle_a", "idle_a");
                }

                player.TriggerNewClient("openComputer");
            }
            else
            {
                if (dbPlayer.RankId == 1 || dbPlayer.RankId == 2 || dbPlayer.RankId == 3 || dbPlayer.RankId == 4 || dbPlayer.RankId == 5 || dbPlayer.RankId == 6 || dbPlayer.RankId == 8 || dbPlayer.RankId == 11)
                {
                    player.TriggerNewClient("openIpad");
                }
            }
        }

        [RemoteEvent]
        public void closeComputer(Player player, uint type, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (!dbPlayer.RageExtension.IsInVehicle)
                {
                    dbPlayer.StopAnimation();
                }

                if (type == 1)
                {
                    player.TriggerNewClient("closeComputer");
                }
                else
                {
                    player.TriggerNewClient("closeIpad");
                }
            }));
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_SHOW_LIC(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 12f) return;
            if (destinationDbPlayer.IsInjured()) return;

            dbPlayer.ShowLicenses(destinationPlayer);
            dbPlayer.SendNewNotification(
                "Sie haben Ihre Lizenzen gezeigt!");
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_TAKE_LIC(Player client, Player destinationPlayer, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            if (!dbPlayer.IsACop() || !dbPlayer.Duty) return;

            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 12f) return;

            if (!destinationDbPlayer.IsCuffed && !destinationDbPlayer.IsTied) return;

            destinationDbPlayer.ShowLicenses(client);
            dbPlayer.SendNewNotification("Sie haben sich die Lizenzen genommen!");
            destinationDbPlayer.SendNewNotification("Ein Beamter hat sich Ihre Lizenzen genommen!");
        }

        //TODO: Following Events Migrationwwwd
        [RemoteEventPermission]
        [RemoteEvent]
        public async Task ToggleCrouch(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            await NAPI.Task.WaitForMainThread(0);
            if (!player.HasSharedData("IsCrouched"))
            {
                player.SetSharedData("IsCrouched", true);
            }
            else
            {
                player.ResetSharedData("IsCrouched");
            }
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async Task Indicator(Player player, int indicator, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            if (!dbPlayer.RageExtension.IsInVehicle) return;

            await NAPI.Task.WaitForMainThread(0);
            if (!player.Vehicle.HasSharedData("INDICATOR_" + indicator))
            {
                player.Vehicle.SetSharedData("INDICATOR_" + indicator, true);
            }
            else
            {
                player.Vehicle.ResetSharedData("INDICATOR_" + indicator);
            }
            
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async Task Siren(Player player, string key)
        {
            // Not used anymore
            /*if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            if (!dbPlayer.RageExtension.IsInVehicle) return;
            
            await NAPI.Task.WaitForMainThread(0);
            if (!player.Vehicle.HasSharedData("SIREN"))
            {
                player.Vehicle.SetSharedData("SIREN", true);
            }
            else
            {
                player.Vehicle.ResetSharedData("SIREN");
            }*/
        }

        [RemoteEvent]
        public async Task Pressed_E(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            if (!Configuration.Instance.EKeyActivated)
            {
                dbPlayer.SendNewNotification("Der E-Muskel ist für ein paar Minuten deaktiviert!");
                return;
            }

            await Main.TriggerPlayerPoint(dbPlayer);
        }

        [RemoteEvent]
        public void Pressed_H(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            if (!dbPlayer.CanInteract()) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.PressedH)) return;
            if (dbPlayer.RageExtension.IsInVehicle) return;

            if (dbPlayer.HasData("handsup"))
            {
                dbPlayer.ResetData("handsup");
                dbPlayer.StopAnimation();
                return;
            }
            else
            {
                dbPlayer.SetData("handsup", 1);
                dbPlayer.PlayAnimation(49, "missfbi5ig_21",
                    "hand_up_scientist");
                return;
            }
        }

        [RemoteEvent]
        public async void Pressed_M(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            try
            {
                if (player == null) return;
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.Character == null || dbPlayer.Character.Clothes == null) return;

                // Anti Spam
                if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.PressedM))
                    return;
                
                if(dbPlayer.RageExtension.IsInVehicle)
                {
                    SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                    if(sxVeh != null && sxVeh.IsValid() && sxVeh.Data != null && (sxVeh.Data.ClassificationId == 9 || (sxVeh.Data.ClassificationId == 8 && sxVeh.teamid == (uint)TeamTypes.TEAM_ARMY)) && sxVeh.GpsTracker)
                    {
                        AirFlightControl.AirFlightControlModule.Instance.TurnOnOffFunkState(dbPlayer);
                        return;
                    }
                }
                else
                {
                    if(AirFlightControl.AirFlightControlModule.Instance.TowerPlayers.Contains(dbPlayer))
                    {
                        AirFlightControl.AirFlightControlModule.Instance.TurnOnOffFunkState(dbPlayer);
                        return;
                    }
                }

                if (dbPlayer.IsInAdminDuty() || dbPlayer.JailTime[0] > 0) return;

                await MobileClothModule.Instance.PlayerSwitchMaskState(dbPlayer);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void Pressed_T(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.PressedT)) return;

            ComponentManager.Get<ChatWindow>().Show()(dbPlayer);
        }
        
        [RemoteEvent]
        public async void Pressed_Num_0(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM0);
        }
        [RemoteEvent]
        public async void Pressed_Num_1(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM1);
        }

        [RemoteEvent]
        public async void Pressed_Num_2(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM2);
        }

        [RemoteEvent]
        public async void Pressed_Num_3(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM3);
        }
        [RemoteEvent]
        public async void Pressed_Num_4(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM4);
        }
        [RemoteEvent]
        public async void Pressed_Num_5(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM5);
        }
        [RemoteEvent]
        public async void Pressed_Num_6(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM6);
        }
        [RemoteEvent]
        public async void Pressed_Num_7(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM7);
        }
        [RemoteEvent]
        public async void Pressed_Num_8(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM8);
        }
        [RemoteEvent]
        public async void Pressed_Num_9(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            await Modules.Instance.OnKeyPressed(dbPlayer, Key.NUM9);
        }
        [RemoteEvent]
        public async Task Pressed_NUM(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()) return;

        }

        [RemoteEvent]
        public async void Pressed_K(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.PressedK)) return;

            await Main.TriggerPlayer_K(dbPlayer);
        }

        [RemoteEvent]
        public async void Pressed_L(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.PressedL)) return;

            await Main.TriggerPlayer_L(dbPlayer);
        }


        [RemoteEvent]
        public async void Pressed_J(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.PressedJ)) return;

            await Main.TriggerPlayer_J(dbPlayer);
        }

        [RemoteEvent]
        public async void Pressed_KOMMA(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsCuffed || dbPlayer.IsTied) return;
            if (!dbPlayer.CanInteract()) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.PressedKomma)) return;

            await new ItemsModuleEvents().useInventoryItem(player, 4, key);
        }

        [RemoteEvent]
        public async void Pressed_PUNKT(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsCuffed || dbPlayer.IsTied) return;
            if (!dbPlayer.CanInteract()) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.PressedPunkt)) return;

            await new ItemsModuleEvents().useInventoryItem(player, 5, key);
        }

        [RemoteEvent]
        public void requestPlayerSyncData(Player player, Player requestedPlayer, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = requestedPlayer.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.Character == null) return;


            //Prop Sync
            Dictionary<int, uint> equipedProps = dbPlayer.Character.EquipedProps;
            var propsToSync = new Dictionary<int, List<int>>();

            foreach (var kvp in equipedProps.ToList())
            {
                var prop = PropModule.Instance[kvp.Value];
                if (prop == null) continue;

                var propValues = new List<int>
                {
                    prop.Variation,
                    prop.Texture
                };

                propsToSync.Add(kvp.Key, propValues);
            }

            //New clothes sync
            var clothesToSync = new Dictionary<int, List<int>>();
            try
            {
                clothesToSync = dbPlayer.HasData("clothes") ?
                ((Dictionary<int, List<int>>)dbPlayer.GetData("clothes")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : clothesToSync;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
            
            // Animation Sync
            bool HasAnimation = dbPlayer.PlayingAnimation;
            string CurrentAnimation = dbPlayer.AnimationName;
            int AnimationFlags = dbPlayer.CurrentAnimFlags;
            string CurrentAnimationDict = dbPlayer.AnimationDict;
            float AnimationSpeed = dbPlayer.AnimationSpeed;

            bool crouch = false;
            if (dbPlayer.HasData("isCrouched")) crouch = true;
            
            AnimationSyncItem animationSyncItem = new AnimationSyncItem(HasAnimation, CurrentAnimationDict, CurrentAnimation, AnimationFlags, AnimationSpeed, dbPlayer.Player.Heading);

            player.TriggerNewClient("responsePlayerSyncData", requestedPlayer,
                dbPlayer.HasData("alkTime"),
                JsonConvert.SerializeObject(animationSyncItem),
                crouch,
                JsonConvert.SerializeObject(propsToSync),
                JsonConvert.SerializeObject(clothesToSync));
        }
        
        [RemoteEventPermission(AllowedDeath = false, AllowedOnCuff = false, AllowedOnTied = false)]
        [RemoteEvent]
        public void Keks(Player player, bool state, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            if (player.IsReloading) return;
            if (dbPlayer.IsInAnimation()) return;
            if (dbPlayer.ActionsBlocked) return;

            if (dbPlayer.Container.GetItemAmount(174) < 1) return;

            // Anti Spam
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.Smartphone)) return;

            if (state && !dbPlayer.RageExtension.IsInVehicle)
            {
                if (!dbPlayer.CanInteract()) return;
                AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachment.HANDY);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_stand_mobile@male@text@base", "base");
            }

            if (!state && !dbPlayer.RageExtension.IsInVehicle)
            {
                dbPlayer.StopAnimation();
            }

            //Call remote trigger phone
            player.TriggerNewClient("hatNudeln", state);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void changeVoiceRange(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            // 1 = normal, 2 = whisper, 3 = schreien 4 (optional) = megaphone
            int voicetype = 1;
            if (dbPlayer.HasData("voiceType"))
            {
                voicetype = dbPlayer.GetData("voiceType");
            }

            if (dbPlayer.JailTime[0] > 0) return; // in jail ignore it...

            if (voicetype == 1)
            {
                player.SetSharedData("voiceRange", (int)VoiceRangeTypes.Shout);
                dbPlayer.SetData("voiceType", 2);
                player.TriggerNewClient("setVoiceType", 2);
            }
            else if (voicetype == 2)
            {
                player.SetSharedData("voiceRange", (int)VoiceRangeTypes.Whisper);
                dbPlayer.SetData("voiceType", 3);
                player.TriggerNewClient("setVoiceType", 3);
            }
            else if (voicetype == 3)
            {
                if (dbPlayer.CanUseMegaphone())
                {
                    player.SetSharedData("voiceRange", (int)VoiceRangeTypes.Megaphone);
                    dbPlayer.SetData("voiceType", 4);
                    player.TriggerNewClient("setVoiceType", 4);
                }
                else
                {
                    player.SetSharedData("voiceRange", (int)VoiceRangeTypes.Normal);
                    dbPlayer.SetData("voiceType", 1);
                    player.TriggerNewClient("setVoiceType", 1);
                }
            }
            else if (voicetype == 4)
            {
                player.SetSharedData("voiceRange", (int)VoiceRangeTypes.Normal);
                dbPlayer.SetData("voiceType", 1);
                player.TriggerNewClient("setVoiceType", 1);
            }
        }
    }
}