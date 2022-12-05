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
        public static async Task<bool> Drink(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "amb@world_human_drinking@coffee@male@idle_a", "idle_a");
            dbPlayer.SetCannotInteract(true);
            await NAPI.Task.WaitForMainThread(5000);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();
            return true;
        }
        public static async Task<bool> AttachedDrink(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;


            if (!int.TryParse(ItemData.Script.Split("_")[1], out int type)) return false;

            dbPlayer.StopAnimation();

            Module.Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, type);

            dbPlayer.StopAnimation(Module.Players.PlayerAnimations.AnimationLevels.User, true);

            await NAPI.Task.WaitForMainThread(500);

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "amb@world_human_drinking@coffee@male@idle_a", "idle_a");
            dbPlayer.SetCannotInteract(true);
            await NAPI.Task.WaitForMainThread(5000);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();
            return true;
        }
    }
}