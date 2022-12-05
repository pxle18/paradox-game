using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace VMP_CNR.Module.Freiberuf.Mower
{
    public sealed class MowerModule : Module<MowerModule>
    {
        public static int MowerJobVehMarkId = 20;
        public static Vector3 MowerGetPoint = new Vector3(-949.348, 332.97, 71.3311);
        public static Vector3 MowerSpawnPoint = new Vector3(-938.013, 329.984, 70.8813);
        public static float MowerSpawnRotation = 267.621f;
        public static Vector3 MowerMowPoint = new Vector3(-980.331, 318.863, 70.0861);
        public static List<DbPlayer> PlayersInJob = new List<DbPlayer>();

        public override bool Load(bool reload = false)
        {
            PlayerNotifications.Instance.Add(MowerGetPoint,
            "Freiberuf Rasenarbeiten",
            "Benutze \"E\" um den Freiberuf zu starten!"); // Perso
            return true;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle) return false;

            if (dbPlayer.Player.Position.DistanceTo(MowerGetPoint) < 2.0f)
            {
                MenuManager.Instance.Build(PlayerMenu.FreiberufMowerMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }

        public override void OnTenSecUpdate()
        {
            foreach (DbPlayer dbPlayer in PlayersInJob.ToList())
            {
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.RageExtension.IsInVehicle || dbPlayer.Player.Vehicle == null)
                    continue;

                if (dbPlayer.Player.Vehicle.HasData("loadage") && dbPlayer.Player.Vehicle.GetModel().Equals(VehicleHash.Mower))
                {
                    if (dbPlayer.Player.Vehicle.GetVehicle().GetSpeed() > 5.0f && dbPlayer.Player.Position.DistanceTo(MowerMowPoint) < 30.0f)
                    {
                        if (dbPlayer.HasData("lastRasenPoint"))
                        {
                            if (dbPlayer.GetData("lastRasenPoint").DistanceTo(dbPlayer.Player.Position) < 4.0f) continue; //Anti Kreisfahren
                        }
                        dbPlayer.SetData("lastRasenPoint", dbPlayer.Player.Position);

                        Random random = new Random();
                        int rnd = random.Next(1, 5);
                        int newLoadage = dbPlayer.Player.Vehicle.GetData<int>("loadage") + rnd;

                        // SetData bitte im MainThread ausführen
                        NAPI.Task.Run(() =>
                        {
                            dbPlayer.Player.Vehicle.SetData<int>("loadage", newLoadage);
                        });

                        dbPlayer.SendNewNotification($"Rasen gemaeht! (Inhalt {newLoadage - rnd} (+{rnd}))");
                    }
                }
            }
        }
    }
}