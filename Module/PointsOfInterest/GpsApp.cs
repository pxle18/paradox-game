using System.Linq;
using VMP_CNR.Module.ClientUI.Apps;
using GTANetworkAPI;
using System.Collections.Generic;
using Newtonsoft.Json;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.VehicleRent;
using VMP_CNR.Handler;
using VMP_CNR.Module.Storage;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Business.FuelStations;

namespace VMP_CNR.Module.PointsOfInterest
{
    public class GpsApp : SimpleApp
    {
        public GpsApp() : base("GpsApp")
        {
        }

        [RemoteEvent]
        public void requestVehicleGps(Player p_Player, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var l_GpsList = new List<GpsObject>();
            var l_FactionVehicles = new Dictionary<uint, List<LocationObject>>();
            var l_PrivateVehicles = new List<LocationObject>();
            var l_BusinessVehicles = new List<LocationObject>();
            var l_RentVehicles = new List<LocationObject>();


            foreach (var vehicle in VehicleHandler.Instance.GetAllVehicles())
            {
                if (vehicle == null || !vehicle.IsValid()) continue;

                if (dbPlayer.CanControl(vehicle) && vehicle.GpsTracker)
                {
                    var l_Name = "";
                    if (vehicle.Data.IsModdedCar == 1)
                        l_Name = $" ({vehicle.databaseId.ToString()}) {vehicle.Data.mod_car_name}";
                    else
                        l_Name = $"({vehicle.databaseId.ToString()}) {vehicle.Data.Model}";

                    if (vehicle.teamid != 0)
                    {
                        Team team = TeamModule.Instance.GetById((int)vehicle.teamid);
                        if( team != null)
                        {
                            l_Name = "(" + team.ShortName + ") " + l_Name;
                        }

                        var l_Location = new LocationObject()
                        {
                            name = l_Name,
                            X = vehicle.entity.Position.X,
                            Y = vehicle.entity.Position.Y
                        };

                        // Sort by Teamid...
                        if (l_FactionVehicles.ContainsKey(vehicle.teamid))
                        {
                            l_FactionVehicles[vehicle.teamid].Add(l_Location);
                        }
                        else
                        {
                            l_FactionVehicles.Add(vehicle.teamid, new List<LocationObject>());
                            l_FactionVehicles[vehicle.teamid].Add(l_Location);
                        }
                    }
                    else if (dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusiness().VehicleKeys.ContainsKey(vehicle.databaseId))
                    {
                        var l_Location = new LocationObject()
                        {
                            name = l_Name,
                            X = vehicle.entity.Position.X,
                            Y = vehicle.entity.Position.Y
                        };

                        l_BusinessVehicles.Add(l_Location);
                    }
                    else if (VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.VehicleId == vehicle.databaseId && k.PlayerId == dbPlayer.Id).Count() > 0)
                    {
                        var l_Location = new LocationObject()
                        {
                            name = l_Name,
                            X = vehicle.entity.Position.X,
                            Y = vehicle.entity.Position.Y
                        };

                        l_RentVehicles.Add(l_Location);
                    }
                    else
                    {
                        var l_Location = new LocationObject()
                        {
                            name = l_Name,
                            X = vehicle.entity.Position.X,
                            Y = vehicle.entity.Position.Y
                        };

                        l_PrivateVehicles.Add(l_Location);
                    }
                }
            }

            if (dbPlayer.TeamId != 0 && l_FactionVehicles.Count > 0)
            {
                var l_GpsEntry = new GpsObject()
                {
                    name = "Fraktion",
                    locations = new List<LocationObject>()
                };

                foreach(uint xkey in l_FactionVehicles.Keys)
                {
                    if (!l_FactionVehicles.ContainsKey(xkey) || l_FactionVehicles[xkey].Count == 0) continue;

                    foreach(LocationObject locationObject in l_FactionVehicles[xkey])
                    {
                        l_GpsEntry.locations.Add(locationObject);
                    }
                }

                l_GpsList.Add(l_GpsEntry);
            }

            if (dbPlayer.IsMemberOfBusiness() && l_BusinessVehicles.Count > 0)
            {
                var l_GpsEntry = new GpsObject()
                {
                    name = "Business",
                    locations = l_BusinessVehicles
                };

                l_GpsList.Add(l_GpsEntry);
            }

            if (l_RentVehicles.Count > 0)
            {
                var l_GpsEntry = new GpsObject()
                {
                    name = "Mietfahrzeuge",
                    locations = l_RentVehicles
                };

                l_GpsList.Add(l_GpsEntry);
            }

            if (l_PrivateVehicles.Count > 0)
            {
                var l_GpsEntry = new GpsObject()
                {
                    name = "Privat",
                    locations = l_PrivateVehicles
                };

                l_GpsList.Add(l_GpsEntry);
            }

            string l_Json = NAPI.Util.ToJson(l_GpsList);
            TriggerNewClient(p_Player, "gpsLocationsResponse", l_Json);
        }

        [RemoteEvent]
        public void requestGpsLocations(Player p_Player, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            var l_GPSList = new List<GpsObject>();

            var l_PointsOfInterestCategory = PointOfInterestCategoryModule.Instance.GetAll();
            foreach (var l_Category in l_PointsOfInterestCategory.Values)
            {
                var l_CategoryName = l_Category.Name;
                var l_CategoryList = new List<LocationObject>();

                foreach (var l_Object in PointOfInterestModule.Instance.GetAll().Where(p => p.Value.CategoryId == l_Category.Id))
                {
                    var l_Location = new LocationObject()
                    {
                        name = l_Object.Value.Name,
                        X = l_Object.Value.X,
                        Y = l_Object.Value.Y
                    };

                    l_CategoryList.Add(l_Location);
                }

                var l_GpsEntry = new GpsObject()
                {
                    name = l_CategoryName,
                    locations = l_CategoryList
                };

                l_GPSList.Add(l_GpsEntry);
            }


            var l_FuelStations = new List<LocationObject>();
            foreach (FuelStation fuel in FuelStationModule.Instance.GetAll().Values)
            {
                var l_LocationObject = new LocationObject()
                {
                    name = fuel.Name,
                    X = fuel.Position.X,
                    Y = fuel.Position.Y
                };

                l_FuelStations.Add(l_LocationObject);
            }

            var l_GPSGroupEntry = new GpsObject()
            {
                name = "Tankstellen",
                locations = l_FuelStations
            };

            l_GPSList.Add(l_GPSGroupEntry);


            var l_HouseStoragesList = new List<LocationObject>();
            if (dbPlayer.OwnHouse[0] > 0 || dbPlayer.IsTenant())
            {
                House iHouse = HouseModule.Instance.Get(dbPlayer.OwnHouse[0]);
                if (iHouse != null)
                {
                    var l_LocationObject = new LocationObject()
                    {
                        name = "Haus",
                        X = iHouse.Position.X,
                        Y = iHouse.Position.Y
                    };

                    l_HouseStoragesList.Add(l_LocationObject);
                }
                else
                {
                    iHouse = HouseModule.Instance.Get(dbPlayer.GetTenant().HouseId);
                    if (iHouse != null)
                    {
                        var l_LocationObject = new LocationObject()
                        {
                            name = "Wohnsitz",
                            X = iHouse.Position.X,
                            Y = iHouse.Position.Y
                        };

                        l_HouseStoragesList.Add(l_LocationObject);
                    }
                }
            }

            foreach (KeyValuePair<uint, StorageRoom> kvp in dbPlayer.GetStoragesOwned())
            {
                var l_Object = new LocationObject()
                {
                    name = $"Lager ({kvp.Value.Id})",
                    X = kvp.Value.Position.X,
                    Y = kvp.Value.Position.Y
                };

                l_HouseStoragesList.Add(l_Object);
            }

            var l_HouseGpsEntry = new GpsObject()
            {
                name = "Haus/Lager",
                locations = l_HouseStoragesList
            };

            l_GPSList.Add(l_HouseGpsEntry);
            TriggerNewClient(p_Player, "gpsLocationsResponse", NAPI.Util.ToJson(l_GPSList));
        }

        public class CategoryObject
        {
            [JsonProperty(PropertyName = "id")]
            public int id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }
        }

        public class LocationObject
        {
            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "x")]
            public float X { get; set; }

            [JsonProperty(PropertyName = "y")]
            public float Y { get; set; }
        }

        public class GpsObject
        {
            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "locations")]
            public List<LocationObject> locations { get; set; }
        }
    }
}