using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Robbery
{
    public sealed class LifeInvaderRobberyModule : Module<LifeInvaderRobberyModule>
    {
        // Global Defines
        public bool IsActive = false;
        public int TimeLeft = 0;
        public int RobberyTime = 60; // max zeit in SB
        public Team RobberTeam = null;
        public string robname = "Liveinvader";
        public bool IsHacked = false;

        public static string SecureSystemIPL = "";

        public DateTime LastVespucciBank = DateTime.Now.AddHours(-2);

        public Vector3 RobPosition = new Vector3(-1082.66, -245.444, 37.7633);

        public override bool Load(bool reload = false)
        {
            IsActive = false;
            TimeLeft = 0;
            RobberyTime = Configurations.Configuration.Instance.DevMode ? 3 : 20;
            return true;
        }
        
        public void LoadContainerLifeInvader(Container container)
        {
            container.ClearInventory();
            container.AddItem(1101, 1);
        }

        public bool CanLifeinvaderRobbed()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (Configurations.Configuration.Instance.DevMode) return true;

            // Check other Robs
            if(RobberyModule.Instance.Robberies.Where(r => r.Value.Type == RobType.Juwelier && RobberyModule.Instance.IsActive(r.Value.Id)).Count() > 0 || StaatsbankRobberyModule.Instance.IsActive || VespucciBankRobberyModule.Instance.IsActive)
            {
                return false;
            }

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 10)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 15)
                    {
                        return false;
                    }

                    break;
            }


            return true;
        }

        public async Task StartRob(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsAGangster() && !dbPlayer.IsBadOrga())
            {
                dbPlayer.SendNewNotification("Große Heists sind nur fuer Gangs/Mafien!");
                return;
            }

            if (Configurations.Configuration.Instance.DevMode != true)
            {
                // Timecheck +- 30 min restarts
                if (!Instance.CanLifeinvaderRobbed())
                {
                    dbPlayer.SendNewNotification("Es scheint als ob die Generatoren nicht bereit sind, das geht nicht. (mind 30 min vor und nach Restarts!)");
                    return;
                }
            }

            if (Instance.IsActive || RobberyModule.Instance.LastScenario.AddHours(2) > DateTime.Now || (LastVespucciBank.AddHours(2) > DateTime.Now && !Configurations.Configuration.Instance.DevMode))
            {
                dbPlayer.SendNewNotification("Der Lifeinvader wurde bereits ausgeraubt oder ist derzeit nicht verfügbar!");
                return;
            }

            if (TeamModule.Instance.DutyCops < 20 && !Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification("Es muessen mindestens 20 Beamte im Dienst sein!");
                return;
            }

            var vtc = RobberyModule.Instance.ValidTeamScenario(this.robname, dbPlayer.Team.Id);
            if (!vtc.check)
            {
                dbPlayer.SendNewNotification($"Sie sind noch auf der Fahndungsliste, nächste Möglichkeit am {vtc.lastrob}");
                return;
            }

            // Set start datas
            TimeLeft = RobberyTime;
            IsActive = true;
            RobberTeam = dbPlayer.Team;
            IsHacked = false;

            // Messages
            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, ein Einbruch im Lifeinvader wurde gemeldet!");
            TeamModule.Instance.SendMessageToTeam("Sie beginnen einen Ueberfall auf den Lifeinvader!", (TeamTypes)RobberTeam.Id);

            LastVespucciBank = DateTime.Now;

            RobberyModule.Instance.LastScenario = DateTime.Now;
            RobberyModule.Instance.SetTeamScenario(this.robname, dbPlayer.Team.Id);
                
            int time = 300000;
            if (Configuration.Instance.DevMode) time = 30000;
            // Aufschließen lul
            dbPlayer.SendNewNotification("Sie beginnen nun damit die Sicherheitssysteme auszuschalten!");

            Chats.sendProgressBar(dbPlayer, time);

            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@prison_heistig1_P1_guard_checks_bus", "loop");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetCannotInteract(true);

            await NAPI.Task.WaitForMainThread(time);

            dbPlayer.SetCannotInteract(false);
            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured())
            {
                CancelRob();
                return;
            }
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();

            dbPlayer.SendNewNotification("Serverschrank aufgeschweißt!", notificationType: PlayerNotification.NotificationType.SUCCESS);

            IsHacked = true;
        }

        public void CloseRob()
        {
            StaticContainer StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.LIFEINVADERROB);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;


            this.IsActive = false;
            this.RobberTeam = null;
            this.TimeLeft = RobberyTime;
        }

        public void CancelRob()
        {
            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, der Einbruch auf den Lifeinvader wurde erfolgreich verhindert!");
            TeamModule.Instance.SendMessageToTeam("Der Überfall ist gescheitert!", (TeamTypes)RobberTeam.Id);

            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;
        }

        public override void OnMinuteUpdate()
        {
            if(IsActive)
            {
                // Check if Teamplayer is in Reange
                if(RobberTeam == null || RobberTeam.GetTeamMembers().Where(p => p != null && p.IsValid() && !p.IsInjured() && p.Player.Position.DistanceTo(RobPosition) < 50.0f).Count() <= 0)
                {
                    CancelRob();
                    return;
                }

                if(TimeLeft == 45) // nach 15 min weil 60 XX
                {
                    int playersAtRob = RobberTeam.GetTeamMembers().Where(m => m.Player.Position.DistanceTo(RobPosition) < 300f).Count();
                    RobberTeam.TeamMetaData.AddRespect(playersAtRob * 100);

                    TeamModule.Instance.SendMessageToTeam("Durch den Überfall erhält ihr Team Ansehen! (" + playersAtRob * 100 + "P)", (TeamTypes)RobberTeam.Id);
                }
                if(TimeLeft == 60)
                {
                    CloseRob();
                }

                Logger.Debug($"Lifeinvader timeleft {TimeLeft}");
                TimeLeft--;
            }
        }
    }
}