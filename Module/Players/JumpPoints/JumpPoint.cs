using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.Jailescape;
using VMP_CNR.Module.Bunker;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Handler;
using VMP_CNR.Module.Einreiseamt;

namespace VMP_CNR.Module.Players.JumpPoints
{
    public class JumpPoint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public uint Dimension { get; set; }
        public DimensionType DimensionType { get; set; }
        public int DestinationId { get; set; }
        public HashSet<Team> Teams { get; set; }
        public bool Locked { get; set; }
        public bool InsideVehicle { get; set; }
        public float Range { get; set; }
        public GTANetworkAPI.Object Object { get; set; }
        public ColShape ColShape { get; set; }
        public DateTime LastBreak { get; set; }
        public bool Unbreakable { get; set; }
        public bool AdminUnbreakable { get; set; }
        public JumpPoint Destination { get; set; }
        public bool EnterOnColShape { get; set; }
        public bool HideInfos { get; set; }
        public bool Disabled { get; set; }

        public string ClientLoading { get; set; }

        public bool DisableInfos { get; set; }
        public HashSet<uint> Houses { get; }

        public int RangRestriction { get; set; }

        public int Group { get; set; }
        public List<LastUsedFrom> LastUseds { get; set; }
        public bool Einreiseamt { get; set; }

        public JumpPoint(MySqlDataReader reader)
        {
            Id = reader.GetInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Dimension = (uint)reader.GetInt32("dimension");
            DimensionType = (DimensionType)reader.GetInt32("dimension_type");
            DestinationId = reader.GetInt32("destionation");
            RangRestriction = reader.GetInt32("rangrestriction");
            Group = reader.GetInt32("group");
            Disabled = reader.GetInt32("disabled") == 1;
            HideInfos = reader.GetInt32("hide_infos") == 1;
            var teamsString = reader.GetString("teams");
            var teams = new HashSet<Team>();
            LastUseds = new List<LastUsedFrom>();
            if (!string.IsNullOrEmpty(teamsString))
            {
                var splittedTeams = teamsString.Split(',');
                foreach (var splittedTeam in splittedTeams)
                {
                    if (!uint.TryParse(splittedTeam, out var teamId) || teams.Contains(TeamModule.Instance.Get(teamId))) return;
                    teams.Add(TeamModule.Instance.Get(teamId));
                }
            }

            var housestring = reader.GetString("houses");
            Houses = new HashSet<uint>();
            if (!string.IsNullOrEmpty(housestring))
            {
                var splittedHouses = housestring.Split(',');
                foreach (var houseIdString in splittedHouses)
                {
                    if (!uint.TryParse(houseIdString, out var houseid)) continue;
                    Houses.Add(houseid);
                }
            }

            LastBreak = DateTime.Now.Add(new TimeSpan(0, -5, 0)); // set lastbreak for load now -5 min

            Teams = teams;
            InsideVehicle = reader.GetInt32("inside_vehicle") == 1;
            Range = reader.GetFloat("range");
            Locked = reader.GetBoolean("locked");
            ColShape = ColShapes.Create(Position, Range, Dimension);
            ColShape.SetData("jumpPointId", Id);
            Unbreakable = reader.GetInt32("unbreakable") == 1;
            AdminUnbreakable = reader.GetInt32("unbreakable") == 2;
            EnterOnColShape = reader.GetInt32("colshape") == 1;

            ClientLoading = reader.GetString("client_loading");

            Einreiseamt = reader.GetUInt32("einreiseamt") == 1;
        }

        public JumpPoint()
        {

        }

        public bool TravelThrough(DbPlayer player)
        {
            if (Locked || Disabled) return false;

            if (Id == 216 || Id == 215)
            {
                if (player.HasPerso[0] == 0) return false; // Only Use with perso
            }

            if (!InsideVehicle || !player.RageExtension.IsInVehicle)
            {
                player.SetDimension(Destination.Dimension);
                player.DimensionType[0] = Destination.DimensionType;

                if (JailescapeModule.jailTunnelEscape != null && JailescapeModule.jailTunnelEntrance != null)
                {
                    if (this == JailescapeModule.jailTunnelEntrance || this == JailescapeModule.jailTunnelEscape)
                    {
                        if (JailescapeModule.UsedAmount >= JailescapeModule.MaxUsableAmountPerTunnel)
                        {
                            return false; // Restrict tunnel only X Amount defined in JailescapeModule
                        }
                    }
                }

                if(Destination.Dimension != 0)
                {
                    Task.Run(async () =>
                    {
                        if (Laboratories.MethlaboratoryModule.Instance.GetAll().Where(lab => lab.Value.JumpPointAusgang.Id.Equals(DestinationId)).Count() > 0 || Laboratories.HeroinlaboratoryModule.Instance.GetAll().Where(lab => lab.Value.JumpPointAusgang.Id.Equals(DestinationId)).Count() > 0)
                        {
                            player.SetData("inMethLaboraty", true);
                        }

                        if (ClientLoading != "" && ClientLoading.Length > 3)
                        {
                            player.Player.TriggerNewClient(ClientLoading);
                        }

                        Vector3 PortPosition = new Vector3(Destination.Position.X, Destination.Position.Y, Destination.Position.Z - 15.0f);

                        player.SetData("Teleport", 3);
                        player.SetData("lastPosition", await player.RageExtension.GetPositionAsync());
                        player.Player.TriggerNewClient("freezePlayer", true);

                        await Task.Delay(500);
                        player.Player.SetPosition(PortPosition);
                        player.Player.SetRotation(Destination.Heading);
                        await Task.Delay(2000);

                        player.Player.TriggerNewClient("spawnProtection", 3000, 255, false); //Sec,alpha,notify

                        player.SetData("ignoreGodmode", 10);
                        player.Player.SetPosition(Destination.Position);
                        player.Player.SetRotation(Destination.Heading);
                        player.Player.TriggerNewClient("freezePlayer", false);
                        player.StopAnimation(); // Es sollen keine Anims gesynct werden, schließlich geht man ja durch den Jumppoint
                    });
                }
                else
                {
                    Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
                    {
                        if ((Laboratories.MethlaboratoryModule.Instance.GetAll().Where(lab => lab.Value.JumpPointAusgang.Id.Equals(DestinationId)).Count() > 0 || Laboratories.HeroinlaboratoryModule.Instance.GetAll().Where(lab => lab.Value.JumpPointAusgang.Id.Equals(DestinationId)).Count() > 0) && player.HasData("inMethLaboraty"))
                        {
                            player.ResetData("inMethLaboraty");
                        }

                        if (ClientLoading != "" && ClientLoading.Length > 3)
                        {
                            player.Player.TriggerNewClient(ClientLoading);
                        }

                        player.SetData("Teleport", 3);
                        player.Player.SetPosition(Destination.Position);
                        player.Player.SetRotation(Destination.Heading);
                        player.Player.TriggerNewClient("spawnProtection", 3000, 255, false); //Sec,alpha,notify
                        player.SetData("ignoreGodmode", 10);
                        player.ResetData("lastPosition");
                        player.StopAnimation(); // Es sollen keine Anims gesynct werden, schließlich geht man ja durch den Jumppoint
                    }));
                }
            }
            else
            {
                var vehicle = player.Player.Vehicle;

                SxVehicle sxVeh = vehicle.GetVehicle();
                if (sxVeh == null || !sxVeh.IsValid()) return false;

                NAPI.Task.Run(() =>
                {
                    vehicle.Dimension = Destination.Dimension;
                });

                foreach (DbPlayer occupant in sxVeh.GetOccupants().GetLegacyDictionary().Values)
                {
                    if (occupant == null) continue;

                    occupant.SetData("Teleport", 3);
                    occupant.SetDimension(Destination.Dimension);
                }

                if (Destination.Dimension != 0)
                {
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        player.SetData("Teleport", 3);
                        player.SetData("lastPosition", player.Player.Position);
                        player.Player.TriggerNewClient("freezePlayer", true);

                        // Main Thread
                        await NAPI.Task.WaitForMainThread(1000);
                        vehicle.Rotation = new Vector3(0, 0, Destination.Heading);
                        vehicle.Position = Destination.Position;
                        player.Player.TriggerNewClient("spawnProtection", 3000, 255, false); //Sec,alpha,notify
                        player.SetData("ignoreGodmode", 10);
                        
                        // Background Thread
                        await Task.Delay(1500);
                        player.Player.TriggerNewClient("freezePlayer", false);
                        player.StopAnimation(); // Es sollen keine Anims gesynct werden, schließlich geht man ja durch den Jumppoint
                    }));
                }
                else
                {
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async() =>
                    {
                        player.SetData("Teleport", 3);
                        player.Player.TriggerNewClient("freezePlayer", true);
                        
                        // Main Thread
                        NAPI.Task.Run(() =>
                        {
                            vehicle.Rotation = new Vector3(0, 0, Destination.Heading);
                            vehicle.Position = Destination.Position;
                        });
                        
                        // Worker Thread
                        await Task.Delay(1000);
                        player.Player.TriggerNewClient("spawnProtection", 3000, 255, false); //Sec,alpha,notify
                        player.SetData("ignoreGodmode", 10);
                        
                        // Background Thread
                        await Task.Delay(1500);
                        player.Player.TriggerNewClient("freezePlayer", false);
                        player.ResetData("lastPosition");
                        player.StopAnimation(); // Es sollen keine Anims gesynct werden, schließlich geht man ja durch den Jumppoint
                    }));
                }
            }
            return true;
        }

        public bool CanOpen(DbPlayer dbPlayer)
        {
            if (Houses.Contains(HouseModule.Instance.Get(dbPlayer.OwnHouse[0]).Id)) return true;
            foreach (uint houseId in Houses)
            {
                if (dbPlayer.IsTenant() && dbPlayer.GetTenant().HouseId == houseId) return true;
                if (dbPlayer.HouseKeys.Contains(houseId)) return true;
            }

            return false;
        }

        public bool CanInteract(DbPlayer dbPlayer)
        {
            if (Disabled) return false;

            if (HalloweenModule.isActive) return false;

            if (Group == 99 && dbPlayer.IsNSADuty) return true;

            if (Teams.Contains(TeamModule.Instance.Get((uint)TeamTypes.TEAM_IAA)) && dbPlayer.IsNSADuty) return true;

            if ((Teams.Contains(dbPlayer.Team) && dbPlayer.TeamRank >= RangRestriction) ||
                dbPlayer.Rank.CanAccessFeature("enter_all") ||
                (Houses.Count > 0 && CanOpen(dbPlayer))) return true;

            if (dbPlayer.IsEinreiseAmt() && Einreiseamt) return true;

            return false;
        }

        public bool ToggleLock(DbPlayer player)
        {
            if (!CanInteract(player)) return false;

            if (LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Locked = !Locked;
                Destination.Locked = !Destination.Locked;
                if (Locked)
                {
                    player.SendNewNotification("Tuer abgeschlossen", title: "Tür", notificationType: PlayerNotification.NotificationType.ERROR);
                }
                else
                {
                    player.SendNewNotification("Tuer aufgeschlossen", title: "Tür", notificationType: PlayerNotification.NotificationType.SUCCESS);
                }

                // Add
                LastUseds.Add(new LastUsedFrom() { Name = player.GetName(), DateTime = DateTime.Now, Opened = !Locked });
            }));

            return true;
        }
    }
}