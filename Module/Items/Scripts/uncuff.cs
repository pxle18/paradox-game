using System.Collections.Generic;
using System.Linq;
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
        public static async Task<bool> uncuff(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (!dbPlayer.CanInteract())
            {
                return false;
            }
            
            foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers().Where(xp => xp.Player.Position.DistanceTo(dbPlayer.Player.Position) < 3.0f))
            {
                if ((dbPlayer.Player != xPlayer.Player) && (xPlayer.IsTied || xPlayer.IsCuffed || xPlayer.HasData("follow")))
                {
                    // Wenn Spieler in Range, gecufft oder gefesselt ist
                    dbPlayer.SendNewNotification(
                         "Sie versuchen die Fesseln zu knacken...");

                    Chats.sendProgressBar(dbPlayer, 5000);

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "a_uncuff");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);

                    await NAPI.Task.WaitForMainThread(5000);

                    // Recheck Distance
                    if (xPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.0f) return false;

                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.StopAnimation();
                xPlayer.SetCuffed(false);

                    xPlayer.SendNewNotification(
                            dbPlayer.GetName() +
                        " hat Ihre Handschellen gelöst!");
                    dbPlayer.SendNewNotification(
                            
                        "Sie haben die Handschellen von " +
                        xPlayer.GetName() + " gelöst!");
                    return true;
                }
            }
            return false;
        }
    }
}