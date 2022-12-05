using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VMP_CNR.Module.Configurations;

namespace VMP_CNR.Handler
{
    public static class BashCommandHelper
    {
        public static void ExecuteCommand(this string cmd)
        {
            if (!Configuration.Instance.DevMode)
                return;

            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
        }
    }
}
