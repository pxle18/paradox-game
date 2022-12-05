using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Robbery;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Heist.Planning.Menu
{
    public class PlanningroomHeistInfoMenu : MenuBuilder
    {
        public PlanningroomHeistInfoMenu() : base(PlayerMenu.PlanningroomInfoTableMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Module.Menu.Menu(Menu, "Heists");

            menu.Add($"Schließen");
            menu.Add($"Human Labs");
            menu.Add($"Staatsbank");
            menu.Add($"Vespucci Bank");
            menu.Add($"Life Invader");

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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    bool canStarted = true;

                    if(!Robbery.WeaponFactoryRobberyModule.Instance.CanWeaponFactoryRob())
                    {
                        canStarted = false;
                    }
                    if ((TeamModule.Instance.Get((int)teams.TEAM_ARMY).GetTeamMembers().Where(ip => ip != null && ip.IsValid() && ip.Duty).Count() < 30) && !Configurations.Configuration.Instance.DevMode)
                    {
                        //dbPlayer.SendNewNotification("Es muessen mindestens 30 Soldaten im Dienst sein!");
                        canStarted = false;
                    }

                    var vtc = RobberyModule.Instance.ValidTeamScenario(WeaponFactoryRobberyModule.Instance.robname, dbPlayer.Team.Id);
                    if (!vtc.check)
                    {
                        //dbPlayer.SendNewNotification($"Sie sind noch auf der Fahndungsliste, nächste Möglichkeit am {vtc.lastrob}");
                        canStarted = false;
                    }

                    if (WeaponFactoryRobberyModule.Instance.IsActive || WeaponFactoryRobberyModule.Instance.HasWFRobbed)
                    {
                        canStarted = false;
                    }

                    if (canStarted) 
                    {
                        dbPlayer.SendNewNotification($"Der Lagerbestand im Human Labs konnte nicht bestätigt werden.");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification($"Der Lagerbestand im Human Labs wurde bestätigt, es sind Waffenkisten vorhanden!");
                    }

                    return true;
                }
                else if (index == 2)
                {
                    bool canStarted = true;

                    if (StaatsbankRobberyModule.Instance.IsActive || RobberyModule.Instance.LastScenario.AddHours(2) > DateTime.Now || (StaatsbankRobberyModule.Instance.LastStaatsbank.AddHours(2) > DateTime.Now && !Configurations.Configuration.Instance.DevMode))
                    {
                        //dbPlayer.SendNewNotification("Die Staatsbank wurde bereits ausgeraubt oder ist derzeit nicht verfügbar!");
                        canStarted = false;
                    }

                    if (TeamModule.Instance.DutyCops < 20 && !Configurations.Configuration.Instance.DevMode)
                    {
                        //dbPlayer.SendNewNotification("Es muessen mindestens 20 Beamte im Dienst sein!");
                        canStarted = false;
                    }

                    var vtc = RobberyModule.Instance.ValidTeamScenario(StaatsbankRobberyModule.Instance.robname, dbPlayer.Team.Id);
                    if (!vtc.check)
                    {
                        //dbPlayer.SendNewNotification($"Sie sind noch auf der Fahndungsliste, nächste Möglichkeit am {vtc.lastrob}");
                        canStarted = false;
                    }

                    if (canStarted)
                    {
                        dbPlayer.SendNewNotification($"Die Goldreserven der Staatsbank konnten nicht bestätigt werden.");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification($"Die Goldreserven der Staatsbank wurde bestätigt!");
                    }

                    return true;
                }
                else if (index == 3)
                {
                    bool canStarted = true;

                    if (VespucciBankRobberyModule.Instance.IsActive || RobberyModule.Instance.LastScenario.AddHours(2) > DateTime.Now || (VespucciBankRobberyModule.Instance.LastVespucciBank.AddHours(2) > DateTime.Now && !Configurations.Configuration.Instance.DevMode))
                    {
                        //dbPlayer.SendNewNotification("Die Vespucci Bank wurde bereits ausgeraubt oder ist derzeit nicht verfügbar!");
                        canStarted = false;
                    }

                    if (TeamModule.Instance.DutyCops < 20 && !Configurations.Configuration.Instance.DevMode)
                    {
                        //dbPlayer.SendNewNotification("Es muessen mindestens 20 Beamte im Dienst sein!");
                        canStarted = false;
                    }

                    var vtc = RobberyModule.Instance.ValidTeamScenario(VespucciBankRobberyModule.Instance.robname, dbPlayer.Team.Id);
                    if (!vtc.check)
                    {
                        //dbPlayer.SendNewNotification($"Sie sind noch auf der Fahndungsliste, nächste Möglichkeit am {vtc.lastrob}");
                        canStarted = false;
                    }

                    if (canStarted)
                    {
                        dbPlayer.SendNewNotification($"Die Goldreserven der Vespucci Bank konnten nicht bestätigt werden.");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification($"Die Goldreserven der Vespucci Bank wurde bestätigt!");
                    }

                    return true;
                }
                else if (index == 4)
                {
                    bool canStarted = true;

                    if (LifeInvaderRobberyModule.Instance.IsActive || RobberyModule.Instance.LastScenario.AddHours(2) > DateTime.Now || (LifeInvaderRobberyModule.Instance.LastVespucciBank.AddHours(2) > DateTime.Now && !Configurations.Configuration.Instance.DevMode))
                    {
                        //dbPlayer.SendNewNotification("Er Lifeinvader wurde bereits ausgeraubt oder ist derzeit nicht verfügbar!");
                        canStarted = false;
                    }

                    if (TeamModule.Instance.DutyCops < 20 && !Configurations.Configuration.Instance.DevMode)
                    {
                        //dbPlayer.SendNewNotification("Es muessen mindestens 20 Beamte im Dienst sein!");
                        canStarted = false;
                    }

                    var vtc = RobberyModule.Instance.ValidTeamScenario(LifeInvaderRobberyModule.Instance.robname, dbPlayer.Team.Id);
                    if (!vtc.check)
                    {
                        //dbPlayer.SendNewNotification($"Sie sind noch auf der Fahndungsliste, nächste Möglichkeit am {vtc.lastrob}");
                        canStarted = false;
                    }

                    if (canStarted)
                    {
                        dbPlayer.SendNewNotification($"Der Computer im Lifeinvader gab keine Rückmeldung...");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification($"Der Ping des Lifeinvaders wurde bestätigt und scheint online zu sein!");
                    }

                    return true;
                }

                return true;
            }
        }
    }
}
