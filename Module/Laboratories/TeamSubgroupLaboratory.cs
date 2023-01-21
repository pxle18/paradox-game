using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Laboratories
{
    public class TeamSubgroupLaboratory : Loadable<uint>
    {
        public uint Id { get; }

        public uint TeamSubgroupId { get; }

        public uint DestinationId { get; }
        public JumpPoint JumpPointEingang { get; set; }
        public JumpPoint JumpPointAusgang { get; set; }

        public List<DbPlayer> ActingPlayers { get; set; }

        public List<Marker> Marker { get; set; }

        private List<DbPlayer> playersInsideLaboratory = new List<DbPlayer>();

        private uint sodiumItemId = 0;
        private uint vinegarItemId = 0;
        private uint morphineItemId = 0;
        private uint heroinampulleItemId = 1442;

        private int sodiumPerTick = 25;
        private int vinegarPerTick = 25;
        private int morphinePerTick = 25;
        private int heroinPerTick = 25;

        public TeamSubgroupLaboratory(MySqlDataReader reader) : base(reader)
        {
            try
            {
                Id = reader.GetUInt32("id");

                TeamSubgroupId = reader.GetUInt32("teamsubgroupid");
                DestinationId = reader.GetUInt32("destination_id");
                ActingPlayers = new List<DbPlayer>();

                List<JumpPoint> JumpPoints = JumpPointModule.Instance.jumpPoints.Values.Where(jp => jp.DestinationId == DestinationId && jp.Id != DestinationId).ToList();

                Random rnd = new Random();
                int selectedJumpPoint = rnd.Next(JumpPoints.Count);
                int i = 0;
                JumpPoints.ForEach(jumpPoint =>
                {

                    if (selectedJumpPoint == i)
                    {
                        JumpPointEingang = jumpPoint;
                        JumpPointAusgang = JumpPointModule.Instance.Get(jumpPoint.DestinationId);
                        JumpPointAusgang.Destination = JumpPointEingang;
                        JumpPointAusgang.DestinationId = JumpPointEingang.Id;
                    }
                    else
                    {
                        NAPI.Task.Run(() =>
                        {
                            if (jumpPoint != null)
                            {
                                if (jumpPoint.ColShape != null)
                                {
                                    jumpPoint.ColShape.ResetData("jumpPointId");
                                    NAPI.ColShape.DeleteColShape(jumpPoint.ColShape);
                                }
                                JumpPoints.Remove(jumpPoint);
                            }
                        });
                    }
                    i++;
                });

                ColShape ColShape = Spawners.ColShapes.Create(JumpPointAusgang.Position, 30.0f, this.TeamId);
                ColShape.SetData("methInteriorColshape", this.TeamId);

                // Inventory Markers
                NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryInvFuelPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId);
                NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryInvInputPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId);
                NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryInvOutputPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId);

                // E Markers
                NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryLaptopPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId);
                NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryStartPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId);
            }
            catch (Exception e) { Logger.Print($"TeamSubgroupLaboratory {Id} " + e.ToString()); }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void Processing(DbPlayer dbPlayer)
        {
            if (!ActingPlayers.ToList().Contains(dbPlayer)) return;

            if (!PlayerHasRessourcesForTick(dbPlayer) ||
                !dbPlayer.TeamSubgroupLaboratoryOutputContainer.CanInventoryItemAdded(heroinampulleItemId))
            {
                StopProcess(dbPlayer);
                return;
            }

            dbPlayer.TeamSubgroupLaboratoryInputContainer.RemoveItem(sodiumItemId, sodiumPerTick);
            dbPlayer.TeamSubgroupLaboratoryInputContainer.RemoveItem(morphineItemId, morphinePerTick);
            dbPlayer.TeamSubgroupLaboratoryInputContainer.RemoveItem(vinegarItemId, vinegarPerTick);

            dbPlayer.TeamSubgroupLaboratoryOutputContainer.AddItem(heroinkisteItemId, heroinPerTick);
        }

        public void StopProcess(DbPlayer dbPlayer)
        {
            if (ActingPlayers.ToList().Contains(dbPlayer))
            {
                ActingPlayers.Remove(dbPlayer);
                if (dbPlayer.DimensionType[0] == DimensionTypes.Heroinlaboratory && dbPlayer.Player.Dimension != 0)
                    dbPlayer.SendNewNotification("Prozess gestoppt!");
            }
        }

        public void StartProcess(DbPlayer dbPlayer)
        {

            if (!PlayerHasRessourcesForTick(dbPlayer))
            {
                StopProcess(dbPlayer);
                dbPlayer.SendNewNotification("Es fehlen Materialien...");
                return;
            }

            if (ActingPlayers.ToList().Contains(dbPlayer))
            {
                dbPlayer.SendNewNotification("Der Prozess ist bereits im Gange...");
                return;
            }
            ActingPlayers.Add(dbPlayer);
            dbPlayer.SendNewNotification("Prozess gestartet!");
        }

        public void LoadInterior(DbPlayer dbPlayer)
        {
            int boilerState = 2;
            int tableState = 1;
            int securityState = 1;

            dbPlayer.Player.TriggerNewClient("loadMethInterior", tableState, boilerState, securityState);

            if (!playersInsideLaboratory.Contains(dbPlayer))
            {
                playersInsideLaboratory.Add(dbPlayer);
            }
        }

        public void UnloadInterior(DbPlayer dbPlayer)
        {
            if (playersInsideLaboratory.Contains(dbPlayer))
            {
                playersInsideLaboratory.Remove(dbPlayer);
            }
        }

        private bool PlayerHasRessourcesForTick(DbPlayer dbPlayer)
        {
            if (dbPlayer.TeamSubgroupLaboratoryInputContainer.GetItemAmount(sodiumItemId) >= sodiumPerTick && 
                dbPlayer.TeamSubgroupLaboratoryInputContainer.GetItemAmount(morphineItemId) >= morphinePerTick && 
                dbPlayer.TeamSubgroupLaboratoryInputContainer.GetItemAmount(vinegarItemId) >= vinegarPerTick)
            {
                return true;
            }
            return false;
        }
    }
}
