using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Space.Menu
{
    public class RocketControlMenu : MenuBuilder
    {
        public RocketControlMenu() : base(PlayerMenu.SpaceRocketControlMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Module.Menu.Menu(Menu, "Rocket-Control", "");

            menu.Add($"Schließen");
            menu.Add($"Raketenstart freigeben");
            menu.Add($"Startsequenz initialisieren");

            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                switch (index)
                {
                    case 0:
                        MenuManager.DismissCurrent(dbPlayer);
                        return false;
                    case 1:
                        if (SpaceModule.LaunchPermission)
                        {
                            dbPlayer.SendNewNotification("Es wurde bereits eine Freigabe erteilt - diese kann nicht zurückgenommen werden!");
                            break;
                        }

                        foreach (DbPlayer iPlayer in TeamModule.Instance[(int)teams.TEAM_ARMY].Members.Values.ToList())
                        {
                            if (iPlayer == null || !iPlayer.IsValid())
                                continue;

                            iPlayer.SendNewNotification($"{dbPlayer.GetName()} hat den Start der Rakete freigegeben! Türen sind verriegelt!");
                            SpaceModule.LaunchPermission = true;
                            SpaceModule.RocketLocked = true;
                        }
                        break;
                    case 2:
                        if (!SpaceModule.LaunchPermission)
                        {
                            dbPlayer.SendNewNotification("Es existiert keine Starterlaubnis eines Major Generals oder höher!");
                            break;
                        }

                        foreach (DbPlayer iPlayer in TeamModule.Instance[(int)teams.TEAM_ARMY].Members.Values.ToList())
                        {
                            if (iPlayer == null || !iPlayer.IsValid())
                                continue;

                            iPlayer.SendNewNotification($"{dbPlayer.GetName()} hat die Startsequenz der Rakete initialisiert! Start der Rakete in 10 Sekunden.");
                        }

                        SpaceModule.LaunchSequenceStart = DateTime.Now;

                        if (SpaceModule.CurrentRocketLocation == RocketLocation.Earth)
                        {
                            SpaceModule.DestinationRocketLocation = RocketLocation.Mars;

                            NAPI.Task.Run(() =>
                            {
                                var surroundingUsers = NAPI.Player.GetPlayersInRadiusOfPosition(100.0f, SpaceModule.EarthShuttleEnter);

                                foreach (var user in surroundingUsers)
                                {
                                    if (user.Dimension == dbPlayer.Player.Dimension)
                                    {
                                        var targetPlayer = user.GetPlayer();
                                        if (targetPlayer == null || !targetPlayer.IsValid()) continue;

                                        targetPlayer.SendNewNotification($"1337Allahuakbar$rocketstart", duration: 17000);
                                    }
                                }
                            });
                        }
                        else if (SpaceModule.CurrentRocketLocation == RocketLocation.Mars)
                        {
                            SpaceModule.DestinationRocketLocation = RocketLocation.Earth;

                            NAPI.Task.Run(() =>
                            {
                                var surroundingUsers = NAPI.Player.GetPlayersInRadiusOfPosition(100.0f, SpaceModule.MarsShuttleEnter);

                                foreach (var user in surroundingUsers)
                                {
                                    if (user.Position.Z >= 2500) // Mars Höhe
                                    {
                                        var targetPlayer = user.GetPlayer();
                                        if (targetPlayer == null || !targetPlayer.IsValid()) continue;

                                        targetPlayer.SendNewNotification($"1337Allahuakbar$rocketstart", duration: 17000);
                                    }
                                }
                            });
                        }

                        Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(async () =>
                        {
                            await NAPI.Task.WaitForMainThread(10000);
                            foreach (DbPlayer iPlayer in TeamModule.Instance[(int)teams.TEAM_ARMY].Members.Values.ToList())
                            {
                                if (iPlayer == null || !iPlayer.IsValid())
                                    continue;

                                iPlayer.SendNewNotification($"Rakete gestartet! ETA zur Ankunft - 15 Minuten. Funkkontakt währenddessen unterbrochen!");
                            }

                            SpaceModule.CurrentRocketLocation = RocketLocation.Orbit;
                            SpaceModule.Instance.RefreshRocket();

                            foreach (DbPlayer iPlayer in SpaceModule.PlayersInRocket.ToList())
                            {
                                if (PhoneCall.IsPlayerInCall(iPlayer.Player))
                                {
                                    iPlayer.CancelPhoneCall();
                                }

                                Voice.VoiceModule.Instance.turnOffFunk(iPlayer);

                                iPlayer.Player.SetSharedData("voiceRange", (int)VoiceRange.whisper);
                                iPlayer.SetData("voiceType", 3);
                                iPlayer.Player.TriggerNewClient("setVoiceType", 3);
                            }

                        }));

                        break;
                    default:
                        break;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return false;
            }
        }
    }
}
