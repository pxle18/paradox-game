using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Laboratories.Menu;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Laboratories
{
    public class WeaponlaboratoryModule : SqlModule<WeaponlaboratoryModule, Weaponlaboratory, uint>
    {
        public static List<uint> RessourceItemIds = new List<uint> { 300, 468, 470, 462, 464 }; //Eisenbarren, Plastik, Abzug, Alubarren, Broncebarren
        public static uint EndProductItemId = 976; //Waffenset
        public static uint FuelItemId = 537; //Benzin
        public static uint FuelAmountPerProcessing = 1; //Fuelverbrauch pro 15-Minuten-Kochvorgang (Spielerunabhängig)
        public List<Team> HasAlreadyHacked = new List<Team>();

        // Item Id, Price (Aufpreis)
        public Dictionary<uint, int> WeaponHerstellungList = new Dictionary<uint, int>()
        {
            { 996, 40000 }, // Advanced
            { 995, 65000 }, // Sniperrifle
            { 994, 15000 }, // shawnoffshotgun
            { 993, 15000 }, // pumpshotgun
            { 992, 15000 }, // doubleshotgun
            { 991, 15000 }, // smg
            { 990, 15000 }, // pdw
            { 989, 35000 }, // compact
            { 988, 50000 }, // Gusenberg
            { 987, 40000 }, // M4
            { 986, 40000 }, // Bullpup
            { 985, 35000 }, // AK
            { 1267, 15000 }, // MicroSMG
            { 82, 55000 }, // 50000
        };

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_weaponlaboratories`";
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(JumpPointModule) };
        }
        protected override void OnLoaded()
        {
            HasAlreadyHacked = new List<Team>();
            MenuManager.Instance.AddBuilder(new WeaponlaboratoryWeaponMenu());
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Weaponlaboratory weaponlaboratory = Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);

                if(weaponlaboratory != null)
                {
                    if (weaponlaboratory.ActingPlayers.Contains(dbPlayer)) weaponlaboratory.ActingPlayers.Remove(dbPlayer);
                    if(weaponlaboratory.HackInProgess || weaponlaboratory.ImpoundInProgress)
                    {
                        if(!weaponlaboratory.LoggedOutCombatAvoid.ToList().Contains(dbPlayer.Id))
                        {
                            weaponlaboratory.LoggedOutCombatAvoid.Add(dbPlayer.Id);
                        }
                    }
                }
            }));
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.DimensionType[0] != DimensionTypes.Weaponlaboratory) return false;
            if (dbPlayer == null) return false;

            Weaponlaboratory weaponlaboratory = WeaponlaboratoryModule.Instance.GetAll().Values.Where(laboratory => laboratory.TeamId == dbPlayer.Player.Dimension).FirstOrDefault();
            if (weaponlaboratory != null && weaponlaboratory.TeamId == dbPlayer.TeamId && dbPlayer.Player.Position.DistanceTo(Coordinates.WeaponlaboratoryComputerPosition) < 1.0f)
            {
                // Processing
                if (weaponlaboratory.ActingPlayers.Contains(dbPlayer)) weaponlaboratory.StopProcess(dbPlayer);
                else weaponlaboratory.StartProcess(dbPlayer);
                return true;
            }
            if (weaponlaboratory != null && dbPlayer.Player.Position.DistanceTo(Coordinates.WeaponlaboratoryComputerPosition) < 1.0f)
            {
                if (weaponlaboratory.Hacked)
                {
                    MenuManager.Instance.Build(PlayerMenu.LaboratoryOpenInvMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }
            if (weaponlaboratory != null && dbPlayer.Player.Position.DistanceTo(Coordinates.WeaponlaboratoryWeaponBuildMenuPosition) < 1.0f)
            {
                if (dbPlayer.Container.GetItemAmount(976) <= 0)
                {
                    dbPlayer.SendNewNotification("Sie benötigen ein Waffenset!");
                    return true;
                }

                MenuManager.Instance.Build(PlayerMenu.LaboratoryWeaponMenu, dbPlayer).Show(dbPlayer);

                return true;
            }
            return false;
        }

        public override void OnFifteenMinuteUpdate()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Random rnd = new Random();
                foreach (Weaponlaboratory weaponlaboratory in GetAll().Values.ToList())
                {
                    if (weaponlaboratory == null) continue;
                    if (weaponlaboratory.LastAttacked.AddHours(LaboratoryModule.HoursDisablingAfterHackAttack) > DateTime.Now)
                    {
                        if(weaponlaboratory.SkippedLast)
                        {
                            weaponlaboratory.SkippedLast = false;
                        }
                        else
                        {   // Skipp all 2. intervall
                            weaponlaboratory.SkippedLast = true;
                            continue;
                        }
                    }

                    uint fuelAmount = (uint)weaponlaboratory.FuelContainer.GetItemAmount(FuelItemId);
                    foreach (DbPlayer dbPlayer in weaponlaboratory.ActingPlayers.ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                        if (fuelAmount >= FuelAmountPerProcessing)
                        {
                            weaponlaboratory.Processing(dbPlayer);
                        }
                        else
                            weaponlaboratory.StopProcess(dbPlayer);
                    }
                    if (weaponlaboratory.ActingPlayers.Count > 0)
                    {
                        weaponlaboratory.FuelContainer.RemoveItem(FuelItemId, (int)FuelAmountPerProcessing);
                    }
                }
            }));
            return;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            if (TeamModule.Instance.IsWeaponTeamId(dbPlayer.TeamId))
            {
                dbPlayer.WeaponlaboratoryInputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WEAPONLABORATORYINPUT);
                dbPlayer.WeaponlaboratoryOutputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WEAPONLABORATORYOUTPUT);
            }
        }
        public async Task HackWeaponlaboratory(DbPlayer dbPlayer)
        {
            if (dbPlayer.DimensionType[0] != DimensionTypes.Weaponlaboratory) return;
            Weaponlaboratory weaponlaboratory = this.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (weaponlaboratory == null) return;
            await weaponlaboratory.HackLaboratory(dbPlayer);
        }

        public bool CanWeaponLaboratyRaided(Weaponlaboratory weaponlaboratory, DbPlayer dbPlayer)
        {
            if (Configurations.Configuration.Instance.DevMode) return true;
            if (dbPlayer.IsACop() && dbPlayer.IsInDuty()) return true;
            if (GangwarTownModule.Instance.IsTeamInGangwar(TeamModule.Instance.Get(weaponlaboratory.TeamId))) return false;
            if (TeamModule.Instance.Get(weaponlaboratory.TeamId).Members.Count < 15 && !weaponlaboratory.LaborMemberCheckedOnHack) return false;
            // Geht nicht wenn in Gangwar, weniger als 10 UND der Typ kein Cop im Dienst ist (macht halt kein sinn wenn die kochen können < 10 und mans nicht hochnehmen kann (cops))
            return true;
        }

        public Weaponlaboratory GetLaboratoryByDimension(uint dimension)
        {
            return WeaponlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == dimension).FirstOrDefault();
        }
        public Weaponlaboratory GetLaboratoryByPosition(Vector3 position)
        {
            return WeaponlaboratoryModule.Instance.GetAll().Values.Where(Lab => position.DistanceTo(Lab.JumpPointEingang.Position) < 3.0f).FirstOrDefault();
        }
        public Weaponlaboratory GetLaboratoryByJumppointId(int id)
        {
            return WeaponlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.JumpPointEingang.Id == id).FirstOrDefault();
        }
        public Weaponlaboratory GetLaboratoryByTeamId(uint teamId)
        {
            return WeaponlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == teamId).FirstOrDefault();
        }
    }
}
