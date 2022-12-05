using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Computer.Apps.VehicleImpoundApp
{
    public class VehicleImpoundFunctions
    {
        public static List<VehicleImpoundOverview> GetVehicleImpoundOverviews(DbPlayer dbPlayer, uint vehicleId)
        {
            List<VehicleImpoundOverview> overviewVehicles = new List<VehicleImpoundOverview>();

            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = "SELECT * FROM vehicle_impound WHERE vehicle_id = @vehicleId ORDER BY vehicle_impound.date";
                cmd.Parameters.AddWithValue("@vehicleId", vehicleId);
                cmd.Prepare();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            VehicleImpoundOverview overview = new VehicleImpoundOverview
                            {
                                VehicleId = vehicleId,
                                Model = reader.GetString("model"),
                                Officer = reader.GetString("officer"),
                                Reason = reader.GetString("reason"),
                                Date = reader.GetDateTime("date").ToUniversalTime().ToTimestamp().Seconds,
                                Release = reader.GetDateTime("release").ToUniversalTime().ToTimestamp().Seconds
                            };
                            overview.Release -= overview.Date;
                            overviewVehicles.Add(overview);
                        }
                    }
                }
            }
            return overviewVehicles;
        }

        public static List<VehicleImpoundOverview> GetVehicleImpoundOverviewsByMember(DbPlayer dbPlayer, string member)
        {
            List<VehicleImpoundOverview> overviewVehicles = new List<VehicleImpoundOverview>();

            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = "SELECT * FROM vehicle_impound WHERE officer like '%@member%' ORDER BY vehicle_impound.date";
                cmd.Parameters.AddWithValue("@member", member);
                cmd.Prepare();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            VehicleImpoundOverview overview = new VehicleImpoundOverview
                            {
                                VehicleId = reader.GetUInt32("vehicle_id"),
                                Model = reader.GetString("model"),
                                Officer = reader.GetString("officer"),
                                Reason = reader.GetString("reason"),
                                Date = reader.GetDateTime("date").ToUniversalTime().ToTimestamp().Seconds,
                                Release = reader.GetDateTime("release").ToUniversalTime().ToTimestamp().Seconds
                            };
                            overview.Release -= overview.Date;
                            overviewVehicles.Add(overview);
                        }
                    }
                }
            }
            return overviewVehicles;
        }

        public static void RemoveVehicleAndGiveReward(DbPlayer dbPlayer, SxVehicle sxVehicle)
        {
            if (sxVehicle.IsPlayerVehicle())
            {
                sxVehicle.SetPrivateCarGarage(1, 31);
            }
            else
            {
                sxVehicle.SetTeamCarGarage(true);
            }
            VehicleHandler.Instance.DeleteVehicleByEntity(sxVehicle.entity);
            dbPlayer.SendNewNotification(
                "Fahrzeug wurde verwahrt! (Provision 1000$)");
            dbPlayer.GiveMoney(1000);
        }



        public static void ImpoundVehicle(DbPlayer dbPlayer, SxVehicle sxVehicle, VehicleImpoundOverview vIO)
        {
            if (!VehicleHandler.Instance.GetAllVehicles().ToList().Contains(sxVehicle)) return;
            if (sxVehicle.Visitors.Count > 0) return;
            DateTime dDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(Double.Parse(vIO.Release.ToString())).ToLocalTime();
            String release = dDateTime.ToString("yyyy-MM-dd H:mm:ss");
            MySQLHandler.ExecuteAsync($"INSERT INTO vehicle_impound (`vehicle_id`, `model`, `officer`, `reason`, `release`) VALUES ({vIO.VehicleId}, '{vIO.Model}', '{vIO.Officer}', '{vIO.Reason}', '{release}')");
            RemoveVehicleAndGiveReward(dbPlayer, sxVehicle);
            string table = sxVehicle.teamid == 0 ? "vehicles" : "fvehicles";
            MySQLHandler.ExecuteAsync($"UPDATE {table} SET impound_release = '{release}' WHERE id = {sxVehicle.databaseId}");
            dbPlayer.Team.SendNotification($"{vIO.Officer} hat das Fahrzeug {vIO.Model} bis zum {release} verwahrt. Grund : {vIO.Reason}");
        }



    }
}
