using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Teams.Blacklist.Menu;

namespace VMP_CNR.Module.Teams.Blacklist
{

    public class BlacklistType
    {
        public int TypeId { get; set; }
        public int DeathLimit { get; set; }
        public int Costs { get; set; }
        public int RequiredRang { get; set; }
        public string Description { get; set; }

        public string ShortDesc { get; set; }
    }

    public class BlacklistEntry
    {
        public int TeamId { get; set; }
        public int BlacklistPlayerId { get; set; }
        public int BlacklistSetterId { get; set; }
        public int ActualBlacklistDeaths { get; set; }
        public DateTime BlacklistEntryDate { get; set; }
        public int TypeId { get; set; }

        public BlacklistEntry(int teamId, int blacklistPlayerId, int blacklistSetterId, int actualBlacklistDeaths, DateTime entryDate, int typeId)
        {
            TeamId = teamId;
            BlacklistPlayerId = blacklistPlayerId;
            BlacklistSetterId = blacklistSetterId;
            ActualBlacklistDeaths = actualBlacklistDeaths;
            BlacklistEntryDate = entryDate;
            TypeId = typeId;
        }
    }

    public class BlacklistModule : Module<BlacklistModule>
    {

        public Dictionary<int, BlacklistType> blacklistTypes = new Dictionary<int, BlacklistType>();

        public override bool Load(bool reload = false)
        {
            blacklistTypes = new Dictionary<int, BlacklistType>();
            blacklistTypes.Add(0, new BlacklistType() { TypeId = 0, DeathLimit = 10, Costs = 50000, Description = "Rufmord / Beleidigung 50.000$", RequiredRang = 2, ShortDesc = "Rufmord/Beleidigung" });
            blacklistTypes.Add(1, new BlacklistType() { TypeId = 1, DeathLimit = 15, Costs = 75000, Description = "Körperverletzung / Respektlosigkeit 75.000$", RequiredRang = 6, ShortDesc = "Körperverletzung/Respektlosigkeit" });
            blacklistTypes.Add(2, new BlacklistType() { TypeId = 2, DeathLimit = 20, Costs = 100000, Description = "Mord / Schädigen der Fraktion 100.000$", RequiredRang = 10, ShortDesc = "Mord/Schädigen der Fraktion" });


            MenuManager.Instance.AddBuilder(new BlacklistMenuBuilder());
            MenuManager.Instance.AddBuilder(new BlacklistTypeMenuBuilder());
            return base.Load(reload);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandblacklist(Player player, string level)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.IsAGangster()) return;

            Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.BlacklistMenu, dbPlayer).Show(dbPlayer);
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsetblacklist(Player player, string level)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.IsAGangster()) return;

            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Blacklist", Callback = "SetBlacklist", Message = "Geben Sie den Namen der Person ein, welche auf die Blacklist soll:" });

            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandremoveblacklist(Player player, string returnstring)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.IsAGangster() || dbPlayer.TeamRank < 10) return;
            if (returnstring.Length < 2) return;

            DbPlayer target = Players.Players.Instance.FindPlayer(returnstring);
            if(target != null && target.IsValid())
            {
                if (target.IsOnBlacklist((int)dbPlayer.TeamId))
                {
                    BlacklistEntry blacklistEntry = dbPlayer.Team.blacklistEntries.FirstOrDefault(pb => pb.BlacklistPlayerId == target.Id);
                    if (blacklistEntry == null || !BlacklistModule.Instance.blacklistTypes.ContainsKey(blacklistEntry.TypeId)) return;

                    blacklistEntry.Delete();
                    dbPlayer.Team.SendNotification($"{target.GetName()} wurde von {dbPlayer.GetName()} von der Blacklist entfernt!");
                    return;
                }
                else dbPlayer.SendNewNotification("Person befindet sich nicht auf der Blacklist!");
            }

            return;
        }
    }

    public static class BlacklistTeamExtension
    {
        public static void LoadBlacklistEntries(this Team team)
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM team_blacklist WHERE team_id = '{team.Id}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            team.blacklistEntries.Add(new BlacklistEntry(reader.GetInt32("team_id"), reader.GetInt32("player_id"), reader.GetInt32("setter_id"), reader.GetInt32("actual_deaths"), reader.GetDateTime("entry_date"), reader.GetInt32("type_id")));
                        }
                    }
                }
                conn.Close();
            }
        }

        public static bool IsOnBlacklist(this DbPlayer dbPlayer, int TeamId)
        {
            return TeamModule.Instance.GetById(TeamId).blacklistEntries.FindAll(pb => pb.BlacklistPlayerId == dbPlayer.Id).Count > 0;
        }

        public static int GetBlacklistType(this DbPlayer dbPlayer, int TeamId)
        {
            return TeamModule.Instance.GetById(TeamId).blacklistEntries.FirstOrDefault(pb => pb.BlacklistPlayerId == dbPlayer.Id).TypeId;
        }

        public static void AddBlacklistEntry(this Team team, DbPlayer setter, DbPlayer target, int TypeId)
        {
            if (!BlacklistModule.Instance.blacklistTypes.ContainsKey(TypeId)) return;
            // get Type
            BlacklistType type = BlacklistModule.Instance.blacklistTypes[TypeId];

            team.blacklistEntries.Add(new BlacklistEntry((int)team.Id, (int)target.Id, (int)setter.Id, 0, DateTime.Now, TypeId));

            MySQLHandler.ExecuteAsync($"INSERT INTO team_blacklist (team_id, player_id, setter_id, actual_deaths, type_id, entry_date) VALUES ('{team.Id}', '{target.Id}', '{setter.Id}', '0', '{type.TypeId}', '{DateTime.Now.ToString("yyyy-MM-dd H:mm:ss")}')");
        }

        public static void IncreaseBlacklist(this Team team, DbPlayer target)
        {
            if (target == null || !target.IsValid()) return;
            
            BlacklistEntry blacklistEntry = team.blacklistEntries.FirstOrDefault(pb => pb.BlacklistPlayerId == target.Id);
            if (blacklistEntry == null || !BlacklistModule.Instance.blacklistTypes.ContainsKey(blacklistEntry.TypeId)) return;

            blacklistEntry.ActualBlacklistDeaths++;
            
            if (blacklistEntry.ActualBlacklistDeaths >= BlacklistModule.Instance.blacklistTypes[blacklistEntry.TypeId].DeathLimit)
            {
                blacklistEntry.Delete();
            }
            else blacklistEntry.Save();
        }

        public static void Save(this BlacklistEntry entry)
        {
            MySQLHandler.Execute($"UPDATE team_blacklist SET actual_deaths = '{entry.ActualBlacklistDeaths}' WHERE player_id = '{entry.BlacklistPlayerId}' AND setter_id = '{entry.BlacklistSetterId}' AND team_id = '{entry.TeamId}'"); // Do saving from object
        }

        public static void Delete(this BlacklistEntry entry)
        {
            MySQLHandler.Execute($"DELETE FROM team_blacklist WHERE player_id = '{entry.BlacklistPlayerId}' AND setter_id = '{entry.BlacklistSetterId}' AND team_id = '{entry.TeamId}'"); 

            int team = entry.TeamId;
            TeamModule.Instance.GetById(team).blacklistEntries.Remove(entry);
        }
    }
}