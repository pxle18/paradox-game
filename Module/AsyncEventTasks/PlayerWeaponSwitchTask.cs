using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.Banks;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles.Garages;
using VMP_CNR.Module.Weapons.Component;

namespace VMP_CNR.Module.AsyncEventTasks
{
    public static partial class AsyncEventTasks
    {
        public static void PlayerWeaponSwitchTask(Player player, WeaponHash oldgun, WeaponHash newWeapon)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            if (!dbPlayer.IsValid()) return;

            Modules.Instance.OnPlayerWeaponSwitch(dbPlayer, oldgun, newWeapon);

            if (dbPlayer.IsCuffed)
            {
                dbPlayer.Player.PlayAnimation("mp_arresting", dbPlayer.RageExtension.IsInVehicle ? "sit" : "idle", 0);
            }

            if (dbPlayer.IsTied)
            {
                if (dbPlayer.RageExtension.IsInVehicle) dbPlayer.Player.PlayAnimation("mp_arresting", "sit", 0);
                else dbPlayer.Player.PlayAnimation("anim@move_m@prisoner_cuffed_rc", "aim_low_loop", 0);
            }

            if ((dbPlayer.Lic_Gun[0] <= 0 && dbPlayer.Level < 3) || dbPlayer.HasPerso[0] == 0)
            {
                dbPlayer.RemoveWeapons();
                dbPlayer.ResetAllWeaponComponents();
            }

            if (dbPlayer.PlayingAnimation)
            {
                NAPI.Player.SetPlayerCurrentWeapon(player, WeaponHash.Unarmed);
            }
        }
    }
}
