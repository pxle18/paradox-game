using GTANetworkAPI;
using System.Linq;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Animal
{
    public class AnimalEvent : Script
    {

        [RemoteEvent]
        public void Pessed_B_Aiming(Player player, Player target, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (target != null)
            {
                DbPlayer targetPlayer = target.GetPlayer();

                if (targetPlayer != null && targetPlayer.IsValid() && !targetPlayer.isInjured())
                {
                    if (dbPlayer.PlayerPed != null && dbPlayer.PlayerPed.Spawned)
                    {
                        dbPlayer.PlayerPed.Attack(targetPlayer.Player);
                        dbPlayer.SendNewNotification("Attack " + targetPlayer.GetName());
                    }
                }
            }
        }
    }
}
