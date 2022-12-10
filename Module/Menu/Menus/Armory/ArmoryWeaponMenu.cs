using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Staatskasse;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR
{
    public class ArmoryWeaponMenuBuilder : MenuBuilder
    {
        public ArmoryWeaponMenuBuilder() : base(PlayerMenu.ArmoryWeapons)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Armory Waffen");

            menu.Add(GlobalMessages.General.Close(), "");
            
            menu.Add("Zurueck", "");
            
            if (!dbPlayer.HasData("ArmoryId")) return menu;
            var ArmoryId = dbPlayer.GetData("ArmoryId");
            Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return menu;
            
            foreach (var ArmoryWeapon in Armory.ArmoryWeapons)
            {
                menu.Add("R: " + ArmoryWeapon.GetDefconRequiredRang() + " " + ArmoryWeapon.WeaponName + (ArmoryWeapon.Price > 0 ? (" ($" + ArmoryWeapon.Price + ") ") : ""));
            }

            if(Armory.Id == 3 || Armory.Id == 17 || Armory.Id == 19)
            {
                menu.Add("R: 10 SpecialCarbineMK2");
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
                if (!dbPlayer.HasData("ArmoryId")) return false;
                var ArmoryId = dbPlayer.GetData("ArmoryId");
                Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
                if (Armory == null) return false;

                if (index == 0)
                {
                    MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.ArmoryWeapons);
                    return false;
                }
                else if (index == 1)
                {
                    MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.Armory);
                    return false;
                }
                else {

                    if(index == Armory.ArmoryWeapons.Count()+2)
                    {
                        if (Armory.Id == 3 || Armory.Id == 17 || Armory.Id == 19)
                        {
                            if(dbPlayer.TeamId == (uint)TeamTypes.TEAM_ARMY && dbPlayer.TeamRank < 10)
                            {
                                return false;
                            }

                            if (!dbPlayer.CanWeightAdded(12))
                            {
                                dbPlayer.SendNewNotification("Sie haben nicht genug Platz!");
                                return false;
                            }

                            dbPlayer.GiveWeapon((WeaponHash)2526821735, 999);
                        }
                    }

                    int actualIndex = 0;
                    foreach (ArmoryWeapon ArmoryWeapon in Armory.ArmoryWeapons)
                    {
                        if (actualIndex == index - 2)
                        {
                            // Rang check
                            if (dbPlayer.TeamRank < ArmoryWeapon.GetDefconRequiredRang())
                            {
                                dbPlayer.SendNewNotification(
                                    "Sie haben nicht den benötigten Rang fuer diese Waffe!");
                                return false;
                            }

                            if (!dbPlayer.IsInDuty() && !dbPlayer.IsNSADuty)
                            {
                                dbPlayer.SendNewNotification(
                                    "Sie muessen dafuer im Dienst sein!");
                                return false;
                            }

                            // Check Armor
                            if (Armory.GetPackets() < ArmoryWeapon.Packets)
                            {
                                dbPlayer.SendNewNotification(
                                    $"Die Waffenkammer hat nicht mehr genuegend Waffenkisten! (Benötigt: {ArmoryWeapon.Packets} )");
                                return false;
                            }                                                                                  

                            if (ArmoryWeapon.Price > 0 && !dbPlayer.TakeBankMoney(ArmoryWeapon.Price))
                            {
                                dbPlayer.SendNewNotification(
                                    $"Diese Waffe kostet {ArmoryWeapon.Price}$ (Bank)!");
                                return false;
                            }

                            // Beamter Remove...
                            if(dbPlayer.IsACop())
                            {
                                WeaponData weaponData = WeaponDataModule.Instance.GetAll().Values.Where(wd => (WeaponHash)wd.Hash == ArmoryWeapon.Weapon).FirstOrDefault();

                                if (weaponData != null)
                                {
                                    // Spieler besitzt aktuelle Waffe
                                    if(dbPlayer.Weapons.Where(w => w.WeaponDataId == weaponData.Id).Count() > 0)
                                    {
                                        dbPlayer.RemoveWeapon(ArmoryWeapon.Weapon);
                                        dbPlayer.GiveBankMoney(ArmoryWeapon.Price, $"Rückzahlung - Dienstwaffe: {ArmoryWeapon.WeaponName}");
                                        dbPlayer.SendNewNotification($"${ArmoryWeapon.Price} als Rückzahlung für {ArmoryWeapon.WeaponName} erhalten!");
                                    }
                                }
                            }

                            // Find Item..
                            var weapon = ItemModelModule.Instance.GetByScript($"bw_{ArmoryWeapon.Weapon.ToString()}");
                            if (weapon == null) return false;

                            if (dbPlayer.TeamId == (uint)TeamTypes.TEAM_SWAT)
                            {
                                if (!dbPlayer.CanWeaponAdded(ArmoryWeapon.Weapon))
                                {

                                    dbPlayer.SendNewNotification("Sie haben nicht genug Platz!");
                                    return false;
                                }
                                dbPlayer.GiveWeapon(ArmoryWeapon.Weapon, 999);
                            }
                            else
                            {
                                if (!dbPlayer.Container.CanInventoryItemAdded(weapon))
                                {
                                    dbPlayer.SendNewNotification("Sie haben nicht genug Platz!");
                                    return false;
                                }

                                dbPlayer.Container.AddItem(weapon);
                            }
                            if (ArmoryWeapon.Price > 0)
                            {
                                dbPlayer.SendNewNotification($"{ArmoryWeapon.WeaponName} für ${ArmoryWeapon.Price} entnommen!");
                                KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, -ArmoryWeapon.Price);
                            }


                            Armory.RemovePackets(ArmoryWeapon.Packets);
                            return false;
                        }

                        actualIndex++;
                    }

                }

                return false;
            }
        }
    }
}
