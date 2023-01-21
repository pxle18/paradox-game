using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Laboratories.Windows;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.TeamSubgroups;

namespace VMP_CNR.Module.Laboratories
{
    public class TeamSubgroupLaboratoryModule : SqlModule<TeamSubgroupLaboratoryModule, TeamSubgroupLaboratory, uint>
    {
        public static List<uint> RessourceItemIds = new List<uint> { 0, 0, 0 }; //natrium, essig, morphin
        public static List<uint> EndProductItemIds = new List<uint> { 1442 }; //heroin

        protected override string GetQuery()
        {
            return "SELECT * FROM `teamsubgroup_laboratories`";
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(JumpPointModule) };
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                TeamSubgroupLaboratory laboratory = Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);

                if (laboratory != null)
                {
                    if (laboratory.ActingPlayers.Contains(dbPlayer)) laboratory.ActingPlayers.Remove(dbPlayer);
                    if (laboratory.HackInProgess || laboratory.ImpoundInProgress)
                    {
                        if (!laboratory.LoggedOutCombatAvoid.ToList().Contains(dbPlayer.Id))
                        {
                            laboratory.LoggedOutCombatAvoid.Add(dbPlayer.Id);
                        }
                    }
                }
            }));
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.DimensionType[0] != DimensionTypes.TeamSubgroupLaboratory) return false;

            TeamSubgroupLaboratory teamSubgroupLaboratory = TeamSubgroupLaboratoryModule.Instance.GetAll().Values.Where(laboratory => laboratory.TeamId == dbPlayer.Player.Dimension).FirstOrDefault();
            if (teamSubgroupLaboratory != null && heroinlaboratory.TeamId == dbPlayer.TeamId && dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryStartPosition) < 1.0f)
            {
                // Processing
                ComponentManager.Get<TeamSubgroupStartWindow>().Show()(dbPlayer, teamSubgroupLaboratory);
                return true;
            }
            if (teamSubgroupLaboratory != null && dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryLaptopPosition) < 1.0f)
            {
                if (teamSubgroupLaboratory.Hacked)
                {
                    MenuManager.Instance.Build(PlayerMenu.LaboratoryOpenInvMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }

            return false;
        }

        public override void OnFifteenMinuteUpdate()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Random rnd = new Random();
                foreach (TeamSubgroupLaboratory teamSubgroupLaboratory in GetAll().Values.ToList())
                {
                    if (teamSubgroupLaboratory == null) continue;
                    foreach (DbPlayer dbPlayer in teamSubgroupLaboratory.ActingPlayers.ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                            teamSubgroupLaboratory.Processing(dbPlayer);
                    }
                }
            }));
            return;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            if (TeamSubgroupModule.Instance[dbPlayer.TeamSubgroupId] != null)
            {
                dbPlayer.TeamSubgroupLaboratoryInputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.TEAMSUBGROUPLABORATORYINPUT);
                dbPlayer.TeamSubgroupLaboratoryOutputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.TEAMSUBGROUPLABORATORYOUTPUT);
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!Configurations.Configuration.Instance.MethLabEnabled) return false;
            if (!colShape.HasData("methInteriorColshape")) return false;
            if (colShapeState == ColShapeState.Enter)
            {
                if (dbPlayer.HasData("inMethLaboraty"))
                {
                    TeamSubgroupLaboratory laboratory = GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                    if (laboratory == null) return false;
                    laboratory.LoadInterior(dbPlayer);
                    return true;
                }
            }
            if (colShapeState == ColShapeState.Exit)
            {
                if (dbPlayer.HasData("inMethLaboraty"))
                {
                    TeamSubgroupLaboratory laboratory = GetLaboratoryByDimension(colShape.Dimension);
                    if (laboratory == null)
                    {
                        return false;
                    }
                    laboratory.UnloadInterior(dbPlayer);
                    return true;
                }
            }
            return false;
        }

        public TeamSubgroupLaboratory GetLaboratoryByDimension(uint dimension)
        {
            return TeamSubgroupLaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == dimension).FirstOrDefault();
        }
        public TeamSubgroupLaboratory GetLaboratoryByPosition(Vector3 position)
        {
            return TeamSubgroupLaboratoryModule.Instance.GetAll().Values.Where(Lab => position.DistanceTo(Lab.JumpPointEingang.Position) < 3.0f).FirstOrDefault();
        }
        public TeamSubgroupLaboratory GetLaboratoryByJumppointId(int id)
        {
            return TeamSubgroupLaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.JumpPointEingang.Id == id).FirstOrDefault();
        }
        public TeamSubgroupLaboratory GetLaboratoryByTeamId(uint teamId)
        {
            return TeamSubgroupLaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == teamId).FirstOrDefault();
        }
    }
}
