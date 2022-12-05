using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Players.Buffs;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        private const int JointBuff = 20;
        private const int MaxArmor = 80;

        public static async Task<bool> joint(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle)
            {
                return false;
            }

            bool antiinterrupt = false;
            if (dbPlayer.HasData("disableAnim"))
            {
                antiinterrupt = Convert.ToBoolean(dbPlayer.GetData("disableAnim"));
            }

            if (antiinterrupt)
            {
                return false;
            }
            
            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["joint_start"].Split()[0], Main.AnimationList["joint_start"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            await NAPI.Task.WaitForMainThread(8000);
            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["joint_end"].Split()[0], Main.AnimationList["joint_end"].Split()[1]);
            await NAPI.Task.WaitForMainThread(2500);
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();
            /*int currentArmor = NAPI.Player.GetPlayerArmor(dbPlayer.Player);
            int newArmor = 0;

            if (currentArmor >= MaxArmor)
                newArmor = currentArmor;
            else if (currentArmor >= MaxArmor - JointBuff)
                newArmor = MaxArmor;
            else
                newArmor = currentArmor + JointBuff;
            if (dbPlayer.Container.GetItemAmount(ItemData.Id) <= 0)
                return false;
            NAPI.Player.SetPlayerArmor(dbPlayer.Player, newArmor);
            dbPlayer.Armor[0] = newArmor;
            
            ItemModelModule.Instance.LogItem((int)ItemData.Id, (int)dbPlayer.TeamId, 1);
            */

            return true;
        }
    }
}