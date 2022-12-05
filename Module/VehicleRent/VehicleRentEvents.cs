using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.VehicleRent
{
    class VehicleRentEvents : Script
    {
        [RemoteEvent]
        public void GivePlayerRentKey(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.HasData("vehicleRentDays") || !dbPlayer.HasData("vehicleRentId")) return;

                try
                {
                    // Get Player To give key
                    DbPlayer targetPlayer = Players.Players.Instance.FindPlayer(returnstring);
                    if (targetPlayer == null || !targetPlayer.IsValid())
                    {
                        dbPlayer.SendNewNotification("Spieler nicht gefunden!");
                        return;
                    }

                    if (dbPlayer.Container.GetItemAmount(626) <= 0)
                    {
                        dbPlayer.SendNewNotification("Kein Mietvertrag vorhanden!");
                        return;
                    }

                    int days = dbPlayer.GetData("vehicleRentDays");
                    uint databaseId = dbPlayer.GetData("vehicleRentId");

                    SxVehicle sxVeh = VehicleHandler.Instance.FindPlayerVehicle(dbPlayer.Id, databaseId);
                    if (sxVeh != null && sxVeh.IsValid())
                    {
                        if (VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.PlayerId == targetPlayer.Id && k.VehicleId == sxVeh.databaseId).Count() > 0)
                        {
                            dbPlayer.SendNewNotification("Spieler hat bereits einen Schlüssel für dieses Fahrzeug!");
                            return;
                        }

                        if (VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.VehicleId == sxVeh.databaseId).Count() > 0)
                        {
                            dbPlayer.SendNewNotification("Dieses Fahrzeug wurde bereits an jemanden anders vermietet!");
                            return;
                        }

                        if(!targetPlayer.Container.CanInventoryItemAdded(VehicleRentModule.VehicleRentItemId))
                        {
                            dbPlayer.SendNewNotification("Spieler hat nicht genug Platz im Inventar für den Mietvertrag!");
                            return;
                        }
                        dbPlayer.Container.RemoveItem(626, 1);

                        DateTime mietDate = DateTime.Now;
                        DateTime endMietDate = mietDate.AddDays(days);
                        
                        Dictionary<string, dynamic> DataInfo = new Dictionary<string, dynamic>();
                        DataInfo.Add("info", $"Kunde: {targetPlayer.GetName()} | Vermieter: {dbPlayer.GetName()} | " +
                            $"Fahrzeug: ({sxVeh.databaseId}) {(sxVeh.Data.mod_car_name == "" ? sxVeh.Data.Model : sxVeh.Data.mod_car_name)} | Datum: {mietDate.ToString("dd.MM.yyyy HH:mm")} - {endMietDate.ToString("dd.MM.yyyy HH:mm")}");

                        dbPlayer.Container.AddItem(VehicleRentModule.VehicleRentItemId, 1, DataInfo);
                        targetPlayer.Container.AddItem(VehicleRentModule.VehicleRentItemId, 1, DataInfo);

                        dbPlayer.AddPlayerVehicleKeyForPlayer(targetPlayer, databaseId, DateTime.Now.AddDays(days));
                        dbPlayer.SendNewNotification($"{targetPlayer.GetName()} Schlüssel von {(sxVeh.Data.mod_car_name == "" ? sxVeh.Data.Model : sxVeh.Data.mod_car_name)} für {days} Tage gegeben!");
                        targetPlayer.SendNewNotification($"Sie haben den Schlüssel {(sxVeh.Data.mod_car_name == "" ? sxVeh.Data.Model : sxVeh.Data.mod_car_name)} für {days} Tage bekommen!");
                        return;
                    }
                    return;
                }
                catch(Exception e)
                {
                    Logging.Logger.Crash(e);
                    return;
                }
            }));
        }

        [RemoteEvent]
        public void PlayerRentDays(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                
                if(!Int32.TryParse(returnstring, out int days))
                {
                    dbPlayer.SendNewNotification("Falsche Anzahl an Tagen!");
                    return;
                }

                dbPlayer.SetData("vehicleRentDays", days);
                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Fahrzeug vermieten", Callback = "GivePlayerRentKey", Message = "An wen soll das Fahrzeug vermietet werden?" });
            }));
        }
    }
}
