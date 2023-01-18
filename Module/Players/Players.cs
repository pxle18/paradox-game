using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Players.Ranks;
using VMP_CNR.Module.Weapons;

namespace VMP_CNR.Module.Players
{
    public sealed class Players
    {
        public static Players Instance { get; } = new Players();
        public readonly ConcurrentDictionary<uint, DbPlayer> players;
        private readonly HashSet<uint> playerIds;
        private const float SyncRange = 125f;

        private Players()
        {
            players = new ConcurrentDictionary<uint, DbPlayer>();
            playerIds = new HashSet<uint>();
        }

        public DbPlayer GetByDbId(uint id)
        {
            var playerDb = GetValidPlayers().FirstOrDefault(player => player.Id == id && player.IsValid());
            return playerDb?.Player.GetPlayer();
        }

        public DbPlayer GetByName(string name)
        {
            var playerDb = GetValidPlayers().FirstOrDefault(player => player.GetName() == name && player.IsValid());
            return playerDb?.Player.GetPlayer();
        }

        public List<DbPlayer> GetValidPlayers()
        {
            return players.Values.Where(p => p != null && p.IsValid()).ToList();
        }

        public List<DbPlayer> GetJailedPlayers()
        {
            return players.Values.Where(p => p != null && p.IsValid() && p.JailTime[0] > 0).ToList();
        }

        public void SendNotificationToAllUsers(string command, int duration = 5000)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (DbPlayer dbPlayer in players.Values)
                {
                    if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                    dbPlayer.SendNewNotification(command, PlayerNotification.NotificationType.STANDARD, "", duration);
                }
            }));
        }

        public DbPlayer GetPlayerByPhoneNumber(uint HandyNummer)
        {
            return players.Values.Where(p => p != null && p.IsValid() && p.handy[0] == HandyNummer).FirstOrDefault();
        }


        public DbPlayer GetClosestPlayerForPlayerWhoIsCuffed(DbPlayer source, float range = 4.0f)
        {
            try
            {
                return GetPlayersInRange(source.Player.Position, range).Where(pl => pl.Player.Position.DistanceTo(source.Player.Position) <= range && pl.Id != source.Id && (pl.IsCuffed || pl.IsTied)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public DbPlayer GetClosestPlayerForPlayer(DbPlayer source, float range = 4.0f)
        {
            try
            {
                return GetPlayersInRange(source.Player.Position, range).Where(pl => pl.Player.Position.DistanceTo(source.Player.Position) <= range && pl.Id != source.Id).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public DbPlayer GetClosestInjuredForPlayer(DbPlayer source, float range = 4.0f)
        {
            try
            {
                return GetPlayersInRange(source.Player.Position, range).Where(pl => pl.Player.Position.DistanceTo(source.Player.Position) <= range && pl.Id != source.Id && pl.IsInjured()).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public void SendMessageToAuthorizedUsersSync(string feature, string command, int time = 18000)
        {
            foreach (DbPlayer dbPlayer in GetValidPlayers().Where(p => p.Rank.Id != 0))
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                if (!dbPlayer.Rank.CanAccessFeature(feature)) continue;
                if (dbPlayer.Player.HasFeatureIgnored(feature)) continue;
                if (feature.Equals("teamchat"))
                    dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.TEAM);
                else if (feature.Equals("support"))
                    dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.SERVER);
                else if (feature.Equals("highteamchat"))
                    dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.HIGH);
                else
                    dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        public void SendMessageToAuthorizedUsers(string feature, string command, int time = 18000)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (DbPlayer dbPlayer in GetValidPlayers().Where(p => p.Rank.Id != 0))
                {
                    if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                    if (!dbPlayer.Rank.CanAccessFeature(feature)) continue;
                    if (dbPlayer.Player.HasFeatureIgnored(feature)) continue;
                    if (feature.Equals("teamchat"))
                        dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.TEAM);
                    else if (feature.Equals("support"))
                        dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.SERVER);
                    else if (feature.Equals("highteamchat"))
                        dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.HIGH);
                    else
                        dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.ADMIN);
                }
            }));
        }

        public void SendMessageToHighTeam(string feature, string command, int time = 18000)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (DbPlayer dbPlayer in GetValidPlayers().Where(p => p.Rank.Id == 4 || p.Rank.Id == 5 || p.Rank.Id == 6 || p.Rank.Id == 8))
                {
                    if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                    if (dbPlayer.Player.HasFeatureIgnored(feature)) continue;

                    dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.HIGH);
                }
            }));

        }

        public void TriggerNewClientInRange(DbPlayer dbPlayer, string eventName, params object[] args)
        {
            GetPlayersInRange(dbPlayer.Player.Position).TriggerNewClient(eventName, args);
        }

        public void SendChatMessageToAuthorizedUsers(string feature, DbPlayer dbPlayer, string message)
        {
            SendMessageToAuthorizedUsers(feature, $"{dbPlayer.GetName()}: {message}");
        }

        public DbPlayer FindPlayer(object search, bool p_IgnoreFakeName = false)
        {
            try
            {
                List<DbPlayer> playerList = GetValidPlayers();

                var searchString = search.ToString();
                if (string.IsNullOrEmpty(searchString)) return null;
                if (int.TryParse(searchString, out var playerId))
                {
                    foreach (var user in playerList)
                    {
                        if (user == null || !user.IsValid() || user.Id != playerId) continue;
                        return user;
                    }
                    foreach (var user in playerList)
                    {
                        if (user == null || !user.IsValid() || user.ForumId != playerId) continue;
                        return user;
                    }
                }
                else
                {
                    foreach (var user in playerList)
                    {
                        if (user == null || !user.IsValid() || user.Player == null) continue;

                        var l_Name = user.GetName();
                        if (p_IgnoreFakeName)
                        {
                            if (l_Name.ToLower().Contains(search.ToString().ToLower()))
                                return user;

                            if (user.GetName().ToLower().Contains(search.ToString().ToLower()))
                                return user;
                        }
                        else if (l_Name.ToLower().Contains(search.ToString().ToLower())) return user;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }
        /*
        public DbPlayer FindByVoiceHash(string search)
        {
            try
            {
                List<DbPlayer> playerList = GetValidPlayers();
                
                if (string.IsNullOrEmpty(search) || !Int32.TryParse(search, out int searchVH)) return null;
                foreach (var user in playerList)
                {
                    if (user == null || !user.IsValid() || user.Player == null) continue;

                    if (Int32.TryParse(user.VoiceHash, out int userVh))
                    {
                        if (userVh == searchVH) return user;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }*/

        public DbPlayer FindPlayerById(object search, bool valid = true)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;
            if (int.TryParse(searchString, out var playerId))
            {
                foreach (DbPlayer dbPlayerx in GetValidPlayers())
                {
                    if (dbPlayerx.Id != playerId) continue;
                    if (valid && !dbPlayerx.IsValid()) return null;
                    return dbPlayerx;
                }
            }

            return null;
        }
        public DbPlayer FindPlayerByForumId(object search, bool valid = true)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;
            if (int.TryParse(searchString, out var playerId))
            {
                foreach (DbPlayer dbPlayerx in GetValidPlayers())
                {
                    if (dbPlayerx.ForumId != playerId) continue;
                    if (valid && !dbPlayerx.IsValid()) return null;
                    return dbPlayerx;
                }
            }

            return null;
        }

        public IEnumerable<DbPlayer> GetPlayersInRange(Vector3 position, float range = SyncRange)
        {
            return GetValidPlayers().Where((player) => player.Player.Position.DistanceTo(position) < range);
        }

        public List<DbPlayer> GetPlayersListInRange(Vector3 position, float range = SyncRange)
        {
            List<DbPlayer> dbPlayers = new List<DbPlayer>();
            foreach (var lDbPlayer in GetValidPlayers())
            {
                if (lDbPlayer.Player.Position.DistanceTo(position) <= range)
                    dbPlayers.Add(lDbPlayer);
            }

            return dbPlayers;
        }

        private static string GetTextExtensionForFeature(string feature)
        {
            string name;
            switch (feature)
            {
                case "adminchat":
                    name = "AdminChat";
                    break;
                case "teamchat":
                    name = "TeamChat";
                    break;
                default:
                    name = feature;
                    break;
            }

            return $"[{name}]: ";
        }

        public DbPlayer FindByVoiceHash(string search)
        {
            try
            {
                List<DbPlayer> playerList = GetValidPlayers();

                if (string.IsNullOrEmpty(search) || !Int32.TryParse(search, out int searchVH)) return null;
                foreach (var user in playerList)
                {
                    if (user == null || !user.IsValid() || user.Player == null) continue;

                    if (Int32.TryParse(user.VoiceHash, out int userVh))
                    {
                        if (userVh == searchVH) return user;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public DbPlayer FindByRageName(string search)
        {
            try
            {
                List<DbPlayer> playerList = GetValidPlayers();

                if (string.IsNullOrEmpty(search)) return null;

                foreach (var user in playerList)
                {
                    if (user == null || !user.IsValid() || user.Player == null) continue;

                    if (user.Player.Name.ToLower() == search.ToLower())
                    {
                        return user;
                    }
                }

                foreach (var user in playerList)
                {
                    if (user == null || !user.IsValid() || user.Player == null) continue;

                    if (user.Player.Name.ToLower().Contains(search.ToLower()))
                    {
                        return user;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public async Task<DbPlayer> Load(MySqlDataReader reader, Player player, bool reload = false)
        {
            DbPlayer dbPlayer = new DbPlayer(reader)
            {
                Player = player,


                RemoteHashKey = HashThis.GetSha256Hash(VoiceListHandler.RandomString(64) + reader.GetString("Name"))
            };

            try
            {

                dbPlayer.Player.TriggerNewClient("setRemoteHashKey", dbPlayer.RemoteHashKey);
                Logger.Debug("RemoteHashkey for " + dbPlayer.GetName() + ": " + dbPlayer.RemoteHashKey);

                // Setting RageExtensionObject
                await NAPI.Task.WaitForMainThread(0);
                dbPlayer.RageExtension = new PlayerRageExtension(player);

                dbPlayer.PlayerWrapper = new PlayerWrapper(player);

                dbPlayer.Id = reader.GetUInt32("id");
                dbPlayer.Password = reader.GetString("Pass");
                dbPlayer.Salt = reader.GetString("Salt");

                // Add Verweis
                player.SetData("player", dbPlayer);
                player.Name = reader.GetString("Name");

                // Forumid
                dbPlayer.ForumId = reader.GetInt32("forumid");
                dbPlayer.TemporaryPlayerId = GetFreeId();

                dbPlayer.AnimationScenario = new AnimationScenario();

                dbPlayer.Money = new int[2];
                dbPlayer.Money[1] = reader.GetInt32("Money");
                dbPlayer.Money[0] = reader.GetInt32("Money");
                dbPlayer.BankMoney = new int[2];
                dbPlayer.BankMoney[1] = reader.GetInt32("BankMoney");
                dbPlayer.BankMoney[0] = reader.GetInt32("BankMoney");
                dbPlayer.BlackMoney = new int[2];
                dbPlayer.BlackMoney[1] = reader.GetInt32("blackmoney");
                dbPlayer.BlackMoney[0] = reader.GetInt32("blackmoney");
                dbPlayer.BlackMoneyBank = new int[2];
                dbPlayer.BlackMoneyBank[1] = reader.GetInt32("blackmoneybank");
                dbPlayer.BlackMoneyBank[0] = reader.GetInt32("blackmoneybank");
                dbPlayer.PayDay = new int[2];
                dbPlayer.PayDay[1] = reader.GetInt32("payday");
                dbPlayer.PayDay[0] = reader.GetInt32("payday");
                dbPlayer.RP = new int[2];
                dbPlayer.RP[1] = reader.GetInt32("rp");
                dbPlayer.RP[0] = reader.GetInt32("rp");
                dbPlayer.OwnHouse = new uint[2];
                dbPlayer.OwnHouse[1] = reader.GetUInt32("ownHouse");
                dbPlayer.OwnHouse[0] = reader.GetUInt32("ownHouse");
                dbPlayer.Wanteds = new int[2];
                dbPlayer.Wanteds[1] = reader.GetInt32("wanteds");
                dbPlayer.Wanteds[0] = reader.GetInt32("wanteds");

                //Licenses
                dbPlayer.Lic_Car = new int[2];
                dbPlayer.Lic_Car[1] = reader.GetInt32("Lic_Car");
                dbPlayer.Lic_Car[0] = reader.GetInt32("Lic_Car");
                dbPlayer.Lic_LKW = new int[2];
                dbPlayer.Lic_LKW[1] = reader.GetInt32("Lic_LKW");
                dbPlayer.Lic_LKW[0] = reader.GetInt32("Lic_LKW");
                dbPlayer.Lic_Bike = new int[2];
                dbPlayer.Lic_Bike[1] = reader.GetInt32("Lic_Bike");
                dbPlayer.Lic_Bike[0] = reader.GetInt32("Lic_Bike");
                dbPlayer.Lic_PlaneA = new int[2];
                dbPlayer.Lic_PlaneA[1] = reader.GetInt32("Lic_PlaneA");
                dbPlayer.Lic_PlaneA[0] = reader.GetInt32("Lic_PlaneA");
                dbPlayer.Lic_PlaneB = new int[2];
                dbPlayer.Lic_PlaneB[1] = reader.GetInt32("Lic_PlaneB");
                dbPlayer.Lic_PlaneB[0] = reader.GetInt32("Lic_PlaneB");
                dbPlayer.Lic_Boot = new int[2];
                dbPlayer.Lic_Boot[1] = reader.GetInt32("Lic_Boot");
                dbPlayer.Lic_Boot[0] = reader.GetInt32("Lic_Boot");
                dbPlayer.Lic_Gun = new int[2];
                dbPlayer.Lic_Gun[1] = reader.GetInt32("Lic_Gun");
                dbPlayer.Lic_Gun[0] = reader.GetInt32("Lic_Gun");
                dbPlayer.Lic_Hunting = new int[2];
                dbPlayer.Lic_Hunting[1] = reader.GetInt32("Lic_Hunting");
                dbPlayer.Lic_Hunting[0] = reader.GetInt32("Lic_Hunting");
                dbPlayer.Lic_Biz = new int[2];
                dbPlayer.Lic_Biz[1] = reader.GetInt32("Lic_Biz");
                dbPlayer.Lic_Biz[0] = reader.GetInt32("Lic_Biz");
                dbPlayer.spawnchange = new int[2];
                dbPlayer.spawnchange[1] = reader.GetInt32("spawnchange");
                dbPlayer.spawnchange[0] = reader.GetInt32("spawnchange");
                dbPlayer.job = new int[2];
                dbPlayer.job[1] = reader.GetInt32("job");
                dbPlayer.job[0] = reader.GetInt32("job");
                dbPlayer.JobSkill = new int[2];
                dbPlayer.JobSkill[1] = reader.GetInt32("jobskills");
                dbPlayer.JobSkill[0] = reader.GetInt32("jobskills");
                dbPlayer.JailTime = new int[2];
                dbPlayer.JailTime[1] = reader.GetInt32("jailtime");
                dbPlayer.JailTime[0] = reader.GetInt32("jailtime");
                dbPlayer.jailtimeReducing = new int[2];
                dbPlayer.jailtimeReducing[1] = reader.GetInt32("jailtime_reduce");
                dbPlayer.jailtimeReducing[0] = reader.GetInt32("jailtime_reduce");
                dbPlayer.HasPerso = new int[2];
                dbPlayer.HasPerso[1] = reader.GetInt32("Perso");
                dbPlayer.HasPerso[0] = reader.GetInt32("Perso");
                dbPlayer.fakePerso = false;
                dbPlayer.fakeName = "";
                dbPlayer.fakeSurname = "";
                dbPlayer.donator = new int[2];
                dbPlayer.donator[1] = reader.GetInt32("Donator");
                dbPlayer.donator[0] = reader.GetInt32("Donator");
                dbPlayer.uni_points = new int[2];
                dbPlayer.uni_points[1] = reader.GetInt32("uni_points");
                dbPlayer.uni_points[0] = reader.GetInt32("uni_points");
                dbPlayer.uni_economy = new int[2];
                dbPlayer.uni_economy[1] = reader.GetInt32("uni_economy");
                dbPlayer.uni_economy[0] = reader.GetInt32("uni_economy");
                dbPlayer.uni_business = new int[2];
                dbPlayer.uni_business[1] = reader.GetInt32("uni_business");
                dbPlayer.uni_business[0] = reader.GetInt32("uni_business");
                dbPlayer.uni_workaholic = new int[2];
                dbPlayer.uni_workaholic[1] = reader.GetInt32("uni_workaholic");
                dbPlayer.uni_workaholic[0] = reader.GetInt32("uni_workaholic");
                dbPlayer.birthday = new string[2];
                dbPlayer.birthday[1] = reader.GetString("birthday");
                dbPlayer.birthday[0] = reader.GetString("birthday");
                dbPlayer.fspawn = new uint[2];
                dbPlayer.fspawn[1] = reader.GetUInt32("fspawn");
                dbPlayer.fspawn[0] = reader.GetUInt32("fspawn");

                dbPlayer.hasPed = new string[2];
                dbPlayer.hasPed[1] = reader.GetString("hasPed");
                dbPlayer.hasPed[0] = reader.GetString("hasPed");
                dbPlayer.Lic_FirstAID = new int[2];
                dbPlayer.Lic_FirstAID[1] = reader.GetInt32("Lic_FirstAID");
                dbPlayer.Lic_FirstAID[0] = reader.GetInt32("Lic_FirstAID");
                dbPlayer.timeban = new int[2];
                dbPlayer.timeban[1] = reader.GetInt32("timeban");
                dbPlayer.timeban[0] = reader.GetInt32("timeban");
                dbPlayer.job_skills = new string[2];
                dbPlayer.job_skills[1] = reader.GetString("job_skills");
                dbPlayer.job_skills[0] = reader.GetString("job_skills");
                dbPlayer.warns = new int[2];
                dbPlayer.warns[1] = reader.GetInt32("warns");
                dbPlayer.warns[0] = reader.GetInt32("warns");
                dbPlayer.Ausschluss = new int[2];
                dbPlayer.Ausschluss[1] = reader.GetInt32("ausschluss");
                dbPlayer.Ausschluss[0] = reader.GetInt32("ausschluss");
                dbPlayer.HardwareID = new string[2];
                dbPlayer.HardwareID[1] = reader.GetString("Hwid");
                dbPlayer.HardwareID[0] = reader.GetString("Hwid");
                dbPlayer.fgehalt = new int[2];
                dbPlayer.fgehalt[1] = reader.GetInt32("fgehalt");
                dbPlayer.fgehalt[0] = reader.GetInt32("fgehalt");
                dbPlayer.paycheck = new int[2];
                dbPlayer.paycheck[1] = reader.GetInt32("paycheck");
                dbPlayer.paycheck[0] = reader.GetInt32("paycheck");

                dbPlayer.PedLicense = new bool[2];
                dbPlayer.PedLicense[1] = reader.GetInt32("pedlicense") == 1;
                dbPlayer.PedLicense[0] = reader.GetInt32("pedlicense") == 1;

                var handy = reader.GetUInt32("handy");
                if (handy <= 0)
                {
                    handy = 1275 + dbPlayer.Id;
                }

                dbPlayer.handy = new uint[2];
                dbPlayer.handy[1] = handy;
                dbPlayer.handy[0] = handy;
                dbPlayer.guthaben = new int[2];
                dbPlayer.guthaben[1] = reader.GetInt32("guthaben");
                dbPlayer.guthaben[0] = reader.GetInt32("guthaben");
                dbPlayer.Lic_Transfer = new int[2];
                dbPlayer.Lic_Transfer[1] = reader.GetInt32("lic_transfer");
                dbPlayer.Lic_Transfer[0] = reader.GetInt32("lic_transfer");
                dbPlayer.married = new uint[2];
                dbPlayer.married[1] = reader.GetUInt32("married");
                dbPlayer.married[0] = reader.GetUInt32("married");
                dbPlayer.Lic_Taxi = new int[2];
                dbPlayer.Lic_Taxi[1] = reader.GetInt32("Lic_Taxi");
                dbPlayer.Lic_Taxi[0] = reader.GetInt32("Lic_Taxi");

                // Setting SavedPos Params
                dbPlayer.pos_x = new float[2];
                dbPlayer.pos_x[1] = reader.GetFloat("pos_x");
                dbPlayer.pos_x[0] = reader.GetFloat("pos_x");
                dbPlayer.pos_y = new float[2];
                dbPlayer.pos_y[1] = reader.GetFloat("pos_y");
                dbPlayer.pos_y[0] = reader.GetFloat("pos_y");
                dbPlayer.pos_z = new float[2];
                dbPlayer.pos_z[1] = reader.GetFloat("pos_z");
                dbPlayer.pos_z[0] = reader.GetFloat("pos_z");
                dbPlayer.pos_heading = new float[2];
                dbPlayer.pos_heading[1] = reader.GetFloat("pos_heading");
                dbPlayer.pos_heading[0] = reader.GetFloat("pos_heading");
                dbPlayer.Armor = new int[2];
                dbPlayer.Armor[1] = reader.GetInt32("armor");
                dbPlayer.Armor[0] = reader.GetInt32("armor");

                dbPlayer.Dimension = new uint[2];
                dbPlayer.Dimension[0] = reader.GetUInt32("dimension");
                dbPlayer.Dimension[1] = reader.GetUInt32("dimension");

                dbPlayer.MetaData = new MetaDataObject
                {
                    Position = new Vector3(dbPlayer.pos_x[0], dbPlayer.pos_y[0], dbPlayer.pos_z[0]),
                    Dimension = dbPlayer.Dimension[0],
                    Heading = 0f,
                    Armor = dbPlayer.Armor[0],
                    Health = dbPlayer.Hp
                };

                dbPlayer.ApplyPlayerHealth();

                dbPlayer.CanSeeNames = false;

                dbPlayer.grade = new int[2];
                dbPlayer.grade[1] = reader.GetInt32("grade");
                dbPlayer.grade[0] = reader.GetInt32("grade");

                dbPlayer.zwd = new int[2];
                dbPlayer.zwd[1] = reader.GetInt32("zwd");
                dbPlayer.zwd[0] = reader.GetInt32("zwd");

                // drink
                dbPlayer.drink = new int[2];
                dbPlayer.drink[1] = reader.GetInt32("drink");
                dbPlayer.drink[0] = reader.GetInt32("drink");

                // food
                dbPlayer.food = new int[2];
                dbPlayer.food[1] = reader.GetInt32("food");
                dbPlayer.food[0] = reader.GetInt32("food");

                // fitness
                dbPlayer.fitness = new int[2];
                dbPlayer.fitness[1] = reader.GetInt32("fitness");
                dbPlayer.fitness[0] = reader.GetInt32("fitness");

                dbPlayer.WatchMenu = 0;
                dbPlayer.SocialClubName = reader.GetString("SCName");

                dbPlayer.DimensionType = new DimensionType[2];
                dbPlayer.DimensionType[0] = (DimensionType)reader.GetInt32("dimensionType");
                dbPlayer.DimensionType[1] = (DimensionType)reader.GetInt32("dimensionType");

                dbPlayer.LastInteracted = DateTime.Now;

                dbPlayer.VehicleKeys = new Dictionary<uint, string>();
                dbPlayer.OwnVehicles = new Dictionary<uint, string>();

                dbPlayer.HouseKeys = new HashSet<uint>();

                dbPlayer.Weapons = new List<WeaponDetail>();
                if (reader.GetString("weapons") != "")
                    dbPlayer.Weapons = NAPI.Util.FromJson<List<WeaponDetail>>(reader.GetString("weapons"));

                // secure remove before adding
                try
                {
                    foreach (var itr in Players.Instance.players.Where(p => p.Value.GetName() == player.Name))
                    {
                        Players.Instance.players.TryRemove(itr.Key, out DbPlayer tmpDbPlayer);
                    }

                    // secure remove id
                    if (playerIds.Contains(reader.GetUInt32("id")))
                    {
                        Players.Instance.playerIds.Remove(reader.GetUInt32("id"));
                    }
                }
                catch (Exception e)
                {
                    Logging.Logger.Crash(e);
                }

                // secure adding
                while (!players.ContainsKey(reader.GetUInt32("id")))
                {
                    await NAPI.Task.WaitForMainThread(500);
                    players.TryAdd(dbPlayer.Id, dbPlayer);
                    Logger.Debug("Player addet to players " + dbPlayer.GetName());
                }

                while (!playerIds.Contains(reader.GetUInt32("id")))
                {
                    await NAPI.Task.WaitForMainThread(500);
                    playerIds.Add(dbPlayer.Id);
                    Logger.Debug("Player addet to playersIds " + dbPlayer.GetName());
                }
                await dbPlayer.LoadCrimes();

                Modules.Instance.OnPlayerLoadData(dbPlayer, reader);

                Modules.Instance.OnPlayerConnect(dbPlayer);
            }
            catch (Exception e)
            {
                Logger.Print("Players - " + e.ToString());
            }

            return dbPlayer;
        }

        public void RemovePlayerId(uint id)
        {
            playerIds.Remove(id);
        }

        public bool DoesPlayerExists(uint id)
        {
            return playerIds.Contains(id);
        }

        private int GetFreeId()
        {
            var freeId = 0;
            while (players.Values.FirstOrDefault(player => player != null && player.IsValid() && player.TemporaryPlayerId == freeId) != null)
            {
                freeId++;
            }

            return freeId;
        }
    }

    public static class PlayerListExtensions
    {
        public static void TriggerNewClient(this IEnumerable<DbPlayer> players, string eventName, params object[] args)
        {
            foreach (var player in players)
            {
                player.Player.TriggerNewClient(eventName, args);
            }
        }
    }
}