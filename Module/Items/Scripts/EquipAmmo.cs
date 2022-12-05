using System;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.Weapons.Data;
//Possible problem. Removed on use, but not possible to add without weapon. Readd item?
namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> EquipAmo(DbPlayer dbPlayer, ItemModel ItemData, int Amount)
        {
            try
            {
                if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;

                string[] parts = ItemData.Script.ToLower().Replace("ammo_", "").Split('_');
                if (parts.Length < 1) return false;

                string weaponstring = parts[0];
                int BulletsInMagazine = Convert.ToInt32(parts[1]);

                WeaponData weaponData = WeaponDataModule.Instance.GetAll().Values.Where(d => d.Name.ToLower().Equals(weaponstring)).FirstOrDefault();

                if (weaponData == null) return false;

                if (dbPlayer.Weapons.Count == 0 || !dbPlayer.Weapons.Exists(detail => detail.WeaponDataId == weaponData.Id))
                {
                    dbPlayer.SendNewNotification(
                        "Sie müssen diese Waffe ausgerüstet haben!");
                    return false;
                }

                var l_Details = dbPlayer.Weapons.FirstOrDefault(detail => detail.WeaponDataId == weaponData.Id);

                if (!Int32.TryParse(parts[1], out int magazineSize)) return false;

                if(l_Details.Ammo > magazineSize*35)
                {
                    dbPlayer.SendNewNotification("Sie haben bereits die maximale Anzahl an Magazinen ausgerüstet!");
                    return false;
                }

                int addableAmmoAmount = Amount;

                int actualMags = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(l_Details.Ammo / BulletsInMagazine)));

                if ((actualMags + Amount) > 35)
                {
                    addableAmmoAmount = 35 - actualMags;
                }

                dbPlayer.SetData("no-packgun", true);
                dbPlayer.SetData("packgun-timestamp", DateTime.Now);
                dbPlayer.SetCannotInteract(true);
                Chats.sendProgressBar(dbPlayer, 800 * addableAmmoAmount);
                dbPlayer.SetCannotInteract(false);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);

                dbPlayer.Container.RemoveItem(ItemData, addableAmmoAmount);

                int l_AmmoToAdd = 0;
                for (int i = 0; i < addableAmmoAmount; i++)
                {
                    if (l_Details.Ammo > magazineSize * 35)
                        break;

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@weapons@first_person@aim_rng@generic@pistol@minismg@str", "reload_aim", 8, true);

                    await NAPI.Task.WaitForMainThread(800);
                    dbPlayer.StopAnimation();
                    l_AmmoToAdd += Convert.ToInt32(parts[1]);
                }

                l_Details.Ammo += l_AmmoToAdd;
                dbPlayer.SetWeaponAmmo((WeaponHash)weaponData.Hash, l_Details.Ammo);
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.SetData("no-packgun", false);
                dbPlayer.SendNewNotification("Sie haben ein Magazin fuer Ihre Waffe ausgeruestet!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return false;
        }

        public static async Task<bool> EquipAmoCop(DbPlayer dbPlayer, ItemModel ItemData, int Amount)
        {
            try
            {
                if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;

                string[] parts = ItemData.Script.ToLower().Replace("bammo_", "").Split('_');
                string weaponstring = parts[0];

                if (!dbPlayer.IsCopPackGun()) return false;

                WeaponData weaponData = WeaponDataModule.Instance.GetAll().Where(d => d.Value.Name.ToLower().Equals(weaponstring)).FirstOrDefault().Value;

                if (weaponData == null) return false;

                if (dbPlayer.Weapons.Count == 0 || !dbPlayer.Weapons.Exists(detail => detail.WeaponDataId == weaponData.Id) || (int)dbPlayer.Player.CurrentWeapon != weaponData.Hash)
                {
                    dbPlayer.SendNewNotification(
                        "Sie müssen diese Waffe ausgerüstet haben!");
                    return false;
                }

                var l_Details = dbPlayer.Weapons.FirstOrDefault(detail => detail.WeaponDataId == weaponData.Id);

                if (!Int32.TryParse(parts[1], out int magazineSize)) return false;

                if (l_Details.Ammo > magazineSize * 35)
                {
                    dbPlayer.SendNewNotification("Sie haben bereits die maximale Anzahl an Magazinen ausgerüstet!");
                    return false;
                }

                int addableAmmoAmount = Amount;

                int actualMags = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(l_Details.Ammo / Convert.ToInt32(parts[1]))));

                if((actualMags+Amount) > 35)
                {
                    addableAmmoAmount = 35 - actualMags;
                }

                dbPlayer.SetData("no-packgun", true);
                dbPlayer.SetData("packgun-timestamp", DateTime.Now);
                dbPlayer.SetCannotInteract(true);
                Chats.sendProgressBar(dbPlayer, 800 * addableAmmoAmount);
                dbPlayer.SetCannotInteract(false);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);

                dbPlayer.Container.RemoveItem(ItemData, addableAmmoAmount);

                int l_AmmoToAdd = 0;
                for (int i = 0; i < addableAmmoAmount; i++)
                {
                    if (l_Details.Ammo > magazineSize * 35)
                        break;

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@weapons@first_person@aim_rng@generic@pistol@minismg@str", "reload_aim", 8, true);

                    await NAPI.Task.WaitForMainThread(800);
                    dbPlayer.StopAnimation();
                    l_AmmoToAdd += Convert.ToInt32(parts[1]);
                }

                l_Details.Ammo += l_AmmoToAdd;
                dbPlayer.SetWeaponAmmo((WeaponHash)weaponData.Hash, l_Details.Ammo);
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.SetData("no-packgun", false);
                dbPlayer.SendNewNotification("Sie haben ein Magazin fuer Ihre Waffe ausgeruestet!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return false;
        }

    }
}
