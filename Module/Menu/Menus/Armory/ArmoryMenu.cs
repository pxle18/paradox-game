using GTANetworkAPI;
using System.Linq;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Swat;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Weapons;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR
{
    public class ArmoryMenuBuilder : MenuBuilder
    {
        public ArmoryMenuBuilder() : base(PlayerMenu.Armory)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, $"Armory"); 

            menu.Add(GlobalMessages.General.Close(), "");
            
            if (!dbPlayer.HasData("ArmoryId")) return menu;
            var ArmoryId = dbPlayer.GetData("ArmoryId");
            Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return menu;
            
            menu.Description = $"{Armory.Packets} Pakete";

            menu.Add("Dienst verlassen", "Waffen, Munition und Schutzweste wird entfernt");
            menu.Add("Dienst betreten", "Sie registrieren sich fuer den Dienst");

            if (Armory.ArmoryItems.Count > 0 || Armory.ArmoryWeapons.Count > 0 || Armory.ArmoryArmors.Count > 0)
            {
                menu.Add("Schutzwesten", "Schutzwestenschrank öffnen");
                menu.Add("Waffen", "Waffenarsenal öffnen");
                menu.Add("Items", "Items öffnen");
                menu.Add("Munition", "Munitionsauswahl öffnen");
                menu.Add("Erweiterungen", "Waffenerweiterungen öffnen");
            }
            if (dbPlayer.HasSwatRights()) menu.Add("Swat-Dienst verlassen", "");
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
                    case 1:
                        if (Armory.AccessableTeams.Contains(TeamModule.Instance.Get((uint)TeamTypes.TEAM_IAA))) return false;
                        // Out of Duty
                        dbPlayer.SendNewNotification("Sie haben sich vom Dienst abgemeldet!", title:"Dienstende", notificationType:PlayerNotification.NotificationType.ERROR);
                        dbPlayer.SetArmor(0);
                        dbPlayer.SetDuty(false);

                        int back = 0;
                        foreach(WeaponDetail wdetail in dbPlayer.Weapons)
                        {
                            var WeaponData = WeaponDataModule.Instance.Get(wdetail.WeaponDataId);

                            // Weapon is in Armory
                            ArmoryWeapon armoryWeapon = Armory.ArmoryWeapons.Where(aw => aw.Weapon == (WeaponHash)WeaponData.Hash).FirstOrDefault();
                            if(armoryWeapon != null)
                            {
                                // Gebe 50% an Geld zurück
                                back += armoryWeapon.Price;
                            }
                        }
                        if(back > 0)
                        {
                            dbPlayer.SendNewNotification($"Sie haben ${back} als Rückzahlung für Ihr Equipment erhalten!");
                            dbPlayer.GiveBankMoney(back, "Rückzahlung - Dienstequipment");
                            back = 0;
                        }

                        dbPlayer.RemoveWeapons();
                        dbPlayer.ResetAllWeaponComponents();

                        if (dbPlayer.TeamId != (uint)TeamTypes.TEAM_SWAT)
                            dbPlayer.Team.SendNotification("Rang " + dbPlayer.TeamRank + " | " + $"{dbPlayer.GetName()} meldet sich vom Dienst ab.");

                        MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.Armory);
                        break;
                    case 2:
                        if (Armory.AccessableTeams.Contains(TeamModule.Instance.Get((uint)TeamTypes.TEAM_IAA))) return false;

                        if (dbPlayer.Suspension && (dbPlayer.IsACop() || dbPlayer.TeamId == (int)TeamTypes.TEAM_DPOS || dbPlayer.TeamId == (int)TeamTypes.TEAM_DRIVINGSCHOOL || dbPlayer.TeamId == (int)TeamTypes.TEAM_MEDIC))
                        {
                            dbPlayer.SendNewNotification("Sie koennen nicht in Dienst gehen, wenn sie suspendiert sind!");
                            return false;
                        }

                        // in Duty
                        dbPlayer.SendNewNotification("Sie haben sich zum Dienst gemeldet!", title: "Dienstbereit", notificationType:PlayerNotification.NotificationType.SUCCESS);
                        dbPlayer.SetDuty(true);
                        dbPlayer.SetHealth(100);

                        if (dbPlayer.TeamId != (uint)TeamTypes.TEAM_SWAT)
                            dbPlayer.Team.SendNotification("Rang " + dbPlayer.TeamRank + " | " + $"{dbPlayer.GetName()} meldet sich zum Dienst an.");

                        MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.Armory);
                        break;
                    case 3:
                        if(Armory.ArmoryArmors.Count <= 0)
                        {
                            dbPlayer.SendNewNotification("Ihr Team hat keine Schutzwesten verfügbar!");
                            MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.Armory);
                            return true;
                        }
                        // Westen
                        MenuManager.Instance.Build(PlayerMenu.ArmoryArmorMenu, dbPlayer).Show(dbPlayer);
                        break;
                    case 4:
                        if (Armory.ArmoryWeapons.Count <= 0)
                        {
                            dbPlayer.SendNewNotification("Ihr Team hat keine Waffen verfügbar!");
                            MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.Armory);
                            return true;
                        }
                        // Waffen
                        MenuManager.Instance.Build(PlayerMenu.ArmoryWeapons, dbPlayer).Show(dbPlayer);
                        break;
                    case 5:
                        if (Armory.ArmoryItems.Count <= 0)
                        {
                            dbPlayer.SendNewNotification("Ihr Team hat keine Gegenstände verfügbar!");
                            MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.Armory);
                            return true;
                        }
                        // Items
                        MenuManager.Instance.Build(PlayerMenu.ArmoryItems, dbPlayer).Show(dbPlayer);
                        break;
                    case 6:
                        if (Armory.ArmoryWeapons.Count <= 0)
                        {
                            dbPlayer.SendNewNotification("Ihr Team hat keine Munition verfügbar!");
                            MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.Armory);
                            return true;
                        }
                        // Ammo
                        MenuManager.Instance.Build(PlayerMenu.ArmoryAmmo, dbPlayer).Show(dbPlayer);
                        break;
                    case 7:
                        if (Armory.ArmoryWeapons.Count <= 0)
                        {
                            dbPlayer.SendNewNotification("Ihr Team hat keine Erweiterungen verfügbar!");
                            MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.Armory);
                            return true;
                        }
                        // Ammo
                        MenuManager.Instance.Build(PlayerMenu.ArmoryComponentMenu, dbPlayer).Show(dbPlayer);
                        break;
                    case 8:
                        if (dbPlayer.HasSwatRights() && dbPlayer.IsSwatDuty())
                        {
                            dbPlayer.SetSwatDuty(false);
                            break;
                        }
                        MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.Armory);
                        break;
                     default:
                        MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.Armory);
                        break;
                }

                return false;
            }
        }
    }
}
 