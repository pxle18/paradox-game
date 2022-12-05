using System;
using GTANetworkAPI;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Jobs.Taxi
{
    public class TaxiEventHandler : Script
    {
        [RemoteEvent]
        public void resultTaxometer(Player client, double distance, int price, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.SendNewNotification(
                "Taxometer lief fuer " + distance + "km. Gesamtpreis: " + Math.Round(distance * price) +
                "$");
        }
    }
}