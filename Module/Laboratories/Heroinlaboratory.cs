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
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Laboratories
{
    public class Heroinlaboratory : Loadable<uint>
    {
        public uint Id { get; }

        public uint TeamId { get; }

        public uint DestinationId { get; }
        public JumpPoint JumpPointEingang { get; set; }
        public JumpPoint JumpPointAusgang { get; set; }

        public List<DbPlayer> ActingPlayers { get; set; }

        public Container FuelContainer { get; set; }
        public List<Marker> Marker { get; set; }

        public List<DbPlayer> PlayersInsideLaboratory = new List<DbPlayer>();

        public bool HackInProgess = false;
        public bool Hacked = false;
        public bool FriskInProgess = false;
        public bool ImpoundInProgress = false;
        public bool LaborMemberCheckedOnHack = false;

        public DateTime LastAttacked { get; set; }

        public List<uint> LoggedOutCombatAvoid = new List<uint>();
        public bool SkippedLast { get; set; }

        public bool HasDefended { get; set; }

        public uint heroinampulleItemId = 1442;
        public uint toilettenreinigerItemId = 14;
        public uint heroinkisteItemId = 1443;

        private int heroinPerTick = 50;
        private int toilettenreinigerPerTick = 10;

        public Heroinlaboratory(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            TeamId = reader.GetUInt32("teamid");
            DestinationId = reader.GetUInt32("destination_id");
            ActingPlayers = new List<DbPlayer>();
            FuelContainer = ContainerManager.LoadContainer(Id, ContainerTypes.HEROINLABORATORYFUEL);
            HackInProgess = false;
            Hacked = false;
            FriskInProgess = false;
            ImpoundInProgress = false;

            HasDefended = false;

            SkippedLast = false;

            LoggedOutCombatAvoid = new List<uint>();

            LastAttacked = reader.GetDateTime("last_attacked");

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
        public override uint GetIdentifier()
        {
            return Id;
        }

        public void Processing(DbPlayer dbPlayer)
        {
            if (!ActingPlayers.ToList().Contains(dbPlayer)) return;

            if (!PlayerHasRessourcesForTick(dbPlayer) ||
                !dbPlayer.HeroinlaboratoryOutputContainer.CanInventoryItemAdded(heroinkisteItemId))
            {
                StopProcess(dbPlayer);
                return;
            }

            dbPlayer.HeroinlaboratoryInputContainer.RemoveItem(heroinampulleItemId, heroinPerTick);
            dbPlayer.HeroinlaboratoryInputContainer.RemoveItem(toilettenreinigerItemId, toilettenreinigerPerTick);

            dbPlayer.HeroinlaboratoryOutputContainer.AddItem(heroinkisteItemId, 1);
        }

        public void StopProcess(DbPlayer dbPlayer)
        {
            if (ActingPlayers.ToList().Contains(dbPlayer))
            {
                ActingPlayers.Remove(dbPlayer);
                if (dbPlayer.DimensionType[0] == DimensionType.Heroinlaboratory && dbPlayer.Player.Dimension != 0)
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

            uint fuelAmount = (uint)FuelContainer.GetItemAmount(HeroinlaboratoryModule.FuelItemId);
            if (fuelAmount < HeroinlaboratoryModule.FuelAmountPerProcessing)
            {
                dbPlayer.SendNewNotification("Es fehlt Kraftstoff...");
                StopProcess(dbPlayer);
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

        public async Task<bool> HackLaboratory(DbPlayer dbPlayer)
        {
            if (HeroinlaboratoryModule.Instance.HasAlreadyHacked.Contains(dbPlayer.Team))
            {
                dbPlayer.SendNewNotification("Deine Fraktion hat bereits ein Labor gehackt...");
                return false;
            }

            if (HackInProgess)
            {
                dbPlayer.SendNewNotification("Das Labor wird bereits gehackt...");
                return false;
            }
            if (Hacked)
            {
                if (dbPlayer.TeamId != this.TeamId)
                {
                    dbPlayer.SendNewNotification("Das Labor ist bereits gehackt worden...");
                    return false;
                }
            }
            if (!HeroinlaboratoryModule.Instance.CanHeroinlaboratyRaided(this, dbPlayer))
            {
                dbPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                return false;
            }
            LaborMemberCheckedOnHack = true;
            int timeToHack = LaboratoryModule.TimeToHack;

            TeamModule.Instance.Get(this.TeamId).SendNotification("Das Labor wird gehackt...", 30000);
            timeToHack = timeToHack * 3;

            dbPlayer.SendNewNotification("Labor wird gehackt...");
            Chats.sendProgressBar(dbPlayer, timeToHack);
            dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@prison_heistig1_P1_guard_checks_bus", "loop");
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            await Task.Delay(timeToHack);
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
            {
                HackInProgess = false;
                return false;
            }
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();

            dbPlayer.SendNewNotification("Labor erfolgreich gehackt...");

            if (ActingPlayers.Count() > 0) ActingPlayers.Clear();

            if (dbPlayer.IsAGangster())
            {
                HeroinlaboratoryModule.Instance.HasAlreadyHacked.Add(dbPlayer.Team);
            }

            Hacked = true;
            HackInProgess = false;
            LaborMemberCheckedOnHack = false;
            return true;

        }

        public void SaveLastHacked()
        {
            MySQLHandler.ExecuteAsync($"UPDATE team_heroinlaboratories SET last_attacked = '{LastAttacked.Date.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE id = '{Id}'");
        }

        public async Task<bool> FriskLaboratory(DbPlayer dbPlayer)
        {
            if (dbPlayer.TeamId == 0)
            {
                return false;
            }
            if (!Hacked)
            {
                dbPlayer.SendNewNotification("Das Labor muss zuerst gehackt werden...");
                return false;
            }
            if (FriskInProgess)
            {
                dbPlayer.SendNewNotification("Das Labor wird schon durchsucht...");
                return false;
            }
            CheckDefending();
            FriskInProgess = true;

            Dictionary<int, int> itemsFound = new Dictionary<int, int>();
            foreach (int itemId in HeroinlaboratoryModule.RessourceItemIds)
            {
                if (itemsFound.ContainsKey(itemId)) continue;
                itemsFound.Add(itemId, 0);
            }
            foreach (int itemId in HeroinlaboratoryModule.EndProductItemIds)
            {
                if (itemsFound.ContainsKey(itemId)) continue;
                itemsFound.Add(itemId, 0);
            }

            bool found = false;
            int timeToFrisk = LaboratoryModule.TimeToFrisk;
            timeToFrisk = timeToFrisk * 3;

            TeamModule.Instance.Get(this.TeamId).SendNotification("Das Labor wird durchsucht...", 60000);

            Chats.sendProgressBar(dbPlayer, timeToFrisk);
            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            await Task.Delay(timeToFrisk);
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
            {
                FriskInProgess = false;
                return false;
            }
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();
            foreach (KeyValuePair<uint, DbPlayer> kvp in TeamModule.Instance.GetById((int)TeamId).Members)
            {
                if (kvp.Value == null || !kvp.Value.IsValid() || kvp.Value.HeroinlaboratoryInputContainer == null) continue;
                if (!kvp.Value.HeroinlaboratoryInputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.HeroinlaboratoryInputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            itemsFound[(int)kvpSlots.Value.Model.Id] += kvpSlots.Value.Amount;
                            found = true;
                        }
                    }
                }
                if (!kvp.Value.HeroinlaboratoryOutputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.HeroinlaboratoryOutputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            itemsFound[(int)kvpSlots.Value.Model.Id] += kvpSlots.Value.Amount;
                            found = true;
                        }
                    }
                }
            }
            if (found)
            {
                string info = "Funde: ";
                foreach (KeyValuePair<int, int> kvp in itemsFound)
                    info += kvp.Value + " " + ItemModelModule.Instance.GetById((uint)kvp.Key).Name + ", ";
                info = info.Substring(0, info.Length - 1);
                dbPlayer.SendNewNotification(info);
            }
            else
            {
                dbPlayer.SendNewNotification("Nichts gefunden");
            }
            FriskInProgess = false;
            return false;
        }

        public async Task<bool> ImpoundLaboratory(DbPlayer dbPlayer)
        {
            if (dbPlayer.TeamId == 0)
            {
                return false;
            }
            if (!Hacked)
            {
                dbPlayer.SendNewNotification("Das Labor muss zuerst gehackt werden...");
                return false;
            }
            if (ImpoundInProgress)
            {
                dbPlayer.SendNewNotification("Die Laborinhalte werden schon beschlagnahmt...");
                return false;
            }
            CheckDefending();
            ImpoundInProgress = true;

            Dictionary<int, int> itemsImpounded = new Dictionary<int, int>();
            foreach (int itemId in HeroinlaboratoryModule.RessourceItemIds)
            {
                if (itemsImpounded.ContainsKey(itemId)) continue;
                itemsImpounded.Add(itemId, 0);
            }
            foreach (int itemId in HeroinlaboratoryModule.EndProductItemIds)
            {
                if (itemsImpounded.ContainsKey(itemId)) continue;
                itemsImpounded.Add(itemId, 0);
            }

            bool impounded = false;
            SxVehicle closestVeh = VehicleHandler.Instance.GetClosestVehicleFromTeam(JumpPointEingang.Position, (int)dbPlayer.TeamId, 15.0f);
            if (closestVeh == null)
            {
                dbPlayer.SendNewNotification("Du benötigst ein passendes Fraktionsfahrzeug vor dem Tor...");
                ImpoundInProgress = false;
                return false;
            }
            if (!LaboratoryModule.Instance.IsImpoundVehicle(closestVeh)
                    && closestVeh.teamid == dbPlayer.TeamId &&
                    !closestVeh.TrunkStateOpen && closestVeh.Container != null)
            {
                dbPlayer.SendNewNotification("Du benötigst ein Lagerfahrzeug (Burrito, Brickade, ...) mit offenem Kofferraum.");
                ImpoundInProgress = false;
                return false;
            }
            int timeToImpound = LaboratoryModule.TimeToImpound;

            TeamModule.Instance.Get(this.TeamId).SendNotification("Die Inhalte des Labors werden entwendet...", 60000);
            timeToImpound = timeToImpound * 3;

            Chats.sendProgressBar(dbPlayer, timeToImpound);
            dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            await Task.Delay(timeToImpound);
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
            {
                ImpoundInProgress = false;
                return false;
            }
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();
            closestVeh = VehicleHandler.Instance.GetClosestVehicleFromTeam(JumpPointEingang.Position, (int)dbPlayer.TeamId, 15.0f);
            if (closestVeh == null)
            {
                dbPlayer.SendNewNotification("Du benötigst ein passendes Fraktionsfahrzeug vor dem Tor...");
                ImpoundInProgress = false;
                return false;
            }
            if (!(LaboratoryModule.Instance.IsImpoundVehicle(closestVeh))
                    && closestVeh.teamid == dbPlayer.TeamId &&
                    (!closestVeh.TrunkStateOpen) && closestVeh.Container != null)
            {
                dbPlayer.SendNewNotification("Du benötigst ein Lagerfahrzeug (Burrito, Brickade, ...) mit offenem Kofferraum.");
                ImpoundInProgress = false;
                return false;
            }
            foreach (KeyValuePair<uint, DbPlayer> kvp in TeamModule.Instance.GetById((int)TeamId).Members)
            {
                if (kvp.Value == null || !kvp.Value.IsValid()) continue;
                if (LoggedOutCombatAvoid.ToList().Contains(kvp.Value.Id)) LoggedOutCombatAvoid.Remove(kvp.Value.Id);

                if (!kvp.Value.HeroinlaboratoryInputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.HeroinlaboratoryInputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            if (closestVeh.Container.CanInventoryItemAdded(kvpSlots.Value.Id, kvpSlots.Value.Amount) || (dbPlayer.IsACop() && dbPlayer.IsInDuty()))
                            {
                                if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())
                                    closestVeh.Container.AddItem(kvpSlots.Value.Model.Id, kvpSlots.Value.Amount);
                                itemsImpounded[(int)kvpSlots.Value.Model.Id] = itemsImpounded[(int)kvpSlots.Value.Model.Id] + kvpSlots.Value.Amount;
                                kvp.Value.HeroinlaboratoryInputContainer.RemoveFromSlot(kvpSlots.Key, kvpSlots.Value.Amount);
                                impounded = true;
                            }
                        }
                    }
                }
                if (!kvp.Value.HeroinlaboratoryOutputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.HeroinlaboratoryOutputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            if (closestVeh.Container.CanInventoryItemAdded(kvpSlots.Value.Id, kvpSlots.Value.Amount))
                            {
                                if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())
                                    closestVeh.Container.AddItem(kvpSlots.Value.Id, kvpSlots.Value.Amount, kvpSlots.Value.Data);
                                itemsImpounded[(int)kvpSlots.Value.Id] = itemsImpounded[(int)kvpSlots.Value.Id] + kvpSlots.Value.Amount;
                                kvp.Value.HeroinlaboratoryOutputContainer.RemoveFromSlot(kvpSlots.Key, kvpSlots.Value.Amount);
                                impounded = true;
                            }
                        }
                    }
                }
            }


            foreach (uint PlayerId in LoggedOutCombatAvoid)
            {
                ContainerManager.ClearOfflineInvDBOnly(ContainerTypes.HEROINLABORATORYINPUT, PlayerId);
                ContainerManager.ClearOfflineInvDBOnly(ContainerTypes.HEROINLABORATORYOUTPUT, PlayerId);
            }

            LoggedOutCombatAvoid.Clear();

            if (impounded == true)
            {
                string info = "Waren: ";
                foreach (KeyValuePair<int, int> kvp in itemsImpounded)
                {
                    info += kvp.Value + " " + ItemModelModule.Instance.GetById((uint)kvp.Key).Name + ", ";
                }
                info = info.Substring(0, info.Length - 1);
                dbPlayer.SendNewNotification(info);

                CheckDefending();

                if (!HasDefended)
                {
                    LastAttacked = DateTime.Now;
                    SaveLastHacked();
                }
            }
            else
            {
                dbPlayer.SendNewNotification("Nichts gefunden oder kein Platz im Fahrzeug");
            }
            ImpoundInProgress = false;
            return false;
        }
        public void CheckDefending()
        {
            if (HasDefended) return;
            int count = 0;
            foreach (DbPlayer defender in TeamModule.Instance.Get(TeamId).Members.Values.ToList())
            {
                if (defender == null || !defender.IsValid()) continue;
                if (defender.Player.Position.DistanceTo(JumpPointEingang.Position) < 150 || (defender.Dimension[0] == TeamId && defender.DimensionType[0] == DimensionType.Labor))
                {
                    count++;
                }
            }
            if (count >= 5) HasDefended = true;
            return;
        }
        public void LoadInterior(DbPlayer dbPlayer)
        {
            int boilerState = 2;
            int tableState = 1;
            int securityState = 1;

            dbPlayer.Player.TriggerNewClient("loadMethInterior", tableState, boilerState, securityState);

            if (!PlayersInsideLaboratory.Contains(dbPlayer))
            {
                PlayersInsideLaboratory.Add(dbPlayer);
            }
        }

        public void UnloadInterior(DbPlayer dbPlayer)
        {
            if (PlayersInsideLaboratory.Contains(dbPlayer))
            {
                PlayersInsideLaboratory.Remove(dbPlayer);
            }
        }

        private bool PlayerHasRessourcesForTick(DbPlayer dbPlayer)
        {
            if (dbPlayer.HeroinlaboratoryInputContainer.GetItemAmount(heroinampulleItemId) >= heroinPerTick && dbPlayer.HeroinlaboratoryInputContainer.GetItemAmount(toilettenreinigerItemId) >= toilettenreinigerPerTick)
            {
                return true;
            }
            return false;
        }
    }
}
