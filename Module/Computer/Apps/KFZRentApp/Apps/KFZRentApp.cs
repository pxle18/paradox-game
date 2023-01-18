using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.FahrzeugUebersichtApp;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.VehicleRent;

namespace VMP_CNR.Module.Computer.Apps.KFZRentApp.Apps
{
    public class KFZRentObject
    {
        [JsonProperty(PropertyName = "playerid")]
        public int PlayerId { get; set; }

        [JsonProperty(PropertyName = "playername")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "playerphone")]
        public int PlayerPhone { get; set; }

        [JsonProperty(PropertyName = "vehiclename")]
        public string VehicleName { get; set; }

        [JsonProperty(PropertyName = "vehicleid")]
        public int VehicleId { get; set; }


        [JsonProperty(PropertyName = "information")]
        public string Information { get; set; }

        public KFZRentObject(int renterId, string playerName, int playerPhone, string vehicleName, int vehicleId, DateTime start, DateTime end)
        {
            PlayerId = renterId;
            PlayerName = playerName;
            PlayerPhone = playerPhone;
            VehicleName = vehicleName;
            VehicleId = vehicleId;
            Information = $"Zeitraum: {start.ToString("g", CultureInfo.CreateSpecificCulture("de-DE"))} - {end.ToString("g", CultureInfo.CreateSpecificCulture("de-DE"))}";
        }
    }

    public class KFZRentApp : SimpleApp
    {
        public KFZRentApp() : base("KFZRentApp") { }

        [RemoteEvent]
        public void requestkfzrent(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer p_DbPlayer = client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            List<KFZRentObject> kFZRentObjects = new List<KFZRentObject>();

            foreach (PlayerVehicleRentKey playerVehicleRentKey in p_DbPlayer.GetPlayerVehicleRents())
            {
                PlayerName.PlayerNameModel pName = PlayerNameModule.Instance.GetAll().Values.ToList().Where(pn => pn.Id == playerVehicleRentKey.PlayerId).FirstOrDefault();
                if (pName == null) continue;

                kFZRentObjects.Add(new KFZRentObject((int)playerVehicleRentKey.PlayerId, pName.Name, pName.HandyNr, "Fahrzeug", (int)playerVehicleRentKey.VehicleId, playerVehicleRentKey.BeginDate, playerVehicleRentKey.EndingDate));
            }

            Logging.Logger.Debug(NAPI.Util.ToJson(kFZRentObjects));
            TriggerNewClient(client, "responsekfzrent", NAPI.Util.ToJson(kFZRentObjects));
        }

        [RemoteEvent]
        public void cancelkfzrent(Player client, int renterId, int vehicleId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer p_DbPlayer = client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            VehicleRentModule.PlayerVehicleRentKeys.RemoveAll(k => k.PlayerId == renterId && k.VehicleId == vehicleId && k.OwnerId == p_DbPlayer.Id);

            MySQLHandler.ExecuteAsync($"DELETE FROM player_vehicle_rent WHERE owner_id = '{p_DbPlayer.Id}' AND player_id = '{renterId}'");

            List<KFZRentObject> kFZRentObjects = new List<KFZRentObject>();

            foreach (PlayerVehicleRentKey playerVehicleRentKey in p_DbPlayer.GetPlayerVehicleRents())
            {
                PlayerName.PlayerNameModel pName = PlayerNameModule.Instance.GetAll().Values.ToList().Where(pn => pn.Id == playerVehicleRentKey.PlayerId).FirstOrDefault();
                if (pName == null) continue;

                kFZRentObjects.Add(new KFZRentObject((int)playerVehicleRentKey.PlayerId, pName.Name, pName.HandyNr, "Fahrzeug", (int)playerVehicleRentKey.VehicleId, playerVehicleRentKey.BeginDate, playerVehicleRentKey.EndingDate));
            }

            Logging.Logger.Debug(NAPI.Util.ToJson(kFZRentObjects));
            TriggerNewClient(client, "responsekfzrent", NAPI.Util.ToJson(kFZRentObjects));
        }
    }
}
