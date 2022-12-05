using System.Collections.Generic;
using GTANetworkAPI;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Anticheat
{
    public static class Anticheat
    {
        public static void ValidePlayerComponents(Player player)
        {
            //var comps = player.GetAllWeaponComponents(player.CurrentWeapon);

            // Defaultweapons hasent any components so....
            /*foreach (var comp in comps)
            {
                Players.Instance.SendMessageToAuthorizedUsers("anticheat",
                    $"ANTICHEAT (WEAPON CLIP HACK) {player.Name} :: {comp}");
            }*/
        }

        private static readonly List<WeaponHash> ForbiddenWeapons =
        new List<WeaponHash>(new[] {
            WeaponHash.Railgun, WeaponHash.Rpg, WeaponHash.Minigun, WeaponHash.Proximine, WeaponHash.Stickybomb, WeaponHash.Pipebomb
        });

        public static void CheckForbiddenWeapons(DbPlayer dbPlayer)
        {
            var currW = dbPlayer.Player.CurrentWeapon;

            if (!ForbiddenWeapons.Contains(currW)) return;
        }
    }
}