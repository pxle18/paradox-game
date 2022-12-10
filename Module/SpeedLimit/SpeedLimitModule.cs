using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.SpeedLimit
{
    public class SpeedLimitModule : Module<SpeedLimitModule>
    {
        public static Vector3 MesspunktSpawn = new Vector3(-2246, 4294, 46);
        public static float MesspunktHeading = 148.77f;


        protected override bool OnLoad()
        {
            Spawners.ColShapes.Create(new Vector3(-2584.11f, 3255.31f, 12.9928f), 20.0f).SetData<int>("messpunkt", 1);
            Spawners.ColShapes.Create(new Vector3(-2667.79f, 2569.23f, 16.2506f), 20.0f).SetData<int>("messpunkt", 2);
            Spawners.ColShapes.Create(new Vector3(-2700.89f, 2348.3f, 16.5187f), 20.0f).SetData<int>("messpunkt", 3);

            return base.OnLoad();
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!dbPlayer.IsValid() || !Configurations.Configuration.Instance.DevMode) return false;
            if (!colShape.HasData("messpunkt")) return false;

            if(dbPlayer.HasData("messMode") && dbPlayer.RageExtension.IsInVehicle)
            {
                int messPunkt = colShape.GetData<int>("messpunkt");

                SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVehicle == null || !sxVehicle.IsValid()) return false;

                int speed = sxVehicle.GetSpeed();

                if (messPunkt == 1)
                {
                    dbPlayer.SetData("max_speed", speed);
                }
                else
                {
                    if(dbPlayer.GetData("max_speed") < speed)
                    {
                        dbPlayer.SetData("max_speed", speed);
                    }
                }

                dbPlayer.SendNewNotification($"Messwert {messPunkt}: {speed} km/h");

                if(messPunkt == 3)
                {
                    MySQLHandler.Execute($"UPDATE vehicledata SET max_speed = '{dbPlayer.GetData("max_speed")}' WHERE id = '{sxVehicle.Data.Id}';");

                    dbPlayer.SendNewNotification($"Fahrzeug {sxVehicle.GetName()} wurde mit {dbPlayer.GetData("max_speed")} km/h eingetragen!");

                    VehicleHandler.Instance.DeleteVehicleByEntity(sxVehicle.Entity);
                    dbPlayer.ResetData("messMode");
                }

            }

            return false;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandtestspeed(Player player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null) return;

            if (!iPlayer.IsValid() || !Configurations.Configuration.Instance.DevMode) return;

            // groups 1 4 6 7

            uint ToSpawnId = 0;

            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                keyConn.Open();
                keyCmd.CommandText = $"SELECT * FROM vehicledata WHERE (classification = 1 OR classification = 4 OR classification = 6 OR classification = 7) " +
                    $"AND max_speed = 0 ORDER BY RAND() LIMIT 1;";
                using (var keyReader = keyCmd.ExecuteReader())
                {
                    if (keyReader.HasRows)
                    {
                        while (keyReader.Read())
                        {
                            ToSpawnId = keyReader.GetUInt32("id");
                        }
                    }
                }
                keyConn.Close();
            }

            if(ToSpawnId != 0)
            {

                int color1 = 0;
                int color2 = 0;

                var data = VehicleDataModule.Instance.GetDataById(ToSpawnId);

                if (data == null) return;
                if (data.Disabled) return;

                NAPI.Task.Run(async () =>
                {
                    iPlayer.Player.SetPosition(MesspunktSpawn);

                    SxVehicle myveh = VehicleHandler.Instance.CreateServerVehicle(
                    data.Id, true, MesspunktSpawn,
                    MesspunktHeading, color1, color2, iPlayer.Player.Dimension, true, false, false, 0, iPlayer.GetName(),
                    0, 999, (uint)iPlayer.Id, 200, 1000, "", "", 0, null, null, true);

                    await Task.Delay(2000);

                    if (myveh != null) player.SetIntoVehicle(myveh.Entity, -1);

                    await Task.Delay(500);

                    if (myveh != null) player.WarpOutOfVehicle();

                    await Task.Delay(1000);
                    if (myveh != null) player.SetIntoVehicle(myveh.Entity, -1);


                });

                iPlayer.SetWaypoint(-2700.89f, 2348.3f);
                iPlayer.SetData("messMode", true);
            }


            return;
        }

    }
}
