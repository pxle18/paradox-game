using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Players;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool WeaponUnpack(DbPlayer dbPlayer, ItemModel ItemData, Item item)
        {
            string weaponstring = ItemData.Script.ToLower().Replace("w_", "");

            WeaponData weaponData = WeaponDataModule.Instance.GetAll().Where(d => d.Value.Name.ToLower().Equals(weaponstring)).FirstOrDefault().Value;

            if (weaponData == null) return false;

            WeaponHash weapon = (WeaponHash)weaponData.Hash;

            if (!dbPlayer.CanWeaponAdded(weapon)) return false;

            if (dbPlayer.Weapons.Where(w => w.WeaponDataId == weaponData.Id).Count() > 0)
            {
                dbPlayer.SendNewNotification("Diese Waffe ist bereits ausgerüstet", PlayerNotification.NotificationType.ERROR);
                return false;
            }

            if (!dbPlayer.Team.CanWeaponEquipedForTeam(weapon))
            {
                dbPlayer.SendNewNotification("Diese Waffe können Sie nicht ausrüsten!");
                return false;
            }

            List<uint> Components = new List<uint>();
            if(item.Data != null && item.Data.ContainsKey("components"))
            {
                Components = NAPI.Util.FromJson<List<uint>>(item.Data["components"]);
            }

            dbPlayer.SendNewNotification("Sie haben Ihre Waffe ausgeruestet!");

            int defaultammo = 0;
            if (weapon == WeaponHash.Molotov || weapon == WeaponHash.Grenade ||
                weapon == WeaponHash.Flare)
            {
                defaultammo = 1;
            }

            if (weapon == WeaponHash.Snowball)
            {
                defaultammo = 10;
            }

            dbPlayer.GiveWeapon(weapon, defaultammo);

            if(Components.Count > 0)
            {
                foreach(uint compId in Components)
                {
                    Weapons.Component.WeaponComponent comp = WeaponComponentModule.Instance.Get((int)compId);
                    if (comp != null) dbPlayer.GiveWeaponComponent((uint)weapon, comp.Hash);
                }
            }

            return true;
        }

        public static bool WeaponUnpackCop(DbPlayer dbPlayer, ItemModel ItemData, Item item)
        {
            string weaponstring = ItemData.Script.ToLower().Replace("bw_", "");

            if (!dbPlayer.IsCopPackGun()) return false;

            WeaponData weaponData = WeaponDataModule.Instance.GetAll().Where(d => d.Value.Name.ToLower().Equals(weaponstring)).FirstOrDefault().Value;

            if (weaponData == null) return false;

            WeaponHash weapon = (WeaponHash)weaponData.Hash;

            if (!dbPlayer.CanWeaponAdded(weapon)) return false;

            if (!dbPlayer.Team.CanWeaponEquipedForTeam(weapon))
            {
                dbPlayer.SendNewNotification("Diese Waffe können Sie nicht ausrüsten!");
                return false;
            }

            List<uint> Components = new List<uint>();
            if (item.Data != null && item.Data.ContainsKey("components"))
            {
                Components = NAPI.Util.FromJson<List<uint>>(item.Data["components"]);
            }

            dbPlayer.SendNewNotification("Sie haben Ihre Waffe ausgeruestet!");

            int defaultammo = 0;
            if (weapon == WeaponHash.Molotov || weapon == WeaponHash.Grenade ||
                weapon == WeaponHash.Flare)
            {
                defaultammo = 1;
            }

            if (weapon == WeaponHash.Snowball)
            {
                defaultammo = 10;
            }

            dbPlayer.GiveWeapon(weapon, defaultammo);

            if (Components.Count > 0)
            {
                foreach (uint compId in Components)
                {
                    Weapons.Component.WeaponComponent comp = WeaponComponentModule.Instance.Get((int)compId);
                    if (comp != null) dbPlayer.GiveWeaponComponent((uint)weapon, comp.Hash);
                }
            }

            return true;
        }

        public static async Task<bool> ZerlegteWaffeUnpack(DbPlayer dbPlayer, ItemModel ItemData, Item item)
        {
            string weaponstring = ItemData.Script.ToLower().Replace("zw_", "");

            if (!dbPlayer.IsAGangster()) return false;

            if (weaponstring.Length <= 0 || !uint.TryParse(weaponstring, out uint WeaponItemId))
            {
                return false;
            }
            int time = 2500; // 2,5 sek
            Chats.sendProgressBar(dbPlayer, time);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);

            dbPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

            await NAPI.Task.WaitForMainThread(time);

            dbPlayer.Container.RemoveItem(ItemData, 1);
            dbPlayer.Container.AddItem(WeaponItemId, 1, new Dictionary<string, dynamic>() { { "fingerprint" , dbPlayer.GetName() } });

            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetData("userCannotInterrupt", false);
            dbPlayer.StopAnimation();
            return false; // wird ja durch den Remove schon gemacht.
        }
    }
}
