using System;
using System.Threading.Tasks;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Drunk;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Alk(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract()) return false;

            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody),
                    "amb@world_human_drinking@coffee@male@idle_a",
                    "idle_a");

            dbPlayer.SetCannotInteract(true);
            await Task.Delay(5000);
            dbPlayer.SetCannotInteract(false);
            var level = Convert.ToInt32(ItemData.Script.Split("_")[1]);
            DrunkModule.Instance.IncreasePlayerAlkLevel(dbPlayer, level);
            dbPlayer.StopAnimation();

            return true;
        }
    }
}
