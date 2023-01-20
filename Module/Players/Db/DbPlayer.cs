using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VMP_CNR.Module.Animal;
using VMP_CNR.Module.Anticheat;
using VMP_CNR.Module.Attachments;
using VMP_CNR.Module.Boerse;
using VMP_CNR.Module.Clothes.Character;
using VMP_CNR.Module.Clothes.InventoryBag;
using VMP_CNR.Module.Clothes.Outfits;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Email;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.FIB;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Nutrition;
using VMP_CNR.Module.PlayerDataCustom;
using VMP_CNR.Module.Players.Buffs;
using VMP_CNR.Module.Players.Phone.Apps;
using VMP_CNR.Module.Players.Phone.Contacts;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Players.Ranks;
using VMP_CNR.Module.Procedures.Interfaces;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Permission;
using VMP_CNR.Module.TeamSubgroups;
using VMP_CNR.Module.TeamSubgroups.Models;
using VMP_CNR.Module.Telefon.App.Settings;
using VMP_CNR.Module.Telefon.App.Settings.Ringtone;
using VMP_CNR.Module.Telefon.App.Settings.Wallpaper;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Weapons;
using VMP_CNR.Module.Weapons.Component;
using static VMP_CNR.Module.Players.Events.EventStateModule;

namespace VMP_CNR.Module.Players.Db
{
    public enum EconomyIndex
    {
        Low = 1,
        Mid = 2,
        Rich = 3,
        Superrich = 4,
        Jeff = 5
    }

    public enum DimensionTypes : uint
    {
        World = 0,
        House = 1,
        Basement = 2,
        Labor = 3,
        Business = 4,
        Storage = 6,
        WeaponFactory = 7,
        Camper = 8,
        House_Shop_Interior = 9,
        Methlaboratory = 10,
        MoneyKeller = 11,
        Gangwar = 12,
        Weaponlaboratory = 13,
        Cannabislaboratory = 14,
        RacingArea = 15,
        Paintball = 16,
        Rocket = 17,
        RCRacing = 18,
        Heroinlaboratory = 19,
        ShopTakeover = 20
    }

    public class MetaDataObject
    {
        public Vector3 Position { get; set; }
        public uint Dimension { get; set; }
        public float Heading { get; set; }

        public int Health { get; set; }
        public int Armor { get; set; }
        public bool SaveBlocked { get; set; }

        public MetaDataObject()
        {
            Position = new Vector3();
            Dimension = 0;
            Heading = 0.0f;
            Health = 100;
            Armor = 0;
            SaveBlocked = false;
        }
    }

    //Todo: runtime property indexer for db columns
    public class DbPlayer
    {
        public enum Value
        {
            Duty = 0,
            RankId = 1,
            TeamId = 2,
            Level = 3,
            TeamRang = 4,
            DeathStatus = 5,
            IsCuffed = 6,
            IsTied = 7,
            Hp = 8,
            Armor = 9,
            Swat = 10,
            UHaftTime = 11,
            Einwanderung = 12,
            SwatDuty = 13,
            Teamfight = 14,
            Suspension = 15,
            WDutyTime = 16,
            Paintball = 17,
            TeamSubgroupId = 18,
        }

        public enum OperationType
        {
            ClothesPacked = 0,
            InventoryOpened,
            PressedL,
            PressedT,
            PressedK,
            PressedJ,
            PressedKomma,
            PressedPunkt,
            Smartphone,
            PressedM,
            PressedH,
            ItemMove,
            ContactAdd,
            ContactRemove,
            ContactUpdate,
            BusinessCreate,
            WeaponAmmoSync
        }

        public DateTime LastQueryBreak = DateTime.Now;

        public enum RankDutyStatus
        {
            OffDuty = 0,
            AdminDuty = 1,
            GuideDuty = 2,
            CasinoDuty = 3,
            GameDesignDuty = 4
        }

        public static readonly string[] DbColumns =
        {
            "duty",
            "rankId",
            "team", //Todo: needs db rename
            "Level",
            "rang", //Todo: needs db rename
            "Deadstatus",
            "isCuffed",
            "isTied",
            "hp",
            "armor",
            "swat",
            "uhaft",
            "einwanderung",
            "swatduty",
            "teamfight",
            "suspendate",
            "w_dutytime",
            "paintball",
        };

        public Player Player { get; set; }

        public PlayerWrapper PlayerWrapper { get; set; }

        public PlayerRageExtension RageExtension { get; set; }

        // Character
        public Character Character { get; set; }

        // Customization
        public CharacterCustomization Customization { get; set; }

        // PlayerDataObject
        public PlayerBuffs Buffs { get; set; }

        // PlayerRights
        public TeamRankPermission TeamRankPermission { get; set; }

        public ConcurrentDictionary<string, dynamic> PlayerData { get; set; }
        public MetaDataObject MetaData { get; set; }

        public uint Id { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }

        public int Level { get; set; }
        public RankDutyStatus RankDuty { get; set; }
        public uint RankId { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public int PassAttempts { get; set; }
        public uint TeamId { get; private set; }
        public uint TeamSubgroupId { get; private set; }
        public int[] Money { get; set; }
        public int[] BankMoney { get; set; }

        public int[] BlackMoney { get; set; }
        public int[] BlackMoneyBank { get; set; }
        public uint TeamRank { get; set; }
        public int[] PayDay { set; get; }
        public bool IsFirstSpawn { get; set; }
        public bool IsSwimmingOrDivingDoNotUse { get; set; }
        public int[] RP { get; set; }
        public uint[] OwnHouse { get; set; }
        public int[] Wanteds { get; set; }
        public uint WatchMenu { get; set; }
        public bool IsFreezed { get; set; }
        public uint WatchDialog { get; set; }
        public int[] Lic_Car { get; set; }
        public int[] Lic_LKW { get; set; }
        public int[] Lic_Bike { get; set; }
        public int[] Lic_PlaneA { get; set; }
        public int[] Lic_PlaneB { get; set; }
        public int[] Lic_Boot { get; set; }
        public int[] Lic_Gun { get; set; }
        public int[] Lic_Hunting { get; set; }
        public int[] Lic_Biz { get; set; }
        public int marryLic { get; set; }
        public int[] spawnchange { get; set; }
        public int[] job { get; set; }
        public int[] JobSkill { get; set; }
        public string[] job_skills { get; set; }
        public int[] JailTime { get; set; }
        public int[] jailtimeReducing { get; set; }
        public int[] uni_points { get; set; }
        public int[] uni_economy { get; set; }
        public int[] uni_business { get; set; }
        public int[] uni_workaholic { get; set; }
        public int[] deadtime { get; set; }
        public InjuryType Injury { get; set; }
        public float[] dead_x { get; set; }
        public float[] dead_z { get; set; }
        public float[] dead_y { get; set; }
        public int[] HasPerso { get; set; }
        public bool fakePerso { get; set; }
        public string fakeName { get; set; }
        public string fakeSurname { get; set; }
        public uint[] fspawn { get; set; }

        public string[] birthday { get; set; }
        public int[] donator { get; set; }
        public string[] hasPed { get; set; }
        public int[] Lic_FirstAID { get; set; }
        public int[] timeban { get; set; }
        public int TemporaryPlayerId { get; set; }
        public int[] Ausschluss { get; set; }
        public int[] warns { get; set; }
        public string[] HardwareID { get; set; }
        public int[] fgehalt { get; set; }
        public int[] paycheck { get; set; }
        public uint[] handy { get; set; }
        public int[] guthaben { get; set; }
        public int[] Lic_Transfer { get; set; }
        public uint[] married { get; set; }
        public int[] Lic_Taxi { get; set; }
        public float[] pos_x { get; set; }
        public float[] pos_y { get; set; }
        public float[] pos_z { get; set; }
        public float[] pos_heading { get; set; }
        public bool Duty { get; set; }
        public int Hp { get; set; }
        public int[] Armor { get; set; }

        public int VisibleArmorType { get; set; }

        public bool CanSeeNames { get; set; }
        public bool visibleArmor { get; set; }
        public bool IsCuffed { get; set; }
        public bool IsTied { get; set; }
        public bool IsFarming { get; set; }
        public bool IsInRob { get; set; }

        public int[] grade { get; set; }
        public int[] zwd { get; set; }
        public int UHaftTime { get; set; }

        public int Einwanderung { get; set; }

        public int SwatDuty { get; set; }
        public int Teamfight { get; set; }
        public int Paintball { get; set; }
        public int WDutyTime { get; set; }

        public bool Suspension { get; set; }

        public string UndercoverName { get; set; }

        public string SocialClubName { get; set; }

        public bool VoiceEnabled { get; set; }

        public string VoiceHash { get; set; }

        public int[] drink { get; set; }
        public int[] food { get; set; }

        public List<LastNutritionItem> lastnutritionitems = new List<LastNutritionItem>();
        public int[] fitness { get; set; }

        public int ForumId { get; set; }

        public int IsNSAState { get; set; }

        public bool IsNSADuty { get; set; }

        public GTANetworkAPI.Object AdminObject { get; set; }
        public double AdminObjectSpeed { get; set; }

        public string saveQuery { get; set; }

        public DateTime lastKeySend { get; set; }
        public DateTime spawnProtection { get; set; }

        // Temp Wanted
        public int TempWanteds { get; set; }

        public uint[] Dimension { get; set; }
        public DimensionTypes[] DimensionType { get; set; }

        public DateTime LastInteracted { get; set; }
        public DateTime LastEInteract { get; set; }

        //TOOD: move to Phone object
        //Phone
        public PhoneContacts PhoneContacts { get; set; }

        public PhoneApps PhoneApps { get; set; }

        public DbPlayer Slammer { get; set; }
        public DateTime LastSlam { get; set; }

        public Dictionary<uint, String> VehicleKeys { get; set; }

        public Dictionary<uint, String> OwnVehicles { get; set; }

        public Dictionary<int, Attachments.AttachmentItem> Attachments { get; set; }

        public Dictionary<uint, PlayerTask.PlayerTask> PlayerTasks { get; set; }

        public HashSet<uint> HouseKeys { get; set; }
        public HashSet<uint> StorageKeys { get; set; }

        public List<WeaponDetail> Weapons { get; set; }

        public bool IsInTask { get; set; }
        public Rank Rank { get; set; }

        public Procedure Procedure { get; set; }
        public Dictionary<string, string> ProcedureData { get; set; } = new Dictionary<string, string>();
        public string CurrentProcedureKey { get; set; }

        // Reworked Stuff
        public Team Team { get; private set; }
        public TeamSubgroup TeamSubgroup { get; private set; }

        public List<Banks.BankHistory.BankHistory> BankHistory { get; set; }
        public AnimationScenario AnimationScenario { get; set; }

        public uint ActiveBusinessId { get; set; }

        public List<CrimePlayerReason> Crimes { get; set; }

        public List<CrimePlayerHistory> CrimeHistories { get; set; }

        public Animal.Animal PlayerPed { get; set; }

        public bool[] PedLicense { get; set; }

        public DateTime? FreezedUntil { get; set; }

        public dynamic[] DbValues { get; }

        public int CurrentSeat { get; set; } = 0;
        public int CurrentSeatIndex { get; set; } = 0;

        public Container Container { get; set; }
        public Container TeamFightContainer { get; set; }
        public FunkStatus funkStatus { get; set; }
        public FunkStatus funkAirStatus { get; set; }

        public int Swat { get; set; }
        public DateTime xmasLast { get; set; }
        public DateTime DrugCreateLast { get; set; }
        public DateTime LastPhoneNumberChange { get; set; }

        public PhoneSetting PhoneSettings { get; set; }

        public Wallpaper Wallpaper { get; set; }
        public Ringtone Ringtone { get; set; }
        public List<DbPlayer> PlayerWhoHearRingtone { get; set; }

        public CustomData CustomData { get; set; }
        public int VehicleTaxSum { get; set; }

        public bool IsSpeaking = false;

        public bool NoClip = false;

        public int TeamfightKillCounter = 0;

        public DateTime LastUninvite { get; set; }

        public DateTime LastPaydayChanged { get; set; }

        public Dictionary<uint, uint> AnimationShortcuts { get; set; }

        public List<Outfit> Outfits { get; set; }

        public DateTime LastReport { get; set; }
        public Container PrisonLockerContainer { get; set; }
        public Container MethlaboratoryInputContainer { get; set; }
        public Container MethlaboratoryOutputContainer { get; set; }
        public Container WeaponlaboratoryInputContainer { get; set; }
        public Container WeaponlaboratoryOutputContainer { get; set; }
        public Container CannabislaboratoryInputContainer { get; set; }
        public Container CannabislaboratoryOutputContainer { get; set; }
        public Container HeroinlaboratoryInputContainer { get; set; }
        public Container HeroinlaboratoryOutputContainer { get; set; }

        public Container WorkstationFuelContainer { get; set; }
        public Container WorkstationSourceContainer { get; set; }
        public Container WorkstationEndContainer { get; set; }

        public Container ChristmasContainer { get; set; }

        public uint WorkstationId { get; set; }

        public Dictionary<uint, int> DeliveryJobSkillPoints { get; set; }

        public bool ParamedicLicense { get; set; }

        public string GovLevel { get; set; }

        public int RacingBestTimeSeconds { get; set; }

        public Dictionary<EventListIds, int> EventDoneList = new Dictionary<EventListIds, int>();

        public EconomyIndex EconomyIndex { get; set; }
        public Dictionary<uint, DbEmail> Emails { get; set; }
        public Dictionary<string, int> ChristmasPresents = new Dictionary<string, int>();
        public Dictionary<string, DbPlayerDataCustom> PlayerDataCustom { get; set; }
        //public Dictionary<DbPlayer, HashSet<VoicePlayerExtension.VoiceEnabledState>> EnabledToVoicePlayers { get; set; }
        public bool Mars { get; set; }
        public List<PlayerAktie> Aktien { get; set; }
        public PlayerDepot Depot { get; set; }
        public DateTime TimeSinceTreatment { get; set; }
        public bool RecentlyInjured { get; set; }

        // Anim Sync
        public bool PlayingAnimation { get; set; }
        public int CurrentAnimFlags { get; set; }
        public string AnimationDict { get; set; }
        public string AnimationName { get; set; }
        public float AnimationSpeed { get; set; }
        public bool IsInWater { get; set; }
        public DateTime LastDeath { get; set; }
        public DateTime LastSpawnEvent { get; set; }

        public Dictionary<uint, PlayerCWS> CWS { get; set; }

        public int InsuranceType { get; set; }
        public Dictionary<uint, List<Weapons.Component.WeaponComponent>> WeaponComponents { get; set; }

        public int AnimalType { get; set; }

        public PlayerInventoryBag InventoryClothesBag { get; set; }

        public ConcurrentDictionary<OperationType, DateTime> SpamProtection { get; set; }

        public FindFlags FindFlags { get; set; }

        public bool InParamedicDuty { get; set; }

        public string AuthKey { get; set; }

        public bool IsProgressBarRunning { get; set; }
        public CancellationTokenSource CancellationToken { get; set; } = null;

        public Payment.PaymentStatus ChoosenPaymentState { get; set; }

        public DateTime LastShapeSynced { get; set; }

        public string RemoteHashKey { get; set; }

        public List<uint> AssignedAnimals { get; set; }
        public bool ActionsBlocked { get; set; } // hauptsächlich beim ejecten vom Fahrzeug genutzt, damit das Handy + Funkgerät nicht genutzt werden kann

        public DbPlayer(MySqlDataReader reader)
        {
            DbValues = new dynamic[DbColumns.Length];

            CWS = new Dictionary<uint, PlayerCWS>();
            AssignedAnimals = new List<uint>();

            IsInTask = false;
            Crimes = new List<CrimePlayerReason>();
            CrimeHistories = new List<CrimePlayerHistory>();

            WeaponComponents = new Dictionary<uint, List<Weapons.Component.WeaponComponent>>();

            LastShapeSynced = DateTime.Now.AddMinutes(-2);

            RankDuty = RankDutyStatus.OffDuty;

            funkAirStatus = FunkStatus.Deactive;

            RankId = reader.GetUInt32("rankId");
            TeamId = reader.GetUInt32("team");
            TeamSubgroupId = reader.GetUInt32("teamSubgroupId");
            TeamRank = reader.GetUInt32("rang");
            Level = reader.GetInt32("Level");
            Duty = reader.GetUInt32("duty") == 1;
            IsCuffed = reader.GetInt32("isCuffed") == 1;
            IsTied = reader.GetInt32("isTied") == 1;
            Hp = reader.GetInt32("hp");
            Swat = reader.GetInt32("swat");
            xmasLast = reader.GetDateTime("xmasLast");
            DrugCreateLast = reader.GetDateTime("drugcreatelast");
            UHaftTime = reader.GetInt32("uhaft");
            Einwanderung = reader.GetInt32("einwanderung");
            SwatDuty = reader.GetInt32("swatduty");
            Teamfight = reader.GetInt32("teamfight");
            Paintball = reader.GetInt32("paintball");
            InsuranceType = reader.GetInt32("health_insurance");

            AnimalType = reader.GetInt32("animaltype");

            IsNSAState = reader.GetInt32("nsalic");
            IsNSADuty = false;

            VoiceEnabled = false;

            Suspension = reader.GetInt32("suspendate") == 1;
            WDutyTime = reader.GetInt32("w_dutytime");
            marryLic = reader.GetInt32("marrylic");
            ParamedicLicense = reader.GetInt32("mediclic") == 1;
            GovLevel = reader.GetString("gov_level");
            RacingBestTimeSeconds = reader.GetInt32("racing_besttime");

            int[] temp = new int[] { 0, 0 };

            temp[0] = reader.GetInt32("armor");
            VisibleArmorType = reader.GetInt32("visibleArmorType");
            IsSwimmingOrDivingDoNotUse = false;

            DbValues[(uint)Value.Duty] = Duty;
            DbValues[(uint)Value.RankId] = RankId;
            DbValues[(uint)Value.TeamId] = TeamId;
            DbValues[(uint)Value.TeamSubgroupId] = TeamSubgroupId;
            DbValues[(uint)Value.Level] = Level;
            DbValues[(uint)Value.TeamRang] = TeamRank;
            DbValues[(uint)Value.IsCuffed] = IsCuffed;
            DbValues[(uint)Value.IsTied] = IsTied;
            DbValues[(uint)Value.Hp] = Hp;
            DbValues[(uint)Value.Armor] = temp[0];
            DbValues[(uint)Value.Swat] = Swat;
            DbValues[(uint)Value.UHaftTime] = UHaftTime;
            DbValues[(uint)Value.Einwanderung] = Einwanderung;
            DbValues[(uint)Value.SwatDuty] = SwatDuty;
            DbValues[(uint)Value.Teamfight] = Teamfight;
            DbValues[(uint)Value.Suspension] = Suspension;
            DbValues[(uint)Value.WDutyTime] = WDutyTime;
            DbValues[(uint)Value.Paintball] = Paintball;

            Armor = temp;

            PlayerData = new ConcurrentDictionary<string, dynamic>();
            VehicleKeys = new Dictionary<uint, string>();
            Attachments = new Dictionary<int, Attachments.AttachmentItem>();
            LastReport = DateTime.Now.Subtract(TimeSpan.FromMinutes(10));
            DeliveryJobSkillPoints = new Dictionary<uint, int>();

            Mars = false;
            Depot = null;
            TimeSinceTreatment = DateTime.Now;
            RecentlyInjured = false;

            PlayingAnimation = false;
            AnimationDict = "";
            AnimationName = "";
            CurrentAnimFlags = 0;
            AnimationSpeed = 8.0f;

            IsInWater = false;
            LastDeath = DateTime.Now;
            LastSpawnEvent = DateTime.Now.AddMinutes(-2);

            SpamProtection = new ConcurrentDictionary<OperationType, DateTime>();
            SpamProtection.TryAdd(OperationType.InventoryOpened, DateTime.Now.AddSeconds(10));
            ActionsBlocked = false;

            FindFlags = (FindFlags)reader.GetInt32("fib_find_flags");
            InParamedicDuty = false;

            AuthKey = Helper.Helper.GenerateAuthKey();
            CancellationToken = null;

            this.SetACLogin();
        }

        public void SetData(string key, dynamic value)
        {
            if (PlayerData.ContainsKey(key))
            {
                PlayerData[key] = value;
            }
            else
            {
                lock (PlayerData)
                {
                    if (PlayerData.ContainsKey(key))
                        PlayerData.TryRemove(key, out dynamic val);

                    PlayerData.TryAdd(key, value);
                }
            }
        }

        public bool HasData(string key)
        {
            return (PlayerData.ContainsKey(key));
        }

        public void ResetData(string key)
        {
            if (PlayerData.ContainsKey(key)) PlayerData.TryRemove(key, out dynamic val);
        }

        public dynamic GetData(string key)
        {
            var result = (PlayerData.ContainsKey(key)) ? PlayerData[key] : "";
            return result;
        }

        public bool TryData<T>(string key, out T value)
        {
            var tmpdata = PlayerData.ContainsKey(key);
            value = tmpdata ? (T)PlayerData[key] : default(T);
            return tmpdata;
        }

        public void SetRank(uint rank)
        {
            RankId = rank;
            Player?.TriggerEvent("updateTeamRank", rank);
        }

        public void Spawn(Vector3 pos)
        {
            NAPI.Player.SpawnPlayer(this.Player, pos);
        }

        public void SetArmorPlayer(int Armor)
        {
            if (AccountStatus != AccountStatus.LoggedIn) return;

            Task.Run(async () =>
            {
                if (Armor >= 99) Armor = 99;

                SetData("ac_lastArmor", Armor);

                SetData("serverArmorChanged", 2);
                MetaData.Armor = Armor;

                NAPI.Task.Run(() => { Player.Armor = Armor; });

                await Task.Delay(500);

                this.ApplyArmorVisibility();
                this.Save();
            });


        }

        public void SetDimension(uint dimension)
        {
            if (AccountStatus != AccountStatus.LoggedIn) return;

            SetData("serverDimensionChange", 2);
            Dimension[0] = dimension;

            NAPI.Task.Run(() =>
            {
                Player.Dimension = dimension;

                if (this.IsAnimalActiv())
                {
                    PlayerPed.Ped.Dimension = dimension;
                }
            });
        }

        /// <summary>
        /// ACHTUNG! Das ist Fake-Async. Warum? Im Code werden synchrone Funktionen im asynchronen Kontext aufgerufen.
        /// Das heißt, dass auch synchrone Funktionen außerhalb des MainThreads laufen (teilweise).
        /// Deshalb, wird hier nochmal explizit auf den Main-Thread gewartet (was in einem async-Task passieren muss).
        /// </summary>
        /// <param name="dimension">Dimension, in die der Spieler gesetzt werden soll.</param>
        public void SetDimensionAsync(uint dimension, bool resetToPrevAfterDelay = false, int resetDelay = 100)
        {
            Task.Run(async () =>
            {
                if (AccountStatus != AccountStatus.LoggedIn)
                    return;

                await NAPI.Task.WaitForMainThread();
                SetData("serverDimensionChange", 2);

                if (this.IsAnimalActiv())
                {
                    PlayerPed.Ped.Dimension = dimension;
                }

                if (resetToPrevAfterDelay)
                {
                    uint prevDimension = Dimension[0];

                    Player.Dimension = dimension;
                    Dimension[0] = dimension;

                    await Task.Delay(resetDelay);
                    SetDimensionAsync(prevDimension); // Rekursiv
                }
                else
                {
                    Player.Dimension = dimension;
                    Dimension[0] = dimension;
                }

                await Task.Delay(5);
            });
        }

        public void WarpOutOfVehicle(bool checkAC = false)
        {
            if (AccountStatus != AccountStatus.LoggedIn) return;

            if (RageExtension.IsInVehicle)
            {
                Player.WarpOutOfVehicle();
            }
            if (checkAC)
            {
                NAPI.Task.Run(() =>
                {
                    if (RageExtension.IsInVehicle)
                    {
                        Player.Kick();
                    }
                }, 5000);
            }
        }
        public void SetTeam(uint teamid, bool resetweaponsonDuty = true)
        {
            if (Team != null && Team.HasDuty && resetweaponsonDuty)
            {
                if (Duty)
                {
                    Duty = false;
                    this.RemoveWeapons();
                    this.ResetAllWeaponComponents();
                }
            }

            // remove if exist
            if (TeamId != 0)
            {
                Team team = TeamModule.Instance[TeamId];
                if (team != null && team.Members.Values.ToList().Contains(this))
                {
                    TeamModule.Instance[TeamId]?.RemoveMember(this);
                }
            }

            TeamId = teamid;
            Team = TeamModule.Instance[teamid];

            // add to new team
            if (teamid != 0)
            {
                Team?.AddMember(this);
            }

            NAPI.Task.Run(() =>
            {
                Player?.TriggerEvent("updateTeamId", teamid);
            });
        }

        public void SetTeamSubgroup(uint teamSubgroupId)
        {
            // remove if exist
            if (TeamSubgroupId != 0)
            {
                TeamSubgroup teamSubgroup = TeamSubgroupModule.Instance[TeamSubgroupId];
                if (teamSubgroup != null && teamSubgroup.Members.Values.ToList().Contains(this))
                {
                    TeamSubgroupModule.Instance[TeamSubgroupId]?.RemoveMember(this);
                }
            }

            TeamSubgroupId = teamSubgroupId;
            TeamSubgroup = TeamSubgroupModule.Instance[teamSubgroupId];

            // add to new team
            if (teamSubgroupId != 0)
            {
                TeamSubgroup?.AddMember(this);
            }

            NAPI.Task.Run(() =>
            {
                Player?.TriggerEvent("updateTeamSubgroupId", teamSubgroupId);
            });
        }

        public void SetTeamfight()
        {
            Teamfight = Team.IsInTeamfight() ? 1 : 0;
            PlayerDb.Save(this);
        }

        public void SetPaintball(int p)
        {
            Paintball = p;
            SavePaintballState();
        }

        public bool UpdateApps()
        {
            if (TeamId == (uint)TeamList.Zivilist)
            {
                this.PhoneApps.Remove("TeamApp");
            }
            else
            {
                this.PhoneApps.Add("TeamApp");
            }

            if (ActiveBusinessId == 0)
            {
                this.PhoneApps.Remove("BusinessApp");
            }
            else
            {
                this.PhoneApps.Add("BusinessApp");
            }

            if (!VoiceModule.Instance.hasPlayerRadio(this))
            {
                this.funkStatus = FunkStatus.Deactive;
                VoiceModule.Instance.ChangeFrequenz(this, 0.0f);
                this.PhoneApps.Remove("FunkApp");
            }
            else
            {
                this.PhoneApps.Add("FunkApp");
            }

            return true;
        }

        public bool IsAMedic()
        {
            return Team != null && Team.IsMedics();
        }

        public bool IsCopPackGun()
        {
            return Team != null && (Duty && (Team.IsCops() || Team.IsDpos() || Team.IsMedics() || Team.Id == (int)TeamTypes.TEAM_DRIVINGSCHOOL || Team.Id == (int)TeamTypes.TEAM_NEWS) || IsNSADuty);
        }

        public bool IsACop()
        {
            return Team != null && Team.IsCops();
        }

        public bool IsStaatsfraktion()
        {
            return Team != null && Team.IsStaatsfraktion();
        }

        public bool IsAnimal()
        {
            return AnimalType > 0;
        }

        public bool CanEquipBeamtenWesten()
        {
            return IsCopPackGun();
        }
        public bool IsGoverment()
        {
            return Team != null && Team.Id == 14;
        }

        public bool IsAGangster()
        {
            return Team.IsGangsters();
        }

        public bool IsBadOrga()
        {
            return Team.IsBadOrga();
        }

        public bool IsHomeless()
        {
            return !this.IsTenant() && OwnHouse[0] == 0;
        }
        public void UpdateWimmingOrDiving()
        {
            this.Player.TriggerEvent("isPlayerSwimming");
        }

        [RemoteEvent]
        public void swimmingOrDivingResponse(Player player, bool swimStatus, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            IsSwimmingOrDivingDoNotUse = swimStatus;
        }

        public void Kick(string reason = "")
        {
            Player.SendNotification("Du wurdest gekickt. Grund: " + reason);
            Player.Kick(reason);
        }

        internal void PlayAnimation(int animationFlags, string animationDict, string animationName, float speed = 8f, bool holdweapon = false)
        {
            if (Player == null) return;

            PlayingAnimation = true;
            AnimationDict = animationDict;
            AnimationName = animationName;
            CurrentAnimFlags = animationFlags;
            AnimationSpeed = speed;

            Player.TriggerNewClient("SetOwnAnimData", JsonConvert.SerializeObject(new AnimationSyncItem(this)));

            // Sync für den Fall, dass man durch eine Tür geht. Damit die Anim für andere nicht wieder startet
            var nearPlayers = Players.Instance.GetPlayersListInRange(Player.Position, 100.0f); //NAPI.Player.GetPlayersInRadiusOfPlayer(100.0f, Player);
            foreach (DbPlayer dbPlayer in nearPlayers)
            {
                if (dbPlayer != null && dbPlayer.IsValid())
                {
                    dbPlayer.Player.TriggerNewClient("SetAnimDataNear", Player, JsonConvert.SerializeObject(new AnimationSyncItem(this)));
                }
            }

            NAPI.Task.Run(() =>
            {
                NAPI.Player.PlayPlayerAnimation(Player, animationFlags, animationDict, animationName, speed);
                if (!holdweapon)
                    NAPI.Player.SetPlayerCurrentWeapon(Player, WeaponHash.Unarmed);
            });
        }

        public string GetName(bool ReturnFakeNameIfAssigned = false)
        {
            // Fallback für z.B. Loginwindow
            if (RageExtension == null)
            {
                return Player.Name;
            }

            var l_Name = RageExtension.Name;

            if (ReturnFakeNameIfAssigned)
            {
                if (fakePerso && fakeName.Length > 0 && fakeSurname.Length > 0)
                    l_Name = $"{fakeName}_{fakeSurname}";
            }

            return l_Name;
        }

        public async Task PlayInventoryInteractAnimation(int time = 1500)
        {
            if (RageExtension.IsInVehicle) return;

            PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");
            Player.TriggerNewClient("freezePlayer", true);
            await Task.Delay(time);
            Player.TriggerNewClient("freezePlayer", false);

            this.StopAnimation();

            await Task.Delay(500);
            this.SyncAttachmentOnlyItems();
        }

        public async Task PlayInventoryGiveInteractAnimation(int time = 2800)
        {
            if (RageExtension.IsInVehicle) return;

            PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_common", "givetake1_a");
            Player.TriggerNewClient("freezePlayer", true);
            await Task.Delay(time);
            Player.TriggerNewClient("freezePlayer", false);

            this.StopAnimation();

            await Task.Delay(500);
            this.SyncAttachmentOnlyItems();
        }

        public void SaveArmorType(int type)
        {
            string query = $"UPDATE `player` SET visibleArmorType = '{type}' WHERE id = '{Id}';";
            MySQLHandler.ExecuteAsync(query);
        }

        public void SaveBlackMoneyBank()
        {
            string query = $"UPDATE `player` SET blackmoneybank = '{BlackMoneyBank[0]}' WHERE id = '{Id}';";
            BlackMoneyBank[1] = BlackMoneyBank[0];
            MySQLHandler.ExecuteAsync(query);
        }

        public void SaveAnimalType()
        {
            string query = $"UPDATE `player` SET animaltype = '{AnimalType}' WHERE id = '{Id}';";
            MySQLHandler.ExecuteAsync(query);
        }

        public void SaveInsurance()
        {
            string query = $"UPDATE `player` SET health_insurance = '{InsuranceType}' WHERE id = '{Id}';";
            BlackMoneyBank[1] = BlackMoneyBank[0];
            MySQLHandler.ExecuteAsync(query);
        }
        public void SavePaintballState()
        {
            string query = $"UPDATE `player` SET paintball = '{Paintball}' WHERE id = '{Id}';";
            MySQLHandler.Execute
                (query);
        }

        public void SetSkin(PedHash Skin)
        {
            SetData("ignoreGodmode", 2);
            SetData("ac-healthchange", 2);
            Player.SetSkin(Skin);
            this.SetHealth(99);

            AttachmentModule.Instance.RemoveAllAttachments(this);
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                AttachmentModule.Instance.RemoveAllAttachments(this);
            });
        }

        public void GiveServerWeapon(WeaponHash weaponHash, int ammo)
        {
            NAPI.Task.Run(() =>
            {
                Dictionary<WeaponHash, int> weapons = new Dictionary<WeaponHash, int>();

                if (HasData("ac-compareweaponobject"))
                {
                    weapons = GetData("ac-compareweaponobject");
                }

                Player.GiveWeapon(weaponHash, ammo);

                if (weapons.ContainsKey(weaponHash))
                {
                    weapons[weaponHash] += ammo;
                }
                else weapons.Add(weaponHash, ammo);

                SetData("ac-compareweaponobject", weapons);
            });
        }

        public void RemoveServerWeapon(WeaponHash weaponHash)
        {
            NAPI.Task.Run(() =>
            {
                Dictionary<WeaponHash, int> weapons = new Dictionary<WeaponHash, int>();


                if (this.HasWeaponComponentsForWeapon((uint)weaponHash))
                {
                    this.RemoveAllWeaponComponents((uint)weaponHash);
                }

                Player.RemoveWeapon(weaponHash);

                if (HasData("ac-compareweaponobject"))
                {
                    weapons = GetData("ac-compareweaponobject");

                    if (weapons.ContainsKey(weaponHash))
                    {
                        weapons.Remove(weaponHash);
                    }
                }

                SetData("ac-compareweaponobject", weapons);
            });
        }

        public void RemoveAllServerWeapons(bool pWipeCollection = true)
        {
            NAPI.Task.Run(() =>
            {
                this.SetData("ac-ignorews", 2);

                Player.RemoveAllWeapons();

                if (pWipeCollection)
                    Weapons.Clear();

                SetData("ac-compareweaponobject", new Dictionary<WeaponHash, int>());
            });
        }

        public void SetServerWeaponAmmo(WeaponHash weaponHash, int ammo)
        {
            NAPI.Task.Run(() =>
            {
                Dictionary<WeaponHash, int> weapons = new Dictionary<WeaponHash, int>();

                if (HasData("ac-compareweaponobject"))
                {
                    weapons = GetData("ac-compareweaponobject");
                }

                Player.SetWeaponAmmo(weaponHash, ammo);

                if (weapons.ContainsKey(weaponHash))
                {
                    weapons[weaponHash] = ammo;
                }
                else weapons.Add(weaponHash, ammo);

                SetData("ac-compareweaponobject", weapons);
            });
        }

        public void SetSkin(uint SkinUint)
        {
            string s = "" + SkinUint;

            if (Enum.TryParse<PedHash>(s, true, out PedHash skin))
            {
                SetSkin(skin);
            }
        }

        public bool IsOrtable(DbPlayer fromPlayer, bool ignoreTimer = false)
        {
            // Fib ortung vor allem
            if (fromPlayer.IsNSADuty)
            {
                if (GovLevel.ToLower() == "a" || GovLevel.ToLower() == "b" || GovLevel.ToLower() == "c") return true;

                if (IsNSAState >= (int)NSA.NSARangs.LIGHT) return true;

                NSAObservation nSAObservation = NSAObservationModule.ObservationList.ToList().FirstOrDefault(o => o.Value.PlayerId == Id).Value;
                if (nSAObservation != null && nSAObservation.Agreed &&
                    (this.Container.GetItemAmount(174) >= 1 ||
                    this.Container.GetItemAmount(173) >= 1 ||
                    this.Container.GetItemAmount(183) >= 1)) return true;
            }

            if (IsStaatsfraktion())
            {
                if (CrimeModule.Instance.CalcWantedStars(this.Crimes) == 0 && !fromPlayer.FindFlags.HasFlag(FindFlags.Beamte))
                {
                    fromPlayer.SendNewNotification("Beamte können nicht ohne Haftbefehl geortet werden.");
                    fromPlayer.SendNewNotification("Um Beamte ohne Haftbefehl Orten zu koennen, benoetigen Sie eine seperate Freischaltung der FIB Direktion.");
                    return false;
                }
            }
            else
            {
                if (CrimeModule.Instance.CalcWantedStars(this.Crimes) == 0 && !fromPlayer.FindFlags.HasFlag(FindFlags.WithoutWarrant))
                {
                    fromPlayer.SendNewNotification("Es liegt kein Haftbefehl gegen den Bürger vor.");
                    fromPlayer.SendNewNotification("Um ohne Haftbefehl Orten zu koennen, benoetigen Sie eine Freischaltung der FIB Direktion.");
                    return false;
                }
            }

            if (this.IsAGangster() && GangwarTownModule.Instance.IsTeamInGangwar(this.Team) && this.Dimension[0] == GangwarModule.Instance.DefaultDimension)
            {
                fromPlayer.SendNewNotification("Person konnte nicht geortet werden!");
                return false;
            }

            if (!ignoreTimer && this.HasData("isOrted_" + fromPlayer.TeamId))
            {
                DateTime isOrted = this.GetData("isOrted_" + fromPlayer.TeamId);
                if (isOrted > DateTime.Now)
                {
                    fromPlayer.SendNewNotification(
                        "Bürger wurde bereits geortet! (Nur jede Minute 1x möglich)");
                    return false;
                }
            }

            if (this.Container.GetItemAmount(174) >= 1 && !this.PhoneSettings.flugmodus)
            {
                return true;
            }
            return false;
        }
    }

    public class AnimationSyncItem
    {
        public bool Active { get; set; }
        public string AnimationDict { get; set; }
        public string AnimationName { get; set; }
        public int AnimationFlags { get; set; }
        public float AnimationSpeed { get; set; }
        public float Heading { get; set; }

        public AnimationSyncItem(bool active, string animationDict, string animationName, int animationFlags,
            float animationSpeed, float heading)
        {
            Active = active;
            AnimationDict = animationDict;
            AnimationName = animationName;
            AnimationFlags = animationFlags;
            AnimationSpeed = animationSpeed;
            Heading = heading;
        }

        public AnimationSyncItem(DbPlayer dbPlayer)
        {
            Active = dbPlayer.PlayingAnimation;
            AnimationDict = dbPlayer.AnimationDict;
            AnimationFlags = dbPlayer.CurrentAnimFlags;
            AnimationName = dbPlayer.AnimationName;
            AnimationSpeed = dbPlayer.AnimationSpeed;
            Heading = dbPlayer.Player.Heading;
        }
    }
}