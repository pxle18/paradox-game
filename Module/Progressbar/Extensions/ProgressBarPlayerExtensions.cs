using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Progressbar.Extensions
{
    static class ProgressBarPlayerExtensions
    {
        public static async Task<bool> RunProgressBar(this DbPlayer player, Func<Task> action, string title, string message, int duration, bool abortable = true) => await ProgressBarModule.Instance.RunProgressBar(player, action, title, message, duration, abortable);
        public static bool CancelProgressBar(this DbPlayer player) => ProgressBarModule.Instance.CancelProgressBar(player);
    }
}
