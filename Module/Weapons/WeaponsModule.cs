using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR.Module.Weapons
{
    public sealed class WeaponsModule : Module<WeaponsModule>
    {
        public override void OnPlayerEnterVehicle(DbPlayer dbPlayer, Vehicle vehicle, sbyte seat)
        {
            Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(0);
                NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed);

                await NAPI.Task.WaitForMainThread(500);
                NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed);
            });
        }

        public override void OnPlayerExitVehicle(DbPlayer dbPlayer, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed);
            }, 500);
        }

        public override void OnPlayerWeaponSwitch(DbPlayer dbPlayer, WeaponHash oldgun, WeaponHash newgun)
        {
            if (dbPlayer.RageExtension.IsInVehicle)
            {
                if (IsRestrictedForDriveBy(newgun))
                {
                    if (dbPlayer.Id == 33655)
                    {
                        dbPlayer.SendNewNotification($"[DEBUG] OnPlayerWeaponSwitch - Forbidden Driveby Weapon!");

                        NAPI.Task.Run(() =>
                        {
                            NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed);
                        });
                    }
                }
                else if (!dbPlayer.CanInteract())
                {
                    if (dbPlayer.Id == 33655)
                    {
                        dbPlayer.SendNewNotification($"[DEBUG] OnPlayerWeaponSwitch - CanInteract == false!");

                        NAPI.Task.Run(() =>
                        {
                            NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed);
                        });
                    }
                }
                else if (dbPlayer.RecentlyInjured && dbPlayer.TimeSinceTreatment.AddMinutes(15) > DateTime.Now)
                {
                    if (dbPlayer.Id == 33655)
                    {
                        dbPlayer.SendNewNotification($"[DEBUG] OnPlayerWeaponSwitch - RecentlyInjured triggered!");

                        NAPI.Task.Run(() =>
                        {
                            NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed);
                        });
                    }
                }
            }

            if (dbPlayer.Player.Dimension != (uint)9999)
            {
                VMP_CNR.Anticheat.Anticheat.CheckForbiddenWeapons(dbPlayer);
                dbPlayer.ResyncWeaponAmmo(false);
                var l_WeaponDatas = WeaponDataModule.Instance.GetAll();

                int l_OldGun = 0;
                int l_NewGun = 0;
                if (dbPlayer.DimensionType[0] != DimensionType.Paintball)
                {
                    foreach (var l_Data in l_WeaponDatas)
                    {
                        if (l_OldGun != 0 && l_NewGun != 0)
                            break;

                        if (l_Data.Value.Hash != (int)oldgun && l_Data.Value.Hash != (int)newgun)
                            continue;

                        if (l_Data.Value.Hash == (int)oldgun)
                        {
                            if (dbPlayer.Weapons.Exists(detail => detail.WeaponDataId == l_Data.Key))
                            {
                                l_OldGun = l_Data.Value.Id;
                            }
                            else
                            {
                                if (oldgun != WeaponHash.Unarmed)
                                    dbPlayer.RemoveServerWeapon(oldgun); // Interessant

                                continue;
                            }
                        }
                        else if (l_Data.Value.Hash == (int)newgun)
                        {
                            if (!dbPlayer.Weapons.Exists(detail => detail.WeaponDataId == l_Data.Value.Id))
                            {
                                if (newgun != WeaponHash.Unarmed)
                                    dbPlayer.RemoveServerWeapon(newgun); // Interessant

                                continue;
                            }

                            l_NewGun = l_Data.Value.Id;
                            dbPlayer.Player.TriggerNewClient("setCurrentWeapon", l_NewGun);
                        }
                    }
                }
            }
        }

        public bool IsRestrictedForDriveBy(WeaponHash p_Weapon)
        {
            return (p_Weapon == WeaponHash.Microsmg ||
                    p_Weapon == WeaponHash.Minismg ||
                    p_Weapon == WeaponHash.Machinepistol ||
                    p_Weapon == WeaponHash.Revolver);
        }
    }
}
