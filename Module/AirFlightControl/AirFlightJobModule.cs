using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.AirFlightControl
{
    public class AirFlightJobModule : Module<AirFlightJobModule>
    {
        // dbplayer, questid
        public Dictionary<uint, uint> ActiveQuests = new Dictionary<uint, uint>();

        protected override bool OnLoad()
        {
            // Load Menu
            ActiveQuests = new Dictionary<uint, uint>();

            MenuManager.Instance.AddBuilder(new AirFlightControl.Menu.AirFlightJobStartMenu());
            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            AirFlightAirport airport = AirFlightAirportModule.Instance.GetByPosition(dbPlayer.Player.Position);
            if (airport == null)
            {
                if (!dbPlayer.RageExtension.IsInVehicle)
                    return false;

                if (!dbPlayer.HasActiveAirflightQuest())
                    return false;

                airport = AirFlightAirportModule.Instance.GetByLoadingPosition(dbPlayer.Player.Position);
                if (airport == null)
                    return false;

                SxVehicle plane = dbPlayer.Player.Vehicle.GetVehicle();
                if (plane == null || !plane.IsValid() || plane.databaseId <= 0 || plane.Data.ClassificationId != 9)
                    return false;

                AirFlightAirportQuests quest = dbPlayer.GetActiveAirflightQuest();
                if (quest == null)
                    return false;

                // entladepunkt
                if (airport.Id == quest.DestinationAirport)
                {
                    if (!plane.HasData("airflight_loaded"))
                        return true;

                    plane.ResetData("airflight_loaded");

                    Task.Run(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, (12000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        plane.CanInteract = false;
                        plane.SyncExtension.SetEngineStatus(false);

                        await Task.Delay(12000);

                        if (plane == null || dbPlayer == null || !dbPlayer.IsValid()) return;

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        plane.CanInteract = true;
                        plane.SyncExtension.SetEngineStatus(true);

                        int reward = new Random().Next(quest.MinReward, quest.MaxReward);

                        int fuel = Convert.ToInt32(plane.fuel) + 150;

                        if (fuel > plane.Data.Fuel)
                        {
                            fuel = plane.Data.Fuel;
                        }

                        plane.fuel = fuel;
                        dbPlayer.SendNewNotification($"Auftrag erfolgreich ausgeführt! Verdienst ${reward}");
                        dbPlayer.GiveBankMoney(reward, "Job-Verdienst: Pilot");

                        if (ActiveQuests.ContainsKey(dbPlayer.Id))
                        {
                            ActiveQuests.Remove(dbPlayer.Id);
                        }
                    });

                    return true;
                } // Beladepunkt
                else if (airport.Id == quest.SourceAirport)
                {
                    if (plane.HasData("airflight_loaded"))
                        return false;

                    AirFlightAirport destinationAirport = AirFlightAirportModule.Instance.GetAll().Values.Where(a => a.Id == quest.DestinationAirport).FirstOrDefault();
                    if (destinationAirport == null)
                        return false;

                    plane.SetData("airflight_loaded", true);

                    Task.Run(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, (12000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        plane.CanInteract = false;
                        plane.SyncExtension.SetEngineStatus(false);

                        await Task.Delay(12000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        if (plane == null || dbPlayer == null || !dbPlayer.IsValid()) return;

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        plane.CanInteract = true;
                        plane.SyncExtension.SetEngineStatus(true);

                        dbPlayer.SendNewNotification($"Flugzeug beladen, gebe diese am Zielflughafen {destinationAirport.Name} ab. (markiert im GPS)");
                        dbPlayer.Player.TriggerNewClient("setPlayerGpsMarker", destinationAirport.LoadingPoint.X, destinationAirport.LoadingPoint.Y);
                    });

                    return true;
                }

                return true;
            }

            if (dbPlayer.IsInDuty())
                return true;

            if (dbPlayer.Lic_PlaneA[0] != 1)
            {
                dbPlayer.SendNewNotification("Sie benötigen einen Flugschein A um diesen Beruf auszuüben!");
                return true;
            }

            Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.AirFlightJobStartMenu, dbPlayer).Show(dbPlayer);
            return true;
        }

    }

    public static class AirportJobExtension
    {
        public static bool HasActiveAirflightQuest(this DbPlayer dbPlayer)
        {
            return AirFlightJobModule.Instance.ActiveQuests.ContainsKey(dbPlayer.Id);
        }

        public static AirFlightAirportQuests GetActiveAirflightQuest(this DbPlayer dbPlayer)
        {
            if (!AirFlightJobModule.Instance.ActiveQuests.ContainsKey(dbPlayer.Id)) return null;

            return AirFlightAirportQuestsModule.Instance.GetAll().Values.ToList().Where(q => q.Id == AirFlightJobModule.Instance.ActiveQuests[dbPlayer.Id]).FirstOrDefault(); 
        }
    }
}
