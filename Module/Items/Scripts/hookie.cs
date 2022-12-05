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
        public static async Task<bool> hookie(DbPlayer dbPlayer, ItemModel ItemData)
        {
            
                dbPlayer.PlayAnimation( (int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["rock"].Split()[0], Main.AnimationList["rock"].Split()[1]); 
                dbPlayer.Player.TriggerNewClient("freezePlayer", true); 
                await NAPI.Task.WaitForMainThread(3000); 
                dbPlayer.Player.TriggerNewClient("freezePlayer", false); 
                dbPlayer.StopAnimation(); 
            
            return true;
        }
    }
}