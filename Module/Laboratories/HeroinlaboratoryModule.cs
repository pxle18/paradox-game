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

namespace VMP_CNR.Module.Laboratories
{
    public class HeroinlaboratoryModule : SqlModule<HeroinlaboratoryModule, Heroinlaboratory, uint>
    {
        public static List<uint> RessourceItemIds = new List<uint> { 1442, 14 }; //heroinampullen, toilettenreiniger
        public static List<uint> EndProductItemIds = new List<uint> { 1443 }; //Kiste veredeltes Heroin
        public static uint FuelItemId = 537; //Benzin
        public static uint FuelAmountPerProcessing = 1; //Fuelverbrauch pro 15-Minuten-Kochvorgang (Spielerunabhängig)
        public List<Team> HasAlreadyHacked = new List<Team>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_heroinlaboratories`";
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(JumpPointModule) };
        }
        protected override void OnLoaded()
        {
            HasAlreadyHacked = new List<Team>();
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Heroinlaboratory laboratory = Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);

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
            if (key != Key.E || dbPlayer.DimensionType[0] != DimensionTypes.Heroinlaboratory) return false;

            Heroinlaboratory heroinlaboratory = HeroinlaboratoryModule.Instance.GetAll().Values.Where(laboratory => laboratory.TeamId == dbPlayer.Player.Dimension).FirstOrDefault();
            if (heroinlaboratory != null && heroinlaboratory.TeamId == dbPlayer.TeamId && dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryStartPosition) < 1.0f)
            {
                // Processing
                ComponentManager.Get<HeroinlaboratoryStartWindow>().Show()(dbPlayer, heroinlaboratory);
                return true;
            }
            if (heroinlaboratory != null && dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryLaptopPosition) < 1.0f)
            {
                if (heroinlaboratory.Hacked)
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
                foreach (Heroinlaboratory heroinlaboratory in GetAll().Values.ToList())
                {
                    if (heroinlaboratory == null) continue;
                    if (heroinlaboratory.LastAttacked.AddHours(LaboratoryModule.HoursDisablingAfterHackAttack) > DateTime.Now)
                    {
                        if (heroinlaboratory.SkippedLast)
                        {
                            heroinlaboratory.SkippedLast = false;
                        }
                        else
                        {   // Skipp all 2. intervall
                            heroinlaboratory.SkippedLast = true;
                            continue;
                        }
                    }
                    uint fuelAmount = (uint)heroinlaboratory.FuelContainer.GetItemAmount(FuelItemId);
                    foreach (DbPlayer dbPlayer in heroinlaboratory.ActingPlayers.ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                        if (fuelAmount >= FuelAmountPerProcessing)
                        {
                            heroinlaboratory.Processing(dbPlayer);
                        }
                        else
                            heroinlaboratory.StopProcess(dbPlayer);
                    }
                    if (heroinlaboratory.ActingPlayers.Count > 0)
                    {
                        heroinlaboratory.FuelContainer.RemoveItem(FuelItemId, (int)FuelAmountPerProcessing);
                    }
                }
            }));
            return;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            if (TeamModule.Instance.IsHeroinTeamId(dbPlayer.TeamId))
            {
                dbPlayer.HeroinlaboratoryInputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.HEROINLABORATORYINPUT);
                dbPlayer.HeroinlaboratoryOutputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.HEROINLABORATORYOUTPUT);
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
                    Heroinlaboratory laboratory = GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                    if (laboratory == null) return false;
                    laboratory.LoadInterior(dbPlayer);
                    return true;
                }
            }
            if (colShapeState == ColShapeState.Exit)
            {
                if (dbPlayer.HasData("inMethLaboraty"))
                {
                    Heroinlaboratory laboratory = GetLaboratoryByDimension(colShape.Dimension);
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

        public async Task HackHeroinlaboratory(DbPlayer dbPlayer)
        {
            if (dbPlayer.DimensionType[0] != DimensionTypes.Heroinlaboratory) return;
            Heroinlaboratory heroinlaboratory = this.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (heroinlaboratory == null) return;
            await heroinlaboratory.HackLaboratory(dbPlayer);
        }

        public bool CanHeroinlaboratyRaided(Heroinlaboratory heroinlaboratory, DbPlayer dbPlayer)
        {
            if (Configurations.Configuration.Instance.DevMode) return true;
            if (dbPlayer.IsACop() && dbPlayer.IsInDuty()) return true;
            if (GangwarTownModule.Instance.IsTeamInGangwar(TeamModule.Instance.Get(heroinlaboratory.TeamId))) return false;
            if (TeamModule.Instance.Get(heroinlaboratory.TeamId).Members.Count < 15 && !heroinlaboratory.LaborMemberCheckedOnHack) return false;
            // Geht nicht wenn in Gangwar, weniger als 10 UND der Typ kein Cop im Dienst ist (macht halt kein sinn wenn die kochen können < 10 und mans nicht hochnehmen kann (cops))
            return true;
        }

        public Heroinlaboratory GetLaboratoryByDimension(uint dimension)
        {
            return HeroinlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == dimension).FirstOrDefault();
        }
        public Heroinlaboratory GetLaboratoryByPosition(Vector3 position)
        {
            return HeroinlaboratoryModule.Instance.GetAll().Values.Where(Lab => position.DistanceTo(Lab.JumpPointEingang.Position) < 3.0f).FirstOrDefault();
        }
        public Heroinlaboratory GetLaboratoryByJumppointId(int id)
        {
            return HeroinlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.JumpPointEingang.Id == id).FirstOrDefault();
        }
        public Heroinlaboratory GetLaboratoryByTeamId(uint teamId)
        {
            return HeroinlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == teamId).FirstOrDefault();
        }
    }
}
