using System;
using VMP_CNR.Module.Players.Db;
using Newtonsoft.Json;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Vehicles.Garages;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Tasks;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.JobFactions.Carsell;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players.BigDataSender;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Vehicles.Windows
{
    public class GarageWindow : Window<Func<DbPlayer, Garage, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "id")] private uint GarageId { get; }
            [JsonProperty(PropertyName = "name")] private string GarageName { get; }

            public ShowEvent(DbPlayer dbPlayer, Garage garage) : base(dbPlayer)
            {
                GarageId = garage.Id;
                GarageName = garage.Name;
            }
        }

        public GarageWindow() : base("Garage")
        {
        }

        public override Func<DbPlayer, Garage, bool> Show()
        {
            return (player, garage) => OnShow(new ShowEvent(player, garage));
        }

        [RemoteEvent]
        public void requestVehicleList(Player client, uint garageId, string state, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            if (!dbPlayer.TryData("garageId", out uint playerGarageId)) return;
            if (playerGarageId != garageId) return;

            var garage = GarageModule.Instance[garageId];
            if (garage == null) return;
            switch (state)
            {
                case "takeout":
                    SynchronizedTaskManager.Instance.Add(new GarageVehiclesTask(dbPlayer, garage));
                    break;
                case "takein":
                    if (garage.Id == 0) return;
                    if (garage.Type == GarageType.VehicleCollection) return;

                    var vehicles = garage.GetAvailableVehicles(dbPlayer, garage.Radius);

                    var vehicleJson = JsonConvert.SerializeObject(vehicles);

                    dbPlayer.Player.TriggerNewClientBig("componentServerEvent", "Garage", "responseVehicleList", vehicleJson);
                    break;
            }
        }

        [RemoteEvent]
        public void requestVehicle(Player client, string state, uint garageId, uint vehicleId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            if (!dbPlayer.TryData("garageId", out uint playerGarageId)) return;
            if (playerGarageId != garageId) return;

            var garage = GarageModule.Instance[garageId];
            if (garage == null) return;

            uint currTeam = dbPlayer.TeamId;

            // Wenn NSA Duty und IAA Garage ist...
            if (dbPlayer.IsNSADuty && garage.Teams.Contains((uint)TeamTypes.TEAM_IAA)) currTeam = (uint)TeamTypes.TEAM_IAA;

            switch (state)
            {
                case "takeout":
                    var spawn = garage.GetFreeSpawnPosition();

                    if (spawn == null)
                    {
                        dbPlayer.SendNewNotification("Kein freier Ausparkpunkt.");
                        return;
                    }

                    if (garage.IsTeamGarage())
                    {
                        if (garage.Rang > 0 && dbPlayer.TeamRank < garage.Rang)
                        {
                            dbPlayer.SendNewNotification("Sie haben nicht den benötigten Rang!");
                            return;
                        }

                        SynchronizedTaskManager.Instance.Add(new GaragePlayerTeamVehicleTakeOutTask(garage, vehicleId, dbPlayer, spawn));
                    }
                    else if (garage.IsTeamSubgroupGarage())
                    {
                        if (garage.Rang > 0 && dbPlayer.TeamSubgroupRank < garage.Rang)
                        {
                            dbPlayer.SendNewNotification("Sie haben nicht den benötigten Rang!");
                            return;
                        }
                        SynchronizedTaskManager.Instance.Add(new GaragePlayerTeamVehicleTakeOutTask(garage, vehicleId, dbPlayer, spawn));
                    }
                    else
                    {
                        if (garage.Type == GarageType.VehicleCollection)
                        {
                            if (!dbPlayer.TakeMoney(2500))
                            {
                                dbPlayer.SendNewNotification(

                                    "Um ein Fahrzeug freizukaufen benötigst du mindestens $2500 fuer eine Kaution!");
                                return;
                            }
                            else
                            {
                                try
                                {
                                    TeamShelterModule.Instance.Get((int)TeamTypes.TEAM_DPOS).GiveMoney(dbPlayer, 500, "Fahrzeugkaution");
                                }
                                catch { }

                                dbPlayer.SendNewNotification(
                                "Fahrzeug fuer 2500$ Freigekauft.");
                            }
                        }

                        if (garage.Type == GarageType.VehicleAdminGarage)
                        {
                            if (!dbPlayer.TakeMoney(25000))
                            {
                                dbPlayer.SendNewNotification(

                                    "Um ein Fahrzeug freizukaufen benötigst du mindestens $25000 fuer eine Kaution!");
                                return;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    "Fahrzeug fuer 25000$ Freigekauft.");
                            }
                        }

                        if (garage.Id == 493)
                        {
                            if (!dbPlayer.TakeMoney(1000))
                            {
                                dbPlayer.SendNewNotification(

                                    "Dein Fahrzeug wurde zerstört um es zu reparieren benötigst du mindestens 1000$!");
                                return;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification(
                                    "Fahrzeug fuer 1000$ repariert.");
                            }
                        }


                        if (garage.Id == JobCarsellFactionModule.GarageTeam1 || garage.Id == JobCarsellFactionModule.GarageTeam2 || garage.Id == JobCarsellFactionModule.GarageTeam3)
                        {
                            // Check Kaufvertrag
                            if (dbPlayer.Container.GetItemAmount(641) <= 0)
                            {
                                dbPlayer.SendNewNotification($"Sie benötigen Ihren Kaufvertrag um das Fahrzeug zu entnehmen!");
                                return;
                            }
                            Item kaufVertrag = dbPlayer.Container.GetItemById(641);
                            if (kaufVertrag == null || kaufVertrag.Data == null || !kaufVertrag.Data.ContainsKey("vehicleId") || kaufVertrag.Data["vehicleId"] != vehicleId)
                            {
                                dbPlayer.SendNewNotification($"Sie benötigen Ihren Kaufvertrag um das Fahrzeug zu entnehmen!");
                                return;
                            }
                        }

                        SynchronizedTaskManager.Instance.Add(
                            new GaragePlayerVehicleTakeOutTask(garage, vehicleId, dbPlayer, spawn));
                    }

                    break;
                case "takein":

                    if (garage.IsTeamGarage() && garage.Teams.Contains(currTeam))
                    {
                        var vehicle = VehicleHandler.Instance.GetByVehicleDatabaseIdAndTeamId(vehicleId, currTeam);

                        if (vehicle == null || vehicle.teamid != currTeam) return;
                        if (vehicle.Visitors.Count != 0) return;
                        if (vehicle.Entity.Position.DistanceTo(garage.Position) > garage.Radius) return;
                        if (vehicle.GetOccupants().IsEmpty() == false)
                        {
                            dbPlayer.SendNewNotification("Da ist noch ein*e Mitfahrer*in im Kofferraum");
                            return;
                        }

                        if (vehicle.CreatedDate.AddSeconds(10) > DateTime.Now)
                        {
                            dbPlayer.SendNewNotification("Bitte warte kurz bevor du dieses Fahrzeug erneut einparkst!");
                            return;
                        }
                        vehicle.SetTeamCarGarage(true, (int)garage.Id);
                    }
                    else if (garage.IsTeamSubgroupGarage() && garage.TeamSubgroupId.Equals(dbPlayer.TeamSubgroupId))
                    {
                        var vehicle = VehicleHandler.Instance.GetByVehicleDatabaseIdAndTeamSubgroupId(vehicleId, dbPlayer.TeamSubgroupId);

                        if (vehicle == null || vehicle.teamSubgroupId != dbPlayer.TeamSubgroupId) return;
                        if (vehicle.Visitors.Count != 0) return;
                        if (vehicle.Entity.Position.DistanceTo(garage.Position) > garage.Radius) return;
                        if (vehicle.GetOccupants().IsEmpty() == false)
                        {
                            dbPlayer.SendNewNotification("Da ist noch ein*e Mitfahrer*in im Kofferraum");
                            return;
                        }

                        if (vehicle.CreatedDate.AddSeconds(10) > DateTime.Now)
                        {
                            dbPlayer.SendNewNotification("Bitte warte kurz bevor du dieses Fahrzeug erneut einparkst!");
                            return;
                        }
                        vehicle.SetTeamSubgroupCarGarage(true, (int)garage.Id);
                    }
                    else
                    {
                        if (garage.HouseId > 0 && !garage.CanVehiclePutIntoHouseGarage())
                        {
                            dbPlayer.SendNewNotification("Hausgarage ist voll!");
                            return;
                        }
                        if (garage.Type == GarageType.VehicleCollection) return;
                        if (garage.Type == GarageType.Import) return;
                        if (garage.Id == 493) return;
                        // Carsell Garagen NUR ausparken
                        if (garage.Id == JobCarsellFactionModule.GarageTeam1 || garage.Id == JobCarsellFactionModule.GarageTeam2 || garage.Id == JobCarsellFactionModule.GarageTeam3) return;
                        var vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(vehicleId);
                        if (vehicle == null || vehicle.databaseId == 0) return;
                        if (vehicle.Visitors.Count != 0) return;
                        if (vehicle.GetOccupants().IsEmpty() == false)
                        {
                            dbPlayer.SendNewNotification("Da ist noch ein*e Mitfahrer*in im Kofferraum");
                            return;
                        }

                        if (vehicle.CreatedDate.AddSeconds(10) > DateTime.Now)
                        {
                            dbPlayer.SendNewNotification("Bitte warte kurz bevor du dieses Fahrzeug erneut einparkst!");
                            return;
                        }

                        if (vehicle.Entity.Position.DistanceTo(garage.Position) > garage.Radius) return;
                        vehicle.SetPrivateCarGarage(1, garageId);
                    }
                    break;
            }
        }
    }
}