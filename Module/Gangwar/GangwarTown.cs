using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teamfight;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Spawn;

namespace VMP_CNR.Module.Gangwar
{
    public class GangwarTown : Loadable<uint>
    {
        // var methprice = rnd.Next(185, 239);
        // var diamondprice = rnd.Next(2100, 2980); // gold = dia * 4

        public uint Id { get; }
        public Team OwnerTeam { get; set; }
        public string Name { get; }
        public Vector3 Position { get; }
        public uint Range { get; }
        public DateTime LastAttacked { get; set; }

        public Vector3 InteriorPosition { get; }
        public float InteriorHeading { get; }

        public Vector3 Flag_1 { get; }
        public Marker Flag_1Marker { get; set; }

        public Vector3 Flag_2 { get; }
        public Marker Flag_2Marker { get; set; }

        public Vector3 Flag_3 { get; }
        public Marker Flag_3Marker { get; set; }

        public Marker FightMarker { get; set; }

        public Vector3 AttackerSpawnPosition { get; set; }
        public Marker AttackerSpawnMarker { get; set; }

        public Vector3 DefenderSpawnPosition { get; set; }
        public Marker DefenderSpawnMarker { get; set; }

        public int AttackerGarageID { get; set; }
        public int DefenderGarageID { get; set; }

        public int IsActive { get; set; }

        // Temp Datas
        public int AttackerPoints { get; set; }
        public Team AttackerTeam { get; set; }
        public int DefenderPoints { get; set; }
        public Team DefenderTeam { get; set; }
        public bool IsAttacked { get; set; }

        public Blip Blip { get; set; }

        public int Cash { get; set; }
        
        public List<DbPlayer> Visitors { get; set; }
        public List<SxVehicle> Vehicles { get; set; }

        public GangwarTown(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            OwnerTeam = TeamModule.Instance.Get(reader.GetUInt32("owner_team"));
            Name = reader.GetString("name");
            Position = new Vector3(
                reader.GetFloat("pos_x"), 
                reader.GetFloat("pos_y"), 
                reader.GetFloat("pos_z")
            );
            InteriorPosition = new Vector3(
                reader.GetFloat("interior_pos_x"), 
                reader.GetFloat("interior_pos_y"),
                reader.GetFloat("interior_pos_z")
            );
            InteriorHeading = reader.GetFloat("interior_heading");
            Range = reader.GetUInt32("range");
            LastAttacked = reader.GetDateTime("last_attacked");
            Flag_1 = new Vector3(
                reader.GetFloat("flag_1_pos_x"), 
                reader.GetFloat("flag_1_pos_y"),
                reader.GetFloat("flag_1_pos_z")
            );
            Flag_2 = new Vector3(
                reader.GetFloat("flag_2_pos_x"), 
                reader.GetFloat("flag_2_pos_y"),
                reader.GetFloat("flag_2_pos_z")
            );
            Flag_3 = new Vector3(
                reader.GetFloat("flag_3_pos_x"), 
                reader.GetFloat("flag_3_pos_y"),
                reader.GetFloat("flag_3_pos_z")
            );

            Visitors = new List<DbPlayer>();
            Vehicles = new List<SxVehicle>();
            
            Color color = GangwarModule.Instance.StandardColor;

            AttackerSpawnPosition = new Vector3(
                reader.GetFloat("attacker_pos_x"),
                reader.GetFloat("attacker_pos_y"),
                reader.GetFloat("attacker_pos_z")
            );
            
            DefenderSpawnPosition = new Vector3(
                reader.GetFloat("defender_pos_x"),
                reader.GetFloat("defender_pos_y"),
                reader.GetFloat("defender_pos_z")
            );
            
            AttackerGarageID =  reader.GetInt32("attacker_garageID");
            DefenderGarageID =  reader.GetInt32("defender_garageID");

            IsActive = reader.GetInt32("isActive");

            if (IsActive == 1)
            {
                Vector3 newPosition = new Vector3(InteriorPosition.X, InteriorPosition.Y, InteriorPosition.Z - 1f);
                Markers.Create(
                    1, 
                    newPosition, 
                    new Vector3(), 
                    new Vector3(), 
                    1.0f, 
                    color.Alpha, 
                    color.Red, 
                    color.Green,
                    color.Blue
                );
                ColShape l_ColShape = ColShapes.Create(Position, Range, GangwarModule.Instance.DefaultDimension);
                l_ColShape.SetData("gangwarId", Id);

                Flag_1Marker = Spawners.Markers.Create(4, Flag_1, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, GangwarModule.Instance.DefaultDimension);
                Flag_2Marker = Spawners.Markers.Create(4, Flag_2, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, GangwarModule.Instance.DefaultDimension);
                Flag_3Marker = Spawners.Markers.Create(4, Flag_3, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, GangwarModule.Instance.DefaultDimension);
                
                DefenderSpawnMarker = Markers.Create(4, DefenderSpawnPosition, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, GangwarModule.Instance.DefaultDimension);
                AttackerSpawnMarker = Markers.Create(4, AttackerSpawnPosition, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, GangwarModule.Instance.DefaultDimension);

            }

            // Temps
            AttackerPoints = 0;
            DefenderPoints = 0;
            AttackerTeam = null;
            DefenderTeam = null;
            IsAttacked = false;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public static class GangwarTownFunctions
    {
        public static void Attack(this GangwarTown gangwarTown, DbPlayer attacker)
        {
            if (GangwarTownModule.Instance.GetOwnedTownsCount(attacker.Team) >= GangwarModule.Instance.GangwarTownLimit)
            {
                attacker.SendNewNotification("Ihr habt genug Gebiete alla, macht mal mehr Rollplay!");
                return;
            }

            if (!gangwarTown.CanAttacked())
            {
                attacker.SendNewNotification("Das Gebiet kann derzeit nicht angegriffen werden!");
                return;
            }

            if(GangwarTownModule.Instance.IsTeamInGangwar(attacker.Team))
            {
                attacker.SendNewNotification("Sie greifen bereits ein Gebiet an!");
                return;
            }

            if (GangwarTownModule.Instance.IsTeamInGangwar(gangwarTown.OwnerTeam))
            {
                attacker.SendNewNotification("Dieses Team kämpft bereits!");
                return;
            }
            
            // Check min 15 Players each team needs
            if ((gangwarTown.OwnerTeam.Members.Count < 15 || attacker.Team.Members.Count < 15) && !Configurations.Configuration.Instance.DevMode)
            {
                attacker.SendNewNotification("Ein Angriff kann nur bei mindestens 15 Mitgliedern beider Parteien stattfinden!");
                return;
            }

            gangwarTown.StartFight(attacker.Team, gangwarTown.OwnerTeam);
        }
        
        public static void StartFight(this GangwarTown gangwarTown, Team attackerTeam, Team defenderTeam)
        {
            // Set Attacker, Defender
            gangwarTown.AttackerTeam = attackerTeam;
            gangwarTown.DefenderTeam = defenderTeam;
            gangwarTown.DefenderPoints = 0;
            gangwarTown.AttackerPoints = 0;

            Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Gangwar zwischen {attackerTeam.Name} und {defenderTeam.Name} ist gestartet!");

            // Send Notifications to team
            gangwarTown.AttackerTeam.SendNotification($"Sie greifen nun {gangwarTown.Name} an!");
            gangwarTown.DefenderTeam.SendNotification($"Ihr Gebiet {gangwarTown.Name} wird angegriffen!");

            // Create Marker
            NAPI.Task.Run(() =>
            {
                Vector3 newPosition = new Vector3(gangwarTown.Position.X, gangwarTown.Position.Y, gangwarTown.Position.Z - 30f);
                gangwarTown.FightMarker = NAPI.Marker.CreateMarker(1, newPosition, new Vector3(), new Vector3(), (float)gangwarTown.Range * 2, GangwarModule.Instance.StandardColor,
                    true, GangwarModule.Instance.DefaultDimension);

                gangwarTown.AttackerSpawnMarker.Color = attackerTeam.RgbColor;
                gangwarTown.DefenderSpawnMarker.Color = defenderTeam.RgbColor;
            });

            // Set GangwarTown under attack...
            gangwarTown.LastAttacked = DateTime.Now;
            gangwarTown.IsAttacked = true;
            GangwarModule.Instance.ActiveGangwarTowns.Add(gangwarTown);

            Vector3 attackerPosition = GangwarTownModule.Instance.GetGangwarTownSpawnByTeam(attackerTeam);
            foreach (DbPlayer dbPlayer in gangwarTown.AttackerTeam.Members.Values.Where(player => player.Player.Position.DistanceTo(gangwarTown.Position) < gangwarTown.Range))
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) continue;

                dbPlayer.Player.SetPosition(attackerPosition);
                Teamfight.TeamfightFunctions.SetToGangware(dbPlayer);
            }

            gangwarTown.TriggerNewClientOfTownMembers("initializeGangwar", attackerTeam.ShortName, defenderTeam.ShortName, attackerTeam.Id, defenderTeam.Id,
                GangwarModule.Instance.GangwarTimeLimit * 60);
        }
        
        public static bool CanAttacked(this GangwarTown gangwarTown)
        {
            if (Configurations.Configuration.Instance.DevMode) return true;

            if (gangwarTown.LastAttacked.AddHours(40) > DateTime.Now) return false;
            int hour = DateTime.Now.Hour;
            int min = DateTime.Now.Minute;

            // 1h vor restarts...
            // GW erst ab 18 Uhr nach Wende * Frohning 09.12.2020
            if (hour == 7 || hour == 15 || hour == 16 || hour == 17 || hour == 23) return false;

            // 30 min nach restarts...
            if (hour == 8 || hour == 16 || hour == 0)
            {
                if (min < 30) return false;
            }
            return true;
        }

        public static void Finish(this GangwarTown gangwarTown)
        {
            gangwarTown.TriggerNewClientOfAll("finishGangwar");

            // Set Winner
            if (gangwarTown.DefenderPoints > gangwarTown.AttackerPoints)
            {
                // Defender Wins
                gangwarTown.DefenderTeam.SendNotification($"Sie haben {gangwarTown.Name} erfolgreich verteidigt!");
                gangwarTown.AttackerTeam.SendNotification($"Die Eroberung von {gangwarTown.Name} ist gescheitert!");
            }
            else
            {
                // Attacker Wins
                gangwarTown.ChangeOwner(gangwarTown.AttackerTeam);
                gangwarTown.DefenderTeam.SendNotification($"Die Verteidigung von {gangwarTown.Name} ist gescheitert!");
                gangwarTown.AttackerTeam.SendNotification($"Sie haben {gangwarTown.Name} erfolgreich erobert!");
            }

            NAPI.Task.Run(() =>
            {
                if (gangwarTown.FightMarker != null) gangwarTown.FightMarker.Delete();
                gangwarTown.FightMarker = null;

                if (gangwarTown.AttackerSpawnMarker != null) 
                    gangwarTown.AttackerSpawnMarker.Color = GangwarModule.Instance.StandardColor;

                if (gangwarTown.DefenderSpawnMarker != null) 
                    gangwarTown.DefenderSpawnMarker.Color = GangwarModule.Instance.StandardColor;
            });

            gangwarTown.UpdateLastAttackToNow();

            foreach ((uint key, DbPlayer value) in gangwarTown.AttackerTeam.Members.Where(item => item.Value != null && item.Value.IsValid()))
            {
                if (value.IsInjured() || value.Dimension[0] != GangwarModule.Instance.DefaultDimension) continue;

                TeamSpawn spawn = value.Team.TeamSpawns.FirstOrDefault().Value;
                if (spawn == null) continue;
                value.Player.SetPosition(spawn.Position);
                TeamfightFunctions.RemoveFromGangware(value);
            }

            foreach ((uint key, DbPlayer value) in gangwarTown.DefenderTeam.Members.Where(item => item.Value != null && item.Value.IsValid()).Where(item => item.Value != null))
            {
                if (value.IsInjured() || value.Dimension[0] != GangwarModule.Instance.DefaultDimension) continue;

                TeamSpawn spawn = value.Team.TeamSpawns.FirstOrDefault().Value;
                if (spawn == null) continue;
                value.Player.SetPosition(spawn.Position);
                TeamfightFunctions.RemoveFromGangware(value);
            }

            gangwarTown.Vehicles.ForEach(vehicle => VehicleHandler.Instance.DeleteVehicle(vehicle, false));
            gangwarTown.Vehicles.Clear();

            gangwarTown.AttackerTeam = null;
            gangwarTown.DefenderTeam = null;
            gangwarTown.AttackerPoints = 0;
            gangwarTown.DefenderPoints = 0;
            gangwarTown.IsAttacked = false;

            GangwarModule.Instance.ActiveGangwarTowns.Remove(gangwarTown);
            
            gangwarTown.Blip.Color = gangwarTown.OwnerTeam.BlipColor;
        }
        
        public static void TriggerNewClientOfAll(this GangwarTown gangwarTown, string eventName, params object[] args)
        {
            foreach (DbPlayer dbPlayer in gangwarTown.Visitors)
            {
                dbPlayer.Player.TriggerNewClient(eventName, args);
            }
        }

        public static void TriggerNewClientOfTownMembers(this GangwarTown gangwarTown, string eventName, params object[] args)
        {
            foreach (DbPlayer dbPlayer in gangwarTown.Visitors)
            {
                //dimensions check?
                dbPlayer.Player.TriggerNewClient(eventName, args);
            }
        }

        public static void IncreasePoints(this GangwarTown gangwarTown, int defender, int attacker)
        {
            gangwarTown.AttackerPoints += attacker;
            gangwarTown.DefenderPoints += defender;
            gangwarTown.TriggerNewClientOfTownMembers("updateGangwarScore", gangwarTown.AttackerPoints, gangwarTown.DefenderPoints);
        }

        // Updates Owner of Gangwartown
        public static void ChangeOwner(this GangwarTown gangwarTown, Team newOwner)
        {
            gangwarTown.OwnerTeam = newOwner;
            string query = $"UPDATE `gangwar_towns` SET `owner_team` = '{newOwner.Id}' WHERE `id` = '{gangwarTown.Id}'";
            MySQLHandler.ExecuteAsync(query);
        }

        public static void SetCash(this GangwarTown gangwarTown, int newCash)
        {
            gangwarTown.Cash = newCash;
            string query = $"UPDATE `gangwar_towns` SET `cash` = '{newCash}' WHERE `id` = '{gangwarTown.Id}'";
            MySQLHandler.ExecuteAsync(query);
        }

        // Updates Owner of Gangwartown
        public static void ChangeOwner(this GangwarTown gangwarTown, uint newOwnerTeamId)
        {
            gangwarTown.OwnerTeam = TeamModule.Instance.Get(newOwnerTeamId);
            string query = $"UPDATE `gangwar_towns` SET `owner_team` = '{newOwnerTeamId}' WHERE `id` = '{gangwarTown.Id}'";
            MySQLHandler.ExecuteAsync(query);
        }

        // Updates Gangwar Last Attack to now
        public static void UpdateLastAttackToNow(this GangwarTown gangwarTown)
        {
            gangwarTown.LastAttacked = DateTime.Now;
            string query = $"UPDATE `gangwar_towns` SET `last_attacked` = NOW() WHERE `id` = '{gangwarTown.Id}'";
            MySQLHandler.ExecuteAsync(query);
        }
    }
}