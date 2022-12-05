using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Food(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "mp_player_inteat@burger", "mp_player_int_eat_burger");
            dbPlayer.SetCannotInteract(true);
            
            await Task.Delay(5000);

            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();
            return true;
        }

        public static async Task<bool> AttachedFood(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            if (!int.TryParse(ItemData.Script.Split("_")[1], out int type)) return false;

            dbPlayer.StopAnimation();

            Module.Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, type);

            dbPlayer.StopAnimation(Module.Players.PlayerAnimations.AnimationLevels.User, true);

            await Task.Delay(500);

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "mp_player_inteat@burger", "mp_player_int_eat_burger");
            dbPlayer.SetCannotInteract(true);
            await Task.Delay(5000);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();
            return true;
        }
    }
}