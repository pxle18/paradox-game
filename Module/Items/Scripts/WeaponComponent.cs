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
        public static async Task<bool> EquipComponent(DbPlayer dbPlayer, ItemModel ItemData)
        {
            try
            {
                if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;

                string[] parts = ItemData.Script.ToLower().Replace("wc_", "").Split('_');

                if (!Int32.TryParse(parts[0], out int componentId)) return false;

                Weapons.Component.WeaponComponent component = WeaponComponentModule.Instance.Get(componentId);
                if (component == null) return false;

                WeaponData weaponData = WeaponDataModule.Instance.GetAll().Where(d => d.Value.Id == component.WeaponDataId).FirstOrDefault().Value;
                if (weaponData == null) return false;


                if (dbPlayer.Weapons.Count == 0 || !dbPlayer.Weapons.Exists(w => w.WeaponDataId == component.WeaponDataId) || (int)dbPlayer.Player.CurrentWeapon != weaponData.Hash)
                {
                    dbPlayer.SendNewNotification(
                        "Sie müssen diese Waffe ausgerüstet haben!");
                    return false;
                }

                if (dbPlayer.HasWeaponComponent((uint)weaponData.Hash, component.Hash))
                {
                    dbPlayer.SendNewNotification("Sie haben diese Modifikation bereits ausgerüstet!");
                    return false;
                }


                dbPlayer.SetData("no-packgun", true);
                dbPlayer.SetCannotInteract(true);
                Chats.sendProgressBar(dbPlayer, 5000);

                dbPlayer.SetCannotInteract(false);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);


                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base", 8, true);

                dbPlayer.Container.RemoveItem(ItemData, 1);

                await NAPI.Task.WaitForMainThread(5000);

                dbPlayer.StopAnimation();
                dbPlayer.GiveWeaponComponent((uint)weaponData.Hash, component.Hash);

                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.SetData("no-packgun", false);
                dbPlayer.SendNewNotification($"Sie {component.Name} ausgerüstet!");
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
