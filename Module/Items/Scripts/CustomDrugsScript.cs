using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Buffs;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> CustomDrugWeed(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;

            if (dbPlayer.Buffs.LastDrugId == ItemData.Id)
            {
                if (dbPlayer.Buffs.DrugBuff >= 120)
                {
                    dbPlayer.SendNewNotification("Mehr solltest du davon echt nicht mehr nutzen...");
                    return false;
                }
            }
            Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachments.Attachment.JOINT, true);

            Chats.sendProgressBar(dbPlayer, 14000);

            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_smoking@male@male_b@enter", "enter");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetCannotInteract(true);

            await NAPI.Task.WaitForMainThread(14000);

            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return false;

            dbPlayer.SetCannotInteract(false);
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);

            dbPlayer.StopAnimation(AnimationLevels.User, true);

            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "amb@world_human_smoking@male@male_b@base", "base");


            if(dbPlayer.Buffs.LastDrugId == ItemData.Id)
            {
                if(dbPlayer.Buffs.DrugBuildUsed < 10)
                {
                    dbPlayer.Buffs.DrugBuildUsed++;
                }
                else
                {
                    dbPlayer.Buffs.DrugBuff += 15;
                    CustomDrugModule.Instance.SetCustomDrugEffect(dbPlayer);
                }
            }
            else
            {
                dbPlayer.Buffs.LastDrugId = ItemData.Id;
                dbPlayer.Buffs.DrugBuildUsed = 1;
                dbPlayer.Buffs.DrugBuff = 0;
            }

            dbPlayer.SaveBuffs();
            return true;
        }
        public static async Task<bool> CustomDrugMeth (DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            if (dbPlayer.Buffs.LastDrugId == ItemData.Id)
            {
                if (dbPlayer.Buffs.DrugBuff >= 120)
                {
                    dbPlayer.SendNewNotification("Mehr solltest du davon echt nicht mehr nutzen...");
                    return false;
                }
            }

            Chats.sendProgressBar(dbPlayer, 5000);

            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_suicide", "pill");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);

            await NAPI.Task.WaitForMainThread(5000);
            dbPlayer.ResetData("userCannotInterrupt");

            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return false;

            dbPlayer.Player.TriggerNewClient("freezePlayer", false);

            dbPlayer.StopAnimation();

            if (dbPlayer.Buffs.LastDrugId == ItemData.Id)
            {
                if (dbPlayer.Buffs.DrugBuildUsed < 5)
                {
                    dbPlayer.Buffs.DrugBuildUsed++;
                }
                else
                {
                    dbPlayer.Buffs.DrugBuff += 30;
                    CustomDrugModule.Instance.SetCustomDrugEffect(dbPlayer);
                }
            }
            else
            {
                dbPlayer.Buffs.LastDrugId = ItemData.Id;
                dbPlayer.Buffs.DrugBuildUsed = 1;
                dbPlayer.Buffs.DrugBuff = 0;
            }

            dbPlayer.SaveBuffs();
            return true;
        }

        public static async Task<bool> CustomDrugJeff(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            Chats.sendProgressBar(dbPlayer, 5000);

            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_suicide", "pill");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);

            await NAPI.Task.WaitForMainThread(5000);
            dbPlayer.ResetData("userCannotInterrupt");

            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return false;

            dbPlayer.Player.TriggerNewClient("freezePlayer", false);

            dbPlayer.StopAnimation();

            //CustomDrugModule.Instance.SetTrip(dbPlayer, "s_m_y_clown_01", "clown");
            
            return true;
        }


        public static async Task<bool> CustomDrugTeflon(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            Chats.sendProgressBar(dbPlayer, 5000);

            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["joint_start"].Split()[0], Main.AnimationList["joint_start"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);

            await NAPI.Task.WaitForMainThread(4000);
            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["joint_end"].Split()[0], Main.AnimationList["joint_end"].Split()[1]);
            await NAPI.Task.WaitForMainThread(1000);
            dbPlayer.ResetData("userCannotInterrupt");

            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return false;

            dbPlayer.Player.TriggerNewClient("freezePlayer", false);

            dbPlayer.StopAnimation();

            //CustomDrugModule.Instance.SetTrip(dbPlayer, "u_m_y_staggrm_01", "gay");

            return true;
        }
    }
}