using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Buffs;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Meth
{
    public sealed class HeroinModule : Module<HeroinModule>
    {
        public static Vector3 CamperInteriorPosition = new Vector3(1973.07, 3816.15, 33.4287);

        public static float CamperDrugAirRange = 60.0f;

        public static float DrugLabIncreaseRange = 20.0f;

        public static List<DbPlayer> CookingPlayers = new List<DbPlayer>();

        public static List<SxVehicle> CookingVehicles = new List<SxVehicle>();

        public int morphin = 0;
        public int essigsaeure = 0;
        public int natriumcarbonat = 0;
        public int cooker = 0;
        public int heroinampullen = 0;

        private readonly Random _random = new Random();

        public static uint morphinId = 386;
        public static uint essigsaeureId = 1439;
        public static uint natriumcarbonatId = 1441;
        public static uint cookerId = 13;
        public static uint heroinampullenId = 1442;

        public void ResetLogVariables()
        {
            morphin = 0;
            essigsaeure = 0;
            natriumcarbonat = 0;
            cooker = 0;
        }

        public override bool Load(bool reload = false)
        {
            CookingPlayers = new List<DbPlayer>();
            CookingVehicles = new List<SxVehicle>();
            return base.Load(reload);
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (dbPlayer.DimensionType[0] != DimensionType.Camper) return;
            dbPlayer.DimensionType[0] = DimensionType.World;
            var sxVehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.Player.Dimension);
            dbPlayer.SetDimension(0);
            dbPlayer.Dimension[0] = 0;
            if (sxVehicle == null)
            {
                if (!dbPlayer.HasData("CamperEnterPos")) return;
                Vector3 enterPosition = dbPlayer.GetData("CamperEnterPos");
                dbPlayer.Player.SetPosition(new Vector3(enterPosition.X + 3, enterPosition.Y,
                    enterPosition.Z));
            }
            else
            {
                try
                {
                    if (sxVehicle.Visitors.Contains(dbPlayer)) sxVehicle.Visitors.Remove(dbPlayer);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }

                dbPlayer.Player.SetPosition(new Vector3(sxVehicle.entity.Position.X + 3, sxVehicle.entity.Position.Y,
                    sxVehicle.entity.Position.Z));
            }
        }

        public override void OnMinuteUpdate()
        {
            // Reset Cooking State
            CookingVehicles.Clear();


            Dictionary<Vector3, string> Messages = new Dictionary<Vector3, string>();
            var sendMethVehs = new List<uint>();
            foreach (DbPlayer dbPlayer in CookingPlayers.ToList())
            {
                if (!dbPlayer.IsValid())
                    continue;
                

                //Meth Cooking
                if (dbPlayer.HasData("cooking") && dbPlayer.DimensionType[0] == DimensionType.Camper)
                {
                    if(dbPlayer.Player.Position.DistanceTo(CamperInteriorPosition) > 20.0f)
                    {
                        Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                            dbPlayer.GetName() + " Camper glitch (kochend entfernt) wurde gekickt!");
                        DatabaseLogging.Instance.LogAdminAction(dbPlayer.Player, dbPlayer.GetName(), AdminLogTypes.kick, "Camper kochend entfernt", 0,
                            Configuration.Instance.DevMode);
                        dbPlayer.ResetData("cooking");
                        if (CookingPlayers.Contains(dbPlayer)) CookingPlayers.Remove(dbPlayer);
                        dbPlayer.Kick("Du musst im Camper bleiben");
                        continue;
                    }

                    if (dbPlayer.Container.GetItemAmount(cookerId) >=
                        1 && dbPlayer.Container.GetItemAmount(natriumcarbonatId) >= 1 && 
                        dbPlayer.Container.GetItemAmount(essigsaeureId) >=
                        1 && dbPlayer.Container.GetItemAmount(morphinId) >= 1)
                    {
                        var explode = _random.Next(1, 16);
                        if (explode == 1)
                        {
                            dbPlayer.SendNewNotification("1337Allahuakbar$explode");
                            dbPlayer.Container.RemoveItem(cookerId, 1);
                            cooker++;

                            dbPlayer.SetHealth(dbPlayer.Player.Health - 20);

                            dbPlayer.ResetData("cooking");
                            if (CookingPlayers.Contains(dbPlayer)) CookingPlayers.Remove(dbPlayer);

                            var journeyDbId = dbPlayer.Player.Dimension;
                            if (!sendMethVehs.Contains(journeyDbId))
                            {
                                sendMethVehs.Add(journeyDbId);
                                var sxveh =
                                    VehicleHandler.Instance.GetByVehicleDatabaseId(journeyDbId);
                                if (sxveh != null && sxveh.entity != null)
                                {
                                    Messages.Add(sxveh.entity.Position, "1337Allahuakbar$explode");
                                }
                            }
                        }
                        else
                        {
                            SxVehicle sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.Player.Dimension);
                            if (sxVeh != null && sxVeh.IsValid())
                            {
                                sxVeh.respawnInteractionState = true;
                                sxVeh.respawnInterval = 0;
                            }

                            var heroinAmount = _random.Next(6, 11); //4 included, 11 excluded
                            
                            dbPlayer.Container.RemoveItem(morphinId, 1);
                            dbPlayer.Container.RemoveItem(natriumcarbonatId, 1);
                            dbPlayer.Container.RemoveItem(essigsaeureId, 1);

                            dbPlayer.SendNewNotification("Sie haben erfolgreich " + heroinAmount +
                                " Heroinampullen gekocht!");
                            dbPlayer.IncreasePlayerDrugInfection();

                            if (!dbPlayer.Container.CanInventoryItemAdded(heroinampullenId, heroinAmount))
                            {
                                dbPlayer.SendNewNotification("Dein Inventar ist voll!");
                                dbPlayer.ResetData("cooking");
                                if (CookingPlayers.Contains(dbPlayer)) CookingPlayers.Remove(dbPlayer);
                            }
                            else
                            {
                                dbPlayer.Container.AddItem(heroinampullenId, heroinAmount);
                                morphin += 10;
                                this.heroinampullen += heroinAmount;

                                var journeyDbId = dbPlayer.Player.Dimension;
                                if (!sendMethVehs.Contains(journeyDbId))
                                {
                                    sendMethVehs.Add(journeyDbId);
                                    var sxveh =
                                        VehicleHandler.Instance.GetByVehicleDatabaseId(journeyDbId);
                                    
                                    if (sxveh != null && sxveh.entity != null)
                                    {
                                        if (!CookingVehicles.Contains(sxveh)) CookingVehicles.Add(sxveh);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Da sie keine Materialien mehr haben, ist der Kocher ausgegangen!");
                        dbPlayer.ResetData("cooking");
                        if (CookingPlayers.Contains(dbPlayer)) CookingPlayers.Remove(dbPlayer);
                    }
                }
            }

            // Send Messages together....
            foreach (var xPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if (xPlayer == null || !xPlayer.IsValid()) continue;
                foreach(KeyValuePair<Vector3, string> kvp in Messages)
                {
                    if (xPlayer.Player.Position.DistanceTo(kvp.Key) < 30)
                    {
                        xPlayer.SendNewNotification(kvp.Value, title: "Chemikalien");
                    }
                }
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            switch (key)
            {
                case Key.E:
                    // Check Vehicle in Range (Wohnwagen)
                    SxVehicle sxVeh;

                    if (dbPlayer.DimensionType[0] == DimensionType.Camper && dbPlayer.Player.Dimension != 0)
                    {
                        if (dbPlayer.Player.Position.DistanceTo(CamperInteriorPosition) > 2.5f) return false;

                        sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.Player.Dimension);
                        if (sxVeh == null) return false;
                        if (sxVeh.SyncExtension.Locked) return false;
                        if (sxVeh.entity.Model != (uint) VehicleHash.Journey &&
                            sxVeh.entity.Model != (uint) VehicleHash.Camper)

                            if (sxVeh.Visitors.Contains(dbPlayer))
                                sxVeh.Visitors.Remove(dbPlayer);
                        dbPlayer.DimensionType[0] = DimensionType.World;
                        dbPlayer.Dimension[0] = 0;
                        dbPlayer.SetDimension(0);

                        // Reset Cooking on Exit
                        if (dbPlayer.HasData("cooking"))
                        {
                            dbPlayer.ResetData("cooking");
                        }
                        if (CookingPlayers.Contains(dbPlayer)) CookingPlayers.Remove(dbPlayer);

                        
                        dbPlayer.Player.SetPosition(new Vector3(sxVeh.entity.Position.X + 3.0f, sxVeh.entity.Position.Y,
                            sxVeh.entity.Position.Z + 0.5f));
                        dbPlayer.ResetData("CamperEnterPos");
                        return true;
                    }

                    sxVeh = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);

                    if (sxVeh == null || sxVeh.databaseId == 0) return false;
                    if (sxVeh.entity.Model != (uint) VehicleHash.Journey &&
                        sxVeh.entity.Model != (uint) VehicleHash.Camper)
                        return false;
                    if (sxVeh.SyncExtension.Locked) return false;
                    
                    Task.Run(async () =>
                    {
                        dbPlayer.SetData("CamperEnterPos", dbPlayer.Player.Position);
                        dbPlayer.SetDimension(sxVeh.databaseId);
                        dbPlayer.DimensionType[0] = DimensionType.Camper;
                        dbPlayer.Dimension[0] = sxVeh.databaseId;
                        sxVeh.Visitors.Add(dbPlayer);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.Player.SetPosition(CamperInteriorPosition);

                        await NAPI.Task.WaitForMainThread(1000);
                        dbPlayer.Player.SetPosition(CamperInteriorPosition);

                        await NAPI.Task.WaitForMainThread(1500);
                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    });
                    // Set Player INTO
                    sxVeh.Visitors.Add(dbPlayer);
                    return true;
                case Key.L:
                    if (dbPlayer.DimensionType[0] == DimensionType.Camper)
                    {
                        var vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.Player.Dimension);

                        if (vehicle == null) return false;

                        // player has no right to operate vehicle
                        if (!dbPlayer.CanControl(vehicle)) return false;

                        if (vehicle.SyncExtension.Locked)
                        {
                            // closed to opene
                            vehicle.SyncExtension.SetLocked(false);
                            dbPlayer.SendNewNotification("Fahrzeug aufgeschlossen!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        }
                        else
                        {
                            // open to closed
                            vehicle.SyncExtension.SetLocked(true);
                            dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.ERROR);
                        }

                        return true;
                    }

                    break;
            }

            return false;
        }
    }
}