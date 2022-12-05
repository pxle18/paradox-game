using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Robbery
{
    public sealed class VespucciBankRobberyModule : Module<VespucciBankRobberyModule>
    {
        // Global Defines
        public bool IsActive = false;
        public int TimeLeft = 0;
        public int RobberyTime = 60; // max zeit in SB
        public Team RobberTeam = null;
        public string robname = "Vespucci";
        public int CountInBreakTresor = 0;

        public static string SecureSystemIPL = "Bank_Vespucci";

        public DateTime LastVespucciBank = DateTime.Now.AddHours(-2);

        public Vector3 RobPosition = new Vector3(-1308.69, -812.482, 17.1483);

        public override bool Load(bool reload = false)
        {
            IsActive = false;
            TimeLeft = 0;
            RobberyTime = Configurations.Configuration.Instance.DevMode ? 3 : 20;
            return true;
        }
        
        public void LoadContainerBankInv(Container container)
        {
            //Console.WriteLine("### VESPUCCI BANK - LoadContainerBankInv START ###");
            Random rnd = new Random();
            container.ClearInventory();
            container.AddItem(487, rnd.Next(42,57));
            //Console.WriteLine("### VESPUCCI BANK - LoadContainerBankInv END ###");
        }

        public bool CanVespucciBankRobbed()
        {
            //Console.WriteLine("### VESPUCCI BANK - CanVespucciBankRobbed START ###");
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (Configurations.Configuration.Instance.DevMode) return true;

            // Check other Robs
            if(RobberyModule.Instance.Robberies.Where(r => r.Value.Type == RobType.Juwelier && RobberyModule.Instance.IsActive(r.Value.Id)).Count() > 0 || StaatsbankRobberyModule.Instance.IsActive)
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


            //Console.WriteLine("### VESPUCCI BANK - CanVespucciBankRobbed END ###");
            return true;
        }

        public void StartRob(DbPlayer dbPlayer)
        {
            //Console.WriteLine("### VESPUCCI BANK - StartRob START ###");
            
            if (!dbPlayer.IsAGangster() && !dbPlayer.IsBadOrga())
            {
                dbPlayer.SendNewNotification("Große Heists sind nur fuer Gangs/Mafien!");
                return;
            }

            if (Configurations.Configuration.Instance.DevMode != true)
            {
                // Timecheck +- 30 min restarts
                if (!Instance.CanVespucciBankRobbed())
                {
                    dbPlayer.SendNewNotification("Es scheint als ob die Generatoren nicht bereit sind, das geht nicht. (mind 30 min vor und nach Restarts!)");
                    return;
                }
            }

            if (Instance.IsActive || RobberyModule.Instance.LastScenario.AddHours(2) > DateTime.Now || (LastVespucciBank.AddHours(2) > DateTime.Now && !Configurations.Configuration.Instance.DevMode))
            {
                dbPlayer.SendNewNotification("Die Vespucci Bank wurde bereits ausgeraubt oder ist derzeit nicht verfügbar!");
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


            dbPlayer.SendNewNotification("Sie versuchen nun den Tresor zu knacken!");

            // Set start datas
            TimeLeft = RobberyTime;
            IsActive = true;
            RobberTeam = dbPlayer.Team;

            // Messages
            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, ein Einbruch in der Vespucci Bank wurde gemeldet!");
            TeamModule.Instance.SendMessageToTeam("Sie beginnen einen Ueberfall auf die Vespucci Bank!", (teams)RobberTeam.Id);

            RobberyModule.Instance.LastScenario = DateTime.Now;
            RobberyModule.Instance.SetTeamScenario(this.robname, dbPlayer.Team.Id);
            NAPI.Task.Run(() =>
                {
                    NAPI.World.RequestIpl(SecureSystemIPL);
                });

            //Console.WriteLine("### VESPUCCI BANK - StartRob END ###");
            LastVespucciBank = DateTime.Now;
        }

        public void CloseRob()
        {
            //Console.WriteLine("### VESPUCCI BANK - CloseRob START ###");
            StaticContainer StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.VESPUCCIBANK1);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.VESPUCCIBANK2);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.VESPUCCIBANK3);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.VESPUCCIBANK4);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.VESPUCCIBANK5);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            this.IsActive = false;
            this.RobberTeam = null;
            this.TimeLeft = RobberyTime;
            //Console.WriteLine("### VESPUCCI BANK - CloseRob END ###");
        }

        public void CancelRob()
        {
            //Console.WriteLine("### VESPUCCI BANK - CancelRob START ###");
            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, der Einbruch auf die Vespucci Bank wurde erfolgreich verhindert!");
            TeamModule.Instance.SendMessageToTeam("Der Überfall ist gescheitert!", (teams)RobberTeam.Id);

            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;

            NAPI.Task.Run(() =>
            {
                NAPI.World.RemoveIpl(SecureSystemIPL);
            });
            
            //Console.WriteLine("### VESPUCCI BANK - CancelRob END ###");
        }

        public override void OnMinuteUpdate()
        {
            //Console.WriteLine("### VESPUCCI BANK - OnMinuteUpdate START ###");
            if(IsActive)
            {
                // Check if Teamplayer is in Reange
                if(RobberTeam == null || RobberTeam.GetTeamMembers().Where(p => p != null && p.IsValid() && !p.isInjured() && p.Player.Position.DistanceTo(RobPosition) < 15.0f).Count() <= 0)
                {
                    //Console.WriteLine("### VESPUCCI BANK - OnMinuteUpdate ABORT 1 ###");
                    CancelRob();
                    return;
                }

                if(TimeLeft == 45) // nach 15 min weil 60 XX
                {
                    // Get Players For Respect
                    int playersAtRob = RobberTeam.GetTeamMembers().Where(m => m.Player.Position.DistanceTo(RobPosition) < 300f).Count();
                    RobberTeam.TeamMetaData.AddRespect(playersAtRob * 100);
                    TeamModule.Instance.SendMessageToTeam("Durch den Überfall erhält ihr Team Ansehen! (" + playersAtRob * 100 + "P)", (teams)RobberTeam.Id);
                    
                }
                if(TimeLeft == 60)
                {
                    CloseRob();
                }

                Logger.Debug($"Vespuccy Bank Rob timeleft {TimeLeft}");
                TimeLeft--;
            }
            
            //Console.WriteLine("### VESPUCCI BANK - OnMinuteUpdate END ###");
        }
    }
}