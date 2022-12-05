using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Government.Menu
{
    public class GovComputerMenuBuilder : MenuBuilder
    {
        public GovComputerMenuBuilder() : base(PlayerMenu.GOVComputerMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "GOV Computermenü");
            l_Menu.Add($"Schließen");
            l_Menu.Add($"Scheidung bearbeiten");
            l_Menu.Add($"Namensänderung");
            l_Menu.Add($"Gewerbe Registrieren");
            return l_Menu;
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
                    return true;
                }
                else if (index == 1)
                {
                    if(dbPlayer.TeamId != (uint)teams.TEAM_GOV || (dbPlayer.Player.Position.DistanceTo(GovernmentModule.ComputerBuero1Pos) > 5.0 && dbPlayer.Player.Position.DistanceTo(GovernmentModule.ComputerBuero2Pos) > 5.0))
                    {
                        return true;
                    }

                    MenuManager.DismissCurrent(dbPlayer);
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Scheidung bearbeiten", Callback = "DivorceGovConfirm", Message = "Bitte gib den Namen der Person ein die du Scheiden möchtest. ((Kosten belaufen sich auf (VisumstufePartner1 + VisumstufePartner2) * 40.000$) /2" });
                    return false;
                }
                else if (index == 2)
                { 
                    if (dbPlayer.TeamId != (uint)teams.TEAM_GOV || (dbPlayer.Player.Position.DistanceTo(GovernmentModule.ComputerBuero1Pos) > 5.0 && dbPlayer.Player.Position.DistanceTo(GovernmentModule.ComputerBuero2Pos) > 5.0))
                    {
                        return true;
                    }

                    MenuManager.DismissCurrent(dbPlayer);
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Namensänderung", Callback = "NamechangeGovConfirm", Message = "Bitte gib den Namen der Person welche eine Namensänderung beantragt:" });
                    return false;
                }
                else if (index == 3)
                {
                    if (dbPlayer.TeamId != (uint)teams.TEAM_GOV || (dbPlayer.Player.Position.DistanceTo(GovernmentModule.ComputerBuero1Pos) > 5.0 && dbPlayer.Player.Position.DistanceTo(GovernmentModule.ComputerBuero2Pos) > 5.0))
                    {
                        return true;
                    }

                    MenuManager.DismissCurrent(dbPlayer);
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Gewerbe Registrierung", Callback = "RegisterGovBusiness", Message = "Bitte gib den Namen des Gewerbes ein:" });
                    return false;
                }
                return true;
            }
        }
    }

    public class GovComputerEvents : Script
    {
        [RemoteEvent]
        public void RegisterGovBusiness(Player p_Player, string bizInsertName, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (dbPlayer.TeamId != (uint)teams.TEAM_GOV || dbPlayer.TeamRank < 6) return;

            Business.Business biz = BusinessModule.Instance.GetAll().Values.ToList().Where(b => b.Name.ToLower().Trim() == bizInsertName.ToLower().Trim()).FirstOrDefault();

            if (biz == null)
            {
                dbPlayer.SendNewNotification("Dieses Business wurde nicht gefunden.");
                return;
            }
            if(biz.GovRegisterState)
            {
                dbPlayer.SendNewNotification("Dieses Business ist bereits registriert!");
                return;
            }

            bool ManagerHere = false;
            foreach (Business.Business.Member bizMember in biz.Members.Values.ToList())
            {
                if(bizMember.Manage)
                {
                    DbPlayer xPlayer = Players.Players.Instance.FindPlayerById(bizMember.PlayerId);
                    if (xPlayer == null || !xPlayer.IsValid()) continue;
                    ManagerHere = true;
                    break;
                }
            }

            if(!ManagerHere)
            {
                dbPlayer.SendNewNotification("Ein Manager des Business muss vorort sein!");
                return;
            }
            dbPlayer.SendNewNotification($"Sie haben das Business {biz.Name} erfolgreich registriert!");
            biz.GovRegister();
        }

        [RemoteEvent]
        public void NamechangeGovConfirm(Player p_Player, string nameChangePlayer, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (dbPlayer.TeamId != (uint)teams.TEAM_GOV || dbPlayer.TeamRank < 2) return;

            DbPlayer playerToNameChange = Players.Players.Instance.FindPlayer(nameChangePlayer);
            if (playerToNameChange == null || !playerToNameChange.IsValid()) return;
            if (playerToNameChange.Player.Position.DistanceTo(dbPlayer.Player.Position) > 10) return;

            int kosten = playerToNameChange.Level * 50000;
            if (playerToNameChange.Container.GetItemAmount(670) >= 1)
            {
                kosten = playerToNameChange.Level * 10000;
            }

            dbPlayer.SetData("playerToChangeNameGov", playerToNameChange.Id);

            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Namensänderung", Callback = "DoNamechangeGov", Message = $"Kosten ${kosten} | Namensänderung von {playerToNameChange.RageExtension.Name}. Gib den neuen Namen an (Vorname_Nachname):" });

        }

        [RemoteEvent]
        public static void DoNamechangeGov(Player p_Player, string newName, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            if (!dbPlayer.HasData("playerToChangeNameGov")) return;

            if (dbPlayer.TeamId != (uint)teams.TEAM_GOV || dbPlayer.TeamRank < 2) return;

            DbPlayer playerToNameChange = Players.Players.Instance.FindPlayerById(dbPlayer.GetData("playerToChangeNameGov"));
            if (playerToNameChange == null || !playerToNameChange.IsValid()) return;
            if (playerToNameChange.Player.Position.DistanceTo(dbPlayer.Player.Position) > 10) return;

            var split = newName.Split("_");
            if (split[0].Length < 3 || split[1].Length < 3)
            {
                dbPlayer.SendNewNotification("Der Vor & Nachname muss jeweils mindestens 3 Buchstaben beinhalten!");
                return;
            }

            int count = newName.Count(f => f == '_');
            if (!Regex.IsMatch(newName, @"^[a-zA-Z_-]+$") || count != 1)
            {
                dbPlayer.SendNewNotification("Bitte gib einen Namen in dem Format Max_Mustermann an.");
                return;
            }

            if (newName.Length > 40 || newName.Length < 7)
            {
                dbPlayer.SendNewNotification("Der Name ist zu lang oder zu kurz!");
                return;
            }
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT name FROM player WHERE LOWER(name) = '{newName.ToLower()}' LIMIT 1";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            dbPlayer.SendNewNotification("Dieser Name ist bereits vergeben.");
                            conn.Close();
                            return;
                        }
                    }
                    conn.Close();
                }
            }


            int kosten = playerToNameChange.Container.GetItemAmount(670) > 0 ? playerToNameChange.Level * 10000 : playerToNameChange.Level * 50000;
 
            if (!playerToNameChange.TakeBankMoney(kosten, $"Namensänderung - {newName}"))
            {
                dbPlayer.SendNewNotification($"Die Namensänderung würde {kosten} $ kosten. Diese Summe hat die Person nicht auf dem Konto");
                return;
            }

            if (playerToNameChange.ownHouse[0] != 0)
            {
                House house = HouseModule.Instance.GetByOwner(playerToNameChange.Id);
                house.OwnerName = newName;
                house.SaveOwner();
            }

            // Remove Marriage Item
            if (playerToNameChange.Container.GetItemAmount(670) > 0) playerToNameChange.Container.RemoveItem(670);

            Logger.AddNameChangeLog(playerToNameChange.Id, playerToNameChange.Level, playerToNameChange.GetName(), newName, playerToNameChange.Container.GetItemAmount(670) > 0);
            Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                playerToNameChange.GetName() + $"({playerToNameChange.Id}) hat den Namen zu {newName} geändert | Beamter: {dbPlayer.GetName()}");
            MySQLHandler.ExecuteAsync($"UPDATE player SET name = '{newName}' WHERE id = '{playerToNameChange.Id}'");
            playerToNameChange.SendNewNotification($"Du hast deinen Namen erfolgreich zu {newName} geändert! Bitte beende nun das Spiel und trag deinen neuen Namen in den GVMP-Launcher ein!", PlayerNotification.NotificationType.ADMIN, duration: 30000);
            playerToNameChange.Kick("Namensaenderung");

            dbPlayer.ResetData("playerToChangeNameGov");
            dbPlayer.SendNewNotification($"Du hast den Namen von {playerToNameChange.GetName()} erfolgreich für ${kosten} zu {newName} geändert!");

        }

        [RemoteEvent]
        public void DivorceGovConfirm(Player p_Player, string divorcePersonName, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (dbPlayer.TeamId != (uint)teams.TEAM_GOV) return;

            DbPlayer playerToDivorce = Players.Players.Instance.FindPlayer(divorcePersonName);
            if (playerToDivorce == null || !playerToDivorce.IsValid()) return;

            if (playerToDivorce.Player.Position.DistanceTo(dbPlayer.Player.Position) > 10) return;

            if (playerToDivorce.married[0] != 0)
            {
                string marryName = "";
                int marryLevel = 0;
                using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT name, level FROM player WHERE id = '{playerToDivorce.married[0]}' LIMIT 1";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.HasRows)
                            {
                                marryName = reader.GetString("name");
                                marryLevel = reader.GetInt32("level");
                            }
                        }
                        conn.Close();
                    }
                }
                if (marryName != "")
                {
                    if (!playerToDivorce.TakeBankMoney(40000 * (playerToDivorce.Level + marryLevel) / 2, $"Scheidung von - {marryName}"))
                    {
                        dbPlayer.SendNewNotification($"Die Scheidung würde {40000 * (playerToDivorce.Level + marryLevel) / 2} $ kosten. Diese Summe konnte nicht abgebucht werden!");
                        return;
                    }

                    playerToDivorce.SendNewNotification($"Du hast dich erfolgreich von {marryName} scheiden lassen.");

                    var findPlayer = Players.Players.Instance.FindPlayer(playerToDivorce.married[0]);
                    if (findPlayer == null || !findPlayer.IsValid())
                    {
                        MySQLHandler.ExecuteAsync($"UPDATE player SET married = 0 WHERE id = '{playerToDivorce.married[0]}'");
                    }
                    else
                    {
                        findPlayer.married[0] = 0;
                        findPlayer.SendNewNotification($"{playerToDivorce.GetName()} hat sich von dir scheiden lassen.");
                    }

                    Logger.AddDivorceLog(playerToDivorce.Id, playerToDivorce.Level, playerToDivorce.married[0]);
                    playerToDivorce.married[0] = 0;

                    dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat die Scheidung von {playerToDivorce.GetName()} und {marryName} durchgeführt!");

                    return;
                }



                return;
            }

            dbPlayer.SendNewNotification("Diese Person ist nicht verheiratet!");
            return;




        }
    }
}
