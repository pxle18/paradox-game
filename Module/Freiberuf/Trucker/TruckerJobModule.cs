using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Freiberuf.Trucker.Menu;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Freiberuf.Trucker
{
    public class TruckerJobModule : Module<TruckerJobModule>
    {
        // dbplayer ID, questid
        public Dictionary<uint, uint> ActiveQuests = new Dictionary<uint, uint>();

        protected override bool OnLoad()
        {
            ActiveQuests = new Dictionary<uint, uint>();

            MenuManager.Instance.AddBuilder(new TruckerJobStartMenu());
            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            TruckerDepot truckerDepot = TruckerDepotLoadModule.Instance.GetByPosition(dbPlayer.Player.Position);
            if (truckerDepot == null)
            {
                if (!dbPlayer.RageExtension.IsInVehicle)
                    return false;

                if (!dbPlayer.HasActiveTruckerQuest())
                    return false;

                truckerDepot = TruckerDepotLoadModule.Instance.GetByLoadingPosition(dbPlayer.Player.Position);
                if (truckerDepot == null)
                    return false;

                SxVehicle truck = dbPlayer.Player.Vehicle.GetVehicle();
                if (truck == null || !truck.IsValid() || truck.Data.Id != 40 || truck.GetSpeed() > 10)
                    return false;

                TruckerQuest quest = dbPlayer.GetActiveTruckerQuest();
                if (quest == null)
                    return false;

                if (truckerDepot.Id == quest.DestinationTruckerDepot) // Entladepunkt
                {
                    if (!truck.HasData("trucker_loaded_" + dbPlayer.Id))
                        return true;

                    truck.ResetData("trucker_loaded_" + dbPlayer.Id);

                    Task.Run(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, (12000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        truck.CanInteract = false;
                        truck.SyncExtension.SetEngineStatus(false);

                        await Task.Delay(12000);

                        if (truck == null || dbPlayer == null || !dbPlayer.IsValid()) return;

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        truck.CanInteract = true;
                        truck.SyncExtension.SetEngineStatus(true);

                        int reward = new Random().Next(quest.MinReward, quest.MaxReward);

                        dbPlayer.SendNewNotification($"Auftrag erfolgreich ausgeführt! Verdienst ${reward}");
                        dbPlayer.GiveBankMoney(reward, "Job-Verdienst: Trucker");

                        if (TruckerJobModule.Instance.ActiveQuests.ContainsKey(dbPlayer.Id))
                        {
                            TruckerJobModule.Instance.ActiveQuests.Remove(dbPlayer.Id);
                        }
                    });

                    return true;
                }

                if (truckerDepot.Id == quest.SourceTruckerDepot) // Beladepunkt
                {
                    if (truck.HasData("trucker_loaded_" + dbPlayer.Id))
                        return false;

                    TruckerDepot destinationDepot = TruckerDepotLoadModule.Instance.GetAll().Values
                        .FirstOrDefault(a => a.Id == quest.DestinationTruckerDepot);
                    if (destinationDepot == null)
                        return false;

                    truck.SetData("trucker_loaded_" + dbPlayer.Id, true);

                    Task.Run(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, (12000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        truck.CanInteract = false;
                        truck.SyncExtension.SetEngineStatus(false);

                        await Task.Delay(12000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        if (truck == null || dbPlayer == null || !dbPlayer.IsValid()) return;

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        truck.CanInteract = true;
                        truck.SyncExtension.SetEngineStatus(true);

                        dbPlayer.SendNewNotification(
                            $"Pounder beladen! Gebe die Ladung am Zieldepot {destinationDepot.Name} ab. (markiert im GPS)",
                            PlayerNotification.NotificationType.SUCCESS
                        );
                        dbPlayer.Player.TriggerNewClient(
                            "setPlayerGpsMarker",
                            destinationDepot.LoadingPoint.X,
                            destinationDepot.LoadingPoint.Y
                        );
                    });

                    return true;
                }

                return true;
            }

            if (dbPlayer.RageExtension.IsInVehicle)
                return true;

            if (dbPlayer.IsInDuty())
                return true;

            if (dbPlayer.Lic_LKW[0] != 1)
            {
                dbPlayer.SendNewNotification("Sie benötigen einen LKW-Führerschein um diesen Beruf auszuüben!");
                return true;
            }

            MenuManager.Instance.Build(PlayerMenu.TruckerJobStartMenu, dbPlayer).Show(dbPlayer);
            return true;
        }
    }

    public static class TruckerJobExtension
    {
        public static bool HasActiveTruckerQuest(this DbPlayer dbPlayer)
        {
            return TruckerJobModule.Instance.ActiveQuests.ContainsKey(dbPlayer.Id);
        }

        public static TruckerQuest GetActiveTruckerQuest(this DbPlayer dbPlayer)
        {
            return !HasActiveTruckerQuest(dbPlayer)
                ? null
                : TruckerQuestLoadModule.Instance.GetAll().Values.ToList()
                    .FirstOrDefault(q => q.Id == TruckerJobModule.Instance.ActiveQuests[dbPlayer.Id]);
        }
    }
}