using GTANetworkAPI;
using System;
using System.Linq;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Staatskasse;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR
{
    public class ArmoryArmorMenuBuilder : MenuBuilder
    {
        public ArmoryArmorMenuBuilder() : base(PlayerMenu.ArmoryArmorMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Armory Rüstungen");

            menu.Add(GlobalMessages.General.Close(), "");

            if (!dbPlayer.HasData("ArmoryId")) return menu;
            var ArmoryId = dbPlayer.GetData("ArmoryId");
            Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return menu;

            foreach (var ArmoryWeapon in Armory.ArmoryArmors)
            {
                menu.Add("R: " + ArmoryWeapon.RestrictedRang + " " + ArmoryWeapon.Name);
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
                else
                {
                    int actualIndex = 0;
                    foreach (ArmoryArmor armoryArmor in Armory.ArmoryArmors)
                    {
                        if (actualIndex == index - 1)
                        {
                            // Rang check
                            if (dbPlayer.TeamRank < armoryArmor.RestrictedRang)
                            {
                                dbPlayer.SendNewNotification(
                                    "Sie haben nicht den benötigten Rang für diese Schutzweste!");
                                return false;
                            }

                            if (!dbPlayer.IsInDuty() && !dbPlayer.IsNSADuty)
                            {
                                dbPlayer.SendNewNotification(
                                    "Sie müssen dafür im Dienst sein!");
                                return false;
                            }

                            if(dbPlayer.Player.Armor < 90)
                            {
                                dbPlayer.SendNewNotification(
                                    "Sie müssen zuerst eine Schutzweste anziehen um das Aussehen zu ändern!");
                                return false;
                            }

                            dbPlayer.VisibleArmorType = armoryArmor.VisibleArmorType;
                            dbPlayer.SaveArmorType(armoryArmor.VisibleArmorType);
                            dbPlayer.SetArmor(armoryArmor.ArmorValue, true);
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
