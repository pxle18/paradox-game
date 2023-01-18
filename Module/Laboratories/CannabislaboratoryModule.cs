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
    public class CannabislaboratoryModule : SqlModule<CannabislaboratoryModule, Cannabislaboratory, uint>
    {
        public static List<uint> RessourceItemIds = new List<uint> { 984, 966, 997 }; //dünger, batteriezelle, hanfsamenpulver
        public static List<uint> EndProductItemIds = new List<uint> { 983, 982, 981, 980 }; //Pures Meth
        public static uint FuelItemId = 537; //Benzin
        public static uint FuelAmountPerProcessing = 5; //Fuelverbrauch pro 15-Minuten-Kochvorgang (Spielerunabhängig)
        public List<Team> HasAlreadyHacked = new List<Team>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_cannabislaboratories` WHERE 1=2"; //     DISABLE CANNABISLABS
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
                Cannabislaboratory weaponlaboratory = Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);

                if (weaponlaboratory != null)
                {
                    if (weaponlaboratory.ActingPlayers.Contains(dbPlayer)) weaponlaboratory.ActingPlayers.Remove(dbPlayer);
                    if (weaponlaboratory.HackInProgess || weaponlaboratory.ImpoundInProgress)
                    {
                        if (!weaponlaboratory.LoggedOutCombatAvoid.ToList().Contains(dbPlayer.Id))
                        {
                            weaponlaboratory.LoggedOutCombatAvoid.Add(dbPlayer.Id);
                        }
                    }
                }
            }));
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.DimensionType[0] != DimensionTypes.Cannabislaboratory) return false;

            Cannabislaboratory Cannabislaboratory = CannabislaboratoryModule.Instance.GetAll().Values.Where(laboratory => laboratory.TeamId == dbPlayer.Player.Dimension).FirstOrDefault();
            if (Cannabislaboratory != null && Cannabislaboratory.TeamId == dbPlayer.TeamId && dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryComputerPosition) < 1.0f)
            {
                // Processing
                ComponentManager.Get<CannabislaboratoryStartWindow>().Show()(dbPlayer, Cannabislaboratory);
                return true;
            }
            if (Cannabislaboratory != null && dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryComputerPosition) < 1.0f)
            {
                if (Cannabislaboratory.Hacked)
                {
                    MenuManager.Instance.Build(PlayerMenu.LaboratoryOpenInvMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }

            if (dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryCheckBoilerQuality) < 1.0f)
            {
                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    int time = 60000; // 1 min zum Check
                    Chats.sendProgressBar(dbPlayer, time);
                    

                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetData("userCannotInterrupt", true);

                    dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                    await Task.Delay(time);

                    if (dbPlayer == null || !dbPlayer.IsValid()) return;

                    dbPlayer.SetData("userCannotInterrupt", false);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                    dbPlayer.StopAnimation();

                    dbPlayer.SendNewNotification($"Die Qualität wird vorraussichtlich {Cannabislaboratory.Quality}% betragen.");

                }));
                return true;
            }

            if (dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryBatterieSwitch) < 1.0f)
            {
                int BatterieAmount = dbPlayer.Container.GetItemAmount(15);
                int addableAmount = BatterieAmount * 5;
                // 725 -> 966
                if (BatterieAmount >= 1)
                {
                    if (addableAmount > dbPlayer.Container.GetMaxItemAddedAmount(966))
                    {
                        addableAmount = dbPlayer.Container.GetMaxItemAddedAmount(966);
                    }

                    if (addableAmount > 0)
                    {
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            Chats.sendProgressBar(dbPlayer, 100 * addableAmount);

                            dbPlayer.Container.RemoveItem(15, addableAmount / 5);

                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            await Task.Delay(100 * addableAmount);

                            if (dbPlayer == null || !dbPlayer.IsValid()) return;
                            dbPlayer.SetData("userCannotInterrupt", false);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                            dbPlayer.StopAnimation();
                            dbPlayer.Container.AddItem(966, addableAmount);

                            dbPlayer.SendNewNotification($"{addableAmount / 5} {ItemModelModule.Instance.Get(15).Name} wurde in {addableAmount} {ItemModelModule.Instance.Get(966).Name} zerlegt.");

                        }));
                        return true;
                    }
                }
            }

            if (dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryCannabisPulver) < 1.0f)
            {
                int Hanfsamenamount = dbPlayer.Container.GetItemAmount(979);
                int addableAmount = Hanfsamenamount / 2;
                // 979 -> 997
                if (Hanfsamenamount >= 2)
                {
                    if (addableAmount > dbPlayer.Container.GetMaxItemAddedAmount(979))
                    {
                        addableAmount = dbPlayer.Container.GetMaxItemAddedAmount(979);
                    }

                    if (addableAmount > 0)
                    {
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            Chats.sendProgressBar(dbPlayer, 500 * addableAmount);

                            dbPlayer.Container.RemoveItem(979, addableAmount * 2);

                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            await Task.Delay(500 * addableAmount);

                            dbPlayer.SetData("userCannotInterrupt", false);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                            dbPlayer.StopAnimation();
                            dbPlayer.Container.AddItem(997, addableAmount);

                            dbPlayer.SendNewNotification($"{addableAmount * 2} {ItemModelModule.Instance.Get(979).Name} wurde zu {addableAmount} {ItemModelModule.Instance.Get(997).Name} verarbeitet.");

                        }));
                        return true;
                    }
                }
            }
            return false;
        }

        public override void OnFifteenMinuteUpdate()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Random rnd = new Random();
                foreach (Cannabislaboratory Cannabislaboratory in GetAll().Values.ToList())
                {
                    if (Cannabislaboratory == null) continue;
                    if (Cannabislaboratory.LastAttacked.AddHours(LaboratoryModule.HoursDisablingAfterHackAttack) > DateTime.Now)
                    {
                        if (Cannabislaboratory.SkippedLast)
                        {
                            Cannabislaboratory.SkippedLast = false;
                        }
                        else
                        {   // Skipp all 2. intervall
                            Cannabislaboratory.SkippedLast = true;
                            continue;
                        }
                    }
                    uint fuelAmount = (uint)Cannabislaboratory.FuelContainer.GetItemAmount(FuelItemId);
                    foreach (DbPlayer dbPlayer in Cannabislaboratory.ActingPlayers.ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                        if (fuelAmount >= FuelAmountPerProcessing)
                        {
                            Cannabislaboratory.Processing(dbPlayer);
                        }
                        else
                            Cannabislaboratory.StopProcess(dbPlayer);
                    }
                    if (Cannabislaboratory.ActingPlayers.Count > 0)
                    {
                        Cannabislaboratory.FuelContainer.RemoveItem(FuelItemId, (int)FuelAmountPerProcessing);
                    }
                }
            }));
            return;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            if (TeamModule.Instance.IsWeedTeamId(dbPlayer.TeamId))
            {
                dbPlayer.CannabislaboratoryInputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.CANNABISLABORATORYINPUT);
                dbPlayer.CannabislaboratoryOutputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.CANNABISLABORATORYOUTPUT);
            }
        }
        public async Task HackCannabislaboratory(DbPlayer dbPlayer)
        {
            if (dbPlayer.DimensionType[0] != DimensionTypes.Cannabislaboratory) return;
            Cannabislaboratory Cannabislaboratory = this.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (Cannabislaboratory == null) return;
            await Cannabislaboratory.HackLaboratory(dbPlayer);
        }

        public bool CanCannabislaboratyRaided(Cannabislaboratory Cannabislaboratory, DbPlayer dbPlayer)
        {
            if (Configurations.Configuration.Instance.DevMode) return true;
            if (dbPlayer.IsACop() && dbPlayer.IsInDuty()) return true;
            if (GangwarTownModule.Instance.IsTeamInGangwar(TeamModule.Instance.Get(Cannabislaboratory.TeamId))) return false;
            if (TeamModule.Instance.Get(Cannabislaboratory.TeamId).Members.Count < 15 && !Cannabislaboratory.LaborMemberCheckedOnHack) return false;
            // Geht nicht wenn in Gangwar, weniger als 10 UND der Typ kein Cop im Dienst ist (macht halt kein sinn wenn die kochen können < 10 und mans nicht hochnehmen kann (cops))
            return true;
        }

        public Cannabislaboratory GetLaboratoryByDimension(uint dimension)
        {
            return CannabislaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == dimension).FirstOrDefault();
        }
        public Cannabislaboratory GetLaboratoryByPosition(Vector3 position)
        {
            return CannabislaboratoryModule.Instance.GetAll().Values.Where(Lab => position.DistanceTo(Lab.JumpPointEingang.Position) < 3.0f).FirstOrDefault();
        }
        public Cannabislaboratory GetLaboratoryByJumppointId(int id)
        {
            return CannabislaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.JumpPointEingang.Id == id).FirstOrDefault();
        }
        public Cannabislaboratory GetLaboratoryByTeamId(uint teamId)
        {
            return CannabislaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == teamId).FirstOrDefault();
        }
    }
}
