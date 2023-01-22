using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Helper;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.TeamSubgroups;
using VMP_CNR.Module.TeamSubgroups.Models;
using VMP_CNR.Module.Tuning;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;
using VMP_CNR.Module.Vehicles.InteriorVehicles;
using VMP_CNR.Module.Vehicles.RegistrationOffice;
using VehicleData = VMP_CNR.Module.Vehicles.Data.VehicleData;

namespace VMP_CNR.Handler
{
    public sealed class VehicleHandler : Module<VehicleHandler>
    {
        public const int MaxVehicleHealth = 2000;

        public static ConcurrentDictionary<uint, SxVehicle> SxVehicles;

        public static ConcurrentDictionary<uint, List<SxVehicle>> TeamVehicles;
        public static ConcurrentDictionary<uint, List<SxVehicle>> TeamSubgroupVehicles;
        public static ConcurrentDictionary<uint, List<SxVehicle>> PlayerVehicles;
        public static ConcurrentDictionary<uint, List<SxVehicle>> ClassificationVehicles;
        public static ConcurrentDictionary<uint, List<SxVehicle>> SpawnedVehicles;

        protected override bool OnLoad()
        {
            SxVehicles = new ConcurrentDictionary<uint, SxVehicle>();
            TeamVehicles = new ConcurrentDictionary<uint, List<SxVehicle>>();
            TeamSubgroupVehicles = new ConcurrentDictionary<uint, List<SxVehicle>>();
            PlayerVehicles = new ConcurrentDictionary<uint, List<SxVehicle>>();
            ClassificationVehicles = new ConcurrentDictionary<uint, List<SxVehicle>>();
            SpawnedVehicles = new ConcurrentDictionary<uint, List<SxVehicle>>();

            return base.OnLoad();
        }

        public void AddContextTeamVehicle(uint teamid, SxVehicle sxVehicle)
        {
            if (!TeamVehicles.ContainsKey(teamid)) TeamVehicles.TryAdd(teamid, new List<SxVehicle>());

            TeamVehicles[teamid].Add(sxVehicle);
        }

        public void AddContextTeamSubgroupVehicle(uint teamSubgroupId, SxVehicle sxVehicle)
        {
            if (!TeamSubgroupVehicles.ContainsKey(teamSubgroupId)) TeamSubgroupVehicles.TryAdd(teamSubgroupId, new List<SxVehicle>());

            TeamSubgroupVehicles[teamSubgroupId].Add(sxVehicle);
        }

        public void AddContextPlayerVehicle(uint playerId, SxVehicle sxVehicle)
        {
            if (!PlayerVehicles.ContainsKey(playerId)) PlayerVehicles.TryAdd(playerId, new List<SxVehicle>());

            PlayerVehicles[playerId].Add(sxVehicle);
        }

        public void AddContextClassificationVehicle(uint classification, SxVehicle sxVehicle)
        {
            if (!ClassificationVehicles.ContainsKey(classification))
                ClassificationVehicles.TryAdd(classification, new List<SxVehicle>());

            ClassificationVehicles[classification].Add(sxVehicle);
        }

        public void AddContextSpawnedVehicle(uint playerid, SxVehicle sxVehicle)
        {
            if (!SpawnedVehicles.ContainsKey(playerid))
                SpawnedVehicles.TryAdd(playerid, new List<SxVehicle>());

            SpawnedVehicles[playerid].Add(sxVehicle);
        }

        public List<SxVehicle> GetClassificationVehicles(uint classification)
        {
            if (!ClassificationVehicles.ContainsKey(classification))
                return new List<SxVehicle>();

            return ClassificationVehicles[classification];
        }

        public List<SxVehicle> GetPlayerVehicles(uint playerid)
        {
            return PlayerVehicles.ContainsKey(playerid) ? PlayerVehicles[playerid].ToList() : new List<SxVehicle>();
        }

        public SxVehicle FindPlayerVehicle(uint playerid, uint vehicleDatabaseId)
        {
            return GetPlayerVehicles(playerid).Where(v => v.databaseId == vehicleDatabaseId).FirstOrDefault();
        }

        public List<SxVehicle> GetTeamVehicles(uint teamid)
        {
            return TeamVehicles.ContainsKey(teamid) ? TeamVehicles[teamid].Where(v => !v.PlanningVehicle).ToList() : new List<SxVehicle>();
        }

        public List<SxVehicle> GetTeamSubgroupVehicles(uint teamSubgroupId)
        {
            return TeamSubgroupVehicles.ContainsKey(teamSubgroupId) ? TeamSubgroupVehicles[teamSubgroupId].ToList() : new List<SxVehicle>();
        }

        public List<SxVehicle> GetTeamPlanningVehicles(uint teamid)
        {
            return TeamVehicles.ContainsKey(teamid) ? TeamVehicles[teamid].Where(v => v.PlanningVehicle).ToList() : new List<SxVehicle>();
        }

        public bool PlanningVehicleCheckByModel(uint teamid, string model)
        {
            return GetTeamPlanningVehicles(teamid).Where(v => v.Data.Model == model).Count() >= 0;
        }

        public SxVehicle FindTeamVehicle(uint teamid, uint vehicleDatabaseId)
        {
            return GetTeamVehicles(teamid).Where(v => v.databaseId == vehicleDatabaseId).FirstOrDefault();
        }

        public SxVehicle FindTeamSubgroupVehicle(uint teamSubgroupId, uint vehicleDatabaseId)
        {
            return GetTeamSubgroupVehicles(teamSubgroupId).Where(v => v.databaseId == vehicleDatabaseId).FirstOrDefault();
        }

        public SxVehicle FindTeamPlanningVehicle(uint teamid, uint vehicleDatabaseId)
        {
            return GetTeamPlanningVehicles(teamid).Where(v => v.databaseId == vehicleDatabaseId).FirstOrDefault();
        }

        public void DeletePlayerJobVehicle(DbPlayer dbPlayer)
        {
            foreach (var sxVeh in VehicleHandler.Instance.GetJobVehicles())
            {
                if (sxVeh.ownerId == dbPlayer.Id)
                {
                    DeleteVehicle(sxVeh, false);
                }
            }
        }

        public SxVehicle GetByVehicleDatabaseId(uint dbId)
        {
            try
            {
                SxVehicle result = GetAllVehicles().FirstOrDefault(veh => veh != null && veh.databaseId == dbId && veh.IsPlayerVehicle());
                result = result != null ? result : GetAllVehicles().FirstOrDefault(veh => veh.databaseId == dbId && veh.IsTeamVehicle());
                return result;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (dbPlayer != null && dbPlayer.IsValid() && dbPlayer.RageExtension.IsInVehicle)
            {
                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh != null && dbPlayer.IsValid())
                {
                    RemovePlayerFromOccupants(sxVeh, dbPlayer);
                }
            }
        }

        public override void OnPlayerExitVehicle(DbPlayer dbPlayer, Vehicle vehicle)
        {
            SxVehicle sxVeh = vehicle.GetVehicle();
            if (sxVeh != null && dbPlayer.IsValid())
            {
                RemovePlayerFromOccupants(sxVeh, dbPlayer);
            }
        }
        public override void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
            if (dbPlayer != null && dbPlayer.IsValid() && dbPlayer.RageExtension.IsInVehicle)
            {
                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh != null && dbPlayer.IsValid())
                {
                    RemovePlayerFromOccupants(sxVeh, dbPlayer);
                }
            }
        }

        public SxVehicle GetByVehicleDatabaseIdAndTeamId(uint dbId, uint teamId)
        {
            try
            {
                return GetAllVehicles().FirstOrDefault(veh => veh.databaseId == dbId && veh.teamid == teamId);
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return null;
            }
        }

        public SxVehicle GetByVehicleDatabaseIdAndTeamSubgroupId(uint dbId, uint teamSubgroupId)
        {
            try
            {
                return GetAllVehicles().FirstOrDefault(veh => veh.databaseId == dbId && veh.teamSubgroupId == teamSubgroupId);
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return null;
            }
        }

        public IEnumerable<SxVehicle> GetAllVehicles()
        {
            try
            {
                var vehicles = SxVehicles.Values.ToList();

                return vehicles.Where(sx => sx != null && sx.IsValid());
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public IEnumerable<SxVehicle> GetJobVehicles()
        {
            try
            {
                return SxVehicles.Values.ToList().Where(sx => sx != null && sx.IsValid() && sx.jobid != 0);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public IEnumerable<SxVehicle> GetClosestJobVehicles(Vector3 positon, float range = 7.0f)
        {
            try
            {
                return SxVehicles.Values.ToList().Where(sx => sx != null && sx.IsValid() && sx.jobid != 0 && sx.Entity.Position.DistanceTo(positon) < range);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public bool isAJobVeh(SxVehicle sxVeh)
        {
            if (sxVeh.jobid > 0 && sxVeh.databaseId == 0 && sxVeh.teamid == 0 && sxVeh.teamSubgroupId == 0)
            {
                return true;
            }

            return false;
        }

        public bool isJobVeh(SxVehicle sxVeh, int jobid)
        {
            if (sxVeh.jobid == jobid && sxVeh.databaseId == 0 && sxVeh.teamid == 0 && sxVeh.teamSubgroupId == 0)
            {
                return true;
            }

            return false;
        }

        public List<SxVehicle> GetClosestVehiclesPlayerCanControl(DbPlayer dbPlayer, float range = 4.0f)
        {
            try
            {
                return GetAllVehicles().Where(sx => sx.IsValid() && sx.Entity.Position.DistanceTo(dbPlayer.Player.Position) < range && dbPlayer.CanControl(sx)).ToList();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public List<SxVehicle> GetAllVehiclesPlayerCanControl(DbPlayer dbPlayer)
        {
            try
            {
                return GetAllVehicles().Where(sx => sx.IsValid() && dbPlayer.CanControl(sx)).ToList();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }
        public SxVehicle GetClosestVehicle(Vector3 position, float range = 4.0f, UInt32 dimension = 0)
        {
            var dictionary = new Dictionary<float, SxVehicle>();

            foreach (var vehicle in GetAllVehicles())
            {
                if (vehicle.Entity == null || vehicle.Entity.Dimension != dimension) continue;

                var _range = vehicle.Entity.Position.DistanceTo(position);

                if (_range <= range && !dictionary.ContainsKey(_range))
                {
                    dictionary.Add(_range, vehicle);
                }
            }

            var list = dictionary.Keys.ToList();
            list.Sort();

            return (dictionary.Count() > 0 && dictionary.ContainsKey(list[0])) ? dictionary[list[0]] : null;
        }

        public List<SxVehicle> GetClosestVehicles(Vector3 position, float range = 4.0f)
        {
            try
            {
                return GetAllVehicles().Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range).ToList();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public List<SxVehicle> GetClosestTeamVehicles(Vector3 position, float range = 4.0f)
        {
            try
            {
                return GetAllTeamVehicles().Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range).ToList();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }
        public List<SxVehicle> GetAllTeamVehicles()
        {
            var l_List = new List<SxVehicle>();
            foreach (var l_TeamVehicles in TeamVehicles)
            {
                foreach (var l_Veh in l_TeamVehicles.Value)
                {
                    if (l_List.Contains(l_Veh))
                        continue;

                    l_List.Add(l_Veh);
                }
            }

            return l_List;
        }

        public SxVehicle GetClosestVehicleFromTeam(Vector3 position, int teamid, float range = 4.0f)
        {
            try
            {
                IEnumerable<SxVehicle> sxVehicleList = GetTeamVehicles((uint)teamid).Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range);
                return sxVehicleList.Count() > 0 ? sxVehicleList.FirstOrDefault() : null;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public List<SxVehicle> GetClosestVehiclesFromTeam(Vector3 position, int teamid, float range = 4.0f)
        {
            List<SxVehicle> sxVehicles = new List<SxVehicle>();
            sxVehicles = GetTeamVehicles((uint)teamid).Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range).ToList();
            return sxVehicles;
        }

        public List<SxVehicle> GetClosestTeamSubgroupVehicles(Vector3 position, float range = 4.0f)
        {
            try
            {
                return GetAllTeamSubgroupVehicles().Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range).ToList();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public List<SxVehicle> GetAllTeamSubgroupVehicles()
        {
            var l_List = new List<SxVehicle>();
            foreach (var l_TeamSubgroupVehicles in TeamSubgroupVehicles)
            {
                foreach (var l_Veh in l_TeamSubgroupVehicles.Value)
                {
                    if (l_List.Contains(l_Veh))
                        continue;

                    l_List.Add(l_Veh);
                }
            }
            return l_List;
        }


        public SxVehicle GetClosestVehicleFromTeamSubgroup(Vector3 position, int teamSubgroupId, float range = 4.0f)
        {
            try
            {
                IEnumerable<SxVehicle> sxVehicleList = GetTeamSubgroupVehicles((uint)teamSubgroupId).Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range);
                return sxVehicleList.Count() > 0 ? sxVehicleList.FirstOrDefault() : null;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public List<SxVehicle> GetClosestVehiclesFromTeamSubgroup(Vector3 position, int teamSubgroupId, float range = 4.0f)
        {
            List<SxVehicle> sxVehicles = new List<SxVehicle>();
            sxVehicles = GetTeamSubgroupVehicles((uint)teamSubgroupId).Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range).ToList();
            return sxVehicles;
        }



        public List<SxVehicle> GetClosestPlanningVehiclesFromTeam(Vector3 position, int teamid, float range = 4.0f)
        {
            List<SxVehicle> sxVehicles = new List<SxVehicle>();
            sxVehicles = GetTeamPlanningVehicles((uint)teamid).Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range).ToList();
            return sxVehicles;
        }

        public List<SxVehicle> GetClosestVehiclesFromTeamWithContainerOpen(Vector3 position, int teamid, float range = 8.0f)
        {
            List<SxVehicle> sxVehicles = new List<SxVehicle>();
            sxVehicles = GetTeamVehicles((uint)teamid).Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range && !sxVeh.SyncExtension.Locked && sxVeh.TrunkStateOpen).ToList();
            return sxVehicles;
        }

        public List<SxVehicle> GetClosestVehiclesFromTeamSubgroupWithContainerOpen(Vector3 position, int teamSubgroupId, float range = 8.0f)
        {
            List<SxVehicle> sxVehicles = new List<SxVehicle>();
            sxVehicles = GetTeamSubgroupVehicles((uint)teamSubgroupId).Where(sxVeh => sxVeh.Entity.Position.DistanceTo(position) <= range && !sxVeh.SyncExtension.Locked && sxVeh.TrunkStateOpen).ToList();
            return sxVehicles;
        }

        public SxVehicle GetClosestVehicleFromTeamFilter(Vector3 position, int teamid, float range = 4.0f, int seats = 2)
        {
            try
            {
                IEnumerable<SxVehicle> sxVehicleList = GetClosestVehiclesFromTeam(position, teamid, range);
                if (sxVehicleList.Count() == 0) return null;
                SxVehicle sxVehicle = sxVehicleList.FirstOrDefault();
                var pos = sxVehicle.Entity.Position.DistanceTo(position);
                foreach (var sx in sxVehicleList)
                {
                    if (sx.Entity.GetNextFreeSeat() == -2) continue;
                    if (sx.Data.Slots < seats) continue;
                    if (pos > sx.Entity.Position.DistanceTo(position))
                    {
                        pos = sx.Entity.Position.DistanceTo(position);
                        sxVehicle = sx;
                    }
                }
                return sxVehicle;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public SxVehicle GetClosestVehicleFromTeamSubgroupFilter(Vector3 position, int teamSubgroupId, float range = 4.0f, int seats = 2)
        {
            try
            {
                IEnumerable<SxVehicle> sxVehicleList = GetClosestVehiclesFromTeamSubgroup(position, teamSubgroupId, range);
                if (sxVehicleList.Count() == 0) return null;
                SxVehicle sxVehicle = sxVehicleList.FirstOrDefault();
                var pos = sxVehicle.Entity.Position.DistanceTo(position);
                foreach (var sx in sxVehicleList)
                {
                    if (sx.Entity.GetNextFreeSeat() == -2) continue;
                    if (sx.Data.Slots < seats) continue;
                    if (pos > sx.Entity.Position.DistanceTo(position))
                    {
                        pos = sx.Entity.Position.DistanceTo(position);
                        sxVehicle = sx;
                    }
                }
                return sxVehicle;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public void AddPlayerToVehicleOccupants(SxVehicle sxVehicle, DbPlayer dbPlayer, int seat)
        {
            sxVehicle.Occupants.AddPlayer(dbPlayer, seat);
        }

        public void RemovePlayerFromOccupants(SxVehicle sxVehicle, DbPlayer dbPlayer)
        {
            sxVehicle.Occupants.RemovePlayer(dbPlayer);
        }

        public bool TrySetPlayerIntoVehicleOccupants(SxVehicle sxVehicle, DbPlayer dbPlayer, int startSeat = 1)
        {
            if (sxVehicle == null || dbPlayer == null || !sxVehicle.IsValid() || !dbPlayer.IsValid()) return false;
            if (sxVehicle.Data.Slots > 1)
            {
                int key = startSeat;

                // Check ab hinten links ALLE Sitze...
                while (key < sxVehicle.Data.Slots)
                {
                    if (sxVehicle.Occupants.IsSeatFree(key))
                    {
                        dbPlayer.Player.SetIntoVehicleSave(sxVehicle.Entity, key);
                        return true;
                    }
                    key++;
                }

                return false;
            }
            return false;
        }

        public bool TrySetPlayerIntoVehicleOccupantsAdmin(SxVehicle sxVehicle, DbPlayer dbPlayer)
        {
            if (sxVehicle == null || dbPlayer == null || !sxVehicle.IsValid() || !dbPlayer.IsValid()) return false;
            if (sxVehicle.Data.Slots > 1)
            {
                int key = 0;

                // Check ab hinten links ALLE Sitze...
                while (key < sxVehicle.Data.Slots)
                {
                    if (sxVehicle.Occupants.IsSeatFree(key))
                    {
                        dbPlayer.Player.SetIntoVehicleSave(sxVehicle.Entity, key);
                        return true;
                    }
                    key++;
                }

                return false;
            }

            return false;
        }

        public SxVehicle CreateServerVehicle(uint model, bool registered, Vector3 pos, float rotation, int color1, int color2, uint dimension, bool gpsTracker,
            bool spawnClosed = true, bool engineOff = false, uint teamid = 0, string owner = "", uint databaseId = 0,
            int jobId = 0, uint ownerId = 0, int fuel = 100, int zustand = 1000,
            string tuning = "", string neon = "", float km = 0f, Container container = null, string plate = "", bool disableTuning = false, bool InTuningProcess = false,
            int WheelClamp = 0, bool AlarmSystem = false, uint lastgarageId = 0, bool planningvehicle = false, int carSellPrice = 0, Container container2 = null, uint teamSubGroupId = 0)
        {
            try
            {
                // Cannot spawn duplicatings
                if (VehicleHandler.SxVehicles != null && databaseId != 0 && GetAllVehicles().Where(veh => veh.databaseId == databaseId && veh.teamid == teamid && veh.teamSubgroupId == teamSubGroupId).Count() > 0) return null;

                var xVeh = new SxVehicle();
                var data = VehicleDataModule.Instance.GetDataById(model);
                if (data == null)
                {
                    data = VehicleDataModule.Instance.GetDataById(219);
                }

                xVeh.VehicleData = new Dictionary<string, dynamic>();
                xVeh.CreatedDate = DateTime.Now;

                xVeh.TrunkStateOpen = false;

                xVeh.Occupants = new VehicleOccupants(xVeh);
                xVeh.LastInteracted = DateTime.Now;

                float motorMultiplier = data.Multiplier;
                xVeh.Data = data;

                xVeh.EngineDisabled = false;

                xVeh.uniqueServerId = GetFreeID();
                if (xVeh.uniqueServerId == 0)
                {
                    Players.Instance.SendMessageToAuthorizedUsers("adminchat", $"!! ACHTUNG !! KEINE FREIE UNIQUEID FÜR FAHRZEUGE VERFÜGBAR! BITTE IM DISCORD MELDEN!!", 10000);
                    return null;
                }

                if (data.IsModdedCar == 0)
                {
                    if (data.Hash < 0)
                    {
                        int.TryParse(data.Hash.ToString(), out int l_IntHash);
                        xVeh.Entity = NAPI.Vehicle.CreateVehicle(l_IntHash, pos, rotation, color1, color2);
                    }
                    else
                    {
                        uint.TryParse(data.Hash.ToString(), out uint l_IntHash);
                        xVeh.Entity = NAPI.Vehicle.CreateVehicle(l_IntHash, pos, rotation, color1, color2);
                    }
                }
                else
                {
                    var l_Hash = NAPI.Util.GetHashKey(data.Model);
                    xVeh.Entity = NAPI.Vehicle.CreateVehicle(l_Hash, pos, rotation, color1, color2);
                }


                xVeh.PlanningVehicle = planningvehicle;
                xVeh.LastGarage = lastgarageId;
                xVeh.spawnRot = rotation;
                xVeh.spawnPos = pos;
                xVeh.teamid = teamid;
                xVeh.teamSubgroupId = teamSubGroupId;
                xVeh.jobid = jobId;
                xVeh.zustand = zustand;
                xVeh.databaseId = databaseId;
                xVeh.ownerId = ownerId;
                xVeh.saveQuery = "";
                xVeh.respawnInteractionState = true;
                xVeh.Mods = TuningVehicleExtension.ConvertModsToDictonary(tuning);
                xVeh.neon = neon;
                xVeh.Container = container;
                xVeh.Container2 = container2;
                xVeh.Distance = km;
                xVeh.respawnInterval = 0;
                xVeh.spawnPosInterval = 0;
                xVeh.GpsTracker = gpsTracker;
                xVeh.Undercover = false;
                xVeh.Registered = registered;
                xVeh.Entity.NumberPlateStyle = 1;
                xVeh.SirensActive = false;

                xVeh.SpawnTime = DateTime.Now;
                xVeh.RepairState = false;
                xVeh.GarageStatus = VirtualGarageStatus.IN_WORLD;
                xVeh.Visitors = new List<DbPlayer>();

                xVeh.Team = TeamModule.Instance.Get(teamid);
                xVeh.TeamSubgroup = TeamSubgroupModule.Instance.Get(teamSubGroupId);

                xVeh.color1 = color1;
                xVeh.color2 = color2;

                xVeh.CanInteract = true;

                xVeh.SilentSiren = false;
                xVeh.InTuningProcess = InTuningProcess;
                xVeh.WheelClamp = WheelClamp;
                xVeh.AlarmSystem = AlarmSystem;

                xVeh.DynamicMotorMultiplier = Convert.ToInt32(data.Multiplier);

                xVeh.CarsellPrice = carSellPrice;

                xVeh.Attachments = new Dictionary<int, int>();

                if (teamid > 0)
                {
                    if (teamid == (int)TeamTypes.TEAM_FIB && color1 == -1 && color2 == -1)
                    {
                        xVeh.Undercover = true;
                    }
                }

                if (fuel > 0)
                {
                    if (fuel > data.Fuel) fuel = data.Fuel;
                    xVeh.fuel = fuel;
                }

                if (jobId > 0)
                {
                    xVeh.fuel = data.Fuel;
                }

                if (zustand > 0 && zustand < MaxVehicleHealth)
                {
                    if (zustand < 50) zustand = 50;
                    xVeh.SetHealth(zustand);
                }
                else
                {
                    xVeh.SetHealth(MaxVehicleHealth);
                }

                SxVehicles.TryAdd(xVeh.uniqueServerId, xVeh);

                // Add to Contextlists
                if (xVeh.IsTeamVehicle())
                {
                    AddContextTeamVehicle(xVeh.teamid, xVeh);
                }
                else if (xVeh.IsTeamSubGroupVehicle())
                {
                    AddContextTeamSubgroupVehicle(xVeh.teamSubgroupId, xVeh);
                }
                else if (xVeh.IsPlayerVehicle())
                {
                    AddContextPlayerVehicle(xVeh.ownerId, xVeh);
                }

                AddContextClassificationVehicle(xVeh.Data.ClassificationId, xVeh);

                if (xVeh.databaseId == 0)
                    AddContextSpawnedVehicle(ownerId, xVeh);

                Modules.Instance.OnVehicleSpawn(xVeh);
                xVeh.SetNeon(neon);

                //Set Wheeltype first
                if (xVeh.Mods.ContainsKey(1337))
                {
                    xVeh.SetMod(1337, xVeh.Mods[1337]);
                }

                foreach (var l_Pair in xVeh.Mods)
                {
                    xVeh.SetMod(l_Pair.Key, l_Pair.Value);
                }

                if (xVeh.databaseId > 0)
                {
                    xVeh.SetData("position", xVeh.spawnPos);
                }

                Task.Run(async () =>
                {
                    while (xVeh.Entity == null)
                    {
                        await Task.Delay(50);
                    }
                });

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        // Do entity Stuff here...
                        xVeh.Entity.SetData("vehicle", xVeh);

                        xVeh.Entity.Dimension = dimension;

                        xVeh.SyncExtension = new VehicleEntitySyncExtension(xVeh.Entity, spawnClosed, !engineOff);


                        if (engineOff)
                        {
                            xVeh.SyncExtension.SetEngineStatus(false);
                        }

                        if (spawnClosed && data.Id != InteriorVehiclesModule.AirforceDataId)
                        {
                            xVeh.SyncExtension.SetLocked(true);
                            xVeh.SyncExtension.SetEngineStatus(false);
                        }
                        else
                        {
                            xVeh.SyncExtension.SetLocked(false);
                        }

                        if (fuel <= 0)
                        {
                            xVeh.SyncExtension.SetEngineStatus(false);
                            xVeh.fuel = 0;
                        }


                        if (plate == null)
                        {
                            if (owner != "" && owner.Contains("_"))
                            {
                                var crumbs = owner.Split('_');

                                var firstLetter = crumbs[0][0].ToString();

                                var secondLetter = crumbs[1][0].ToString();

                                xVeh.Entity.NumberPlate = firstLetter + secondLetter + " " + PlayerNameModule.Instance.Get(ownerId).ForumId;

                                xVeh.plate = plate;
                            }
                        }
                        else
                        {
                            xVeh.Entity.NumberPlate = plate;
                            xVeh.plate = plate;
                        }

                        if (data.Hash == (uint)VehicleHash.Mule)
                        {
                            for (var i = 0; i < 7; i++)
                            {
                                xVeh.Entity.SetExtra(i, false);
                            }

                            xVeh.Entity.SetExtra(1, true);
                        }

                        // Set Anticheat Data
                        xVeh.Entity.SetData<string>("serverhash", "1312asdbncawssd1ccbSh1");
                        xVeh.Entity.SetData<Vector3>("lastSavedPos", xVeh.Entity.Position);

                        if (xVeh.Undercover)
                        {
                            var l_Rand = new Random();
                            if (xVeh.teamid == (int)TeamTypes.TEAM_FIB)
                            {
                                var color = l_Rand.Next(0, 150);
                                xVeh.color1 = color;
                                xVeh.color2 = color;
                                xVeh.Entity.PrimaryColor = color;
                                xVeh.Entity.SecondaryColor = color;
                            }

                            xVeh.Distance = l_Rand.Next(2000, 3000);

                            var l_ID = l_Rand.Next(630000, 650000);
                            xVeh.Entity.SetData<int>("nsa_veh_id", l_ID);

                            l_ID = l_Rand.Next(60000, 90000);
                            xVeh.Entity.NumberPlate = RegistrationOfficeFunctions.GetRandomPlate(true);
                        }

                        //xVeh.entity.SetSharedData("silentMode", false);
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }
                });

                if (xVeh != null)
                {
                    NAPI.Task.Run(() =>
                    {
                        NAPI.Entity.SetEntityPosition(xVeh.Entity, pos);
                        NAPI.Entity.SetEntityRotation(xVeh.Entity, new Vector3(0, 0, rotation));
                        xVeh.Repair();
                    }, 500);
                }

                return xVeh;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
            return null;
        }

        private uint GetFreeID()
        {
            for (uint itr = 1; itr < uint.MaxValue; itr++)
            {
                if (SxVehicles.ContainsKey(itr))
                    continue;

                return itr;
            }

            return 0;
        }

        public SxVehicle GetByUniqueServerId(uint id)
        {
            return SxVehicles.TryGetValue(id, out var vehicle) ? vehicle : null;
        }

        public string GetPlayerVehicleNameByDatabaseId(uint dbId)
        {
            if (dbId == 0) return "";
            foreach (var vehicle in GetAllVehicles())
            {
                if (vehicle == null) continue;

                if (vehicle.databaseId != dbId) continue;
                if (vehicle.jobid == 0)
                    return vehicle.Data.Model;
            }

            return "";
        }

        public void DeleteVehicleSave(SxVehicle sxVehicle, bool save = false)
        {
            if (sxVehicle == null) return;

            // First we need to check the save, try catching to avoid interrupt for this function
            if (save)
            {
                try
                {
                    if (sxVehicle.IsPlayerVehicle() || sxVehicle.IsTeamVehicle())
                    {
                        sxVehicle.Save();
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }


            // Now we remove it from all dics
            if (sxVehicle.IsTeamVehicle())
            {
                try
                {
                    TeamVehicles[sxVehicle.teamid].Remove(sxVehicle);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }
            else if (sxVehicle.IsTeamSubGroupVehicle())
            {
                try
                {
                    TeamSubgroupVehicles[sxVehicle.teamSubgroupId].Remove(sxVehicle);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            SxVehicles.TryRemove(sxVehicle.uniqueServerId, out SxVehicle vehicle);

            // Give event to module system
            Modules.Instance.OnVehicleDeleteTask(vehicle);

            // last we delete the entity ( do it safely )
            try
            {
                if (vehicle != null && vehicle.Entity != null) vehicle.Entity.DeleteVehicle();
                vehicle = null;
                sxVehicle = null;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public void DeleteVehicleByEntity(Vehicle vehicle, bool save = true)
        {
            var sxVeh = vehicle.GetVehicle();

            if (sxVeh == null)
            {
                vehicle.DeleteVehicle();
                return;
            }

            DeleteVehicleSave(sxVeh, save);
        }

        public void DeleteVehicle(SxVehicle sxVehicle, bool save = true)
        {
            if (sxVehicle == null) return;

            DeleteVehicleSave(sxVehicle, save);
        }

        public bool isFuelCar(SxVehicle sxVehicle)
        {
            var model = sxVehicle.Data.Model.ToLowerInvariant();
            if (model.Equals("tanker") || model.Equals("armytanker") || model.Equals("tanker2") || model.Equals("oiltanker")) return true;
            else return false;
        }
    }
    public enum VirtualGarageStatus
    {
        IN_WORLD = 0,
        IN_VGARAGE = 1,
    }

    public class SxVehicle
    {
        public uint uniqueServerId { get; set; }
        public Vehicle Entity { get; set; }
        public Vector3 spawnPos { get; set; }
        public float spawnRot { get; set; }
        public double fuel { get; set; }
        public int color1 { get; set; }
        public int color2 { get; set; }
        public uint teamid { get; set; }
        public uint teamSubgroupId { get; set; }
        public int jobid { get; set; }
        public uint databaseId { get; set; }
        public int zustand { get; set; }
        public uint ownerId { get; set; }
        public int respawnInterval { get; set; }
        public int spawnPosInterval { get; set; }
        public string saveQuery { get; set; }
        public string plate { get; set; }
        public VirtualGarageStatus GarageStatus { get; set; }
        public DateTime SpawnTime { get; set; }
        public bool RepairState { get; set; }

        public double Distance { get; set; }

        public Team Team { get; set; }
        public TeamSubgroup TeamSubgroup { get; set; }

        public Dictionary<int, int> Attachments { get; set; }
        public string LastDriver { get; set; }

        public Container Container { get; set; }

        public Container Container2 { get; set; }

        public ConcurrentDictionary<int, int> Mods { get; set; }

        public string neon { get; set; }

        public bool respawnInteractionState { get; set; }

        public VehicleData Data { get; set; }

        public List<DbPlayer> Visitors { get; set; }

        public VehicleEntitySyncExtension SyncExtension { get; set; }

        public bool CanInteract { get; set; }

        public DateTime LastInteracted { get; set; }
        public VehicleOccupants Occupants { get; set; }

        public bool GpsTracker { get; set; }

        public bool Undercover { get; set; }

        public bool SilentSiren { get; set; }

        public bool SirensActive { get; set; }

        public bool Registered { get; set; }

        public uint LastGarage { get; set; }
        public bool PlanningVehicle { get; set; }

        public bool InTuningProcess { get; set; }

        //0 -> keine // 1 -> Staat // 2 -> Gang 
        public int WheelClamp { get; set; }
        public bool AlarmSystem { get; set; }

        public float DynamicMotorMultiplier { get; set; }

        public Dictionary<string, dynamic> VehicleData { get; set; }
        public int CarsellPrice { get; set; }

        public bool TrunkStateOpen { get; set; }

        public bool EngineDisabled { get; set; }
        public string GetName()
        {
            return (Data.IsModdedCar == 1) ? Data.mod_car_name : Data.Model;
        }

        public bool IsTrunkOpen()
        {
            if (Entity == null) return false;
            return TrunkStateOpen;
        }

        public DateTime CreatedDate { get; set; }

        public ConcurrentDictionary<uint, bool> DoorStates = new ConcurrentDictionary<uint, bool>();
        /*{
            { 0, false }, // Vorne links
            { 1, false }, // Vorne rechts
            { 2, false }, // hinten link
            { 3, false }, // hinten rechts
            { 4, false }, // motorhaube
            { 5, false }, // kofferraum
            { 6, false }, // back (??)
            { 7, false }  // back2 (??)
        };*/

        public SxVehicle()
        {
            for (uint itr = 0; itr < 8; itr++)
            {
                DoorStates.TryAdd(itr, false);
            }
        }

        public void SetData(string key, dynamic value)
        {
            try
            {
                if (VehicleData.ContainsKey(key))
                {
                    VehicleData[key] = value;
                }
                else
                {
                    lock (VehicleData)
                    {
                        VehicleData.Add(key, value);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public VehicleOccupants GetOccupants()
        {
            return Occupants;
        }

        public bool HasData(string key)
        {
            return (VehicleData.ContainsKey(key));
        }

        public void ResetData(string key)
        {
            if (VehicleData.ContainsKey(key)) VehicleData.Remove(key);
        }

        public dynamic GetData(string key)
        {
            var result = (VehicleData.ContainsKey(key)) ? VehicleData[key] : "";
            return result;
        }

        public bool TryData<T>(string key, out T value)
        {
            var tmpdata = VehicleData.ContainsKey(key);
            value = tmpdata ? (T)VehicleData[key] : default(T);
            return tmpdata;
        }

    }
}