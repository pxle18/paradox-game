using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.Drunk
{
    public sealed class DrunkModule : Module<DrunkModule>
    {
        public void SetPlayerDrunk(DbPlayer dbPlayer, bool state)
        {
            NAPI.Pools.GetAllPlayers().ToList().ForEach((player) =>
            {
                if (player.Position.DistanceTo(dbPlayer.Player.Position) < 280)
                {
                    player.TriggerNewClient("setPlayerDrunk", dbPlayer.Player, state);
                }
            });

            if (state) dbPlayer.SetData("alkTime", DateTime.Now);
        }

        public void IncreasePlayerAlkLevel(DbPlayer dbPlayer, int level)
        {
            if (dbPlayer.HasData("alkLevel"))
            {
                level += (int)dbPlayer.GetData("alkLevel");
            }

            dbPlayer.SetData("alkLevel", level);

            if (level > 39)
            {
                SetPlayerDrunk(dbPlayer, true);
            }
        }

        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            if (dbPlayer.HasData("alkTime"))
            {
                var oldTime = (DateTime)dbPlayer.GetData("alkTime");

                if (oldTime.AddMinutes(5) < DateTime.Now) //5 Minuten
                {
                    dbPlayer.ResetData("alkLevel");
                    dbPlayer.ResetData("alkTime");
                    SetPlayerDrunk(dbPlayer, false);
                }
                if(dbPlayer.HasData("alkLevel") && dbPlayer.GetData("alkLevel") >= 40)
                {
                    dbPlayer.Player.TriggerNewClient("startScreenEffect", "DefaultFlash", 5000, true);
                }
                else
                {
                    dbPlayer.Player.TriggerNewClient("stopScreenEffect", "DefaultFlash");
                }
            }
        }
    }
}
