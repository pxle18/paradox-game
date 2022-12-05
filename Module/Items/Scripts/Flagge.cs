using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Flagge(DbPlayer dbPlayer)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, 59, true);

            await GTANetworkAPI.NAPI.Task.WaitForMainThread(500);

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "amb@world_human_hang_out_street@female_hold_arm@base", "base");

            return true;
        }

        public static async Task<bool> Firemagic(DbPlayer dbPlayer)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, 77, true);

            await GTANetworkAPI.NAPI.Task.WaitForMainThread(500);

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "misschinese2_crystalmazemcs1_cs", "dance_loop_tao");

            return true;
        }
    }
}
