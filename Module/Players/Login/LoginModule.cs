using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Events;

namespace VMP_CNR.Module.Players.Login
{
    public sealed class LoginEventScript : Script
    {
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            if (player == null) return;

            LoginModule.Instance.OnPlayerConnect(player);
        }
    }

    public sealed class LoginModule : Module<LoginModule>
    {
        public void OnPlayerConnect(Player player)
        {
            NAPI.Task.Run(async () =>
            {
                while (player == null) await NAPI.Task.WaitForMainThread(50);

                try
                {
                    if (player == null) return;

                    DbPlayer dbPlayer = player.GetPlayer();

                    string l_EventKey = Helper.Helper.GenerateAuthKey();
                    if (player.HasData("auth_key"))
                        player.ResetData("auth_key");

                    player.SetData("auth_key", l_EventKey);

                    if (!player.HasData("connectedAt") && (dbPlayer == null || !dbPlayer.IsValid()))
                    {
                        player.Health = 99;

                        Player olderPlayer = NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p != null && p.Name == player.Name && p.HasData("connectedAt"));

                        if (olderPlayer != null && olderPlayer != player && (!olderPlayer.HasData("Connected") || olderPlayer.GetData<bool>("Connected") != true))
                        {
                            olderPlayer.Kick("Du bist bereits eingeloggt!");

                            return;
                        }

                        player.SetData("connectedAt", DateTime.Now);
                        PlayerConnect.OnPlayerConnected(player);
                    }

                }
                catch (Exception e) { Logger.Print(e.ToString()); }
            });
        }
    }
}
