using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams.Shelter;
using VMP_CNR.Module.Teams.Spawn;

namespace VMP_CNR.Module.Teams
{
    public sealed class TeamModule : SqlModule<TeamModule, Team, uint>
    {
        public int DutyCops => TeamModule.Instance.Get(1).GetTeamMembers().Where(member => member.Duty).Count() + // LSPD
                               TeamModule.Instance.Get(5).GetTeamMembers().Where(member => member.Duty).Count() + // FIB
                               TeamModule.Instance.Get(23).GetTeamMembers().Where(member => member.Duty).Count() + // IAA
                               TeamModule.Instance.Get(21).GetTeamMembers().Where(member => member.Duty).Count(); // SWAT

        public override Type[] RequiredModules()
        {
            return new[] {typeof(TeamSpawnModule), typeof(ItemModelModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `team`;";
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.LastUninvite = reader.GetDateTime("lastuninvite");
            dbPlayer.LastPaydayChanged = reader.GetDateTime("lastpaydaychanged");

            if (dbPlayer.LastUninvite == null) dbPlayer.LastUninvite = DateTime.Now;
            if (dbPlayer.LastPaydayChanged == null) dbPlayer.LastPaydayChanged = DateTime.Now;
        }

        public override int GetOrder()
        {
            return 2;
        }
        
        protected override bool OnLoad()
        {
            var result = base.OnLoad();
            foreach (var teamSpawn in TeamSpawnModule.Instance.GetAll())
            {
                var team = this[teamSpawn.Value.TeamId];
                team?.TeamSpawns.Add(teamSpawn.Value.Index, teamSpawn.Value);
            }

            return result;
        }

        public bool IsWeaponTeamId(uint Id)
        {
            return Id == (int)TeamTypes.TEAM_HUSTLER || Id == (int)TeamTypes.TEAM_MADRAZO;
        }
        public bool IsMethTeamId(uint Id)
        {
            return Id == (int)TeamTypes.TEAM_TRIADEN ||
                Id == (int)TeamTypes.TEAM_YAKUZA ||
                Id == (int)TeamTypes.TEAM_LCN ||
                Id == (int)TeamTypes.TEAM_ORGANISAZIJA ||
                Id == (int)TeamTypes.TEAM_BRATWA;
        }
        public bool IsWeedTeamId(uint Id)
        {
            return Id == (int)TeamTypes.TEAM_BALLAS || Id == (int)TeamTypes.TEAM_LOST || Id == (int)TeamTypes.TEAM_IRISHMOB ||
                   Id == (int)TeamTypes.TEAM_GROVE || Id == (int)TeamTypes.TEAM_VAGOS || Id == (int)TeamTypes.TEAM_MARABUNTA ||
                   Id == (int)TeamTypes.TEAM_REDNECKS || Id == (int)TeamTypes.TEAM_HOH || Id == (int)TeamTypes.TEAM_MADRAZO;
        }

        public bool IsHeroinTeamId(uint Id)
        {
            return Id == (int)TeamTypes.TEAM_BALLAS || Id == (int)TeamTypes.TEAM_LOST || Id == (int)TeamTypes.TEAM_IRISHMOB ||
                   Id == (int)TeamTypes.TEAM_GROVE || Id == (int)TeamTypes.TEAM_VAGOS || Id == (int)TeamTypes.TEAM_MARABUNTA ||
                   Id == (int)TeamTypes.TEAM_REDNECKS || Id == (int)TeamTypes.TEAM_HOH ||
                   Id == (int)TeamTypes.TEAM_TRIADEN || Id == (int)TeamTypes.TEAM_YAKUZA || Id == (int)TeamTypes.TEAM_LCN ||
                   Id == (int)TeamTypes.TEAM_ORGANISAZIJA || Id == (int)TeamTypes.TEAM_BRATWA || Id == (int)TeamTypes.TEAM_BALKANEROS;
        }

        public bool IsGangsterTeamId(uint Id)
        {
            return IsWeedTeamId(Id) || IsMethTeamId(Id) || IsWeaponTeamId(Id) || IsHeroinTeamId(Id);
        }

        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            dbPlayer.SetTeam(dbPlayer.TeamId);
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (dbPlayer.TeamId != 0)
            {
                dbPlayer.Team.RemoveMember(dbPlayer);
            }
        }

        public Team GetByName(string name)
        {
            foreach (var team in GetAll().Values)
            {
                if (string.Equals(team.Name, name, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(team.ShortName, name, StringComparison.CurrentCultureIgnoreCase)
                    || team.Name.ToLower().Contains(name.ToLower())) return team;
            }
            
            return null;
        }

        public Team GetById(int id)
        {
            foreach (var team in GetAll().Values)
            {
                if (team.Id == id) return team;
            }

            return null;
        }

        public void SendChatMessage(string message, params uint[] ids)
        {
            foreach (var id in ids)
            {
                this[id].SendNotification(message); 
            }
        }

        public void SendMessageToTeam(string message, TeamTypes teamId, int time = 5000, int requiredRang = 0)
        {
            Team team = Get((uint)teamId);

            team.SendNotification(message, time:time, rang: requiredRang);
            return;
        }

        public void SendMessageToTeam(string message, uint teamId, int time = 5000, int requiredRang = 0)
        {
            Team team = Get(teamId);

            team.SendNotification(message, time: time, rang: requiredRang);
            return;
        }

        public void SendMessageToNSA(string message)
        {
            foreach (DbPlayer dbPlayer in NSA.NSAModule.Instance.NSAMember.Where(p => p.IsNSADuty).ToList())
            {
                dbPlayer.SendNewNotification(message, PlayerNotification.NotificationType.FRAKTION);
            }
            return;
        }

        public void SendChatMessageToDepartments(DbPlayer sourceDbPlayer, string message)
        {
            foreach (var team in GetAll())
            {
                if (!team.Value.IsCops()) continue;
                
                team.Value.SendNotification(sourceDbPlayer.Team.ShortName + " Rang " +
                        sourceDbPlayer.TeamRank + " | " + sourceDbPlayer.GetName() + ": " + message);
            }
        }

        public void SendChatMessageToDepartmentsInRange(string message, Vector3 pos, float range)
        {
            foreach (var team in GetAll())
            {
                if (!team.Value.IsCops()) continue;

                team.Value.SendNotificationInRange(message, pos, range);
            }
        }


        public void SendChatMessageToDepartments(string message)
        {
            foreach (var team in GetAll())
            {
                if (!team.Value.IsCops()) continue;

                team.Value.SendNotification(message, 10000);
            }
        }
        
        public DbPlayer CheckIfDutyCopIsInRange(DbPlayer sourceDbPlayer, float range)
        {
            foreach (var team in GetAll())
            {
                if (!team.Value.IsCops()) continue;
                foreach (DbPlayer dbPlayer in team.Value.Members.Values)
                {
                    if (dbPlayer.Duty && 
                        dbPlayer.Player.Position.DistanceTo(sourceDbPlayer.Player.Position) < range && 
                        dbPlayer.Player.Dimension == sourceDbPlayer.Player.Dimension &&
                        dbPlayer.DimensionType == sourceDbPlayer.DimensionType) return dbPlayer;
                }
            }
            return null;
        }

        public override void OnFifteenMinuteUpdate()
        {
            foreach(Team team in GetAll().Values.Where(t => t.IsGangsters()).ToList())
            {
                int teamsumm = 0;
                foreach(DbPlayer member in team.Members.Values.ToList())
                {
                    if(member.BlackMoneyBank[0] > 5000)
                    {
                        member.BlackMoneyBank[0] -= 400;
                        teamsumm += 340; // 90% kurs
                        member.SaveBlackMoneyBank();
                    }
                }
                TeamShelter shelter = TeamShelterModule.Instance.GetByTeam(team.Id);
                if(shelter != null)
                {
                    shelter.GiveMoney(teamsumm);
                }
            }
        }
        public void SendChatMessageToDutyTeamMembers(DbPlayer sourceDbPlayer, string message)
        {
            foreach (DbPlayer dbPlayer in sourceDbPlayer.Team.Members.Values.ToList())
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                if (dbPlayer.IsACop() && !dbPlayer.Duty) continue;
                dbPlayer.SendNewNotification( sourceDbPlayer.Team.ShortName + " Rang " +
                    sourceDbPlayer.TeamRank + " | " + sourceDbPlayer.GetName() + ": " + message);
            }
        }
    }

    public static class TeamPlayerExtension
    {
        public static void SaveLastUninvite(this DbPlayer xPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET `lastuninvite` = '{xPlayer.LastUninvite.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `id` = '{xPlayer.Id}'");
        }

        public static void SaveLastPaydayChanged(this DbPlayer xPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET `lastpaydaychanged` = '{xPlayer.LastPaydayChanged.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `id` = '{xPlayer.Id}'");
        }

        public static void SaveLastBankRobbery(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE team SET `lastbankrob` = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `id` = '{dbPlayer.Team.Id}'");
        }

        public static void SaveLastOutfitPreQuest(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE team SET `lastoutfitprequest` = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `id` = '{dbPlayer.Team.Id}'");
        }
    }
}