using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
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
    class MethlaboratoryModule : SqlModule<MethlaboratoryModule, Methlaboratory, uint>
    {
        public List<ColShape> InteriorColShapes = new List<ColShape>();
        public static List<uint> RessourceItemIds = new List<uint> { 14, 966, 965 }; //Toilettenreiniger, Batterien, Ephedrinkonzentrat (965) 
        public static List<uint> EndProductItemIds = new List<uint> { 726, 727, 728, 729 }; //Pures Meth
        public static uint FuelItemId = 537; //Benzin
        public static uint FuelAmountPerProcessing = 5; //Fuelverbrauch pro 15-Minuten-Kochvorgang (Spielerunabhängig)
        
        public static int temp = 0;
        public static uint RankNeededForParameter = 9;
        public string PlayerIds = "";
        public int AmountPerProcess = 0;
        public List<Team> HasAlreadyHacked = new List<Team>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_methlaboratories` WHERE 1=2"; ////Random in Query rein    METHLAB DISABLED
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
                Methlaboratory weaponlaboratory = Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);

                if (weaponlaboratory != null)
                {
                    if (weaponlaboratory.ProzessingPlayers.Contains(dbPlayer)) weaponlaboratory.ProzessingPlayers.Remove(dbPlayer);

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
            if (!Configurations.Configuration.Instance.MethLabEnabled) return false;
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            if (dbPlayer.DimensionType[0] != DimensionType.Methlaboratory) return false;
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            Methlaboratory methlaboratory = this.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (methlaboratory == null) return false;
            if (key == Key.E)
                return KeyEPressed(dbPlayer, methlaboratory);

            return false;
        }
        private bool KeyEPressed(DbPlayer dbPlayer, Methlaboratory methlaboratory)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            if (dbPlayer.Player.Position.DistanceTo(methlaboratory.StartPosition) < 1.0f)
                return this.StartMenu(dbPlayer, methlaboratory);

            if (dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryInvUpgradePosition) < 1.0f)
            {
                if (methlaboratory.Hacked)
                {
                    MenuManager.Instance.Build(PlayerMenu.LaboratoryOpenInvMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }

            if(dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryBatterieSwitch) < 1.0f)
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

                            dbPlayer.Container.RemoveItem(15, addableAmount/5);

                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            await Task.Delay(100 * addableAmount);

                            dbPlayer.SetData("userCannotInterrupt", false);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                            dbPlayer.StopAnimation();
                            dbPlayer.Container.AddItem(966, addableAmount);

                            dbPlayer.SendNewNotification($"{addableAmount/5} {ItemModelModule.Instance.Get(15).Name} wurde in {addableAmount} {ItemModelModule.Instance.Get(966).Name} zerlegt.");

                        }));
                        return true;
                    }
                }
            }


            if (dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryCheckBoilerQuality) < 1.0f)
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

                    dbPlayer.SetData("userCannotInterrupt", false);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                    dbPlayer.StopAnimation();

                    dbPlayer.SendNewNotification($"Die Qualität wird vorraussichtlich {methlaboratory.Quality}% betragen.");

                }));
                return true;
            }

            if (dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryEphePulver) < 1.0f)
            {
                int EphikonzenAmount = dbPlayer.Container.GetItemAmount(725);
                int addableAmount = EphikonzenAmount / 2;
                // 725 -> 965
                if (EphikonzenAmount >= 2)
                {
                    if(addableAmount > dbPlayer.Container.GetMaxItemAddedAmount(965))
                    {
                        addableAmount = dbPlayer.Container.GetMaxItemAddedAmount(965);
                    }

                    if (addableAmount > 0)
                    {
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            Chats.sendProgressBar(dbPlayer, 500*addableAmount);

                            dbPlayer.Container.RemoveItem(725, addableAmount*2);

                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);
                            
                            await Task.Delay(500 * addableAmount);

                            dbPlayer.SetData("userCannotInterrupt", false);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            
                            dbPlayer.StopAnimation();
                            dbPlayer.Container.AddItem(965, addableAmount);

                            dbPlayer.SendNewNotification($"{addableAmount * 2} {ItemModelModule.Instance.Get(725).Name} wurde zu {addableAmount} {ItemModelModule.Instance.Get(965).Name} verarbeitet.");

                        }));
                        return true;
                    }
                }
            }
            return false;
        }

        private bool StartMenu(DbPlayer dbPlayer, Methlaboratory methlaboratory)
        {
            if (methlaboratory.TeamId != dbPlayer.TeamId) return false;
            //MenuManager.Instance.Build(PlayerMenu.MethlaboratoryStartMenu, dbPlayer).Show(dbPlayer);
            ComponentManager.Get<MethlaboratoryStartWindow>().Show()(dbPlayer, methlaboratory);
            return true;
        }
        
        public override void OnFifteenMinuteUpdate()
        {
            if (!Configurations.Configuration.Instance.MethLabEnabled) return;
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Random rnd = new Random();
                string query = "";
                foreach (Methlaboratory methlaboratory in MethlaboratoryModule.Instance.GetAll().Values)
                {
                    if (methlaboratory == null) continue;
                    if (methlaboratory.LastAttacked.AddHours(LaboratoryModule.HoursDisablingAfterHackAttack) > DateTime.Now)
                    {
                        if (methlaboratory.SkippedLast)
                        {
                            methlaboratory.SkippedLast = false;
                        }
                        else
                        {   // Skipp all 2. intervall
                            methlaboratory.SkippedLast = true;
                            continue;
                        }
                    }

                    PlayerIds = "";
                    AmountPerProcess = 0;
                    uint fuelAmount = (uint)methlaboratory.FuelContainer.GetItemAmount(FuelItemId);
                    foreach (DbPlayer dbPlayer in methlaboratory.ProzessingPlayers.ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                        if (fuelAmount >= FuelAmountPerProcessing)
                        {
                            methlaboratory.Processing(dbPlayer);
                        }
                        else
                            methlaboratory.StopProcess(dbPlayer);
                    }
                    if (methlaboratory.ProzessingPlayers.Count > 0)
                    {
                        methlaboratory.FuelContainer.RemoveItem(FuelItemId, (int)FuelAmountPerProcessing);
                        string qualityString = methlaboratory.Quality.ToString();
                        qualityString = qualityString.Replace(",", ".");
                        if(PlayerIds.Length >= 3)
                            query += $"INSERT INTO `log_methlaboratory` (`team_id`, `player_ids`, `quality`, `amount`, `temperatur`, `druck`, `ruehrgeschwindigkeit`, `menge`) VALUES ('{methlaboratory.TeamId}', '{PlayerIds.Substring(0, PlayerIds.Length - 2)}', '{qualityString}', '{AmountPerProcess}', '{methlaboratory.Parameters[0].ActValue}', '{methlaboratory.Parameters[1].ActValue}', '{methlaboratory.Parameters[2].ActValue}', '{methlaboratory.Parameters[3].ActValue}');";
                    }
                }
                if (!query.Equals("")) MySQLHandler.ExecuteAsync(query);
            }));
        }
        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!Configurations.Configuration.Instance.MethLabEnabled) return false;
            if (!colShape.HasData("methInteriorColshape")) return false;
            if (colShapeState == ColShapeState.Enter)
            {
                if (dbPlayer.HasData("inMethLaboraty"))
                {
                    Methlaboratory methlaboratory = GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                    if (methlaboratory == null) return false;
                    methlaboratory.LoadInterior(dbPlayer);
                    return true;
                }
            }
            if (colShapeState == ColShapeState.Exit)
            {
                if (dbPlayer.HasData("inMethLaboraty"))
                {
                    Methlaboratory methlaboratory = GetLaboratoryByDimension(colShape.Dimension);
                    if (methlaboratory == null)
                    {
                        return false;
                    }
                    methlaboratory.UnloadInterior(dbPlayer);
                    return true;
                }
            }
            return false;
        }
        public async Task HackMethlaboratory(DbPlayer dbPlayer)
        {
            if (!Configurations.Configuration.Instance.MethLabEnabled) return;
            if (dbPlayer.DimensionType[0] != DimensionType.Methlaboratory) return;
            Methlaboratory methlaboratory = this.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (methlaboratory == null) return;
            await methlaboratory.HackMethlaboratory(dbPlayer);
        }
        
        public bool CanMethLaboratyRaided(Methlaboratory methlaboratory, DbPlayer dbPlayer)
        {
            if (!Configurations.Configuration.Instance.MethLabEnabled) return false;
            //if (Configurations.Configuration.Instance.DevMode) return true;
            if (dbPlayer.IsACop() && dbPlayer.IsInDuty()) return true;
            if (GangwarTownModule.Instance.IsTeamInGangwar(TeamModule.Instance.Get(methlaboratory.TeamId))) return false;
            if (Configurations.Configuration.Instance.DevMode)
            {
                if (TeamModule.Instance.Get(methlaboratory.TeamId).Members.Count < 3 && !methlaboratory.LaborMemberCheckedOnHack) return false;
            }
            else
            {
                if (TeamModule.Instance.Get(methlaboratory.TeamId).Members.Count < 15 && !methlaboratory.LaborMemberCheckedOnHack) return false;
            }
            // Geht nicht wenn in Gangwar, weniger als 10 UND der Typ kein Cop im Dienst ist (macht halt kein sinn wenn die kochen können < 10 und mans nicht hochnehmen kann (cops))
            return true;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            if (TeamModule.Instance.IsMethTeamId(dbPlayer.TeamId))
            {
                dbPlayer.MethlaboratoryOutputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.METHLABORATORYOUTPUT);
                dbPlayer.MethlaboratoryInputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.METHLABORATORYINPUT);
            }
        }

        public Methlaboratory GetLaboratoryByDimension(uint dimension)
        {
            return MethlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == dimension).FirstOrDefault();
        }
        public Methlaboratory GetLaboratoryByPosition(Vector3 position)
        {
            return MethlaboratoryModule.Instance.GetAll().Values.Where(Lab => position.DistanceTo(Lab.JumpPointEingang.Position) < 3.0f).FirstOrDefault();
        }
        public Methlaboratory GetLaboratoryByJumppointId(int id)
        {
            return MethlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.JumpPointEingang.Id == id).FirstOrDefault();
        }
        public Methlaboratory GetLaboratoryByTeamId(uint teamId)
        {
            return MethlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == teamId).FirstOrDefault();
        }
    }
}
