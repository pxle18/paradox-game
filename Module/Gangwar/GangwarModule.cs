using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Events;
using VMP_CNR.Module.Teamfight;
using VMP_CNR.Module.Teams.Spawn;

namespace VMP_CNR.Module.Gangwar
{
    public sealed class GangwarModule : Module<GangwarModule>
    {
        public uint DefaultDimension = 9999;

        // Point Settings
        public int KillPoints = 3; // if A kills B, A gets points
        public int TimerFlagPoints = 1; // each ten sec if a player is in range without a enemy
        public Color StandardColor = new Color(255, 140, 0, 255);
        
        public int GangwarTimeLimit = 45;
        public int GangwarTownLimit = 3;

        public List<GangwarTown> ActiveGangwarTowns = new List<GangwarTown>();

        public static List<int> GoldComponentIds = new List<int>() { 28, 147, 18, 261, 518, 451, 253, 140, 267, 44, 33, 201 };
        
        protected override bool OnLoad()
        {
            MySQLHandler.Execute("ALTER TABLE `gangwar_garages` CHANGE `pos_1_x` `pos_1_x` FLOAT(11) NOT NULL, CHANGE `pos_1_y` `pos_1_y` FLOAT(11) NOT NULL, CHANGE `pos_1_z` `pos_1_z` FLOAT(11) NOT NULL, CHANGE `heading_1` `heading_1` FLOAT(11) NOT NULL, CHANGE `pos_2_x` `pos_2_x` FLOAT(11) NOT NULL, CHANGE `pos_2_y` `pos_2_y` FLOAT(11) NOT NULL, CHANGE `pos_2_z` `pos_2_z` FLOAT(11) NOT NULL, CHANGE `heading_2` `heading_2` FLOAT(11) NOT NULL, CHANGE `pos_3_x` `pos_3_x` FLOAT(11) NOT NULL, CHANGE `pos_3_y` `pos_3_y` FLOAT(11) NOT NULL, CHANGE `pos_3_z` `pos_3_z` FLOAT(11) NOT NULL, CHANGE `heading_3` `heading_3` FLOAT(11) NOT NULL;");
            return true;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape p_ColShape, ColShapeState p_ColShapeState)
        {
            if (!p_ColShape.TryData("gangwarId", out uint l_GangWarID)) return false;
            GangwarTown gangwarTown = GangwarTownModule.Instance.Get(l_GangWarID);

            switch (p_ColShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("gangwarId", l_GangWarID);
                    gangwarTown.Visitors.Add(dbPlayer);

                    if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
                    {
                        var attackerTeam = gangwarTown.AttackerTeam;
                        var defenderTeam = gangwarTown.DefenderTeam;
                        if (attackerTeam == null || defenderTeam == null) return true;
                        var l_Limit = Instance.GangwarTimeLimit * 60;
                        var l_Subtraction = (int)DateTime.Now.Subtract(gangwarTown.LastAttacked).TotalSeconds;
                        dbPlayer.Player.TriggerNewClient("initializeGangwar", attackerTeam.ShortName, defenderTeam.ShortName, attackerTeam.Id, defenderTeam.Id, l_Limit - l_Subtraction);
                        dbPlayer.Player.TriggerNewClient("updateGangwarScore", gangwarTown.AttackerPoints, gangwarTown.DefenderPoints);
                    }
                    return true;

                case ColShapeState.Exit:
                    dbPlayer.ResetData("gangwarId");
                    gangwarTown.Visitors.Remove(dbPlayer);

                    if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
                    {
                        dbPlayer.Player.TriggerNewClient("finishGangwar");
                    }
                    return true;
                default:
                    return true;
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            if (key != Key.E) return false;

            GangwarTown gangwarTown = GangwarTownModule.Instance.GetByPosition(dbPlayer.Player.Position);
            if (gangwarTown != null && dbPlayer.Team.IsGangsters())
            {
                MenuManager.Instance.Build(PlayerMenu.GangwarInfo, dbPlayer).Show(dbPlayer);
                return true;
            }

            if (!GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team)) return false;
            if (dbPlayer.Player.Dimension == DefaultDimension && GangwarTownModule.Instance.IsGaragePosition(dbPlayer.Player.Position))
            {
                MenuManager.Instance.Build(PlayerMenu.GangwarVehicleMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }

        [CommandPermission]
        [Command]
        public void Commandquitgw(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod() || dbPlayer.IsInjured()||!dbPlayer.CanInteract()) return;
            if (dbPlayer.Player.Dimension != GangwarModule.Instance.DefaultDimension) return;
            if (dbPlayer.Team.IsNearSpawn(dbPlayer.Player.Position) || GangwarTownModule.Instance.IsTeamSpawn(dbPlayer.Player.Position))
            {
                TeamSpawn spawn = dbPlayer.Team.TeamSpawns.FirstOrDefault().Value;
                if (spawn == null)
                {
                    dbPlayer.SendNewNotification("Du hast den Gangwar verlassen.");
                    TeamfightFunctions.RemoveFromGangware(dbPlayer);
                    return;
                }

                NAPI.Task.Run(() => { player.Position = spawn.Position; });
                dbPlayer.SendNewNotification("Du hast den Gangwar verlassen.");
                TeamfightFunctions.RemoveFromGangware(dbPlayer);
            }
            else
            {
                dbPlayer.SendNewNotification("Verlassen nicht möglich - Du bist nicht am Fraktionsspawn.");
            }

        }

        public override void OnMinuteUpdate()
        {
            foreach (GangwarTown gangwarTown in ActiveGangwarTowns.ToList())
            {
                if (gangwarTown.LastAttacked.AddMinutes(GangwarModule.Instance.GangwarTimeLimit) < System.DateTime.Now)
                {
                    // over time limit
                    gangwarTown.Finish();
                }
            }
        }

        public void TenSecUpdateHandle(GangwarTown gangwarTown)
        {
            // check Flags...
            int attackersFlagOne = 0;
            int defendersFlagOne = 0;
            int attackersFlagTwo = 0;
            int defendersFlagTwo = 0;
            int attackersFlagThree = 0;
            int defendersFlagThree = 0;

            foreach (DbPlayer dbPlayer in gangwarTown.Visitors.ToList())
            {
                if (dbPlayer.TeamId != gangwarTown.AttackerTeam.Id) continue;
                if (dbPlayer.IsInjured()) continue;

                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_1) < 10.0f) attackersFlagOne++;
                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_2) < 10.0f) attackersFlagTwo++;
                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_3) < 10.0f) attackersFlagThree++;
            }

            foreach (DbPlayer dbPlayer in gangwarTown.Visitors.ToList())
            {
                if (dbPlayer.TeamId != gangwarTown.DefenderTeam.Id) continue;
                if (dbPlayer.IsInjured()) continue;

                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_1) < 10.0f) defendersFlagOne++;
                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_2) < 10.0f) defendersFlagTwo++;
                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_3) < 10.0f) defendersFlagThree++;
            }

            // Setting Points

            // Flag 1
            if (attackersFlagOne == 0 && defendersFlagOne > 0)
            {
                gangwarTown.IncreasePoints(GangwarModule.Instance.TimerFlagPoints, 0);
                gangwarTown.Flag_1Marker.Color = gangwarTown.DefenderSpawnMarker.Color;
            }
            else if (defendersFlagOne == 0 && attackersFlagOne > 0)
            {
                gangwarTown.IncreasePoints(0, GangwarModule.Instance.TimerFlagPoints);
                gangwarTown.Flag_1Marker.Color = gangwarTown.AttackerSpawnMarker.Color;
            }
            else
            {
                gangwarTown.Flag_1Marker.Color = StandardColor;
            }

            // Flag 2
            if (attackersFlagTwo == 0 && defendersFlagTwo > 0)
            {
                gangwarTown.IncreasePoints(GangwarModule.Instance.TimerFlagPoints, 0);
                gangwarTown.Flag_2Marker.Color = gangwarTown.DefenderSpawnMarker.Color;

            }
            else if (defendersFlagTwo == 0 && attackersFlagTwo > 0)
            {
                gangwarTown.IncreasePoints(0, GangwarModule.Instance.TimerFlagPoints);
                gangwarTown.Flag_2Marker.Color = gangwarTown.AttackerSpawnMarker.Color;
            }
            else
            {
                gangwarTown.Flag_2Marker.Color = StandardColor;
            }

            // Flag 3
            if (attackersFlagThree == 0 && defendersFlagThree > 0)
            {
                gangwarTown.IncreasePoints(GangwarModule.Instance.TimerFlagPoints, 0);
                gangwarTown.Flag_3Marker.Color = gangwarTown.DefenderSpawnMarker.Color;
            }
            else if (defendersFlagThree == 0 && attackersFlagThree > 0)
            {
                gangwarTown.IncreasePoints(0, GangwarModule.Instance.TimerFlagPoints);
                gangwarTown.Flag_3Marker.Color = gangwarTown.AttackerSpawnMarker.Color;
            }
            else
            {
                gangwarTown.Flag_3Marker.Color = StandardColor;
            }
                
            
        }

        public override void OnTenSecUpdate()
        {
            foreach (GangwarTown gangwarTown in ActiveGangwarTowns.ToList())
            {
                TenSecUpdateHandle(gangwarTown);
            }
        }
    }
}
