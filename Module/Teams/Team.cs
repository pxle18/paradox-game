using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teamfight;
using VMP_CNR.Module.Teams.Blacklist;
using VMP_CNR.Module.Teams.Spawn;

namespace VMP_CNR.Module.Teams
{
    public class Team : DbTeam
    {
        public Dictionary<uint, DbPlayer> Members { get; }
        public Dictionary<uint, TeamSpawn> TeamSpawns { get; }
        public Teamfight.Teamfight RequestedTeamfight = null;

        public List<BlacklistEntry> blacklistEntries { get; set; }

        public List<DbPlayer> OnDutyMedics { get; set; }

        public Team(MySqlDataReader reader) : base(reader)
        {
            Members = new Dictionary<uint, DbPlayer>();
            TeamSpawns = new Dictionary<uint, TeamSpawn>();
            blacklistEntries = new List<BlacklistEntry>();
            OnDutyMedics = new List<DbPlayer>();

            this.LoadBlacklistEntries();
        }

        public bool IsNearSpawn(Vector3 Position, float Distance = 60.0f)
        {
            return TeamSpawns.Where(ts => ts.Value.Position.DistanceTo(Position) < Distance).Count() > 0;
        }

        public bool IsInRobbery()
        {
            if (Robbery.StaatsbankRobberyModule.Instance.RobberTeam != null && Robbery.StaatsbankRobberyModule.Instance.RobberTeam.Id == Id) return true;
            if (Robbery.WeaponFactoryRobberyModule.Instance.RobberTeam != null && Robbery.WeaponFactoryRobberyModule.Instance.RobberTeam.Id == Id) return true;
            if (Robbery.RobberyModule.Instance.Robberies.Where(r => r.Value.Player != null && r.Value.Player.IsValid() && r.Value.Player.Team.Id == Id).Count() > 0) return true;
            return false;
        }

        public bool IsInTeamfight()
        {
            if (TeamfightModule.Instance.IsInTeamfight(Id)) return true;

            return false;
        }

        public void SendChatMessage(string message, int rang = 0)
        {
            if (Id == 0) return;
            foreach (DbPlayer dbPlayer in GetTeamMembers().Where(m => m.TeamRank >= rang).ToList())
            {
                dbPlayer.SendNewNotification(message);
            }

        }

        public void SendNotification(string message, int time = 5000, int rang = 0)
        {
            String colorCode = Utils.HexConverter(this.RgbColor);
            if (Id == 0) return;
            foreach (DbPlayer dbPlayer in GetTeamMembers().Where(m => m.TeamRank >= rang))
            {
                dbPlayer.SendNewNotification(message, title: this.Name, notificationType: PlayerNotification.NotificationType.FRAKTION, duration: time);
            }
        }

        public void SendNotificationInRange(string message, Vector3 Pos, float range, int time = 5000, int rang = 0)
        {
            String colorCode = Utils.HexConverter(this.RgbColor);
            if (Id == 0) return;
            foreach (DbPlayer dbPlayer in GetTeamMembers().Where(m => m != null && m.IsValid() && m.TeamRank >= rang && m.Player.Position.DistanceTo(Pos) < range))
            {
                dbPlayer.SendNewNotification(message, title: this.Name, notificationType: PlayerNotification.NotificationType.FRAKTION, duration: time);
            }
        }

        public void SendNotification(string title, string message, int rang = 0)
        {
            if (Id == 0) return;
            foreach (DbPlayer dbPlayer in GetTeamMembers().Where(m => m.TeamRank >= rang))
            {
                dbPlayer.SendNewNotification(message);
            }
        }

        public void AddMember(DbPlayer dbPlayer)
        {
            Members[dbPlayer.Id] = dbPlayer;
        }

        public void RemoveMember(DbPlayer dbPlayer)
        {
            OnDutyMedics.RemoveAll(x => x.Id == dbPlayer.Id);

            if (Members.ContainsKey(dbPlayer.Id))
                Members.Remove(dbPlayer.Id);
        }

        public bool IsMember(DbPlayer dbPlayer)
        {
            return Members.ContainsKey(dbPlayer.Id);
        }

        public List<DbPlayer> GetTeamMembers()
        {
            List<DbPlayer> tmpMembers = new List<DbPlayer>();

            foreach(DbPlayer dbPlayer in Members.Values.ToList())
            {
                if(dbPlayer != null && dbPlayer.IsValid() && dbPlayer.TeamId == this.Id)
                {
                    tmpMembers.Add(dbPlayer);
                }
            }

            return tmpMembers;
        }

        public bool CanWeaponEquipedForTeam(WeaponHash weaponHash)
        {
            /*
            if(Id == (int)teams.TEAM_CIVILIAN)
            {
                if (weaponHash == WeaponHash.Assaultrifle || weaponHash == WeaponHash.Gusenberg || weaponHash == WeaponHash.CarbineRifle || 
                    weaponHash == WeaponHash.Bullpuprifle || weaponHash == WeaponHash.Advancedrifle || weaponHash == WeaponHash.AssaultSMG) return false;
            }*/

            return true;
        }
    }

    public static class TeamPlayerExtensions
    {
        public static bool SetPlayerInMedicDuty(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid())
                return false;

            // In keiner Fraktion
            if (dbPlayer.TeamId == 0 || dbPlayer.Team == null)
                return false;

            // Frak hat keine Medicslots
            if (dbPlayer.Team.MedicSlots == 0)
                return false;

            // Orgas nur 1 Medic im Dienst
            if (dbPlayer.Team.IsBadOrga() && dbPlayer.Team.OnDutyMedics.Count >= 1)
                return false;

            // Bad-Fraks 2 Medics im Dienst
            if (!dbPlayer.Team.IsBadOrga() && dbPlayer.Team.OnDutyMedics.Count >= 2)
                return false;

            // Ist bereits im Medic-Dienst
            if (dbPlayer.Team.OnDutyMedics.Contains(dbPlayer))
                return false;

            // All Good - geh in den Dienst
            dbPlayer.Team.OnDutyMedics.Add(dbPlayer);
            dbPlayer.InParamedicDuty = true;
            return true;
        }

        public static bool SetPlayerOffMedicDuty(this DbPlayer dbPlayer)
        {
            if (dbPlayer.Team == null || dbPlayer.Team.MedicSlots == 0)
                return false;

            if (dbPlayer.Team.OnDutyMedics.Contains(dbPlayer))
                dbPlayer.Team.OnDutyMedics.Remove(dbPlayer);

            dbPlayer.InParamedicDuty = false;

            return true;
        }
    }
}