using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using Logger = VMP_CNR.Module.Logging.Logger;

namespace VMP_CNR.Module.Computer.Apps.HouseApp.Apps
{
    public class HouseVehicleList : SimpleApp
    {

        public HouseVehicleList() : base("HouseVehicleList")
        {

        }

        [RemoteEvent]
        public void requestHouseVehicles(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (dbPlayer.ownHouse[0] == 0)
            {
                dbPlayer.SendNewNotification("Du besitzt kein Haus.");
                return;
            }
            House house = HouseModule.Instance.GetByOwner(dbPlayer.Id);
            if (house == null) return;

            TriggerNewClient(client, "responseHouseVehicles", NAPI.Util.ToJson(HouseAppFunctions.GetVehiclesForHouseByPlayer(dbPlayer, house)));
        }

        [RemoteEvent]
        public void dropHouseVehicle(Player client, int vehicleId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (dbPlayer.ownHouse[0] == 0) return;

            House xHouse = HouseModule.Instance.GetByOwner(dbPlayer.Id);
            if (xHouse == null) return;

            MySQLHandler.ExecuteAsync($"UPDATE vehicles SET garage_id = 1 WHERE id = '{vehicleId}' AND garage_id = '{xHouse.GarageId}';");
            dbPlayer.SendNewNotification($"Du hast das Fahrzeug mit der ID {vehicleId} erfolgreich aus deiner Hausgarage entfernt.");
        }
    }
}
