using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.FIB;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Storage;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.InteriorVehicles;
using VMP_CNR.Module.Voice;

namespace VMP_CNR.Module.NSA
{
    public enum TransactionType
    {
        MONEY = 1,
        ENERGY = 2,
    }

    public class TransactionHistoryObject
    {
        public string Description { get; set; }
        public Vector3 Position { get; set; }
        public DateTime Added { get; set; }

        public TransactionType TransactionType { get; set; }
    }

    public class NSAModule : Module<NSAModule>
    {
        public List<DbPlayer> NSAMember         = new List<DbPlayer>();
        public static Vector3 NSACloningPoint   = new Vector3(414.284, 4811.11, -58.9979);

        public override void OnPlayerSpawn(DbPlayer dbPlayer)
        {
            if (!NSAMember.ToList().Contains(dbPlayer) && dbPlayer.IsNSAState >= (int)NSARangs.LIGHT)
                NSAMember.Add(dbPlayer);
        }

        public static Vector3 NSAVehicleModifyPosition = new Vector3(204.332f, -995.061f, -99);
        public static List<TransactionHistoryObject> TransactionHistory = new List<TransactionHistoryObject>();

        protected override bool OnLoad()
        {
            TransactionHistory = new List<TransactionHistoryObject>();
            
            MenuManager.Instance.AddBuilder(new NSAVehicleListMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAVehicleModifyMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAComputerMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAListCallsMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAObservationsListMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAObservationsSubMenuMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSABankMenu());
            MenuManager.Instance.AddBuilder(new NSAPeilsenderMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSATransactionHistoryMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAEnergyHistoryMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSADoorUsedsMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAVehicleObersvationListMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAWanzenMenuBuilder());

            Spawners.ColShapes.Create(NSACloningPoint, 3.0f, 0).SetData("cloning", true);

            return base.OnLoad();
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if(colShape.HasData("cloning") && dbPlayer.IsNSADuty && dbPlayer.IsNSAState >= (int)NSA.NSARangs.LEAD)
            {

                if(colShapeState == ColShapeState.Enter)
                {
                    dbPlayer.SetData("nsacloning", true);
                    return true;
                }
                else
                {
                    if (dbPlayer.HasData("nsacloning"))
                    {
                        dbPlayer.ResetData("nsacloning");
                        if (dbPlayer.HasData("clonePerson"))
                        {
                            dbPlayer.ResetData("clonePerson");
                            dbPlayer.SendNewNotification("Cloning beendet!");
                            dbPlayer.ApplyCharacter();
                            return true;
                        }
                    }
                }

            }

            return false;
        }

        public override void OnTenSecUpdate()
        {
            // Aktiv-Ortung für normalo FIB Agenten, falls Lizenz vorhanden
            foreach (DbPlayer member in TeamModule.Instance.Get((uint)teams.TEAM_FIB).GetTeamMembers())
            {
                if (member.HasData("nsaOrtung") == false)
                    continue;

                if (member.FindFlags.HasFlag(FindFlags.Continuous))
                {
                    DbPlayer target = Players.Players.Instance.FindPlayerById(member.GetData("nsaOrtung"));
                    if (target == null || !target.IsValid() || !target.IsOrtable(member, true))
                    {
                        member.ResetData("nsaOrtung");
                        continue;
                    }
                    HandleContinuousFind(member, target);
                }
            }

            foreach(DbPlayer member in NSAMember.ToList())
            {
                if(member.HasData("nsaOrtung"))
                {
                    DbPlayer targetOne = Players.Players.Instance.FindPlayerById(member.GetData("nsaOrtung"));
                    if (targetOne == null || !targetOne.IsValid() || !targetOne.IsOrtable(member, true))
                    {
                        member.ResetData("nsaOrtung");
                        continue;
                    }

                    member.Player.TriggerNewClient("setPlayerGpsMarker", targetOne.Player.Position.X, targetOne.Player.Position.Y);
                }

                if (member.HasData("nsaPeilsenderOrtung"))
                {
                    uint vehicleId = member.GetData("nsaPeilsenderOrtung");

                    if (vehicleId != 0)
                    {
                        SxVehicle sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(vehicleId);
                        if (sxVeh == null || !sxVeh.IsValid())
                        {
                            member.ResetData("nsaPeilsenderOrtung");
                            continue;
                        }

                        // Orten
                        member.Player.TriggerNewClient("setPlayerGpsMarker", sxVeh.entity.Position.X, sxVeh.entity.Position.Y);
                    }
                }
            }
        }

        public void StopNASCAll(int phoneNumber, int phoneNumber2 = 0)
        {
            foreach (DbPlayer xPlayer in TeamModule.Instance.Get((uint)teams.TEAM_FIB).Members.Values.ToList())
            {
                if (xPlayer == null || !xPlayer.IsValid()) continue;
                if (xPlayer.HasData("nsa_activePhone"))
                {
                    if (xPlayer.GetData("nsa_activePhone") == phoneNumber || xPlayer.GetData("nsa_activePhone") == phoneNumber2)
                    {
                        xPlayer.ResetData("nsa_activePhone");
                        xPlayer.Player.TriggerNewClient("setCallingPlayer", "");
                        xPlayer.SendNewNotification("Anruf wurde beendet!");
                    }
                }
            }
        }

        public override void OnPlayerExitVehicle(DbPlayer dbPlayer, Vehicle vehicle)
        {
            if (dbPlayer.IsNSADuty)
            {
                NSAWanze nsaWanze = NSAObservationModule.NSAWanzen.ToList().Where(w => w.HearingAgents.Contains(dbPlayer)).FirstOrDefault();

                if (nsaWanze != null)
                {
                    dbPlayer.SendNewNotification("Wanze nicht mehr erreichbar!..");
                    nsaWanze.HearingAgents.Remove(dbPlayer);
                }
            }
        }

        public override void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
            if (dbPlayer != null && dbPlayer.IsValid() && dbPlayer.IsNSADuty)
            {
                NSAWanze nsaWanze = NSAObservationModule.NSAWanzen.ToList().Where(w => w.HearingAgents.Contains(dbPlayer)).FirstOrDefault();

                if (nsaWanze != null)
                {
                    dbPlayer.SendNewNotification("Wanze nicht mehr erreichbar!..");
                    nsaWanze.HearingAgents.Remove(dbPlayer);
                }
            }
        }

        public void SendMessageToNSALead(string Message)
        {
            foreach(DbPlayer nsa in NSAMember.ToList().Where(t => t.IsNSAState == (int)NSARangs.LEAD))
            {
                nsa.SendNewNotification(Message, PlayerNotification.NotificationType.FRAKTION, "[IT - Observation]");
            }
        }
        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (dbPlayer == null) return;

            if(NSAMember.ToList().Contains(dbPlayer))
            {
                NSAMember.Remove(dbPlayer);
            }

            NSAWanze nsaWanze = NSAObservationModule.NSAWanzen.ToList().Where(w => w.PlayerId == dbPlayer.Id).FirstOrDefault();

            if(nsaWanze != null)
            {
                foreach(DbPlayer agent in nsaWanze.HearingAgents.ToList())
                {
                    agent.SendNewNotification("Wanze nicht mehr erreichbar!..");
                }
            }

            nsaWanze = NSAObservationModule.NSAWanzen.ToList().Where(w => w.HearingAgents.Contains(dbPlayer)).FirstOrDefault();
            if (nsaWanze != null)
            {
                nsaWanze.HearingAgents.Remove(dbPlayer);
            }
        }

        public override void OnFiveSecUpdate()
        {
            foreach (NSAWanze nSAWanze in NSAObservationModule.NSAWanzen.ToList().Where(w => w.active))
            {
                if (nSAWanze.HearingAgents.Count == 0) continue;

                DbPlayer targetPlayer = Players.Players.Instance.GetByDbId(nSAWanze.PlayerId);
                if (targetPlayer == null || !targetPlayer.IsValid()) continue;


                List<DbPlayer> playersInRange = Players.Players.Instance.GetPlayersListInRange(targetPlayer.Player.Position, 7.0f);

                foreach (DbPlayer agent in nSAWanze.HearingAgents.ToList())
                {
                    if (agent == null || !agent.IsValid()) continue;

                    // Out of range
                    if (agent.Player.Position.DistanceTo(targetPlayer.Player.Position) > 400)
                    {
                        agent.Player.TriggerNewClient("setCallingPlayer", "");
                        agent.SendNewNotification("Kein aktiver Empfang...");
                        continue;
                    }

                    string voiceHashPush = "";

                    foreach (DbPlayer playerInRange in playersInRange)
                    {
                        if (playerInRange == null || !playerInRange.IsValid() || playerInRange == targetPlayer) continue;
                        voiceHashPush += playerInRange.VoiceHash + "~3~0~0~2;";
                    }

                    voiceHashPush += targetPlayer.VoiceHash;
                    agent.Player.TriggerNewClient("setCallingPlayer", voiceHashPush);
                }
            }
        }

        public override void OnFiveMinuteUpdate()
        {
            foreach(NSAWanze nSAWanze in NSAObservationModule.NSAWanzen.ToList().Where(w => w.active))
            {
                if(nSAWanze.Added.AddMinutes(45) <= DateTime.Now)
                {
                    foreach (DbPlayer agent in nSAWanze.HearingAgents.ToList())
                    {
                        agent.SendNewNotification("Wanze nicht mehr erreichbar!..");
                    }

                    nSAWanze.active = false;
                }
            }
        }

        public void HandleFind(DbPlayer dbPlayer, DbPlayer playerFromPool)
        {
            Player player = dbPlayer.Player;
            switch (playerFromPool.DimensionType[0])
            {
                case DimensionType.World:
                    player.TriggerNewClient("setPlayerGpsMarker", playerFromPool.Player.Position.X,
                        playerFromPool.Player.Position.Y);
                    break;
                case DimensionType.House:
                    if (!playerFromPool.HasData("inHouse")) return;
                    House house = HouseModule.Instance.Get(playerFromPool.GetData("inHouse"));
                    if (house == null || house.Position == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;
                case DimensionType.MoneyKeller:
                case DimensionType.Basement:
                case DimensionType.Labor:
                    house = HouseModule.Instance.Get(playerFromPool.Player.Dimension);
                    if (house == null || house.Position == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;
                case DimensionType.Camper:
                    var vehicle =
                        VehicleHandler.Instance.GetByVehicleDatabaseId(playerFromPool.Player.Dimension);
                    if (vehicle == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", vehicle.entity.Position.X, vehicle.entity.Position.Y);
                    break;
                case DimensionType.Business:
                    dbPlayer.SendNewNotification("Gesuchte Person " + playerFromPool.GetName() + " befindet sich im BusinessTower!");
                    break;
                case DimensionType.Storage:
                    if(playerFromPool.HasData("storageRoomId"))
                    {
                        StorageRoom storageRoom = StorageRoomModule.Instance.Get((uint)playerFromPool.GetData("storageRoomId"));
                        if(storageRoom != null) player.TriggerNewClient("setPlayerGpsMarker", storageRoom.Position.X, storageRoom.Position.Y);
                    }
                    break;
                case DimensionType.WeaponFactory:
                    break;
                default:
                    Logger.Crash(new ArgumentOutOfRangeException());
                    break;
            }

            playerFromPool.SetData("isOrted_" + dbPlayer.TeamId, DateTime.Now.AddMinutes(1));

            if (!dbPlayer.IsNSADuty)
            {
                dbPlayer.SendNewNotification("Gesuchte Person " + playerFromPool.GetName() + " wurde geortet!");
                dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat die Person {playerFromPool.GetName()} geortet!", 5000, 10);
            }
            return;
        }

        public void HandleContinuousFind(DbPlayer dbPlayer, DbPlayer playerFromPool) {
            Player player = dbPlayer.Player;
            switch (playerFromPool.DimensionType[0]) {
                case DimensionType.World:
                    player.TriggerNewClient("setPlayerGpsMarker", playerFromPool.Player.Position.X, playerFromPool.Player.Position.Y);
                    break;

                case DimensionType.House:
                    if (!playerFromPool.HasData("inHouse")) return;
                    House house = HouseModule.Instance.Get(playerFromPool.GetData("inHouse"));
                    if (house == null || house.Position == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;

                case DimensionType.MoneyKeller:
                case DimensionType.Basement:
                case DimensionType.Labor:
                    house = HouseModule.Instance.Get(playerFromPool.Player.Dimension);
                    if (house == null || house.Position == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;

                case DimensionType.Camper:
                    SxVehicle vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(playerFromPool.Player.Dimension);
                    if (vehicle == null) return;
                    player.TriggerNewClient("setPlayerGpsMarker", vehicle.entity.Position.X, vehicle.entity.Position.Y);
                    break;

                case DimensionType.Business:
                    dbPlayer.SendNewNotification("Gesuchte Person " + playerFromPool.GetName() + " befindet sich im BusinessTower!");
                    break;

                case DimensionType.Storage:
                    if (playerFromPool.HasData("storageRoomId")) {
                        StorageRoom storageRoom = StorageRoomModule.Instance.Get((uint)playerFromPool.GetData("storageRoomId"));
                        if (storageRoom != null) player.TriggerNewClient("setPlayerGpsMarker", storageRoom.Position.X, storageRoom.Position.Y);
                    }
                    break;

                case DimensionType.WeaponFactory:
                    break;
                default:
                    Logger.Crash(new ArgumentOutOfRangeException());
                    break;
            }

            if (!dbPlayer.IsNSADuty) {
                dbPlayer.SendNewNotification("Gesuchte Person " + playerFromPool.GetName() + " wurde geortet!");
            }
            return;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E)
                return false;


            if(dbPlayer.IsNSADuty && dbPlayer.IsNSAState >= (int)NSA.NSARangs.LEAD && dbPlayer.HasData("nsacloning"))
            {
                
                if (dbPlayer.HasData("clonePerson"))
                {
                    dbPlayer.ResetData("clonePerson");
                    dbPlayer.SendNewNotification("Cloning beendet!");
                    dbPlayer.ApplyCharacter();
                    return true;
                }

                if (dbPlayer.IsNSAState == (int)NSARangs.LEAD)
                {
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "PIS - Person Identifying System", Callback = "NSAClonePlayer", Message = "Bitte geben Sie einen Namen ein:" });
                }
                return true;
            }

            if (dbPlayer.TeamId == (int)teams.TEAM_FIB && dbPlayer.IsUndercover())
            {
                if (dbPlayer.Player.Position.DistanceTo(NSAVehicleModifyPosition) < 2.0f)
                {
                    Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.NSAVehicleListMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }
            
            return false;
        }
        
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandtakebm(Player player, string commandParams)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.TeamId != (int)teams.TEAM_FIB || !dbPlayer.IsInDuty()) return;

            try
            {
                if (!dbPlayer.IsACop()) return;
                if (string.IsNullOrWhiteSpace(commandParams))
                {
                    dbPlayer.SendNewNotification(MSG.General.Usage("/takebm", "name"));
                    return;
                }

                if (dbPlayer.TeamRank < 1)
                {
                    dbPlayer.SendNewNotification("Beschlagnahmungen von Schwarzgeld können erst ab Rang 1 vollzogen werden.");
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(commandParams);

                if (findPlayer == null || !findPlayer.IsValid() || findPlayer.Player.Position.DistanceTo(player.Position) >= 3.0f)
                {
                    dbPlayer.SendNewNotification("Person nicht gefunden oder außerhalb der Reichweite!");
                    return;
                }

                if ((!findPlayer.IsCuffed && !findPlayer.IsTied) || findPlayer.isInjured())
                {
                    return;
                }

                if(findPlayer.blackmoney[0] <= 0)
                {
                    dbPlayer.SendNewNotification("Person hat kein Schwarzgeld auf der Hand!");
                    return;
                }

                int amount = findPlayer.blackmoney[0];
                findPlayer.TakeBlackMoney(amount);

                dbPlayer.SendNewNotification($"Sie haben von {findPlayer.GetName()} ${amount} Schwarzgeld konfisziert!");
                findPlayer.SendNewNotification("Ein Beamter hat ihr Schwarzgeld konfisziert!");
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcreatevoltage(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !Configurations.Configuration.Instance.DevMode) return;

            NAPI.Marker.CreateMarker(3, (dbPlayer.Player.Position - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, 0);

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");

            string query = String.Format(
                "INSERT INTO `houses_voltage` (`pos_x`, `pos_y`, `pos_z`) VALUES ('{0}', '{1}', '{2}');",
                x, y, z);

            MySQLHandler.ExecuteAsync(query);
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandpeilsender(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid()) return;

            if (dbPlayer.IsNSADuty && dbPlayer.IsNSAState >= (int)NSA.NSARangs.LIGHT && dbPlayer.Container.GetItemAmount(696) >= 1)
            {
                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Peilsender", Callback = "NSASetPeilsender", Message = "Bitte Peilsender benennen:" });
            }
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandwanze(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid()) return;

            if (!dbPlayer.IsNSADuty || dbPlayer.IsNSAState <= (int)NSARangs.LIGHT) return;

            if (dbPlayer.Container.GetItemAmount(1075) >= 1)
            {
                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Wanze", Callback = "NSASetWanze", Message = "Bitte Wanze benennen:" });
            }
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsuspendieren(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || (dbPlayer.TeamId != (int)teams.TEAM_FIB && dbPlayer.GovLevel.ToLower() != "a" && dbPlayer.IsNSAState != (int)NSARangs.LEAD)) return;

            if (dbPlayer.TeamId == (int)teams.TEAM_FIB && dbPlayer.TeamRank < 11 && dbPlayer.IsNSAState != (int)NSARangs.LEAD) return;

            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Suspendierung", Callback = "NSASuspendate", Message = "Bitte einen Namen angeben:" });
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcloneplayer(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!Configurations.Configuration.Instance.DevMode) return;
            if (!dbPlayer.IsValid()) return;

            if(dbPlayer.HasData("clonePerson"))
            {
                dbPlayer.ResetData("clonePerson");
                dbPlayer.SendNewNotification("Cloning beendet!");
                return;
            }

            if (dbPlayer.TeamId == (int)teams.TEAM_FIB)
            {
                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "PIS - Person Identifying System", Callback = "NSAClonePlayer", Message = "Bitte geben Sie einen Namen ein:" });
            }
            return;
        }
        
        [CommandPermission()]
        [Command(GreedyArg = true)]
        public void Commandsetgovlevel(Player player, string args)
        {
            if (!args.Contains(' ')) return;

            string[] argsSplit = args.Split(' ');
            if (argsSplit.Length < 2) return;

            string name = argsSplit[0];
            string Level = argsSplit[1];


            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;


            var findPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            if (Level.Length <= 0 || Level.Length > 2) return;

            if (Level != "A" || Level != "B" || Level != "C" || Level != "D") return;

            if (dbPlayer.GovLevel != "A") return;

            findPlayer.SetGovLevel(Level);

            dbPlayer.SendNewNotification($"Sie haben {findPlayer.GetName()} die Sicherheitsfreigabe {Level} erteilt!");
            findPlayer.SendNewNotification($"{dbPlayer.GetName()} hat ihnen die Sicherheitsfreigabe {Level} erteilt!");
        }

        [CommandPermission()]
        [Command(GreedyArg = true)]
        public void Commandresetgovlevel(Player player, string name)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || (dbPlayer.TeamId != (int)teams.TEAM_FIB && dbPlayer.TeamId != (int)teams.TEAM_POLICE && dbPlayer.TeamId != (int)teams.TEAM_ARMY && dbPlayer.TeamId != (int)teams.TEAM_GOV) || dbPlayer.TeamRank < 11) return;


            var findPlayer = Players.Players.Instance.FindPlayer(name);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            string Level = findPlayer.GovLevel;

            if (Level != "A" || Level != "B" || Level != "C" || Level != "D") return;

            if (Level == "A" || Level == "B" || Level == "C")
            {
                if ((dbPlayer.TeamId != (int)teams.TEAM_FIB && dbPlayer.TeamId != (int)teams.TEAM_POLICE && dbPlayer.TeamId != (int)teams.TEAM_ARMY && dbPlayer.TeamId != (int)teams.TEAM_GOV) || dbPlayer.TeamRank < 11) return;
            }

            if (Level == "D")
            {
                if (dbPlayer.GovLevel != "A" && dbPlayer.GovLevel != "B") return;
            }

            findPlayer.RemoveGovLevel();

            dbPlayer.SendNewNotification($"Sie haben {findPlayer.GetName()} die Sicherheitsfreigabe entzogen!");
            findPlayer.SendNewNotification($"{dbPlayer.GetName()} hat ihnen die Sicherheitsfreigabe entzogen!");
        }
    }
}
