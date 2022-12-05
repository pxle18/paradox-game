using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using Newtonsoft.Json;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.RemoteEvents;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Module.Jobs.Taxi.App
{
    public class TaxiApp : SimpleApp
    {
        public TaxiApp() : base("TaxiApp")
        {
        }
    }
    public class TaxiListApp : SimpleApp {
        public TaxiListApp() : base("TaxiListApp")
        {
        }
        private class TaxiFound
        {
            [JsonProperty(PropertyName = "id")] public uint PlayerId { get; }
            [JsonProperty(PropertyName = "name")] public string Name { get; }
            [JsonProperty(PropertyName = "number")] public int Number { get; }
            [JsonProperty(PropertyName = "price")] public int Price { get; }

            public TaxiFound(uint playerId, string name, int number, int price)
            {
                PlayerId = playerId;
                Name = name;
                Number = number;
                Price = price;
            }
        }

        [RemoteEvent]
        public void requestTaxiList(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            SendTaxiList(player);
        }

        private void SendTaxiList(Player player)
        {
            try
            {
                var taxiList = new List<TaxiFound>();
                var Users = Players.Players.Instance.GetValidPlayers();

                for (int index = 0; index < Users.Count; index++)
                {
                    if (!Users[index].IsValid()) continue;
                    if (Users[index].HasData("taxi") &&
                        Users[index].Lic_Taxi[0] == 1)
                    {
                        taxiList.Add(new TaxiFound(Users[index].Id, Users[index].GetName(), (int)Users[index].handy[0], Users[index].GetData("taxi")));
                    }
                }

                TriggerNewClient(player, "responseTaxiList", NAPI.Util.ToJson(taxiList));
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return;
            }
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public void requestTaxiDriver(Player client, string driverName, string message, int preis, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            try { 
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent() || !dbPlayer.IsValid()) return;
                var driverDbPlayer = Players.Players.Instance.FindPlayer(driverName);
                if (!driverDbPlayer.IsValid()) return;
                if (driverDbPlayer.Id == dbPlayer.Id) return;

                if (dbPlayer.GetData("taxi_request") == driverDbPlayer.GetName())
                {
                    dbPlayer.SendNewNotification("Sie haben bereits an diesen Taxifahrer eine Anfrage gestellt.");
                    return;
                }

                if (driverDbPlayer.HasData("taxi") &&
                    driverDbPlayer.Lic_Taxi[0] == 1)
                {
                    // taxifahrer gefunden yay
                    driverDbPlayer.SendNewNotification(
                            "Sie haben eine Taxianfrage von " + dbPlayer.GetName() +
                            " (" + dbPlayer.ForumId + ") Ort: " + message + ", benutzen Sie die TaxiApp!", duration: 20000);
                    dbPlayer.SendNewNotification("Anfrage an den Taxifahrer wurde gestellt!");
                    dbPlayer.SetData("taxi_request", driverDbPlayer.GetName());
                    dbPlayer.SetData("taxi_request_message", message);
                    dbPlayer.SetData("taxi_request_price", preis);
                    var tsl = new TaxiServiceListApp();
                    tsl.requestTaxiServiceList(driverDbPlayer.Player, key);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return;
            }
        }
    }
    public class TaxiServiceListApp : SimpleApp
    {
        public TaxiServiceListApp() : base("TaxiServiceListApp")
        {
        }
        private class TaxiRequest
        {
            [JsonProperty(PropertyName = "id")] public uint Id { get; }
            [JsonProperty(PropertyName = "name")] public string Name { get; }
            [JsonProperty(PropertyName = "message")] public string Message { get; }

            public TaxiRequest(uint playerId, string name, string message)
            {
                Id = playerId;
                Name = name;
                Message = message;
            }
        }

        [RemoteEvent]
        public void requestTaxiServiceList(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            try { 
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (dbPlayer.HasData("taxi") && dbPlayer.Lic_Taxi[0] == 1)
                {
                    var taxiRequest = new List<TaxiRequest>();
                    Players.Players.Instance.GetValidPlayers().ForEach(delegate (DbPlayer p) {
                       if(p.HasData("taxi_request")&& dbPlayer.GetName() == p.GetData("taxi_request"))
                        {
                            taxiRequest.Add(new TaxiRequest(p.Id, p.GetName(),p.GetData("taxi_request_message")));
                        }
                    });
                    TriggerNewClient(player, "responseServiceList", true, NAPI.Util.ToJson(taxiRequest));
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return;
            }
        }

    }

    public class TaxiServiceContactApp : SimpleApp
    {
        public TaxiServiceContactApp() : base("TaxiServiceContactApp")
        {
        }

        [RemoteEvent]
        public void acceptServiceTaxi(Player player, uint PlayerID, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (dbPlayer.HasData("taxi") && dbPlayer.Lic_Taxi[0] == 1)
                {
                    DbPlayer findPlayer = Players.Players.Instance.FindPlayerById(PlayerID);
                    if (findPlayer == null || !findPlayer.IsValid()) return;
                    var cmd = new Players.PlayerCommands();
                    cmd.acceptservice(player, findPlayer.GetName());
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return;
            }
        }

        [RemoteEvent]
        public void deleteServiceTaxi(Player player, uint PlayerID, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (dbPlayer.HasData("taxi") && dbPlayer.Lic_Taxi[0] == 1)
                {
                    DbPlayer findPlayer = Players.Players.Instance.FindPlayerById(PlayerID);
                    if (findPlayer == null || !findPlayer.IsValid()) return;
                    if (findPlayer.HasData("taxi_request") && dbPlayer.GetName() == findPlayer.GetData("taxi_request"))
                    {
                        findPlayer.ResetData("taxi_request");
                        findPlayer.ResetData("taxi_request_message");
                        findPlayer.ResetData("taxi_request_price");
                        //findPlayer.Player.ResetSharedData("taxi_request");
                        //findPlayer.Player.ResetSharedData("taxi_request_price");

                        findPlayer.SendNewNotification($"Deine Anfrage wurde von {dbPlayer.GetName()} abgelehnt.");
                        dbPlayer.SendNewNotification($"Du hast die Anfrage von {findPlayer.GetName()} abgelehnt.");

                        var tsl = new TaxiServiceListApp();
                        tsl.requestTaxiServiceList(player, key);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return;
            }

            return;
        }
    }
}