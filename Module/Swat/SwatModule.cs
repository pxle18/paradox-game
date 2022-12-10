using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.Assets.Hair;
using VMP_CNR.Module.Assets.HairColor;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Barber.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tattoo.Windows;
using VMP_CNR.Module.Teams.Apps;
using VMP_CNR.Module.Teams.Permission;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Weapons;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR.Module.Swat
{
    public sealed class SwatModule : Module<SwatModule>
    {
        public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            if (dbPlayer.SwatDuty == 1)
            {
                dbPlayer.SetSwatDuty(true);
            }
        }
    }

    public static class SwatPlayerExtension
    {
        public static bool HasSwatRights(this DbPlayer dbPlayer)
        {
            return dbPlayer.Swat > 0 || dbPlayer.TeamId == (int)TeamTypes.TEAM_SWAT;
        }

        public static bool HasSwatLeaderRights(this DbPlayer dbPlayer)
        {
            return dbPlayer.TeamId == (int)TeamTypes.TEAM_SWAT && dbPlayer.TeamRank >= 11;
        }

        public static void SetSwatRights(this DbPlayer dbPlayer, bool leaderrights)
        {
            dbPlayer.Swat = leaderrights ? 2 : 1;
            dbPlayer.Save();
            return;
        }

        public static void RemoveSwatRights(this DbPlayer dbPlayer)
        {
            dbPlayer.Swat = 0;
            if(dbPlayer.TeamId == (int)TeamTypes.TEAM_SWAT)
            {
                dbPlayer.SetTeam((int)TeamTypes.TEAM_CIVILIAN);
                dbPlayer.SetTeamRankPermission(false, 0, false, "");
            }
            dbPlayer.Save();
            return;
        }

        public static void SetSwatDuty(this DbPlayer dbPlayer, bool duty)
        {
            if (duty)
            {
                dbPlayer.SetData("swatOld_team", dbPlayer.TeamId);
                dbPlayer.SetData("swatOld_rang", dbPlayer.TeamRank);
                dbPlayer.SetData("swatOld_rights_manage", dbPlayer.TeamRankPermission.Manage);
                dbPlayer.SetData("swatOld_rights_bank", dbPlayer.TeamRankPermission.Bank);
                dbPlayer.SetData("swatOld_rights_inv", dbPlayer.TeamRankPermission.Inventory);
                dbPlayer.SetData("swatOld_rights_title", dbPlayer.TeamRankPermission.Title);

                dbPlayer.SetTeam((int)TeamTypes.TEAM_SWAT, false);
                dbPlayer.SetTeamRankPermission(true, dbPlayer.HasSwatLeaderRights() ? 2 : 0, true, "");
                dbPlayer.SendNewNotification("Swatdienst angetreten!");
                dbPlayer.SetDuty(true);
                dbPlayer.SwatDuty = 1;
                dbPlayer.Player.TriggerNewClient("updateDuty", true);
            }
            else
            {
                // Revert Old Data
                dbPlayer.SetTeam(dbPlayer.GetData("swatOld_team"), false);
                dbPlayer.SetTeamRankPermission((bool)dbPlayer.GetData("swatOld_rights_bank"), (int)dbPlayer.GetData("swatOld_rights_manage"), (bool)dbPlayer.GetData("swatOld_rights_inv"), (string)dbPlayer.GetData("swatOld_rights_title"));
                dbPlayer.TeamRank = dbPlayer.GetData("swatOld_rang");

                dbPlayer.SendNewNotification("Swatdienst beendet!");
                dbPlayer.SetDuty(true);
                dbPlayer.SwatDuty = 0;
                dbPlayer.Player.TriggerNewClient("updateDuty", true);

                if (dbPlayer.HasData("ArmoryId"))
                {
                    int armoryId = dbPlayer.GetData("ArmoryId");
                    Armory.Armory armory = ArmoryModule.Instance.Get(armoryId);
                    if (armory != null)
                    {
                        int back = 0;
                        foreach (WeaponDetail wdetail in dbPlayer.Weapons)
                        {
                            var WeaponData = WeaponDataModule.Instance.Get(wdetail.WeaponDataId);

                            // Weapon is in Armory
                            ArmoryWeapon armoryWeapon = armory.ArmoryWeapons.Where(aw => aw.Weapon == (WeaponHash)WeaponData.Hash).FirstOrDefault();
                            if (armoryWeapon != null)
                            {
                                // Gebe 50% an Geld zurück
                                back += armoryWeapon.Price;
                            }
                        }
                        if (back > 0)
                        {
                            dbPlayer.SendNewNotification($"Sie haben ${back} als Rückzahlung für Ihr Equipment erhalten!");
                            dbPlayer.GiveBankMoney(back, "Rückzahlung - Dienstequipment");
                            back = 0;
                        }
                    }
                }

                dbPlayer.RemoveWeapons();
                dbPlayer.ResetAllWeaponComponents();
            }

            dbPlayer.UpdateApps();
            ComponentManager.Get<TeamListApp>().SendTeamMembers(dbPlayer);
        }

        public static bool IsSwatDuty(this DbPlayer dbPlayer)
        {
            if (dbPlayer.SwatDuty == 1 && dbPlayer.TeamId == (int)TeamTypes.TEAM_SWAT) return true;
            else return false;
        }
    }
}