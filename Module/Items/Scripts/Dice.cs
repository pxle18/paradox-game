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
        public static async Task<bool> Dice(DbPlayer dbPlayer, ItemModel itemModel)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            
            if (!int.TryParse(itemModel.Script.Split("_")[1], out int max)) return false;

            var trig = Main.Random.Next(1, max);
            
                
                dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop), "mp_player_int_upperwank", "mp_player_int_wank_01");
                await NAPI.Task.WaitForMainThread(2000);
                dbPlayer.StopAnimation();
            
            
            var surroundingUsers = NAPI.Player.GetPlayersInRadiusOfPlayer(20.0f, dbPlayer.Player);

            foreach (var user in surroundingUsers)
            {
                if (user.Dimension == dbPlayer.Player.Dimension)
                {
                    DbPlayer iPlayer = user.GetPlayer();
                    if (iPlayer == null || !iPlayer.IsValid())continue;

                    iPlayer.SendNewNotification( "* " + dbPlayer.GetName() + " rollt die Wuerfel und bekommt eine " + trig + ".");
                }
            }
            return false;
        }
    }
}