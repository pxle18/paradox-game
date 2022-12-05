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
        public static async Task<bool> Klappstuhl(DbPlayer dbPlayer)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.KLAPPSTUHL, true);

            await GTANetworkAPI.NAPI.Task.WaitForMainThread(500);

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop), "switch@michael@sitting", "idle");
          
            return true;
        }

        public static async Task<bool> Klappstuhlb(DbPlayer dbPlayer)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.KLAPPSTUHLBLAU, true);

            await GTANetworkAPI.NAPI.Task.WaitForMainThread(500);

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop), "switch@michael@sitting", "idle");

            return true;
        }
    }
}
