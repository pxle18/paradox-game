using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.AnimationMenu;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Shops;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Weapons;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR.Module.Robbery
{
    public sealed class WeaponFactoryRobberyModule : Module<WeaponFactoryRobberyModule>
    {
        // Global Defines
        public bool HasWFRobbed = false;
        public bool IsActive = false;
        public int TimeLeft = 0;
        public int RobberyTime = 30;
        public Team RobberTeam = null;
        public string robname = "Humanlabs";
        public Vector3 RobPosition = new Vector3(3606.72f, 3720.58, 29.6894);

        public override bool Load(bool reload = false)
        {
            HasWFRobbed = false;
            IsActive = false;
            TimeLeft = 0;
            RobberyTime = Configurations.Configuration.Instance.DevMode ? 3 : RobberyTime;
            return true;
        }

        public void LoadWeaponFactoryContainer()
        {
            StaticContainer StaticContainer = StaticContainerModule.Instance.Get((int)StaticContainerTypes.WEAPONFACTORY);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Container.AddItem(303, 96);
            StaticContainer.Locked = false;
            
            TeamModule.Instance.SendMessageToTeam("Die Waffenfabrik ist nun offen!", (TeamTypes)RobberTeam.Id);

            IsActive = false;
            TimeLeft = RobberyTime;
            RobberTeam = null;
        }

        public bool CanWeaponFactoryRob()
        {
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 30)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 30)
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }

        public void StartRob(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsAGangster() && !dbPlayer.IsBadOrga())
            {
                dbPlayer.SendNewNotification( "Große Heists sind nur fuer Gangs/Mafien!");
                return;
            }

            if (Configurations.Configuration.Instance.DevMode != true)
            {
                // Timecheck +- 30 min restarts
                if (!Instance.CanWeaponFactoryRob())
                {
                    dbPlayer.SendNewNotification(
                        "Es scheint als ob die Generatoren nicht bereit sind, das geht nicht. (mind 30 min vor und nach Restarts!)");
                    return;
                }
            }

            if ((TeamModule.Instance.Get((int)TeamTypes.TEAM_ARMY).GetTeamMembers().Where(ip => ip != null && ip.IsValid() && ip.Duty).Count() < 30) && !Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification("Es muessen mindestens 30 Soldaten im Dienst sein!");
                return;
            }

            var vtc = RobberyModule.Instance.ValidTeamScenario(this.robname, dbPlayer.Team.Id);
            if (!vtc.check)
            {
                dbPlayer.SendNewNotification($"Sie sind noch auf der Fahndungsliste, nächste Möglichkeit am {vtc.lastrob}");
                return;
            }

            if (Instance.IsActive ||
                Instance.HasWFRobbed)
            {
                dbPlayer.SendNewNotification("Die Waffenfabrik wurde bereits ausgeraubt!");
                return;
            }


            dbPlayer.SendNewNotification( "Sie versuchen nun den Tresor zu knacken!");

            // Set start datas
            TimeLeft = RobberyTime;
            IsActive = true;
            HasWFRobbed = true;
            RobberTeam = dbPlayer.Team;

            RobberyModule.Instance.SetTeamScenario(this.robname, dbPlayer.Team.Id);
            // Get Players For Respect
            int playersAtRob = RobberTeam.GetTeamMembers().Where(m => m.Player.Position.DistanceTo(dbPlayer.Player.Position) < 300f).Count();
            dbPlayer.Team.TeamMetaData.AddRespect(playersAtRob * 80);

            // Messages
            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, ein Einbruch in der Waffenfabrik wurde gemeldet!");
            TeamModule.Instance.SendMessageToTeam("Sie beginnen einen Ueberfall auf die Waffenfabrik!", (TeamTypes)RobberTeam.Id);
            TeamModule.Instance.SendMessageToTeam("Durch den Überfall erhält ihr Team Ansehen! (" + playersAtRob * 80 + "P)", (TeamTypes)RobberTeam.Id);
        }

        public void CancelRob()
        {
            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, der Einbruch auf die Waffenfabrik wurde erfolgreich verhindert!");
            TeamModule.Instance.SendMessageToTeam("Der Ueberfall ist gescheitert!", (TeamTypes)RobberTeam.Id);

            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;
        }
        
        public override void OnMinuteUpdate()
        {
            if(IsActive)
            {
                // Check if Teamplayer is in Reange
                if (RobberTeam == null || RobberTeam.GetTeamMembers().Where(p => p != null && p.IsValid() && !p.IsInjured() && p.Player.Position.DistanceTo(RobPosition) < 20.0f).Count() <= 0)
                {
                    CancelRob();
                    return;
                }

                if (TimeLeft == 1)
                {
                    LoadWeaponFactoryContainer();
                }
                else
                {
                    TimeLeft = TimeLeft - 1;
                }
                Logger.Debug($"Staatsraub timeleft {TimeLeft}");
            }
        }
    }
}