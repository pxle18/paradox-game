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

namespace VMP_CNR.Module.Menu.Menus.Armory
{
    public class ArmoryAmmoMenuBuilder : MenuBuilder
    {
        public ArmoryAmmoMenuBuilder() : base(PlayerMenu.ArmoryAmmo)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Armory Munition");

            menu.Add(GlobalMessages.General.Close(), "");

            menu.Add("Zurueck", "");

            if (!dbPlayer.HasData("ArmoryId")) return menu;
            var ArmoryId = dbPlayer.GetData("ArmoryId");
            VMP_CNR.Module.Armory.Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return menu;

            foreach (var ArmoryWeapon in Armory.ArmoryWeapons)
            {
                menu.Add("R: " + ArmoryWeapon.GetDefconRequiredRang() + " Munition " + ArmoryWeapon.WeaponName + " ($" + ArmoryWeapon.MagazinPrice + ") ");
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
                VMP_CNR.Module.Armory.Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
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
                else
                {
                    int actualIndex = 0;
                    foreach (ArmoryWeapon ArmoryWeapon in Armory.ArmoryWeapons)
                    {
                        if (actualIndex == index - 2)
                        {
                            var ammoprice = ArmoryWeapon.MagazinPrice;
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

                            if (ammoprice > 0 && !dbPlayer.TakeBankMoney(ammoprice))
                            {
                                dbPlayer.SendNewNotification(
                                    $"Diese Waffe kostet {ammoprice}$ (Bank)!");
                                return false;
                            }
                            
                            // Found
                            if (ArmoryWeapon.Weapon == WeaponHash.Grenade ||
                               ArmoryWeapon.Weapon == WeaponHash.Bzgas ||
                               ArmoryWeapon.Weapon == WeaponHash.Molotov ||
                               ArmoryWeapon.Weapon == WeaponHash.Stickybomb ||
                               ArmoryWeapon.Weapon == WeaponHash.Proximine ||
                               ArmoryWeapon.Weapon == WeaponHash.Snowball ||
                               ArmoryWeapon.Weapon == WeaponHash.Pipebomb ||
                               ArmoryWeapon.Weapon == WeaponHash.Ball ||
                               ArmoryWeapon.Weapon == WeaponHash.Smokegrenade ||
                               ArmoryWeapon.Weapon == WeaponHash.Flare ||
                               ArmoryWeapon.Weapon == WeaponHash.Petrolcan ||
                               ArmoryWeapon.Weapon == WeaponHash.Fireextinguisher ||
                               ArmoryWeapon.Weapon == WeaponHash.Parachute)
                            {
                                dbPlayer.SendNewNotification("Hierfuer ist keine Munition verfuegbar!");
                                return false;
                            }


                            // Find Item..
                            var weapon = ItemModelModule.Instance.GetByScript($"bammo_{ArmoryWeapon.Weapon.ToString()}");
                            if (weapon == null) return false;

                            if (!dbPlayer.Container.CanInventoryItemAdded(weapon))
                            {
                                dbPlayer.SendNewNotification("Sie haben nicht genug platz!");
                                return false;
                            }
                            dbPlayer.Container.AddItem(weapon);

                            if (ammoprice > 0)
                            {
                                dbPlayer.SendNewNotification($"Munition {ArmoryWeapon.WeaponName} für ${ammoprice} entnommen!");
                                KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, -ammoprice);
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
