using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Progressbar.Extensions;

namespace VMP_CNR.Module.Progressbar
{
    public class ProgressBarModule : Module<ProgressBarModule>
    {
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            if (dbPlayer.CancelProgressBar())
                return true;

            return false;
        }

        public async Task<bool> RunProgressBar(DbPlayer player, Func<Task> action, string title, string message, int duration, bool abortable = true)
        {
            if (player == null || !player.IsValid()) return false;

            lock (player)
            {
                player.CancellationToken = new CancellationTokenSource();

                Chats.sendProgressBar(player, duration);
            }

            bool result = await Task.Delay(duration, player.CancellationToken.Token).ContinueWith(task => !task.IsCanceled);

            if (result && player != null && player.IsValid()) await action();

            return result;
        }

        public bool CancelProgressBar(DbPlayer player)
        {
            if (player == null || !player.IsValid()) return false;


            if (player.CancellationToken != null)
            {
                player.CancellationToken.Cancel();
                player.CancellationToken = null;

                Chats.StopProgressbar(player);
                player.SendNewNotification("Interaktion abgebrochen.");

                return true;
            }

            return false;
        }
    }
}
