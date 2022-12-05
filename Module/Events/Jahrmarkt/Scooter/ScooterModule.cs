using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Events.Jahrmarkt.Scooter
{
    public class ScooterModule : Module<ScooterModule>
    {
        public Dictionary<uint, Scooter> Scooters = new Dictionary<uint, Scooter>();

        public bool RoundStarted = false;
        public int MinCount = 0;

        public override void OnMinuteUpdate()
        {
            if (!JahrmarktModule.isActive) return;
            if (!ServerFeatures.IsActive("jahrmarkt-scooter")) return;

            if (RoundStarted == false)
            {
                // Init Sound
                foreach(Scooter scooter in Scooters.Values.ToList())
                {
                    if(scooter.sxVehicle != null && scooter.sxVehicle.IsValid())
                    {
                        if (scooter.sxVehicle.SyncExtension.Locked)
                            scooter.sxVehicle.SyncExtension.SetLocked(false);

                        if(!scooter.sxVehicle.GetOccupants().IsEmpty())
                        {
                            foreach(DbPlayer dbPlayer in scooter.sxVehicle.GetOccupants().GetLegacyDictionary().Values)
                            {
                                dbPlayer.SendNewNotification($"1337Allahuakbar$autoscooter", duration: 190000);
                            }
                        }
                    }
                }
                RoundStarted = true;

                Task.Run(async () =>
                {
                    // Init Sound
                    foreach (Scooter scooter in Scooters.Values.ToList())
                    {
                        if (scooter.sxVehicle != null && scooter.sxVehicle.IsValid())
                        {
                            if (scooter.sxVehicle.GetOccupants().IsEmpty() == false)
                            {
                                foreach (DbPlayer dbPlayer in scooter.sxVehicle.GetOccupants().GetLegacyDictionary().Values)
                                {
                                    dbPlayer.SendNewNotification($"1337Allahuakbar$autoscooter", duration: 190000);
                                }
                            }
                        }
                    }
                    await NAPI.Task.WaitForMainThread(7000);
                    foreach (Scooter scooter in Scooters.Values.ToList())
                    {
                        if (scooter.sxVehicle != null && scooter.sxVehicle.IsValid())
                        {
                            if (!scooter.sxVehicle.GetOccupants().IsEmpty() && scooter.CoinInserted)
                            {
                                scooter.CoinInserted = false;
                                scooter.sxVehicle.SyncExtension.SetEngineStatus(true);
                            }
                        }
                    }
                });
            }
            else
            {
                if (MinCount >= 2)
                {
                    // ausmachen
                    Task.Run(async () =>
                    {
                        await NAPI.Task.WaitForMainThread(5000);
                        foreach (Scooter scooter in Scooters.Values.ToList())
                        {
                            if (scooter.sxVehicle != null && scooter.sxVehicle.IsValid())
                            {
                                if (!scooter.sxVehicle.GetOccupants().IsEmpty())
                                {
                                    scooter.sxVehicle.SyncExtension.SetEngineStatus(false);

                                    foreach (DbPlayer dbPlayer in scooter.sxVehicle.GetOccupants().GetLegacyDictionary().Values)
                                    {
                                        dbPlayer.SendNewNotification($"Diese Runde ist vorbei, neuen Coin einschmeißen oder Fahrzeug verlassen!");
                                    }
                                }
                            }
                        }
                        RoundStarted = false;
                        MinCount = 0;
                    });
                }
                else MinCount++;
            }
        }

        protected override bool OnLoad()
        {
            if (!JahrmarktModule.isActive) return true;
            MinCount = 0;
            RoundStarted = false;

            Scooters = new Dictionary<uint, Scooter>();

            Scooters.Add(1, new Scooter() { SpawnPos = new Vector3(-1617.96, -947.17, 8.23718), SpawnRot = 47.098f , CoinInserted = false});
            Scooters[1].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, Scooters[1].SpawnPos, Scooters[1].SpawnRot, -1, -1, 0, false, true, true);
            Scooters[1].sxVehicle.DynamicMotorMultiplier = 35;


            Scooters.Add(2, new Scooter() { SpawnPos = new Vector3(-1620.75, -950.437, 8.23718), SpawnRot = 47.098f, CoinInserted = false });
            Scooters[2].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, Scooters[2].SpawnPos, Scooters[2].SpawnRot, -1, -1, 0, false, true, true);
            Scooters[2].sxVehicle.DynamicMotorMultiplier = 35;

            Scooters.Add(3, new Scooter() { SpawnPos = new Vector3(-1623.23, -953.55, 8.23718), SpawnRot = 47.098f, CoinInserted = false });
            Scooters[3].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, Scooters[3].SpawnPos, Scooters[3].SpawnRot, -1, -1, 0, false, true, true);
            Scooters[3].sxVehicle.DynamicMotorMultiplier = 35;

            Scooters.Add(4, new Scooter() { SpawnPos = new Vector3(-1625.64, -956.67, 8.23718), SpawnRot = 47.098f, CoinInserted = false });
            Scooters[4].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, Scooters[4].SpawnPos, Scooters[4].SpawnRot, -1, -1, 0, false, true, true);
            Scooters[4].sxVehicle.DynamicMotorMultiplier = 35;

            Scooters.Add(5, new Scooter() { SpawnPos = new Vector3(-1628.48, -959.801, 8.23718), SpawnRot = 47.098f, CoinInserted = false });
            Scooters[5].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, Scooters[5].SpawnPos, Scooters[5].SpawnRot, -1, -1, 0, false, true, true);
            Scooters[5].sxVehicle.DynamicMotorMultiplier = 35;

            Scooters.Add(6, new Scooter() { SpawnPos = new Vector3(-1630.56, -962.834, 8.23718), SpawnRot = 47.098f, CoinInserted = false });
            Scooters[6].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, Scooters[6].SpawnPos, Scooters[6].SpawnRot, -1, -1, 0, false, true, true);
            Scooters[6].sxVehicle.DynamicMotorMultiplier = 35;

            Scooters.Add(7, new Scooter() { SpawnPos = new Vector3(-1615.5, -944.159, 8.23718), SpawnRot = 47.098f, CoinInserted = false });
            Scooters[7].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, Scooters[7].SpawnPos, Scooters[7].SpawnRot, -1, -1, 0, false, true, true);
            Scooters[7].sxVehicle.DynamicMotorMultiplier = 35;

            Scooters.Add(8, new Scooter() { SpawnPos = new Vector3(-1612.62, -941.331, 8.23718), SpawnRot = 47.098f, CoinInserted = false });
            Scooters[8].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, Scooters[8].SpawnPos, Scooters[8].SpawnRot, -1, -1, 0, false, true, true);
            Scooters[8].sxVehicle.DynamicMotorMultiplier = 35;

            return base.OnLoad();
        }

        public override void OnPlayerEnterVehicle(DbPlayer dbPlayer, Vehicle vehicle, sbyte seat)
        {
            if (!JahrmarktModule.isActive) return;
            if (!ServerFeatures.IsActive("jahrmarkt-scooter")) return;

            SxVehicle sxVehicle = vehicle.GetVehicle();

            if(sxVehicle != null && sxVehicle.IsValid() && Scooters.Where(s => s.Value.sxVehicle == sxVehicle).Count() > 0)
            {
                dbPlayer.SendNewNotification($"Viel Spaß im Autoscooter, du bekommst während des Fahrens ein paar Jahrmarkt Punkte!");
                if(RoundStarted == false)
                {
                    sxVehicle.SyncExtension.SetEngineStatus(false);
                }
            }
        }

        public override void OnVehicleDeleteTask(SxVehicle sxVehicle)
        {
            if (!JahrmarktModule.isActive) return;
            if (sxVehicle != null)
            {
                Scooter scooter = Scooters.Values.Where(s => s.sxVehicle == sxVehicle).FirstOrDefault();

                if(scooter != null)
                {
                    scooter.CoinInserted = false;
                    scooter.sxVehicle = null;
                }
            }
        }

        public override void OnTenSecUpdate()
        {
            if (!JahrmarktModule.isActive) return;
            if (!ServerFeatures.IsActive("jahrmarkt-scooter")) return;

            foreach (KeyValuePair<uint, Scooter> kvp in Scooters)
            {
                if (kvp.Value == null) continue;


                if (kvp.Value.sxVehicle == null  || !kvp.Value.sxVehicle.IsValid() ||  kvp.Value.sxVehicle.entity == null)
                {
                    NAPI.Task.Run(async () =>
                    {
                        Scooters[kvp.Key].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, Scooters[kvp.Key].SpawnPos, Scooters[kvp.Key].SpawnRot, -1, -1, 0, false, false, true);

                        while (Scooters[kvp.Key].sxVehicle.entity == null)
                        {
                            await NAPI.Task.WaitForMainThread(100);
                        }

                        Scooters[kvp.Key].sxVehicle.DynamicMotorMultiplier = 35;
                    });
                    continue;
                }
                else
                {
                    if(kvp.Value.sxVehicle.entity.Position.DistanceTo(kvp.Value.SpawnPos) > 80)
                    {
                        VehicleHandler.Instance.DeleteVehicle(kvp.Value.sxVehicle, false);
                        continue;
                    }

                    kvp.Value.sxVehicle.fuel = kvp.Value.sxVehicle.Data.Fuel; // fuel
                    kvp.Value.sxVehicle.Repair(); // repair

                    if (kvp.Value.sxVehicle.GetOccupants().IsEmpty() == false)
                    {
                        foreach(DbPlayer dbPlayer in kvp.Value.sxVehicle.GetOccupants().GetLegacyDictionary().Values)
                        {
                            dbPlayer.SetHealth(100); // heal

                            if (dbPlayer.Player.VehicleSeat == 0 && kvp.Value.sxVehicle.GetSpeed() > 10)
                            {
                                dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, 6);
                            }
                        }
                    }
                }
            }
        }
    }

    public class Scooter
    {
        public Vector3 SpawnPos { get; set; }
        public float SpawnRot { get; set; }
        public SxVehicle sxVehicle { get; set; }

        public bool CoinInserted { get; set; }
    }
}
