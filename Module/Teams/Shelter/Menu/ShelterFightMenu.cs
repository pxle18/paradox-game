using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teamfight;
using VMP_CNR.Module.Teamfight.Windows;

namespace VMP_CNR.Module.Teams.Shelter
{
    public class ShelterFightMenuBuilder : MenuBuilder
    {
        public ShelterFightMenuBuilder() : base(PlayerMenu.ShelterFightMenu)
        {
        }

        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.NativeMenu(Menu, "Fraktionskampf", "Fraktionskampf Verwaltung");

            menu.Add($"Schließen");
            menu.Add($"Kampfantrag stellen");
            menu.Add($"Kampfantrag beantworten");
            menu.Add($"Equipment erhalten");
            menu.Add($"Schutzweste kaufen (8000$)");
            menu.Add($"Verbandskästen kaufen (500$)");
            menu.Add($"Punktestand abfragen");
            menu.Add($"Aufgeben");

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
                // Get shelter pos
                var teamShelter = TeamShelterModule.Instance.GetByTeam(dbPlayer.TeamId);

                if (dbPlayer.Player.Position.DistanceTo(teamShelter.MenuPosition) > 3.0f)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }

                // Close menu
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                // Start fight
                else if (index == 1)
                {
                    dbPlayer.SendNewNotification("Aktuell Deaktiviert");
                    return false;
                    /*
                    if (dbPlayer.TeamRankPermission.Manage < 1) return false;

                    MenuManager.DismissCurrent(dbPlayer);
                    ComponentManager.Get<TeamfightWindow>().Show()(dbPlayer, dbPlayer.TeamId, dbPlayer.Team.Name, true);
                    return false;*/
                }
                // Answer fight
                else if (index == 2)
                {
                    if (dbPlayer.TeamRankPermission.Manage < 1) return false;

                    Teamfight.Teamfight requestedFight = dbPlayer.Team.RequestedTeamfight;

                    if (requestedFight == null)
                    {
                        dbPlayer.SendNewNotification("Deine Fraktion hat aktuell keine Anfrage zu einem Fraktionskampf offen!");
                        return false;
                    }

                    ComponentManager.Get<TeamfightRequestWindow>().Show()(dbPlayer, dbPlayer.TeamId, requestedFight.Team_a_money, requestedFight.Team_a, TeamModule.Instance.Get(requestedFight.Team_a).Name, true);

                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                // equipment
                else if (index == 3)
                {
                    if (dbPlayer.Team.IsInTeamfight())
                    {
                        dbPlayer.GiveWeapon(WeaponHash.Heavypistol, 500, true, true);
                        dbPlayer.GiveWeapon(WeaponHash.Assaultrifle, 600, true, true);
                        dbPlayer.GiveWeapon(WeaponHash.Compactrifle, 600, true, true);
                        dbPlayer.GiveWeapon(WeaponHash.Gusenberg, 600, true, true);
                        dbPlayer.SetArmor(100);
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
                // schutzweste
                else if (index == 4)
                {
                    if (dbPlayer.Team.IsInTeamfight())
                    {
                        if (!dbPlayer.Container.CanInventoryItemAdded(655, 1))
                        {
                            dbPlayer.SendNewNotification($"Sie können das nicht mehr tragen, Ihr Inventar ist voll!");
                            return false;
                        }

                        if (!dbPlayer.TakeBankMoney(8000))
                        {
                            dbPlayer.SendNewNotification($"Dieses Item kostet 8000$ (Bank)!");
                            return false;
                        }

                        dbPlayer.Container.AddItem(655, 1);
                        dbPlayer.SendNewNotification($"Sie haben sich eine Schutzweste gekauft!");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
                // Verbandskasten
                else if (index == 5)
                {
                    if (dbPlayer.Team.IsInTeamfight())
                    {
                        if (!dbPlayer.Container.CanInventoryItemAdded(654, 1))
                        {
                            dbPlayer.SendNewNotification($"Sie können das nicht mehr tragen, Ihr Inventar ist voll!");
                            return false;
                        }

                        if (!dbPlayer.TakeBankMoney(500))
                        {
                            dbPlayer.SendNewNotification($"Dieses Item kostet 500$ (Bank)!");
                            return false;
                        }

                        dbPlayer.Container.AddItem(654, 1);
                        dbPlayer.SendNewNotification($"Sie haben sich ein Verbandskasten gekauft!");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
                // Punktestand abfragen
                else if (index == 6)
                {
                    if (dbPlayer.Team.IsInTeamfight())
                    {
                        var teamFight = TeamfightModule.Instance.getOwnTeamFight(dbPlayer.TeamId);
                        dbPlayer.SendNewNotification($"{TeamModule.Instance.GetById((int)teamFight.Team_a).Name} {teamFight.Kills_team_a} : {TeamModule.Instance.GetById((int)teamFight.Team_b).Name} {teamFight.Kills_team_b}", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
                // Aufgeben
                else
                {
                    if (dbPlayer.TeamRankPermission.Manage < 1) return false;

                    if (dbPlayer.Team.IsInTeamfight())
                    {
                        var teamFight = TeamfightModule.Instance.getOwnTeamFight(dbPlayer.TeamId);
                        TeamfightFunctions.surrenderTeamfight(teamFight, dbPlayer.Team);
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
            }
        }
    }
}
