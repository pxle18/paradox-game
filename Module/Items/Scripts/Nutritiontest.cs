using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Injury.InjuryMove;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> NutritionTest(DbPlayer dbPlayer)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            if (!dbPlayer.IsAMedic() || !dbPlayer.IsInDuty()) return false;

            DbPlayer target = Players.Players.Instance.GetClosestPlayerForPlayer(dbPlayer);
            if (target == null || !target.IsValid()) return false;

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "amb@prop_human_parking_meter@male@base", "base");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetCannotInteract(true);
            Chats.sendProgressBar(dbPlayer, 10000);
            await NAPI.Task.WaitForMainThread(10000);
            dbPlayer.StopAnimation(AnimationLevels.User, true);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            if (target == null || !target.IsValid() || target.Player.Position.DistanceTo(dbPlayer.Player.Position) > 4.0) return false;

            dbPlayer.SendNewNotification($"Test durchgeführt: Kcal {target.food[0]}, Wasser {target.drink[0]}", PlayerNotification.NotificationType.STANDARD, "", 10000);
            return true;
        }
    }

    public static partial class ItemScript
    {
        public static async Task<bool> MedicStationaer(DbPlayer dbPlayer)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            if (!dbPlayer.IsAMedic() || !dbPlayer.IsInDuty()) return false;

            DbPlayer target = Players.Players.Instance.GetClosestInjuredForPlayer(dbPlayer, 2.5f);
            if (target == null || !target.IsValid()) return false;

            if (!target.isInjured()) return false;

            InjuryMovePoint injuryMovePoint = InjuryMoveModule.Instance.GetAll().Values.Where(ip => ip.Position.DistanceTo(target.Player.Position) < 2.0f).FirstOrDefault();

            if (injuryMovePoint == null) return false;

            if (!target.HasData("injuredName")) return false;

            string note = target.GetData("injuredName");

            dbPlayer.SendNewNotification($"Person wurde mit {note} eingeliefert!", PlayerNotification.NotificationType.STANDARD, "", 10000);
            return true;
        }
    }
}