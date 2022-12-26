using System.Linq;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Staatskasse;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR
{
    public class ArmoryComponentMenuBuilder : MenuBuilder
    {
        public ArmoryComponentMenuBuilder() : base(PlayerMenu.ArmoryComponentMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Armory Components");

            menu.Add(GlobalMessages.General.Close(), "");
            menu.Add("Zurueck", "");

            if (!dbPlayer.HasData("ArmoryId")) return null;
            var ArmoryId = dbPlayer.GetData("ArmoryId");

            Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return null;
            foreach (var ArmoryItem in Armory.ArmoryWeaponComponents)
            {
                if (!WeaponComponentModule.Instance.GetAll().ContainsKey((int)ArmoryItem.WeaponComponentId)) continue;
                WeaponComponent weaponComponent = WeaponComponentModule.Instance.Get((int)ArmoryItem.WeaponComponentId);

                menu.Add((ArmoryItem.Price > 0 ? ("$" + ArmoryItem.Price + " ") : "") + weaponComponent.Name,
                    "");
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

                switch (index)
                {
                    case 0:
                        MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.ArmoryItems);
                        return false;
                    case 1:
                        MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.Armory);
                        return false;
                    default:
                        var actualIndex = 0;
                        foreach (var ArmoryItem in Armory.ArmoryWeaponComponents)
                        {
                            if (!WeaponComponentModule.Instance.GetAll().ContainsKey((int)ArmoryItem.WeaponComponentId)) continue;
                            WeaponComponent weaponComponent = WeaponComponentModule.Instance.Get((int)ArmoryItem.WeaponComponentId);

                            if (actualIndex == index - 2)
                            {
                                if (!dbPlayer.IsInDuty())
                                {
                                    dbPlayer.SendNewNotification(
                                        "Sie muessen dafuer im Dienst sein!");
                                    return false;
                                }

                                // Check Armory
                                if (Armory.GetPackets() < ArmoryItem.Packets)
                                {
                                    dbPlayer.SendNewNotification(
                                        $"Die Waffenkammer hat nicht mehr genuegend Materialien! (Benötigt: {ArmoryItem.Packets} )");
                                    return false;
                                }

                                var l_WeaponDatas = WeaponDataModule.Instance.GetAll();
                                var l_Weapon = l_WeaponDatas.Values.FirstOrDefault(data => data.Hash == (int)dbPlayer.Player.CurrentWeapon);
                                if (l_Weapon == null) return false;

                                if(l_Weapon.Id != weaponComponent.WeaponDataId)
                                {
                                    dbPlayer.SendNewNotification("Sie müssen diese Waffe ausgerüstet haben!");
                                    return false;
                                }

                                if(dbPlayer.HasWeaponComponent((uint)dbPlayer.Player.CurrentWeapon, weaponComponent.Hash))
                                {

                                    dbPlayer.SendNewNotification("Sie haben diese Modifizierung bereits!");
                                    return false;
                                }

                                if (ArmoryItem.Price > 0 && !dbPlayer.TakeBankMoney(ArmoryItem.Price))
                                {
                                    dbPlayer.SendNewNotification(
                                        $"Dieses Item kostet {ArmoryItem.Price}$ (Bank)!");
                                    return false;
                                }

                                dbPlayer.GiveWeaponComponent((uint)dbPlayer.Player.CurrentWeapon, weaponComponent.Hash);

                                Armory.RemovePackets(ArmoryItem.Packets);

                                if (ArmoryItem.Price > 0)
                                {
                                    dbPlayer.SendNewNotification($"{weaponComponent.Name} für ${ArmoryItem.Price} ausgerüstet!");
                                    KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, +ArmoryItem.Price);
                                }
                                return false;
                            }

                            actualIndex++;
                        }

                        break;
                }

                return false;
            }
        }
    }
}