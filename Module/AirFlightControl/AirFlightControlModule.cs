using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Voice;

namespace VMP_CNR.Module.AirFlightControl
{
    public class AirFlightControlModule : Module<AirFlightControlModule>
    {
        public List<DbPlayer> TowerPlayers = new List<DbPlayer>();
        public List<DbPlayer> Workers = new List<DbPlayer>();

        public List<uint> RegistredPlanes = new List<uint>();

        public static Vector3 TowerPosition = new Vector3(-2358.05, 3249.96, 101.451);
        public static Vector3 ApplyWorkersPosition = new Vector3(-2364.39, 3245.6, 92.9037);

        protected override bool OnLoad()
        {
            TowerPlayers = new List<DbPlayer>();
            Workers = new List<DbPlayer>();
            Spawners.ColShapes.Create(TowerPosition, 4.0f, 0).SetData("towerShape", true);

            return base.OnLoad();
        }

        public override void OnFiveSecUpdate()
        {
            if (!ServerFeatures.IsActive("airflight"))
                return;

            try
            {
                List<CustomMarkerClientObject> Planes = new List<CustomMarkerClientObject>();

                foreach (SxVehicle sxVeh in VehicleHandler.Instance.GetClassificationVehicles(9).Where(v => v.IsValid() && v.databaseId > 0 && v.GetOccupants().GetUsedSeats() > 0 && v.GetSpeed() > 0))
                {
                    int colorCode = 1; // rot

                    if (RegistredPlanes.ToList().Contains(sxVeh.databaseId))
                    {
                        colorCode = 2;
                    }
                    if (sxVeh.teamid == (uint)TeamTypes.TEAM_ARMY || sxVeh.teamid == (uint)TeamTypes.TEAM_POLICE || sxVeh.teamid == (uint)TeamTypes.TEAM_MEDIC)
                    {
                        continue;
                    }
                    if (!sxVeh.GpsTracker)
                    {
                        colorCode = 40;
                    }

                    Planes.Add(new CustomMarkerClientObject() { Name = sxVeh.GetName() + " (" + sxVeh.databaseId + ")", Position = sxVeh.entity.Position, Color = colorCode, MarkerId = 90 });
                }

                foreach (DbPlayer dbPlayer in TowerPlayers.ToList())
                {
                    if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                    dbPlayer.Player.TriggerNewClient("setcustommarks", CustomMarkersKeys.AirFlightControl, false, NAPI.Util.ToJson(Planes));
                    dbPlayer.SetData("planeMarks", true);
                }

                NAPI.Task.Run(() =>
                {
                    foreach (DbPlayer dbPlayer in TeamModule.Instance.Get((uint)TeamTypes.TEAM_ARMY).GetTeamMembers().Where(t => t.RageExtension.IsInVehicle).ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;

                        SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                        if (sxVeh == null || !sxVeh.IsValid() || sxVeh.teamid != (uint)TeamTypes.TEAM_ARMY || (sxVeh.Data.ClassificationId != 9 && sxVeh.Data.ClassificationId != 8)) continue;

                        dbPlayer.Player.TriggerNewClient("setcustommarks", CustomMarkersKeys.AirFlightControl, false, NAPI.Util.ToJson(Planes));
                        dbPlayer.SetData("planeMarks", true);
                    }
                });
            }
            catch(Exception e)
            {
                Logger.Crash(e);
            }
        }

        public override void OnPlayerEnterVehicle(DbPlayer dbPlayer, Vehicle vehicle, sbyte seat)
        {
            if (seat != 0 && seat != 1) return;

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            SxVehicle veh = vehicle.GetVehicle();
            if(veh != null && veh.IsValid() && veh.Data != null && (veh.Data.ClassificationId == 9 || (veh.Data.ClassificationId == 8 && veh.teamid == (uint)TeamTypes.TEAM_ARMY)) && veh.GpsTracker)
            {
                string kennung = veh.GetName() + " (" + veh.databaseId + ")";
                dbPlayer.SendNewNotification("Sie befinden sich nun im Funk für Luftverkehr! (Funk mit M aus/anschaltbar)", PlayerNotification.NotificationType.INFO, "Air-Control Tower", 10000);
                dbPlayer.SendNewNotification($"Ihre Kennung ist {kennung}.", PlayerNotification.NotificationType.INFO, "Air-Control Tower", 10000);

                dbPlayer.funkAirStatus = FunkStatus.Hearing;
                dbPlayer.Player.TriggerNewClient("setAirRadio", true);
                dbPlayer.Player.TriggerNewClient("updateAirRadio", (int)FunkStatus.Hearing);
                VoiceModule.Instance.refreshAirFunk();
            }
        }


        public override void OnPlayerExitVehicle(DbPlayer dbPlayer, Vehicle vehicle)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if(dbPlayer.funkAirStatus != FunkStatus.Deactive)
            {
                dbPlayer.funkAirStatus = FunkStatus.Deactive;
                dbPlayer.SendNewNotification("Sie haben den Funk für Luftverkehr verlassen!", PlayerNotification.NotificationType.INFO, "Air-Control Tower");

                dbPlayer.Player.TriggerNewClient("setAirRadio", false);
                VoiceModule.Instance.RemoveFromAirFunk(dbPlayer);
                VoiceModule.Instance.refreshAirFunk();
            }

            if (vehicle != null)
            {
                SxVehicle veh = vehicle.GetVehicle();
                if (veh != null && veh.IsValid() && veh.teamid == (uint)TeamTypes.TEAM_ARMY && (veh.Data.ClassificationId == 8 || veh.Data.ClassificationId == 9))
                {
                    dbPlayer.Player.TriggerNewClient("clearcustommarks", CustomMarkersKeys.AirFlightControl);
                    dbPlayer.ResetData("planeMarks");
                }
            }
        }

        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.RageExtension.IsInVehicle && !TowerPlayers.Contains(dbPlayer))
            {
                if (dbPlayer.HasData("planeMarks"))
                {
                    dbPlayer.Player.TriggerNewClient("clearcustommarks", CustomMarkersKeys.AirFlightControl);
                    dbPlayer.ResetData("planeMarks");
                }

                if (dbPlayer.funkAirStatus != FunkStatus.Deactive)
                {
                    dbPlayer.funkAirStatus = FunkStatus.Deactive;

                    dbPlayer.Player.TriggerNewClient("setAirRadio", false);
                    VoiceModule.Instance.RemoveFromAirFunk(dbPlayer);
                    VoiceModule.Instance.refreshAirFunk();
                }
            }
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (TowerPlayers.Contains(dbPlayer))
            {
                TowerPlayers.Remove(dbPlayer);
            }

            if (Workers.Contains(dbPlayer))
            {
                Workers.Remove(dbPlayer);
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if(key == Key.E)
            {
                if(dbPlayer != null && dbPlayer.IsValid())
                {
                    if(dbPlayer.Player.Position.DistanceTo(ApplyWorkersPosition) < 3.0f && dbPlayer.TeamId == (uint)TeamTypes.TEAM_ARMY && dbPlayer.TeamRank >= 6)
                    {
                        // Menu
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Air Control Freigabe", Callback = "armysetacf", Message = "Wem möchten Sie eine Freigabe erteilen/entziehen:" });
                        return true;
                    }
                    if (dbPlayer.Player.Position.DistanceTo(TowerPosition) < 3.0f && (dbPlayer.TeamId == (uint)TeamTypes.TEAM_ARMY || Workers.Contains(dbPlayer)))
                    {
                        // Menu
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Air Control Registration", Callback = "armyaddflight", Message = "Flugobjekt registrieren (Kennung):" });
                        return true;
                    }
                }
            }

            return false;
        }

        public override void OnMinuteUpdate()
        {
            foreach(DbPlayer dbPlayer in Workers.ToList())
            {
                if(dbPlayer != null && dbPlayer.IsValid() && dbPlayer.Player.Position.DistanceTo(TowerPosition) < 5.0f)
                {
                    dbPlayer.paycheck[0] += 270;
                }
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if(colShape.HasData("towerShape"))
            {
                if(colShapeState == ColShapeState.Enter)
                {
                    if(!TowerPlayers.Contains(dbPlayer))
                    {
                        if (dbPlayer.TeamId != (uint)TeamTypes.TEAM_ARMY && !Workers.Contains(dbPlayer)) return false;

                        TowerPlayers.Add(dbPlayer);
                        dbPlayer.SendNewNotification("Sie befinden sich nun im Funk für Luftverkehr!", PlayerNotification.NotificationType.INFO, "Air-Control Tower");

                        dbPlayer.funkAirStatus = FunkStatus.Hearing;
                        dbPlayer.Player.TriggerNewClient("setAirRadio", true);
                        dbPlayer.Player.TriggerNewClient("updateAirRadio", (int)FunkStatus.Hearing);
                        VoiceModule.Instance.refreshAirFunk();
                    }
                }
                else
                {
                    if (TowerPlayers.Contains(dbPlayer))
                    {
                        TowerPlayers.Remove(dbPlayer);
                        dbPlayer.Player.TriggerNewClient("clearcustommarks", CustomMarkersKeys.AirFlightControl);
                        dbPlayer.funkAirStatus = FunkStatus.Deactive;
                        dbPlayer.SendNewNotification("Sie haben den Funk für Luftverkehr verlassen!", PlayerNotification.NotificationType.INFO, "Air-Control Tower");

                        dbPlayer.Player.TriggerNewClient("setAirRadio", false);

                        VoiceModule.Instance.RemoveFromAirFunk(dbPlayer);
                        VoiceModule.Instance.refreshAirFunk();
                    }
                }
            }

            return false;
        }

        public void TurnOnOffFunkState(DbPlayer dbPlayer)
        {
            if(dbPlayer.funkAirStatus == FunkStatus.Deactive)
            {
                dbPlayer.funkAirStatus = FunkStatus.Hearing;
                dbPlayer.Player.TriggerNewClient("setAirRadio", true);
                dbPlayer.Player.TriggerNewClient("updateAirRadio", (int)FunkStatus.Hearing);
                VoiceModule.Instance.refreshAirFunk();
            }
            else 
            {
                dbPlayer.funkAirStatus = FunkStatus.Deactive;
                dbPlayer.Player.TriggerNewClient("setAirRadio", false);

                VoiceModule.Instance.RemoveFromAirFunk(dbPlayer);

                VoiceModule.Instance.refreshAirFunk();
            }
        }
    }

    public class AirflightFunctions : Script
    {
        [RemoteEvent]
        public static void changeAirFunk(Player client, int state, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            if (dbPlayer.funkAirStatus == FunkStatus.Deactive) return;

            FunkStatus old = dbPlayer.funkAirStatus;
            dbPlayer.funkAirStatus = (FunkStatus)state;

            dbPlayer.Player.TriggerNewClient("updateAirRadio", state);

            try
            {
                // Send sounds
                if (old == FunkStatus.Hearing && dbPlayer.funkAirStatus == FunkStatus.Active)
                {
                    if (dbPlayer.HasData("lastFunkStartSound"))
                    {
                        DateTime lastSound = dbPlayer.GetData("lastFunkStartSound");
                        if(lastSound.AddSeconds(3) < DateTime.Now)
                        {
                            dbPlayer.SetData("lastFunkStartSound", DateTime.Now);
                            VoiceModule.Instance.sendSoundToAirFunk("Start_Squelch", "CB_RADIO_SFX");
                        }
                    }
                    else {
                        dbPlayer.SetData("lastFunkStartSound", DateTime.Now);
                        VoiceModule.Instance.sendSoundToAirFunk("Start_Squelch", "CB_RADIO_SFX");
                    }
                }
                else if (old == FunkStatus.Active && dbPlayer.funkAirStatus == FunkStatus.Hearing)
                {
                    if (dbPlayer.HasData("lastFunkEndSound"))
                    {
                        DateTime lastSound = dbPlayer.GetData("lastFunkEndSound");
                        if (lastSound.AddSeconds(3) < DateTime.Now)
                        {
                            dbPlayer.SetData("lastFunkEndSound", DateTime.Now);
                            VoiceModule.Instance.sendSoundToAirFunk("End_Squelch", "CB_RADIO_SFX");
                        }
                    }
                    else
                    {
                        dbPlayer.SetData("lastFunkEndSound", DateTime.Now);
                        VoiceModule.Instance.sendSoundToAirFunk("End_Squelch", "CB_RADIO_SFX");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            if (dbPlayer.funkAirStatus == FunkStatus.Active)
            {
                if (!VoiceModule.Instance.airFunkTalkingPlayers.ToList().Contains(dbPlayer))
                {
                    VoiceModule.Instance.airFunkTalkingPlayers.Add(dbPlayer);
                }
            }
            else
            {
                if (VoiceModule.Instance.airFunkTalkingPlayers.ToList().Contains(dbPlayer))
                {
                    VoiceModule.Instance.airFunkTalkingPlayers.Remove(dbPlayer);
                }
            }
            VoiceModule.Instance.refreshAirFunk();
        }


        [RemoteEvent]
        public void armysetacf(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamId != (int)TeamTypes.TEAM_ARMY && dbPlayer.TeamRank < 6) return;

            DbPlayer target = Players.Players.Instance.FindPlayer(returnstring);
            if (target != null && target.IsValid())
            {
                if (target.TeamId != (uint)TeamTypes.TEAM_CIVILIAN) return;

                if(AirFlightControlModule.Instance.Workers.ToList().Contains(target))
                {
                    AirFlightControlModule.Instance.Workers.Remove(target);

                    target.SendNewNotification($"{dbPlayer.GetName()} hat Ihnen die Freigabe für Air-Control entzogen!");

                    dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {target.GetName()} die Freigabe für Air-Control entzogen!");
                    return;
                }
                else
                {
                    AirFlightControlModule.Instance.Workers.Add(target);

                    target.SendNewNotification($"{dbPlayer.GetName()} hat Ihnen die Freigabe für Air-Control erteilt!");

                    dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {target.GetName()} die Freigabe für Air-Control erteilt!");
                    return;
                }
            }
            return;
        }


        [RemoteEvent]
        public void armyaddflight(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamId != (int)TeamTypes.TEAM_ARMY && dbPlayer.TeamRank < 6) return;

            if(!UInt32.TryParse(returnstring, out uint FlightNumber))
            {
                return;
            }

            if(AirFlightControlModule.Instance.RegistredPlanes.Contains(FlightNumber))
            {
                AirFlightControlModule.Instance.RegistredPlanes.Remove(FlightNumber);

                dbPlayer.SendNewNotification($"Flug {FlightNumber} aus der Registration entfernt!");
            }
            else
            {
                if (VehicleHandler.Instance.GetAllVehicles().Where(
                    v => v.IsValid() && v.databaseId > 0 && v.GetOccupants().GetUsedSeats() > 0 
                    && (v.Data.ClassificationId == 9)
                    && v.entity != null && v.databaseId == FlightNumber).Count() > 0) {

                    AirFlightControlModule.Instance.RegistredPlanes.Add(FlightNumber);

                    dbPlayer.SendNewNotification($"Flug {FlightNumber} wurde registriert!");
                }
            }


            return;
        }
    }
}
