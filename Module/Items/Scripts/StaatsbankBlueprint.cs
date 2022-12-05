using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Robbery;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool StaatsbankBlueprint(DbPlayer dbPlayer, ItemModel ItemData)
        {
            // da selbe kriterium wie bankrob
            if (!StaatsbankRobberyModule.Instance.CanStaatsbankRobbed() || !dbPlayer.IsAGangster()) return false;
            
            List<StaatsbankTunnel> list = StaatsbankRobberyModule.Instance.StaatsbankTunnels.Where(t => t.IsActiveForTeam == null).ToList();
            Random rand = new Random();
            
            if (list.Count <= 0) return false;
            
            StaatsbankTunnel staatsbankTunnel = list.ElementAt(rand.Next(0, list.Count));
            if (staatsbankTunnel == null) return false;

            // Setze den vorherigen frei, falls einer da war
            StaatsbankTunnel usedTunnelBevore = StaatsbankRobberyModule.Instance.StaatsbankTunnels.ToList().Where(t => t.IsActiveForTeam == dbPlayer.Team).FirstOrDefault();

            if(usedTunnelBevore != null)
            {
                usedTunnelBevore.IsActiveForTeam = null;
            }

            staatsbankTunnel.IsActiveForTeam = dbPlayer.Team;
            dbPlayer.SendNewNotification("Du konntest durch den Bauplan eine Stelle für einen Tunnelbau ausmachen!");
            dbPlayer.Player.TriggerNewClient("setPlayerGpsMarker", staatsbankTunnel.Position.X, staatsbankTunnel.Position.Y);
            return true;
        }

        public static async Task<bool> StaatsbankDrill(DbPlayer dbPlayer, ItemModel ItemData)
        {
            // da selbe kriterium wie bankrob
            if (dbPlayer.Player.Position.DistanceTo(StaatsbankRobberyModule.DrillingPoint) > 4.0f || !StaatsbankRobberyModule.Instance.IsActive || StaatsbankRobberyModule.Instance.RobberTeam != dbPlayer.Team) return false;

            StaatsbankTunnel staatsbankTunnel = StaatsbankRobberyModule.Instance.StaatsbankTunnels.Where(t => t.IsActiveForTeam == dbPlayer.Team).FirstOrDefault();
            if (staatsbankTunnel == null) return false;

            if(!staatsbankTunnel.IsOutsideOpen)
            {
                dbPlayer.SendNewNotification("Um einen Tunnel zu bohren, müssen erst Gitterstäbe in der Kanalisation durchgeschweißt werden!");
                return false;
            }

            Chats.sendProgressBar(dbPlayer, 60000);

            dbPlayer.SendNewNotification("Du beginnst nun den Tunnel zu bohren!");

            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_const_drill@male@drill@base", "base");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);

            await NAPI.Task.WaitForMainThread(60000);

            dbPlayer.ResetData("userCannotInterrupt");
            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured()) return true;
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();

            staatsbankTunnel.IsInsideOpen = true;
            dbPlayer.SendNewNotification("Du hast einen Tunnel zur Kanalisation gebohrt! Achtung, der Tunnel wird mit der Zeit verschüttet (15 min)");
            
            TeamModule.Instance.SendMessageToTeam("[INFO] Die Seismografen haben unterhalb der Staatsbank erschütterungen wahrgenommen!", teams.TEAM_FIB);

            // jump Points...


            NAPI.Task.Run(() =>
            {
                staatsbankTunnel.jpInside = new JumpPoint
                {
                    Id = JumpPointModule.Instance.jumpPoints.Last().Key + 1,
                    Name = "Tunnel",
                    Position = dbPlayer.Player.Position,
                    AdminUnbreakable = true,
                    DestinationId = JumpPointModule.Instance.jumpPoints.Last().Key + 2,
                    Dimension = 0,
                    Heading = dbPlayer.Player.Heading,
                    InsideVehicle = false,
                    LastBreak = DateTime.Now.Add(new TimeSpan(0, -5, 0)),
                    Locked = false,
                    Range = 2,
                    Teams = new HashSet<Team>(),
                    Unbreakable = true
                };

                staatsbankTunnel.jpOutside = new JumpPoint
                {
                    Id = JumpPointModule.Instance.jumpPoints.Last().Key + 2,
                    Name = "Tunnel",
                    Position = staatsbankTunnel.Position,
                    AdminUnbreakable = true,
                    DestinationId = JumpPointModule.Instance.jumpPoints.Last().Key + 1,
                    Dimension = 0,
                    Heading = staatsbankTunnel.Heading,
                    InsideVehicle = false,
                    LastBreak = DateTime.Now.Add(new TimeSpan(0, -5, 0)),
                    Locked = false,
                    Range = 2,
                    Teams = new HashSet<Team>(),
                    Unbreakable = true
                };

                staatsbankTunnel.jpInside.Destination = staatsbankTunnel.jpOutside;
                staatsbankTunnel.jpOutside.Destination = staatsbankTunnel.jpInside;

                staatsbankTunnel.jpInside.ColShape = ColShapes.Create(staatsbankTunnel.jpInside.Position, 2, 0);
                staatsbankTunnel.jpOutside.ColShape = ColShapes.Create(staatsbankTunnel.jpOutside.Position, 2, 0);
                staatsbankTunnel.jpInside.ColShape.SetData("jumpPointId", staatsbankTunnel.jpInside.Id);
                staatsbankTunnel.jpOutside.ColShape.SetData("jumpPointId", staatsbankTunnel.jpOutside.Id);
                
                JumpPointModule.Instance.jumpPoints.Add(staatsbankTunnel.jpInside.Id, staatsbankTunnel.jpInside);
                JumpPointModule.Instance.jumpPoints.Add(staatsbankTunnel.jpOutside.Id, staatsbankTunnel.jpOutside);

                staatsbankTunnel.TunnelCreated = DateTime.Now;
            });

            return true;
        }
    }
}