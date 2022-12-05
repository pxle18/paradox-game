using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Assets.Hair;
using VMP_CNR.Module.Assets.HairColor;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Barber.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tattoo.Windows;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.SpawnProtection
{
    public sealed class SpawnProtectionModule : Module<SpawnProtectionModule>
    {
        public override bool Load(bool reload = false)
        {
            return true;
        }

        public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            // Set SpawnProtection
            dbPlayer.SetData("spawnProtectionSet", DateTime.Now);
            dbPlayer.SetData("ignoreGodmode", 10);
            dbPlayer.Player.TriggerNewClient("setSpawnProtection", true);
        }

        public override void OnPlayerLoggedIn(DbPlayer dbPlayer)
        {
            // Set SpawnProtection
            dbPlayer.SetData("spawnProtectionSet", DateTime.Now);
            dbPlayer.SetData("ignoreGodmode", 10);
            dbPlayer.Player.TriggerNewClient("setSpawnProtection", true);
        }

        public override void OnTenSecUpdate()
        {
            foreach(DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if(dbPlayer.HasData("spawnProtectionSet"))
                {
                    DateTime spawnProtectionTime = dbPlayer.GetData("spawnProtectionSet");
                    if(spawnProtectionTime.AddSeconds(20) <= DateTime.Now)
                    {
                        dbPlayer.ResetData("spawnProtectionSet"); 
                        dbPlayer.SetData("ignoreGodmode", 1);
                        dbPlayer.Player.TriggerNewClient("setSpawnProtection", false);
                    }
                }
            }
        }
    }
}