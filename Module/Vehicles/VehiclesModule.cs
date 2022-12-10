using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Government;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Helper;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles.Data;
using VMP_CNR.Module.Vehicles.Garages;
using VMP_CNR.Module.Weapons.Data;

//Todo: refactor users and vehicles to new system
//DbPlayer
//Client +
//ServerPlayer

//DbVehicle
//Vehicle +
//ServerVehicle
namespace VMP_CNR.Module.Vehicles
{
    public class VehiclesModule
    {
        public static VehiclesModule Instance { get; } = new VehiclesModule();

        private readonly Dictionary<int, ServerVehicle> vehicles;

        private int uniqueCount;
        
        public VehiclesModule()
        {
            vehicles = new Dictionary<int, ServerVehicle>();
        }

        public static void LoadServerTeamVehicle(Garage garage, uint vehicleid, DbPlayer dbPlayer, GarageSpawn spawnPosition)
        {
            try
            {
                if (dbPlayer.Team.Id > 0)
                {
                    Teams.Team currTeam = dbPlayer.Team;

                    // Wenn NSA Duty und IAA Garage ist...
                    if (dbPlayer.IsNSADuty && garage.Teams.Contains((uint)TeamTypes.TEAM_IAA)) currTeam = TeamModule.Instance.Get((uint)TeamTypes.TEAM_IAA);

                    var query = $"SELECT * FROM `fvehicles` WHERE `team` = '{currTeam.Id}' AND `id` = {vehicleid} AND lastGarage = '{garage.Id}';";

                    NAPI.Task.Run(async () =>
                    {
                        using (var conn =new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                        using (var cmd = conn.CreateCommand())
                        {
                            await conn.OpenAsync();
                            cmd.CommandText = @query;
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                if (reader.HasRows)
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));

                                        if (data == null) continue;
                                        if (data.Disabled)
                                        {
                                            dbPlayer.SendNewNotification("Dieses Fahrzeug ist derzeit deaktiviert.");
                                            continue;
                                        }

                                        if(garage.IsTeamGarage() && reader.GetInt32("defcon_level") > 0 && reader.GetInt32("defcon_level") < GovernmentModule.Defcon.Level)
                                        {
                                            dbPlayer.SendNewNotification("Die aktuelle Defcon Stufe für dieses Fahrzeug ist nicht erreicht!");
                                            continue;
                                        }
                                    
                                        if (!garage.Classifications.Contains(data.ClassificationId)) continue;
                                    
                                        var color1 = reader.GetInt32("color1");
                                        var color2 = reader.GetInt32("color2");                            
                                        var tuning = reader.GetString("tuning");                                                          
                                        var dbid = reader.GetUInt32("id");
                                        var team = reader.GetUInt32("team");
                                        var zustand = reader.GetInt32("zustand");
                                        var fuel = reader.GetInt32("fuel");
                                        var km = reader.GetInt32("km");
                                        var gpstracker = reader.GetInt32("gps_tracker") == 1;
                                        var plate = reader.GetString("plate");
                                        var registered = reader.GetInt32("registered") == 1;
                                        var wheelclamp = reader.GetInt32("WheelClamp");
                                        var alarmSystem = reader.GetInt32("alarm_system") == 1;
                                        var planningVehicle = reader.GetUInt32("planning_vehicle") == 1;
                                        int carSellPrice = reader.GetInt32("carsell_price");

                                        var xVeh = VehicleHandler.Instance.CreateServerVehicle(data.Id, registered,
                                            spawnPosition.Position, spawnPosition.Heading,
                                            color1, color2, spawnPosition.Dimension, gpstracker, true, true,
                                            currTeam.Id, currTeam.ShortName,
                                            dbid, 0, 0, fuel, zustand, tuning, "", km,
                                            ContainerManager.LoadContainer(dbid, ContainerTypes.FVEHICLE, data.InventorySize, data.InventoryWeight), 
                                            plate, false, false, wheelclamp, alarmSystem, garage.Id, planningVehicle, carSellPrice,
                                            container2: ContainerManager.LoadContainer(dbid, ContainerTypes.FVEHICLE2));

                                        if (xVeh == null)
                                            continue;

                                        while (xVeh.Entity == null)
                                        {
                                            await NAPI.Task.WaitForMainThread(100);
                                        }

                                        await Task.Delay(5);
                                        xVeh.SetTeamCarGarage(false);
                                    }
                                }
                            }
                            await conn.CloseAsync();
                        }

                    });
                }
            }

            catch (Exception e)
            {
                Logger.Print(e.StackTrace);
            }
        }

        public static void LoadServerVehicle(uint vehicleid, Garage garage, DbPlayer dbPlayer, GarageSpawn spawnPosition)
        {
            try
            {
                NAPI.Task.Run(async () =>
                {
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        await conn.OpenAsync();
                        cmd.CommandText =
                            $"SELECT * FROM `vehicles` WHERE `garage_id` = {garage.Id} AND `id` = '{vehicleid}';";
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));
                                    if (data == null) return;
                                    if (!garage.Classifications.Contains(data.ClassificationId)) return;
                                    if (data.Disabled)
                                    {
                                        dbPlayer.SendNewNotification("Dieses Fahrzeug ist derzeit deaktiviert.");
                                        return;
                                    }

                                    var ownerId = reader.GetUInt32("owner");

                                    var vehicleId = reader.GetUInt32("id");

                                    var color1 = reader.GetInt32("color1");
                                    var color2 = reader.GetInt32("color2");

                                    var fuel = reader.GetInt32("fuel");

                                    var tuning = reader.GetString("tuning");

                                    var zustand = reader.GetInt32("zustand");

                                    var neon = reader.GetString("Neon");

                                    var km = reader.GetFloat("km");

                                    var plate = reader.GetString("plate");
                                    var gpstracker = reader.GetInt32("gps_tracker") == 1;
                                    var registered = reader.GetInt32("registered") == 1;
                                    var InTuning = reader.GetInt32("TuningState") == 1;
                                    var wheelClamp = reader.GetInt32("WheelClamp");
                                    var alarmSystem = reader.GetInt32("alarm_system") == 1;

                                    if (ownerId != dbPlayer.Id && !dbPlayer.VehicleKeys.ContainsKey(vehicleId) && (dbPlayer.TeamId != (int)TeamTypes.TEAM_LSC && !InTuning))
                                        return;

                                    var sxVeh = VehicleHandler.Instance.CreateServerVehicle(data.Id, registered,
                                    spawnPosition.Position, spawnPosition.Heading,
                                    color1, color2, (uint)garage.dimension, gpstracker, true, true, 0,
                                    "", vehicleId,
                                    0, ownerId, fuel,
                                    zustand, tuning, neon,
                                    km, ContainerManager.LoadContainer(vehicleId, ContainerTypes.VEHICLE, data.InventorySize, data.InventoryWeight),
                                    plate, false, InTuning, WheelClamp: wheelClamp, AlarmSystem: alarmSystem, garage.Id,
                                    container2: ContainerManager.LoadContainer(vehicleId, ContainerTypes.VEHICLE2));

                                    await NAPI.Task.WaitForMainThread(3000);
                                    
                                    if (sxVeh != null && sxVeh.Entity != null)
                                    {
                                        await Task.Delay(5);
                                        sxVeh.Save(false, 0, garage.Id);
                                    }
                                }
                            }
                        }
                        await conn.CloseAsync();
                    }
                });
            }

            catch (Exception e)
            {
                Logger.Print(e.StackTrace);
            }
        }
        
        public Vehicle Create(VehicleHash hash, Vector3 position, float heading,
            int colorPrimary, int colorSecondary)
        {
            return hash.Create(position, heading, colorPrimary, colorSecondary);
        }
        
        public JobVehicle CreateJobVehicle(Vehicle vehicle, int jobId)
        {
            var jobVehicle = new JobVehicle(uniqueCount++, vehicle, jobId);
            vehicles.Add(jobVehicle.Id, jobVehicle);
            return jobVehicle;
        }

        public TeamVehicle CreateTeamVehicle(Vehicle vehicle, int teamId)
        {
            var teamVehicle = new TeamVehicle(uniqueCount++, vehicle, teamId);
            vehicles.Add(teamVehicle.Id, teamVehicle);
            return teamVehicle;
        }

        public ServerVehicle Get(int id)
        {
            return vehicles.ContainsKey(id) ? vehicles[id] : null;
        }

        public void Remove(int id)
        {
            if (vehicles.ContainsKey(id))
            {
                vehicles.Remove(id);
            }
        }
    }

    public static class VehicleExtension
    {
        public static JobVehicle AsJobVehicle(this Vehicle vehicle, int jobId)
        {
            return VehiclesModule.Instance.CreateJobVehicle(vehicle, jobId);
        }

        public static bool IsInAntiFlight(this SxVehicle sxVehicle)
        {
            Vector3 AntiFlightZonePrison = new Vector3(1681, 2604, 44);
            Vector3 AntiFlightZoneIsland = new Vector3(-7240.68, -281.905, 20);
            Vector3 AntiFlightZoneCayoPerico = new Vector3(5263.96, -5421.38, 77);

            if (sxVehicle.Data.ClassificationId == 8 || sxVehicle.Data.ClassificationId == 9)
            {
                if (sxVehicle.teamid != 13 && sxVehicle.Entity.Position.DistanceTo(AntiFlightZonePrison) < 200) return true; // Heli, Planes in Flightzoneprison
            }

            if ((sxVehicle.Data.ClassificationId == 8 || sxVehicle.Data.ClassificationId == 9) && sxVehicle.teamid != (uint)TeamTypes.TEAM_CAYO)
            {
                if (sxVehicle.teamid != 13 && sxVehicle.Entity.Position.DistanceTo(AntiFlightZoneCayoPerico) < 800) return true; // Heli, Planes in Flightzoneprison
            }

            if (sxVehicle.Data.ClassificationId == 8 || sxVehicle.Data.ClassificationId == 3)
            {
                if (sxVehicle.Entity.Position.DistanceTo(AntiFlightZoneIsland) < 400) return true; // Heli, Boote in FlightIsland
            }

            return false;
        }

        public static TeamVehicle AsTeamVehicle(this Vehicle vehicle, int teamId)
        {
            return VehiclesModule.Instance.CreateTeamVehicle(vehicle, teamId);
        }

        public static ServerVehicle GetServerVehicle(this Vehicle vehicle)
        {
            if (!vehicle.HasData("ServerVehicle"))
            {
                vehicle.SafeDelete();
                return null;
            }

            var serverVehicleData = vehicle.GetData<ServerVehicle>("ServerVehicle");
            if (serverVehicleData is ServerVehicle serverVehicle)
            {
                return serverVehicle;
            }

            return null;
        }

        public static void SafeDelete(this Vehicle vehicle)
        {
            if (vehicle.HasData("loadedVehicle"))
            {
                //Vehicle loadedVehicle = vehicle.GetData("loadedVehicle");
                //loadedVehicle.Detach();
                vehicle.ResetData("loadedVehicle");
            }
            
        }
    }

    public static class VehicleModelExtension
    {
        public static Vehicle Create(this VehicleHash hash, Vector3 position, float heading,
            int colorPrimary, int colorSecondary)
        {
            return NAPI.Vehicle.CreateVehicle( /*model:*/ hash, /*pos:*/ position, new Vector3(0, 0, heading),
                colorPrimary, colorSecondary);
            //Todo: delay after create with task
        }

        public static VehicleHash GetModel(this Vehicle vehicle)
        {
            return (VehicleHash) vehicle.Model;
        }
    }
}