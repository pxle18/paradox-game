using System;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Anticheat;
using System.Collections.Generic;
using VMP_CNR.Module.Weapons.Data;
using VMP_CNR.Module.Weapons;
using VMP_CNR.Module.Weapons.Component;
using System.Linq;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.RemoteEvents;

namespace VMP_CNR.Module.Players
{
    public static class PlayerWeapon
    {
        public static int MaxPlayerWeaponWeight = 20;

        public static int MaxPlayerSWATWeaponWeight = 32;

        public static void GiveWeapon(this DbPlayer dbPlayer, WeaponHash weaponHash, int ammo, bool equipNow = false,
            bool loaded = false)
        {
            dbPlayer.GiveServerWeapon(weaponHash, ammo);
            dbPlayer.SetServerWeaponAmmo(weaponHash, ammo);
            
            var l_WeaponData = Weapons.Data.WeaponDataModule.Instance.GetAll().ToList().Where(wp => wp.Value.Hash == (int)weaponHash).FirstOrDefault();
            if(l_WeaponData.Value != null)
            { 
                Predicate<WeaponDetail> l_Detail = (WeaponDetail d) => { return d.WeaponDataId == l_WeaponData.Key; };
                if (dbPlayer.Weapons.Exists(l_Detail))
                    return;

                WeaponDetail l_Details = new WeaponDetail();
                l_Details.Ammo = ammo;
                l_Details.WeaponDataId = l_WeaponData.Key;
                l_Details.Components = new List<int>();

                dbPlayer.Weapons.Add(l_Details);
                //dbPlayer.Player.TriggerNewClient("fillWeaponAmmo", l_Details.WeaponDataId, l_Details.Ammo);
                //dbPlayer.Player.TriggerNewClient("setCurrentWeapon", l_Details.WeaponDataId);
            }
        }

        public static bool CanWeaponAdded(this DbPlayer dbPlayer, WeaponHash weaponHash)
        {

            var l_WeaponData = Weapons.Data.WeaponDataModule.Instance.GetAll().ToList().Where(wp => wp.Value.Hash == (int)weaponHash).FirstOrDefault();
            if (l_WeaponData.Value != null)
            {

                int weaponWeight = l_WeaponData.Value.Weight;
                if (l_WeaponData.Value.Id == 81 && (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())) // SMG und KEIN Cop
                {
                    weaponWeight = 9; // normal weight SMG
                }

                int maxWeight = MaxPlayerWeaponWeight;

                if(dbPlayer.TeamId == (uint)TeamTypes.TEAM_SWAT || dbPlayer.TeamId == (uint)TeamTypes.TEAM_ARMY)
                {
                    if(dbPlayer.Container.GetItemAmount(1134) >= 1)
                    {
                        maxWeight = MaxPlayerSWATWeaponWeight;
                    }
                }

                if (dbPlayer.GetPlayerWeaponsWeight() + weaponWeight > maxWeight)
                {
                    dbPlayer.SendNewNotification("Du kannst diese Waffe nicht tragen!", PlayerNotification.NotificationType.ERROR);
                    return false;
                }
                else return true;
            }
            dbPlayer.SendNewNotification("Du kannst diese Waffe nicht tragen!", PlayerNotification.NotificationType.ERROR);
            return false;
        }

        public static bool CanWeightAdded(this DbPlayer dbPlayer, int weight)
        {
            int maxWeight = MaxPlayerWeaponWeight;

            if (dbPlayer.TeamId == (uint)TeamTypes.TEAM_SWAT || dbPlayer.TeamId == (uint)TeamTypes.TEAM_ARMY)
            {
                if (dbPlayer.Container.GetItemAmount(1134) >= 1)
                {
                    maxWeight = MaxPlayerSWATWeaponWeight;
                }
            }

            if (dbPlayer.GetPlayerWeaponsWeight() + weight > maxWeight)
            {
                dbPlayer.SendNewNotification("Du kannst diese Waffe nicht tragen!", PlayerNotification.NotificationType.ERROR);
                return false;
            }
            else return true;
        }

        public static int GetPlayerWeaponsWeight(this DbPlayer dbPlayer)
        {
            int weight = 0;
            foreach( WeaponDetail weaponDetail in dbPlayer.Weapons)
            {
                WeaponData l_Data = WeaponDataModule.Instance[weaponDetail.WeaponDataId];
                if (l_Data == null) continue;

                int weaponWeight = l_Data.Weight;

                if (l_Data.Id == 81 && (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())) // SMG und KEIN Cop
                {
                    weaponWeight = 9; // PDW
                }

                if (l_Data.Id == 45 && dbPlayer.IsACop() && dbPlayer.IsInDuty())
                {
                    weaponWeight = 2; // sniperrifle
                }

                weight += weaponWeight;
            }
            return weight;
        }

        public static void LoadPlayerWeapons(this DbPlayer dbPlayer)
        {
            dbPlayer.SetData("ac-ignorews", 4);

            dbPlayer.RemoveAllServerWeapons(false);

            if (dbPlayer.Weapons.Count != 0)
            {
                foreach (var l_Detail in dbPlayer.Weapons.ToList())
                {
                    WeaponData l_Data = WeaponDataModule.Instance[l_Detail.WeaponDataId];
                    dbPlayer.GiveServerWeapon((WeaponHash)l_Data.Hash, l_Detail.Ammo);

                    /*
                    if (l_Detail.Components.Count > 0)
                    {
                        foreach (int l_CompID in l_Detail.Components)
                        {
                            VMP_CNR.Module.Weapons.Component.WeaponComponent l_Comp = WeaponComponentModule.Instance[l_CompID];

                            if (!int.TryParse(l_Comp.Hash, out int l_Hash))
                                continue;

                            dbPlayer.Player.SetWeaponComponent((WeaponHash)l_Data.Hash, (GTANetworkAPI.WeaponComponent)l_Hash);
                        }
                    }*/

                }

                dbPlayer.SyncPlayerWeaponComponents();
            }

            NAPI.Task.Run(() => { NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed); });
        }

        public static void RemoveWeapons(this DbPlayer dbPlayer)
        {
            dbPlayer.RemoveAllServerWeapons();
            dbPlayer.ResetAllWeaponComponents();
        }

        public static void RemoveWeapon(this DbPlayer dbPlayer, WeaponHash weapon)
        {
            var l_WeaponID = 0;
            var l_CurrentWeapon = 0;

            var l_WeaponDatas = WeaponDataModule.Instance.GetAll();
            foreach (var l_Weapon in l_WeaponDatas)
            {
                if (l_Weapon.Value.Hash != (int)weapon)
                    continue;

                l_WeaponID = l_Weapon.Key;
                break;
            }

            foreach (var l_Weapon in l_WeaponDatas)
            {
                if (l_Weapon.Value.Hash != (int)dbPlayer.Player.CurrentWeapon)
                    continue;

                l_CurrentWeapon = l_Weapon.Key;
                break;
            }

            WeaponData l_Data = WeaponDataModule.Instance.Get(l_WeaponID);
            if (l_Data == null)
                return;

            foreach (var l_Detail in dbPlayer.Weapons)
            {
                if (l_Detail.WeaponDataId != l_WeaponID)
                    continue;

                dbPlayer.Weapons.Remove(l_Detail);
                break;
            }

            dbPlayer.RemoveServerWeapon(weapon);
        }

        public static void SetWeaponAmmo(this DbPlayer dbPlayer, WeaponHash Weapon, int ammo)
        {
            var l_WeaponDatas = WeaponDataModule.Instance.GetAll();
            int l_WeaponID = 0;
            foreach (var l_Weapon in l_WeaponDatas)
            {
                if (l_Weapon.Value.Hash != (int)Weapon)
                    continue;

                l_WeaponID = l_Weapon.Key;
                break;
            }

            var weaponPlayer = dbPlayer.Weapons.FirstOrDefault(w => w.WeaponDataId == l_WeaponID);
            if(weaponPlayer != null)
            {
                weaponPlayer.Ammo = ammo;
            }
            else
            {
                WeaponDetail l_Details = new WeaponDetail();
                l_Details.Ammo = ammo;
                l_Details.WeaponDataId = l_WeaponID;
                l_Details.Components = new List<int>();

                dbPlayer.Weapons.Add(l_Details);
            }

            NAPI.Task.Run(() => { dbPlayer.Player.SetWeaponAmmo(Weapon, ammo); });
        }

        public static void ResyncWeaponAmmo(this DbPlayer dbPlayer, bool pIgnoreSpam = true)
        {
            if (!pIgnoreSpam)
            {
                if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.WeaponAmmoSync))
                    return;
            }

            //dbPlayer.Player.TriggerNewClient("getWeaponAmmo");
            NAPI.Task.Run(() =>
            {
                var lWeapons = dbPlayer.Weapons;
                foreach (var lData in lWeapons)
                {
                    lData.Ammo = dbPlayer.Player.GetWeaponAmmo((WeaponHash)WeaponDataModule.Instance.Get(lData.WeaponDataId).Hash);
                }
            });
        }
    }
}