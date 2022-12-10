using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Space.Menu;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Space
{
    public enum RocketLocation : uint
    {
        Earth = 1,
        Orbit = 2,
        Mars = 3
    }

    public class SpaceModule : Module<SpaceModule>
    {
        public List<DbPlayer> MarsVisitors = new List<DbPlayer>();

        public GTANetworkAPI.Object RocketMars;
        public GTANetworkAPI.Object RocketEarth;

        public static int RocketObjectModel = -570401971;
        public static Vector3 RocketEarthObjectPosition = new Vector3(2508.345, -315.919464, 120.96);
        public static Vector3 RocketEarthObjectRotation = new Vector3(-9.768503E-08, 3.299751E-09, 0.4771588);

        public static Vector3 RocketMarsObjectPosition = new Vector3(-3969.385, -2368.13428, 2670.95825);
        public static Vector3 RocketMarsObjectRotation = new Vector3(-7.975556E-08, -5.650167E-08, -0.1521234);

        public static Vector3 MarsMiddlePosition = new Vector3(-3784.6, -1959.2, 2640.4);
        public static float MarsShapeSize = 480f;
        public static uint RocketDimension = 99;
        public static Vector3 RocketInteriorPosition = new Vector3(3208.73, 178.855, 19.2811);

        public static Vector3 MarsSaveRangeStation = new Vector3(-3826.79, -2064.28, 2641.03);

        public static uint MinutesToArrive = (uint)(Configurations.Configuration.Instance.DevMode ? 1 : 10);

        public static List<uint> SpaceSuits = new List<uint>() { 1505, 1506, 1507, 1508 };

        public static Vector3 MarsShuttleEnter = new Vector3(-3972.27, -2369.08, 2665.57);

        public static Vector3 EarthShuttleEnter = new Vector3(2510.4, -318.159, 115.265);

        public static Vector3 RocketInsidePosition = new Vector3(-946.372, -3027.44, 14.015);
        public static float RocketHeading = 150.593f;

        public static Vector3 RocketStartEarth = new Vector3(2578.68, -294.804, 93.4072);
        public static Vector3 RocketStartMars = new Vector3(-3972.25, -2334.13, 2641.64);


        public ColShape MarsColShape;
        public static bool RocketLocked;
        public static bool LaunchPermission;
        public static bool LaunchInitiated;
        public static RocketLocation CurrentRocketLocation;
        public static RocketLocation DestinationRocketLocation;

        public static DateTime LaunchSequenceStart;

        public static List<DbPlayer> PlayersInRocket = new List<DbPlayer>();

        protected override bool OnLoad()
        {
            MarsColShape = Spawners.ColShapes.Create(MarsMiddlePosition, MarsShapeSize, 0);
            MarsColShape.SetData("mars", 1);

            RocketLocked = true;
            CurrentRocketLocation = RocketLocation.Earth;
            DestinationRocketLocation = RocketLocation.Earth;
            LaunchInitiated = false;
            LaunchPermission = false;
            LaunchSequenceStart = DateTime.Now;

            NAPI.Task.Run(() =>
            {
                RocketEarth = ObjectSpawn.Create(RocketObjectModel, RocketEarthObjectPosition, RocketEarthObjectRotation);
            });

            MarsVisitors = new List<DbPlayer>();

            MenuManager.Instance.AddBuilder(new RocketControlMenu());

            return base.OnLoad();
        }

        public void RefreshRocket()
        {
            NAPI.Task.Run(() =>
            {
                if (RocketMars != null)
                    SpaceModule.Instance.RocketMars.Delete();

                if (RocketEarth != null)
                    SpaceModule.Instance.RocketEarth.Delete();

                switch (CurrentRocketLocation)
                {
                    case RocketLocation.Earth:
                        RocketEarth = ObjectSpawn.Create(RocketObjectModel, RocketEarthObjectPosition, RocketEarthObjectRotation);
                        break;
                    case RocketLocation.Mars:
                        RocketMars = ObjectSpawn.Create(RocketObjectModel, RocketMarsObjectPosition, RocketMarsObjectRotation);
                        break;
                    case RocketLocation.Orbit:
                        break;
                    default:
                        break;
                }
            });
        }

        // Falls Server Crash während man im Orbit ist -> Rückkehr zur Erde
        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            // TODO: Wenn es die zivile Raumfahrt gibt, muss man an einen anderen Punkt geportet werden auf der Erde
            // ODER
            // Wieder in die Mars Liste eintragen, was sich aber wegen dem Raketenstandort schwierig gestalten wird
            // insofern der aktuelle Standort der Rakete nicht in der DB gespeichert wird. (Aktuell)
            if (dbPlayer.DimensionType[0] == DimensionType.Rocket || dbPlayer.IsInSpace())
            {
                dbPlayer.Mars = false;
                dbPlayer.Player.SetPosition(EarthShuttleEnter);
                dbPlayer.SetDimension(0);
                dbPlayer.Dimension[0] = 0;
                dbPlayer.DimensionType[0] = DimensionType.World;
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            if (key != Key.E && key != Key.L) return false;

            switch (key)
            {
                case Key.E:
                    if (dbPlayer.Player.Position.DistanceTo(RocketStartEarth) < 1.0f)
                    {
                        if (dbPlayer.RankId == (int)AdminLevelTypes.SuperAdministrator || dbPlayer.RankId == (int)AdminLevelTypes.Manager || dbPlayer.RankId == (int)AdminLevelTypes.Projektleitung)
                        {
                            if (CurrentRocketLocation == RocketLocation.Orbit)
                            {
                                dbPlayer.SendNewNotification("Die Rakete befindet sich aktuell nicht auf der Erde oder dem Mars!");
                                break;
                            }

                            MenuManager.Instance.Build(PlayerMenu.SpaceRocketControlMenu, dbPlayer).Show(dbPlayer);
                            break;
                        }

                        if (dbPlayer.TeamId != (uint)TeamTypes.TEAM_ARMY)
                            break;

                        if (dbPlayer.TeamRank < 10)
                            break;

                        if (CurrentRocketLocation == RocketLocation.Orbit)
                        {
                            dbPlayer.SendNewNotification("Die Rakete befindet sich aktuell nicht auf der Erde oder dem Mars!");
                            break;
                        }

                        MenuManager.Instance.Build(PlayerMenu.SpaceRocketControlMenu, dbPlayer).Show(dbPlayer);
                        break;
                    }

                    if (dbPlayer.Player.Position.DistanceTo(RocketStartMars) < 1.0f)
                    {
                        if (dbPlayer.RankId == (int)AdminLevelTypes.SuperAdministrator || dbPlayer.RankId == (int)AdminLevelTypes.Manager || dbPlayer.RankId == (int)AdminLevelTypes.Projektleitung)
                        {
                            if (CurrentRocketLocation == RocketLocation.Orbit)
                            {
                                dbPlayer.SendNewNotification("Die Rakete befindet sich aktuell nicht auf der Erde oder dem Mars!");
                                break;
                            }

                            MenuManager.Instance.Build(PlayerMenu.SpaceRocketControlMenu, dbPlayer).Show(dbPlayer);
                            break;
                        }

                        if (dbPlayer.TeamId != (uint)TeamTypes.TEAM_ARMY)
                            break;

                        if (dbPlayer.TeamRank < 10)
                            break;

                        if (CurrentRocketLocation == RocketLocation.Orbit)
                        {
                            dbPlayer.SendNewNotification("Die Rakete befindet sich aktuell nicht auf der Erde oder dem Mars!");
                            break;
                        }

                        MenuManager.Instance.Build(PlayerMenu.SpaceRocketControlMenu, dbPlayer).Show(dbPlayer);
                        break;
                    }

                    if (dbPlayer.Player.Position.DistanceTo(RocketInsidePosition) < 1.0f && CurrentRocketLocation == RocketLocation.Mars)
                    {
                        if (RocketLocked)
                        {
                            dbPlayer.SendNewNotification("Die Rakete ist zugeschlossen!");
                            break;
                        }

                        dbPlayer.EnterMars(true);

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.SetData("userCannotInterrupt", true);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.Player.SetPosition(MarsShuttleEnter);
                            dbPlayer.SetDimension(0);

                            await NAPI.Task.WaitForMainThread(3000);

                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.SetData("userCannotInterrupt", false);
                        }));

                        dbPlayer.SendNewNotification("Du hast die Rakete verlassen.");
                        break;
                    }
                    
                    if (dbPlayer.Player.Position.DistanceTo(RocketInsidePosition) < 1.0f && CurrentRocketLocation == RocketLocation.Earth)
                    {
                        if (RocketLocked)
                        {
                            dbPlayer.SendNewNotification("Die Rakete ist zugeschlossen!");
                            break;
                        }

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.SetData("userCannotInterrupt", true);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.Player.SetPosition(EarthShuttleEnter);
                            dbPlayer.SetDimension(0);

                            await NAPI.Task.WaitForMainThread(3000);

                            dbPlayer.Player.SetPosition(EarthShuttleEnter);
                            dbPlayer.SetDimension(0);

                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.SetData("userCannotInterrupt", false);
                        }));

                        dbPlayer.SetData("userCannotInterrupt", false);

                        if (PlayersInRocket.Contains(dbPlayer))
                            PlayersInRocket.Remove(dbPlayer);

                        dbPlayer.SendNewNotification("Du hast die Rakete verlassen.");
                        break;
                    }

                    if (dbPlayer.Player.Position.DistanceTo(MarsShuttleEnter) < 3.0f)
                    {
                        if (RocketLocked)
                        {
                            dbPlayer.SendNewNotification("Die Rakete ist zugeschlossen!");
                            break;
                        }

                        if (CurrentRocketLocation != RocketLocation.Mars)
                            break;

                        dbPlayer.LeaveMars(true);

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.SetData("userCannotInterrupt", true);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.Player.SetPosition(RocketInsidePosition);
                            dbPlayer.Player.SetRotation(RocketHeading);
                            dbPlayer.SetDimension(RocketDimension);

                            await NAPI.Task.WaitForMainThread(3000);

                            dbPlayer.Player.SetPosition(RocketInsidePosition);
                            dbPlayer.Player.SetRotation(RocketHeading);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.SetData("userCannotInterrupt", false);
                        }));

                        break;
                    }

                    if (dbPlayer.Player.Position.DistanceTo(EarthShuttleEnter) < 3.0f)
                    {
                        if (RocketLocked)
                        {
                            dbPlayer.SendNewNotification("Die Rakete ist zugeschlossen!");
                            break;
                        }

                        if (CurrentRocketLocation != RocketLocation.Earth)
                            break;

                        Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(async () =>
                        {
                            dbPlayer.SetData("userCannotInterrupt", true);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.Player.SetPosition(RocketInsidePosition);
                            dbPlayer.Player.SetRotation(RocketHeading);
                            dbPlayer.SetDimension(RocketDimension);

                            await NAPI.Task.WaitForMainThread(3000);

                            dbPlayer.Player.SetPosition(RocketInsidePosition);
                            dbPlayer.Player.SetRotation(RocketHeading);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.SetData("userCannotInterrupt", false);
                        }));

                        PlayersInRocket.Add(dbPlayer);
                        break;
                    }
                    break;
                case Key.L:
                    if (!PlayersInRocket.Contains(dbPlayer))
                    {
                        switch (CurrentRocketLocation)
                        {
                            case RocketLocation.Earth:
                                if (dbPlayer.Player.Position.DistanceTo(EarthShuttleEnter) > 2.0f)
                                    return false;
                                break;
                            case RocketLocation.Mars:
                                if (dbPlayer.Player.Position.DistanceTo(MarsShuttleEnter) > 2.0f)
                                    return false;
                                break;
                            default:
                                return false;
                        }
                    }

                    if (dbPlayer.RankId == (int)AdminLevelTypes.SuperAdministrator || dbPlayer.RankId == (int)AdminLevelTypes.Manager || dbPlayer.RankId == (int)AdminLevelTypes.Projektleitung)
                    {
                        if (RocketLocked)
                            dbPlayer.SendNewNotification("Rakete wurde aufgeschlossen!");
                        else
                            dbPlayer.SendNewNotification("Rakete wurde abgeschlossen!");

                        RocketLocked = !RocketLocked;
                        break;
                    }

                    if (dbPlayer.TeamId != (uint)TeamTypes.TEAM_ARMY && dbPlayer.TeamRank < 10)
                        break;

                    if (dbPlayer.TeamRank < 10)
                        break;

                    if (RocketLocked)
                        dbPlayer.SendNewNotification("Rakete wurde aufgeschlossen!");
                    else
                        dbPlayer.SendNewNotification("Rakete wurde abgeschlossen!");

                    RocketLocked = !RocketLocked;
                    break;
                default:
                    break;
            }

            return false;
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (MarsVisitors.Contains(dbPlayer)) MarsVisitors.Remove(dbPlayer);
            if (PlayersInRocket.Contains(dbPlayer)) PlayersInRocket.Remove(dbPlayer);
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if(colShape.HasData("mars"))
            {
                if (colShapeState == ColShapeState.Enter)
                {
                    dbPlayer.Mars = true;

                    dbPlayer.Player.TriggerNewClient("stopScreenEffect", "Rampage", 5000, true);
                    dbPlayer.ResetData("marsGammaScreen");

                    dbPlayer.Player.TriggerNewClient("startScreenEffect", "MP_Celeb_Win", 5000, true);
                    dbPlayer.SetData("marsScreenActive", true);
                }
                else
                {
                    dbPlayer.Mars = false;

                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        await NAPI.Task.WaitForMainThread(1500);

                        if (PlayersInRocket.ToList().Contains(dbPlayer))
                            return;

                        if (dbPlayer.IsOnMars())
                        {
                            dbPlayer.SendNewNotification("Achtung Gammastrahlung zu hoch! Kehre in die Nähe der Basis zurück!");

                            dbPlayer.SetData("marsGammaScreen", true);
                            dbPlayer.Player.TriggerNewClient("startScreenEffect", "Rampage", 5000, true);
                        }
                    }));
                }
                return true;
            }
            return false;
        }

        public override void OnTenSecUpdate()
        {
            foreach(DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if (dbPlayer.IsInjured()) continue;
                if (dbPlayer.IsInAdminDuty()) continue;

                if(dbPlayer.IsOnMars())
                {
                    // In der rakete sollen die keinen Schaden bekommen
                    if (SpaceModule.PlayersInRocket.Contains(dbPlayer))
                        continue;

                    // outside saverange
                    if (!dbPlayer.Mars)
                    {
                        dbPlayer.SetHealth(dbPlayer.Player.Health - 25);
                    }
                    else
                    {
                        // In der Range Save?
                        if (dbPlayer.Player.Position.DistanceTo(SpaceModule.MarsSaveRangeStation) < 25.0f) continue;

                        // kein Spacesuite, Helm aus?
                        if (!dbPlayer.HasData("outfitactive") || !SpaceSuits.Contains((uint)dbPlayer.GetData("outfitactive"))/* || !dbPlayer.Character.ActiveProps.ContainsKey(0) || !dbPlayer.Character.ActiveProps[0]*/) // Müssen testen, obs am Helm liegt
                        {
                            dbPlayer.SetHealth(dbPlayer.Player.Health - 25);
                        }
                    }
                }
            }
        }

        public override void OnMinuteUpdate()
        {
            if (CurrentRocketLocation != RocketLocation.Orbit)
            {
                RefreshRocket();
                return;
            }

            if (LaunchSequenceStart.AddMinutes(MinutesToArrive) <= DateTime.Now)
            {
                foreach (DbPlayer dbPlayer in PlayersInRocket.ToList())
                {
                    switch (DestinationRocketLocation)
                    {
                        case RocketLocation.Earth:
                            dbPlayer.SendNewNotification("Die Rakete ist erfolgreich auf der Erde gelandet!");
                            break;
                        case RocketLocation.Mars:
                            dbPlayer.SendNewNotification("Die Rakete ist erfolgreich auf dem Mars gelandet!");
                            break;
                        default:
                            break;
                    }
                }

                foreach (DbPlayer dbPlayer in TeamModule.Instance[(int)TeamTypes.TEAM_ARMY].Members.Values.ToList())
                {
                    if (dbPlayer == null || !dbPlayer.IsValid())
                        continue;

                    dbPlayer.SendNewNotification($"Rakete erfolgreich gelandet! Funkkontakt wird in Kürze hergestellt!");
                }

                PlayersInRocket.Clear();
                CurrentRocketLocation = DestinationRocketLocation;

                LaunchPermission = false;
                RocketLocked = false;
            }

            RefreshRocket();
        }
    }

    public static class SpacePlayerExtension
    {
        public static bool IsOnMars(this DbPlayer dbPlayer)
        {
            return SpaceModule.Instance.MarsVisitors.Contains(dbPlayer);
        }

        public static bool IsInSpace(this DbPlayer dbPlayer)
        {
            return dbPlayer.Player.Position.Z >= 2500;
        }

        public static void EnterMars(this DbPlayer dbPlayer, bool removeFromRocket = false)
        {
            // Für periodic damage
            SpaceModule.Instance.MarsVisitors.Add(dbPlayer);
            dbPlayer.Mars = true;

            // Screen Effekt
            dbPlayer.Player.TriggerNewClient("startScreenEffect", "MP_Celeb_Win", 5000, true);
            dbPlayer.SetData("marsScreenActive", true);

            // Aus Rakete entfernen
            if (removeFromRocket)
            {
                if (SpaceModule.PlayersInRocket.Contains(dbPlayer))
                    SpaceModule.PlayersInRocket.Remove(dbPlayer);
            }
        }

        public static void LeaveMars(this DbPlayer dbPlayer, bool enterRocket = false)
        {
            // Periodic damage abschalten
            SpaceModule.Instance.MarsVisitors.Remove(dbPlayer);
            dbPlayer.Mars = false;

            // Screen-Effekte entfernen
            dbPlayer.Player.TriggerNewClient("stopScreenEffect", "MP_Celeb_Win");
            dbPlayer.ResetData("marsScreenActive");

            dbPlayer.Player.TriggerNewClient("stopScreenEffect", "Rampage");
            dbPlayer.ResetData("marsGammaScreen");

            //In Rakete eintragen
            if (enterRocket)
            {
                SpaceModule.PlayersInRocket.Add(dbPlayer);
            }
        }
    }
}
