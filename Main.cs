//  ____________________________________________________________________________
// |=========================[  GVMP.DE v0.1a  ]===============================|
// |========= Dieses Script ist Uhreberrechtlich geschuetzt! ==================|
// |====== _________________________ ==========================================|
// |======| Scripter bzw Uhrheber:  |========= ________________ ===============|
// |======| Daniel Albert           |=========| Version: 0.1a  |===============|
// |======| Puettlingerstraße 87    |=========|________________|===============|
// |======| 66773 Schwalbach/Saar   |==========================================|
// |======|_________________________|==========================================|
// |===========================================================================|
// |===========================================================================|
// |===========================================================================|
// |_____________________|   Urheberrecht   |__________________________________
// |                                                                           |
// |  Die durch den Scripter erstellten Inhalte und Werke auf diesem           |
// |  Script unterliegen dem deutschen Urheberrecht.                           |
// |  Die Vervielfaeltigung, Bearbeitung, Verbreitung und jede Art der         |
// |  Verwertung außerhalb der Grenzen des Urheberrechtes beduerfen der        |
// |  schriftlichen Zustimmung des jeweiligen Autors bzw. Erstellers.          |
// |___________________________________________________________________________|

using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module;
using VMP_CNR.Module.Ammunations;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.Banks;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Character;
using VMP_CNR.Module.Clothes.Shops;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Jobs.Bus;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Maps;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Menu.Menus.Account;
using VMP_CNR.Module.Menu.Menus.Business;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Robbery;
using VMP_CNR.Module.Shops;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Sync;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;
using VMP_CNR.Module.Vehicles.Garages;
using VMP_CNR.Module.Vehicles.Mod;
using VMP_CNR.Module.Vehicles.Shops;
using Timer = System.Timers.Timer;
using VehicleData = VMP_CNR.Module.Vehicles.Data.VehicleData;
using VMP_CNR.Module.Vehicles.Windows;
using VMP_CNR.Module.Players.Events;
using VMP_CNR.Module.Banks.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.LifeInvader.App;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.News.App;
using static VMP_CNR.Module.Chat.Chats;
using VMP_CNR.Module.VehicleSpawner;
using VMP_CNR.Module.Konversations;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Weather;
using VMP_CNR.Module.Service;
using VMP_CNR.Module.AsyncEventTasks;
using VMP_CNR.Module.Menu.Menus.Armory;
using VMP_CNR.Module.VehicleRent;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Clothes.Slots;
using VMP_CNR.Module.Anticheat;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Clothes.Windows;
using VMP_CNR.Module.Players.NAPIWrapper;
using VMP_CNR.Module.Threading;

namespace VMP_CNR
{
    public enum AccountStatus
    {
        Registered = 0,
        LoggedIn = 1
    }

    internal enum VoiceRange
    {
        normal = 8,
        whisper = 3,
        shout = 20,
        megaphone = 50,
    }

    internal enum AdminLevelTypes
    {
        Player = 0,
        Supporter = 1,
        Moderator = 2,
        Administrator = 3,
        SuperAdministrator = 4,
        Manager = 5,
        Projektleitung = 6,
        Entwicklungsleitung = 8,
        SeniorEntwickler = 11,
        Gamedesigner = 13,
        Gamedesignleitung = 14,
        SeniorGamedesigner = 23
    }

    public enum adminLogTypes
    {
        perm = 1,
        timeban = 2,
        kick = 3,
        warn = 4,
        log = 5,
        whisper = 6,
        setitem = 7,
        coord = 8,
        setmoney = 9,
        veh = 10,
        arev = 11,
        setdpos = 12,
        setgarage = 13,
    }

    public enum teams
    {
        TEAM_CIVILIAN = 0,
        TEAM_POLICE = 1,
        TEAM_BALLAS = 2,
        TEAM_DRIVINGSCHOOL = 3,
        TEAM_NEWS = 4,
        TEAM_FIB = 5,
        TEAM_LOST = 6,
        TEAM_MEDIC = 7,
        TEAM_IRISHMOB = 8,
        TEAM_LCN = 9,
        TEAM_YAKUZA = 10,
        TEAM_HUSTLER = 11,
        TEAM_GROVE = 12,
        TEAM_ARMY = 13,
        TEAM_GOV = 14,
        TEAM_CAYO = 15,
        TEAM_DPOS = 16,
        TEAM_TRIADEN = 17,
        TEAM_VAGOS = 18,
        TEAM_MARABUNTA = 19,
        TEAM_NNM = 20,
        TEAM_SWAT = 21,
        TEAM_PLACEHOLDER_1 = 22,
        TEAM_IAA = 23,
        TEAM_HOH = 24,
        TEAM_REDNECKS = 25,
        TEAM_LSC = 26,
        TEAM_ICA = 27,
        TEAM_MINE1 = 28,
        TEAM_MINE2 = 29,
        TEAM_UNICORN = 30,
        TEAM_CARSELL1 = 31,
        TEAM_CARSELL2 = 32,
        TEAM_CARSELL3 = 33,
        TEAM_CARSELL4 = 34,
        TEAM_CARSELL5 = 35,
        TEAM_CARSELL6 = 36,
        TEAM_CARSELL7 = 37,
        TEAM_CARSELL8 = 38,
        TEAM_CARSELL9 = 39,
        TEAM_AUCTION = 40,
        TEAM_CAFE_PLAZA = 41,
        TEAM_OUTLAWS_MC = 42,
        TEAM_PANTHERS = 43,
        TEAM_MADRAZO = 44,
        TEAM_BOSOZOKUKAI = 45,
        TEAM_VOLKY = 46,
        TEAM_BRATWA = 47,
        TEAM_ORGANISAZIJA = 48,
        TEAM_FALCONE = 49,
        TEAM_DEADLOCK = 50
    }

    internal enum jobs
    {
        JOB_WEAPONDEALER = 1,
        JOB_PLAGIAT = 2,
        JOB_MECH = 8,
        JOB_Makler = 11,
        JOB_ANWALT = 14,
    }

    [Flags]
    public enum AnimationFlags
    {
        Loop = 1 << 0,
        StopOnLastFrame = 1 << 1,
        OnlyAnimateUpperBody = 1 << 4,
        AllowPlayerControl = 1 << 5,
        Cancellable = 1 << 7,
        AllowRotation = 32,
        CancelableWithMovement = 128,
        RagdollOnCollision = 4194304
    }

    public class Main : Script
    {
        public static bool devmode;
        public static bool ptr;
        public static bool devlog;

        public static bool restart;

        public static string VoiceChannelName;
        public static string VoicePassword;

        public static List<Npc> ServerNpcs = new List<Npc>();
        public static List<Blip> ServerBlips = new List<Blip>();

        // Modcheck defines
        public static bool AllowOpenIV = false;

        public Dictionary<string, string> AllowedAsis = new Dictionary<string, string>();

        public static Dictionary<DbPlayer, DateTime> RefundPlayers = new Dictionary<DbPlayer, DateTime>();

        public static AsyncThread m_AsyncThread;

        //public static MySqlSyncThread m_MySqlThread = new MySqlSyncThread();

        public static string m_RestartReason;
        public static uint m_RestartMinuten;
        public static bool m_RestartScheduled = false;

        public Main()
        {
            // Constructer empty
        }

        static int HourTimer                                    = 1;
        public static DateTime adLastSend                       = DateTime.Now;
        public static List<LifeInvaderApp.AdsFound> adList      = new List<LifeInvaderApp.AdsFound>();
        public static List<NewsListApp.NewsFound> newsList      = new List<NewsListApp.NewsFound>();
        public static GTANetworkAPI.Weather m_CurrentWeather    = GTANetworkAPI.Weather.CLEAR;
        public static GTANetworkAPI.Weather m_DestWeather       = GTANetworkAPI.Weather.CLEAR;
        public static bool WeatherOverride = false;

        static int mysqlSaveInterval = 0;
        static int failelessSaving = 0;

        // Intervals
        DateTime minTimer;

        static DateTime SavePlayerCheck;

        static Timer timer = new Timer();
        static Timer bckpTimer = new Timer();

        public static Random Random = new Random();

        string[] Guns =
        {
            "Parachute", "Unarmed", "SniperRifle", "FireExtinguisher", "Snowball", "VintagePistol",
            "CombatPDW", "HeavySniper", "MicroSMG", "Pistol", "PumpShotgun",
            "APPistol", "Molotov", "SMG", "StickyBomb", "PetrolCan",
            "StunGun", "HeavyShotgun", "Minigun", "GolfClub", "GrenadeLauncherSmoke",
            "Hammer", "CombatPistol", "Gusenberg", "HomingLauncher", "Nightstick",
            "Railgun", "SawnOffShotgun", "BullpupRifle", "Firework", "CombatMG",
            "CarbineRifle", "Crowbar", "Dagger", "Grenade", "Bat",
            "Pistol50", "Knife", "MG", "BullpupShotgun", "BZGas",
            "Musket", "ProximityMine", "AdvancedRifle", "RPG",
            "SNSPistol", "AssaultRifle", "SpecialCarbine", "MarksmanRifle", "HeavyPistol",
            "KnuckleDuster", "MarksmanPistol", "AssaultShotgun", "AssaultSMG", "Hatchet",
            "SmokeGrenade", "Bottle", "Machete", "Flashlight", "SwitchBlade",
            "Poolcue", "Wrench", "Battleaxe", "Revolver", "FlareGun",
            "MachinePistol", "MiniSMG", "CompactRifle", "DoubleBarrelShotgun",
            "Autoshotgun", "Flare", "Unarmed",
            "SMG"
        };

        string[] restrictedGuns = {"railgun", "minigun", "rpg", "GrenadeLauncher", "CompactLauncher", "Pipebomb"};

        #region StartGamemode

        public static Dictionary<string, string> AnimationList = new Dictionary<string, string>
        {
            {"finger", "mp_player_intfinger mp_player_int_finger"},
            {"guitar", "anim@mp_player_intcelebrationmale@air_guitar air_guitar"},
            {"shagging", "anim@mp_player_intcelebrationmale@air_shagging air_shagging"},
            {"synth", "anim@mp_player_intcelebrationmale@air_synth air_synth"},
            {"kiss", "anim@mp_player_intcelebrationmale@blow_kiss blow_kiss"},
            {"bro", "anim@mp_player_intcelebrationmale@bro_love bro_love"},
            {"chicken", "anim@mp_player_intcelebrationmale@chicken_taunt chicken_taunt"},
            {"chin", "anim@mp_player_intcelebrationmale@chin_brush chin_brush"},
            {"dj", "anim@mp_player_intcelebrationmale@dj dj"},
            {"dock", "anim@mp_player_intcelebrationmale@dock dock"},
            {"facepalm", "anim@mp_player_intcelebrationmale@face_palm face_palm"},
            {"fingerkiss", "anim@mp_player_intcelebrationmale@finger_kiss finger_kiss"},
            {"freakout", "anim@mp_player_intcelebrationmale@freakout freakout"},
            {"jazzhands", "anim@mp_player_intcelebrationmale@jazz_hands jazz_hands"},
            {"knuckle", "anim@mp_player_intcelebrationmale@knuckle_crunch knuckle_crunch"},
            {"nose", "anim@mp_player_intcelebrationmale@nose_pick nose_pick"},
            {"no", "anim@mp_player_intcelebrationmale@no_way no_way"},
            {"peace", "anim@mp_player_intcelebrationmale@peace peace"},
            {"photo", "anim@mp_player_intcelebrationmale@photography photography"},
            {"rock", "anim@mp_player_intcelebrationmale@rock rock"},
            {"salute", "anim@mp_player_intcelebrationmale@salute salute"},
            {"shush", "anim@mp_player_intcelebrationmale@shush shush"},
            {"slowclap", "anim@mp_player_intcelebrationmale@slow_clap slow_clap"},
            {"surrender", "anim@mp_player_intcelebrationmale@surrender surrender"},
            {"thumbs", "anim@mp_player_intcelebrationmale@thumbs_up thumbs_up"},
            {"taunt", "anim@mp_player_intcelebrationmale@thumb_on_ears thumb_on_ears"},
            {"vsign", "anim@mp_player_intcelebrationmale@v_sign v_sign"},
            {"wank", "anim@mp_player_intcelebrationmale@wank wank"},
            {"wave", "anim@mp_player_intcelebrationmale@wave wave"},
            {"loco", "anim@mp_player_intcelebrationmale@you_loco you_loco"},
            {"piss", "missbigscore1switch_trevor_piss piss_loop"},
            {"revive", "amb@medic@standing@tendtodead@idle_a idle_a"},
            {"applaus", "amb@world_human_cheering@male_b base"},
            {"lay", "amb@world_human_bum_slumped@male@laying_on_right_side@base base"},
            {"fsit", "amb@world_human_picnic@female@base base"},
            {"msit", "anim@heists@fleeca_bank@ig_7_jetski_owner owner_idle"},
            {"situp", "amb@world_human_sit_ups@male@idle_a idle_a"},
            {"hide", "cover@pinned@ai@unarmed@low@_b pinned_l_corner"},
            {"crossarms", "anim@heists@heist_corona@single_team single_team_loop_boss"},
            {"fixing", "anim@heists@narcotics@funding@gang_idle gang_chatting_idle01"},
            {"joint_start", "amb@world_human_smoking_pot@male@base base"},
            {"joint_end", "amb@incar@male@smoking@exit exit"},
        };

        public static Dictionary<string, string> AnimStripList = new Dictionary<string, string>
        {
            {"boobs", "mini@strip_club@backroom@ stripper_b_backroom_idle_b"},
            {"strip1", "mini@strip_club@idles@stripper stripper_idle_06"},
            {"strip2", "mini@strip_club@idles@stripper stripper_idle_04"},
            {"strip3", "mini@strip_club@idles@stripper stripper_idle_05"},
        };

        public Dictionary<string, string> PedAnimationList = new Dictionary<string, string>
        {
            {"sit", "creatures@rottweiler@amb@world_dog_sitting@base base"},
        };

        public void InitGameMode()
        {
            //SecureEventThread.Instance.Init();

            //connection.Open();
            Modules.Instance.LoadAll();

            StartupScripts.OnStartup();

            //Workaround static main attributes, will be reworked soon
            devmode = Configuration.Instance.DevMode;
            ptr = Configuration.Instance.Ptr;
            devlog = Configuration.Instance.DevLog;

            VoiceChannelName = Configuration.Instance.VoiceChannel;
            VoicePassword = Configuration.Instance.VoiceChannelPassword;

            Logger.Debug("Load Busjob");
            BusJob.Instance.Load();

            Logger.Debug("Load Ammunations");
            Ammunations.Instance.Load();

            Logger.Debug("Load ItemOrderModule");
            ItemOrderModule.Instance.Load();


            Logger.Debug("Init old Menus");
            MenuManager.Instance.AddBuilder(new AccountLicenseMenuBuilder());
            MenuManager.Instance.AddBuilder(new AccountMenuBuilder());
            MenuManager.Instance.AddBuilder(new AccountVehicleKeyMenuBuilder());
            MenuManager.Instance.AddBuilder(new AccountHouseKeyMenuBuilder());
            MenuManager.Instance.AddBuilder(new AccountVehicleKeyChooseMenuBuilder());

            MenuManager.Instance.AddBuilder(new TeamWardrobeMenuBuilder());
            MenuManager.Instance.AddBuilder(new TeamWardrobeSkinsMenu());
            MenuManager.Instance.AddBuilder(new TeamWardrobeClothesMenu());
            MenuManager.Instance.AddBuilder(new TeamWardrobeClothesSelectionMenu());
            MenuManager.Instance.AddBuilder(new TeamWardrobePropsMenu());
            MenuManager.Instance.AddBuilder(new TeamWardrobePropsSelectionMenu());

            // Armory
            MenuManager.Instance.AddBuilder(new ArmoryMenuBuilder());
            MenuManager.Instance.AddBuilder(new ArmoryItemMenuBuilder());
            MenuManager.Instance.AddBuilder(new ArmoryWeaponMenuBuilder());
            MenuManager.Instance.AddBuilder(new ArmoryAmmoMenuBuilder());
            MenuManager.Instance.AddBuilder(new ArmoryArmorMenuBuilder());
            MenuManager.Instance.AddBuilder(new ArmoryComponentMenuBuilder());

            // Business
            MenuManager.Instance.AddBuilder(new BusinessEnterMenuBuilder());
            MenuManager.Instance.AddBuilder(new BusinessBankMenuBuilder());

            MenuManager.Instance.AddBuilder(new BusinessSafeMenuBuilder());

            // Tuning Mechaniker
            MenuManager.Instance.AddBuilder(new JobMechanmicMenuBuilder());

            // ItemOrder
            MenuManager.Instance.AddBuilder(new ItemOrderItemsMenuBuilder());
            MenuManager.Instance.AddBuilder(new ItemOrderMenuBuilder());
            MenuManager.Instance.AddBuilder(new ItemOrderOrdersMenuBuilder());

            // Tattoo
            MenuManager.Instance.AddBuilder(new TattooBuyMenuBuilder());
            MenuManager.Instance.AddBuilder(new TattooLicenseMenuBuilder());
            MenuManager.Instance.AddBuilder(new TattooLicensePaginationMenuBuilder());
            MenuManager.Instance.AddBuilder(new TattooLaseringMenuBuilder());
            MenuManager.Instance.AddBuilder(new TattooBankMenuBuilder());

            //Freiberufe
            MenuManager.Instance.AddBuilder(new FreiberufMowerMenuBuilder());

            // Customization
            MenuManager.Instance.AddBuilder(new CustomizationMenuBuilder());

            // Animtions
            MenuManager.Instance.AddBuilder(new AnimationItemMenuBuilder());
            MenuManager.Instance.AddBuilder(new AnimationMenuBuilder());

            // Storage
            MenuManager.Instance.AddBuilder(new StorageMenuBuilder());

            // Gangwar
            MenuManager.Instance.AddBuilder(new GangwarInfoMenuBuilder());
            MenuManager.Instance.AddBuilder(new GangwarVehicleMenu());
            MenuManager.Instance.AddBuilder(new GangwarWeaponMenu());

            //Loading Maps
            Maps.LoadAssets();

            Logger.Debug("Gamemode wird initialisiert!");

            // Load static Container flows
            ContainerManager.Load();

            // Markers
            Markers.CreateSimple(24, new Vector3(-139.1181, -631.9254, 168.8205), 0.5f, 255, 234, 0, 255, 0); // FS Marker
            Markers.CreateSimple(22, new Vector3(-1635.500, 181.086, 62.757), 0.8f, 255, 234, 0, 255, 0); // Universitaet
            Markers.CreateSimple(29, new Vector3(-1051.296, -238.987, 45.02107), 0.8f, 255, 234, 0, 255, 0); // AD punkt
            Markers.CreateSimple(29, new Vector3(-608.6405, -938.4097, 24.85956), 0.8f, 255, 234, 0, 255, 0); // Eventkasse
            Markers.CreateSimple(29, new Vector3(-555.256, -197.16, 38.2224), 0.8f, 255, 234, 0, 255, 0); // Namechange


            PlayerNotifications.Instance.Add(new Vector3(1699, 2575, -69), "Gefaengnis",
                "Benutze /arrest Name"); // SG
            PlayerNotifications.Instance.Add(new Vector3(-1635.500, 181.086, 62.857), "Universitaet",
                "Benutze E um dich weiterzubilden!"); //
            PlayerNotifications.Instance.Add(new Vector3(440.971, -978.654, 31.690), "L.S.P.D",
                "Benutze E um einen Waffenschein zu erwerben!"); // Police SU
            PlayerNotifications.Instance.Add(new Vector3(1849.671, 3679.881, 33.26051), "L.S.P.D",
                "Benutze E um einen Waffenschein zu erwerben!"); // Police Sandy
            PlayerNotifications.Instance.Add(new Vector3(-427.7528, 6003.179, 30.47934), "L.S.P.D",
                "Benutze E um einen Waffenschein zu erwerben!"); // Police Paleto

            PlayerNotifications.Instance.Add(new Vector3(895.7319, -178.6453, 75.90035),
                "Taxi Zentrale",
                "Benutze E um eine Lizenz zu erwerben! /taxi [Preis] um in den Dienst zu gehen"); // Taxi  
            PlayerNotifications.Instance.Add(new Vector3(-608.6405, -938.4097, 25.05956),
                "Eventkasse",
                "Benutze /eventkasse deposit um Geld fuer Events zu spenden!"); // Eventkasse
            PlayerNotifications.Instance.Add(new Vector3(-205.4892, -1325.464, 32.89039),
                "Tuning Erweiterung",
                "Benutze E um dein Fahrzeug aufzuruesten!"); // NPC Fahrzeugwechsel

            PlayerNotifications.Instance.Add(new Vector3(-780.1838, -0.8764881, 41.12542), "Kirche",
                "Hier kannst du mit /marry heiraten!"); // Help Descr
            PlayerNotifications.Instance.Add(new Vector3(-1051.296, -238.987, 46.32107), "Werbung",
                "Benutze \"E\" um eine Werbung fuer $5/Buchstabe zu schalten!"); // AD Punkt
            PlayerNotifications.Instance.Add(new Vector3(-1609.368, -999.0972, 9.714716),
                "Manfred der Maulwurf",
                "Infos zu Drogendealern fuer $3000! Benutze E"); // Dealerinfo
            PlayerNotifications.Instance.Add(new Vector3(-388.7151, -2282.08, 8.908183),
                "Schrottplatz",
                "Benutze /deletecar um dein Fahrzeug fuer 1/4 des Kaufpreises zu verschrotten!"); // deletecar
            PlayerNotifications.Instance.Add(new Vector3(-432.7557, -2255.613, 2.38292308),
                "Bootsdepot",
                "Benutze /deletecar um dein Boot fuer 1/4 des Kaufpreises zu verschrotten!"); // deletecar


            // Business Bank
            PlayerNotifications.Instance.Add(new Vector3(248.977, 212.425, 106.287), "Business Beantragung",
                "Benutze \"E\" um ein Businesse zu erwerben!");

            // Alter Punkt
            //PlayerNotifications.Instance.Add(new Vector3(-83.3435, -835.366, 40.5581), "Business Tower",
            //    "Benutze \"E\" um ein Business zu betreten!");

            //PlayerNotifications.Instance.Add(new Vector3(-79.6059, -796.427, 44.2273), "Business Tower",
            //                "Benutze \"E\" um ein Business zu betreten!");

            PlayerNotifications.Instance.Add(new Vector3(-59.5738, -812.895, 243.386), "Business Tresor",
                "Benutze \"E\" um den Tresor zu benutzen!");

            //Prison Locker In
            PlayerNotifications.Instance.Add(Coordinates.PrisonLockerPutIn,
                "Info",
                "Benutze I um auf deinen Spind zuzugreifen");

            //Prison Locker Out
            PlayerNotifications.Instance.Add(Coordinates.PrisonLockerTakeOut, "Info",
                "Benutze I um auf deinen Spind zuzugreifen");
            PlayerNotifications.Instance.Add(Coordinates.LSPDLockerPutOut, "Info",
                "Benutze I um auf deinen Spind zuzugreifen");
            PlayerNotifications.Instance.Add(Coordinates.LSPDPDLockerPutIn, "Info",
                "Benutze I um auf deinen Spind zuzugreifen");
            PlayerNotifications.Instance.Add(Coordinates.SandyPDLockerPutInOut, "Info",
                "Benutze I um auf deinen Spind zuzugreifen");
            PlayerNotifications.Instance.Add(Coordinates.SandyPDLockerPutInOutAutomat, "Info",
                "Benutze I um auf deinen Spind zuzugreifen");
            PlayerNotifications.Instance.Add(Coordinates.PaletoPDLockerPutInOut, "Info",
                "Benutze I um auf deinen Spind zuzugreifen");
            PlayerNotifications.Instance.Add(Coordinates.PaletoPDLockerPutInOutAutomat, "Info",
                "Benutze I um auf deinen Spind zuzugreifen");

            //SpecialPeds 
            Peds.Create(PedHash.Paparazzi01AMM, new Vector3(-1609.368, -999.0972, 7.614716), -37.56418f); //Dealerinfo

            Peds.Create(PedHash.AfriAmer01AMM, new Vector3(895.7319, -178.6453, 74.70035), -80.50847f); //Taxi

            // Willys Export
            Peds.Create(PedHash.WillyFist, new Vector3(-59.47939, 6523.819, 31.49083), -46.24131f);

            // Schrottplatz
            Markers.CreateSimple(22, new Vector3(-388.7151, -2282.08, 7.608183 + 1.5f), 1.0f, 255, 255, 0, 255, 0);
            Markers.CreateSimple(22, new Vector3(-432.7557, -2255.613, 1.38292308 + 1.5f), 1.0f, 255, 255, 0, 255, 0);

            // Asservatenkammer
            Markers.CreateSimple(22, new Vector3(1060.247, -3095.411, -39.99995 + 1.5f), 1.0f, 255, 255, 0, 255, 0);


            KonversationMessageModule.Instance.Load(true);

            Modules.Instance.OnDailyReset();

            Logger.Print("  GGGGGG    VVV      VVV    MMMM    MMMM     PPPPPPP      ");
            Logger.Print(" GG     GG  VV        VV   MM  MM  MM  MM    PP    PP     ");
            Logger.Print(" GG          VV      VV    MM   M  M   MM    PP    PP     ");
            Logger.Print(" GG    GGG    VV    VV     MM     M    MM    PPPPPPP      ");
            Logger.Print(" GG     GG     VV  VV      MM          MM    PP           ");
            Logger.Print("  GG    GG      V  V       MM          MM    PP           ");
            Logger.Print("   GGGGGG        VV        MM          MM    PP           ");

            DamageThread.Instance.InitThreads();
            Configuration.Instance.IsServerOpen = true;

            minTimer = DateTime.Now;
            SavePlayerCheck = DateTime.Now;

            NAPI.Server.SetAutoRespawnAfterDeath(false);
        }

        [ServerEvent(Event.ResourceStart)]
        public async Task OnResourceStartHandler()
        {
            m_AsyncThread = new AsyncThread();
            InitGameMode();

            AllowedAsis.Add("SkipIntro.asi", "CF0F68109888CE43AEE861066A853E7A"); // Whitelist Default SkipIntro.asi
            SyncThread.Init();
            await SyncThread.Instance.Start();
        }

#endregion

        [ServerEvent(Event.PlayerExitColshape)]
        public void OnPlayerExitColShape(ColShape shape, Player player)
        {
            if (player == null) return;
            AsyncEventTasks.ExitColShapeTask(shape, player);
        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public void onEntityEnterColShape(ColShape shape, Player player)
        {
            if (player == null) return;
            AsyncEventTasks.EnterColShapeTask(shape, player); // Debug no async
        }


        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnect(Player player, DisconnectionType type, string reason)
        {
            if (player == null)
                return;

            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid())
                return;

            try
            {
                String disconnectReason = "Spiel verlassen";
                switch (type)
                {
                    case DisconnectionType.Timeout:
                        disconnectReason = "Verbindung verloren";
                        break;
                    case DisconnectionType.Kicked:
                        disconnectReason = "Gekickt";
                        break;
                }

                foreach (DbPlayer dbPlayer in Players.Instance.GetValidPlayers())
                {
                    if (dbPlayer != null && !dbPlayer.IsValid()) continue;
                    if (dbPlayer.Player.Dimension != player.Dimension) continue;
                    if (dbPlayer.Player.Position.DistanceTo(player.Position) <= 40.0f)
                    {
                        dbPlayer.SendNewNotification($"Spieler {iPlayer.GetName()} ist disconnected! (Grund: {disconnectReason})", title: "ANTI-OFFLINEFLUCHT", notificationType: PlayerNotification.NotificationType.SERVER);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            try
            {
                if(iPlayer != null) Modules.Instance.OnPlayerDisconnected(iPlayer, reason);
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }

            try
            {
                VoiceListHandler.Instance.RemoveFromVoiceList(iPlayer);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            try
            {
                if (iPlayer.HasData("service") && iPlayer.GetData("service") > 0)
                {
                    bool status = ServiceModule.Instance.CancelOwnService(iPlayer, (uint) iPlayer.GetData("service"));

                    if (status)
                    {
                        TeamModule.Instance[(uint)iPlayer.GetData("service")].SendNotification($"Der Notruf von { iPlayer.GetName() } ({ iPlayer.ForumId }) wurde abgebrochen!");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            if (iPlayer.AccountStatus == AccountStatus.LoggedIn)
            {
                try
                {
                    if (iPlayer.RageExtension.IsInVehicle)
                    {
                        var sxVeh = iPlayer.Player.Vehicle.GetVehicle();
                        if (sxVeh != null && sxVeh.Occupants.HasPlayerOccupant(iPlayer))
                            sxVeh.Occupants.RemovePlayer(iPlayer);
                    }

                    try
                    {
                        if (iPlayer.DimensionType[0] == DimensionType.Business)
                        {
                            var visitedBusiness = iPlayer.GetVisitedBusiness();
                            if (visitedBusiness != null)
                            {
                                visitedBusiness.Visitors.Remove(iPlayer);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    try
                    {
                        RobberyModule.Instance.RemovePlayerRobs(iPlayer);
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
                finally
                {
                    if (iPlayer.AccountStatus == AccountStatus.LoggedIn)
                    {
                        iPlayer.MetaData.SaveBlocked = true;
                        iPlayer.Save(true);
                    }
                }
            }

            try
            {
                if (iPlayer != null && iPlayer.Player != null)
                {
                    // Cleanup Players DB
                    if (iPlayer.Player.HasData("player")) iPlayer.Player.ResetData("player");
                    if (iPlayer.Player.HasData("Connected")) iPlayer.Player.ResetData("Connected");
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }

            try
            {
                foreach (var itr in Players.Instance.players.Where(p => p.Value == iPlayer).ToList())
                {
                    Players.Instance.players.TryRemove(itr.Key, out DbPlayer tmpDbPlayer);
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }

            try
            {
                if (player != null && player.Handle != null)
                {
                    NAPI.Task.Run(() =>
                    {
                        NAPI.Entity.DeleteEntity(player.Handle); //DeleteEntity
                    });
                }

                player.SendNotification("Server verlassen");
                player.Kick();
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint weapon)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (killer == null || killer.IsNull) { killer = player; }
            if (iPlayer.LastDeath.AddSeconds(5) > DateTime.Now) return; // Prevent Double Trigger

            iPlayer.LastDeath = DateTime.Now;

            iPlayer.SetAcPlayerSpawnDeath();

            iPlayer.SetData("killer", killer);

            // Normal Module Death Handling
            Modules.Instance.OnPlayerDeath(iPlayer, killer, weapon);
        }

        public void ShowAccountInformation(DbPlayer iPlayer)
        {
            var menu = MenuManager.Instance.Build(PlayerMenu.Account, iPlayer);
            menu.Show(iPlayer);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public async Task onPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seat)
        {
            if (player == null) return;
            SyncWrapper.Instance.OnPlayerVehicleStateChange(player, true);

            await Task.Delay(2000);
            await AsyncEventTasks.PlayerEnterVehicleTask(player, vehicle, seat);
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle handle)
        {
            if (player == null || handle == null) return;
            SyncWrapper.Instance.OnPlayerVehicleStateChange(player, false);

            Task.Run(async () =>
            {
                await AsyncEventTasks.PlayerExitVehicleTask(player, handle);

                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer != null && dbPlayer.IsValid() && handle != null)
                {
                    // module handling
                    try
                    {
                        Modules.Instance.OnPlayerExitVehicle(dbPlayer, handle);
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }
                }
            });
        }

        public static string BuildString(string[] args, int startat = 0)
        {
            if (args.Length >= 1)
            {
                string result = "";
                string[] subarray = args.Skip(startat).ToArray();
                var len = subarray.Length;
                for (int i = 0; i < len; i++)
                {
                    if (String.IsNullOrEmpty(subarray[i]) == false && subarray[i].Length >= 1)
                    {
                        result += subarray[i] + " ";
                    }
                }

                return result.Trim();
            }

            return "";
        }

        public static bool validateArgs(string arg, int amount, bool ignoreoverflow = false)
        {
            string[] args = arg.Split(' ');

            if (args.Length < amount || (!ignoreoverflow && args.Length > amount))
            {
                return false;
            }

            return true;
        }

        public static bool IsPlayerInRangeOfPoint(Player player, float range, Vector3 pos)
        {
            Vector3 _pos = player.Position;

            if (Utils.IsPointNearPoint(range, _pos, pos))
            {
                return true;
            }

            return false;
        }

        public static Dictionary<uint, string> getPlayerGarageVehicleList(DbPlayer iPlayer,
            Garage garage)
        {
            var vehList = new Dictionary<uint, string>();

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM `vehicles` WHERE `owner` = '{iPlayer.Id}' AND `inGarage` = 1 AND `garage_id` = '{garage.Id}' ORDER BY id;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (vehList.ContainsKey(reader.GetUInt32("id"))) continue;
                            var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));
                            if (data == null) continue;
                            if (!garage.Classifications.Contains(data.ClassificationId)) continue;
                            string note = (reader.GetString("note") != "") ? $"({reader.GetString("note")})" : "";
                            vehList.Add(reader.GetUInt32("id"),
                                data.IsModdedCar == 1 ? $"{data.mod_car_name} {note}" : $"{data.Model} {note}");
                            continue;
                        }
                    }
                }

                if (iPlayer.TeamId == (int) teams.TEAM_LSC)
                {
                    cmd.CommandText = $"SELECT * FROM `vehicles` WHERE `TuningState` = 1 AND `inGarage` = 1 AND `garage_id` = '{garage.Id}'ORDER BY id;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (vehList.ContainsKey(reader.GetUInt32("id"))) continue;
                                var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));
                                if (data == null) continue;
                                if (!garage.Classifications.Contains(data.ClassificationId)) continue;
                                vehList.Add(reader.GetUInt32("id"),
                                    data.IsModdedCar == 1 ? data.mod_car_name : data.Model);
                                continue;
                            }
                        }
                    }
                }
            }

            var keys = iPlayer.VehicleKeys;

            if (iPlayer.IsMemberOfBusiness())
            {
                foreach (var item in iPlayer.GetActiveBusiness().VehicleKeys)
                {
                    if (!keys.ContainsKey(item.Key) && !vehList.ContainsKey(item.Key)) keys.Add(item.Key, item.Value);
                }
            }

            foreach (PlayerVehicleRentKey playerVehicleRent in VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.PlayerId == iPlayer.Id))
            {
                if (!keys.ContainsKey(playerVehicleRent.VehicleId))
                {
                    keys.Add(playerVehicleRent.VehicleId, "Mietfahrzeug");
                }
            }

            foreach (var key in keys)
            {
                string vehResult = getGarageItemByKey(iPlayer, key.Key, garage);
                if (vehResult != "")
                {
                    string[] args = vehResult.Split(',');
                    var id = Convert.ToUInt32(args[0]);
                    if (vehList.ContainsKey(id)) continue;
                    vehList.Add(id, args[1]);
                }
            }

            return vehList;
        }

        public class GarageVehicle
        {
            public uint Id { get; }
            public double Fuel { get; }
            public string Name { get; }
            public string Plate { get; }

            public GarageVehicle(uint id, double fuel, string name, string plate)
            {
                this.Id = id;
                this.Fuel = fuel;
                this.Name = name;
                this.Plate = plate;
            }
        }

        public static string getGarageItemByKey(DbPlayer iPlayer, uint key, Garage garage)
        {
            try
            {
                string query =
                    $"SELECT * FROM `vehicles` WHERE `id` = '{key}' AND `inGarage` = '1' AND `garage_id` = {garage.Id} ORDER BY id;";
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @query;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));
                                if (data == null) continue;
                                if (!garage.Classifications.Contains(data.ClassificationId)) continue;
                                return reader.GetInt32(0) + "," +
                                       (data.IsModdedCar == 1 ? data.mod_car_name : data.Model);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return "";
            }

            return "";
        }

        public GarageVehicle getGarageVehicleByKey(DbPlayer iPlayer, uint key, Garage garage)
        {
            if (VehicleHandler.Instance.GetByVehicleDatabaseId(key) != null) return null;

            try
            {
                string query =
                    $"SELECT * FROM `vehicles` WHERE `id` = '{key}' AND `inGarage` = '1' AND `garage_id` = {garage.Id} ORDER BY id;";
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @query;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));
                                var note = reader.GetString("note");
                                if (data == null) continue;
                                if (!garage.Classifications.Contains(data.ClassificationId)) continue;
                                if (data.IsModdedCar == 1)
                                    return new GarageVehicle(reader.GetUInt32(0), reader.GetFloat(10), data.mod_car_name, note);
                                else
                                    return new GarageVehicle(reader.GetUInt32(0), reader.GetFloat(10), data.Model, note);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }

            return null;
        }

        public static Dictionary<uint, string> getTeamGarageVehicleList(uint teamid, Garage garage)
        {
            if (teamid <= 0) return null;

            var vehList = new Dictionary<uint, string>();

            // frage die datenbank nach den fraktions fahrzeugen ab
            string query =
                $"SELECT * FROM `fvehicles` WHERE `team` = '{teamid}' AND `inGarage` = 1 AND `pos_x` = '0' AND lastGarage = '{garage.Id}' ORDER BY id;";

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32(10));
                            string note = reader.GetString("note");
                            if (data == null) continue;
                            if (!garage.Classifications.Contains(data.ClassificationId)) continue;
                            // es ist ein fahrzeug und eine garage für fahrzeuge
                            vehList.Add(reader.GetUInt32("id"),
                                data.IsModdedCar == 1 ? $"{data.mod_car_name} {note}" : $"{data.Model} {note}");
                            continue;
                        }
                    }
                }
            }

            return vehList;
        }

        public static async Task<NetHandle> LoadTeamVehicle(uint teamid, int index, Garage garage, GarageSpawn spawn)
        {
            NetHandle returnX = new NetHandle();
            try
            {
                if (teamid > 0)
                {
                    var query =
                        $"SELECT * FROM `fvehicles` WHERE `team` = '{teamid}' AND `inGarage` = 1 AND `pos_x` = '0' ORDER BY id;";
                    int idx = 0;

                    using (var conn =
                        new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32(10));
                                    var note = reader.GetString("note");
                                    if (data == null) continue;
                                    if (data.Disabled) continue;
                                    if (!garage.Classifications.Contains(data.ClassificationId)) continue;
                                    if (idx == index)
                                    {
                                        SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(data.Id, reader.GetInt32("registered") == 1,
                                            spawn.Position, spawn.Heading,
                                            reader.GetInt32("color1"), reader.GetInt32("color2"), 0, reader.GetUInt32("gps_tracker") == 1, true, true,
                                            teamid, reader.GetString("plate"),
                                            reader.GetUInt32("id"), 0, 0, data.Fuel,
                                            VehicleHandler.MaxVehicleHealth, reader.GetString("tuning"), "", 0, ContainerManager.LoadContainer(reader.GetUInt32("id"), ContainerTypes.FVEHICLE), 
                                            WheelClamp:reader.GetInt32("WheelClamp"), AlarmSystem:reader.GetInt32("alarm_system") == 1, lastgarageId:garage.Id, container2: ContainerManager.LoadContainer(reader.GetUInt32("id"), ContainerTypes.FVEHICLE2));

                                        while (xVeh.entity == null)
                                        {
                                            await NAPI.Task.WaitForMainThread(100);
                                        }

                                        xVeh.SetTeamCarGarage(false);
                                        return xVeh.entity;
                                    }

                                    idx++;
                                    continue;
                                }
                            }
                        }
                    }
                }

                return returnX;
            }

            catch (Exception e)
            {
                Logger.Crash(e);
                return returnX;
            }
        }

        public static int GetPlayerVehicleTaxes(DbPlayer iPlayer, bool inGarage = false)
        {
            int tax = 0;

            string query = $"SELECT * FROM `vehicles` WHERE `owner` = '{iPlayer.Id}';";
            if (inGarage) query = $"SELECT * FROM `vehicles` WHERE `owner` = '{iPlayer.Id}' AND `inGarage` = '1' AND `registered` = 1 ;";

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var modelId = reader.GetUInt32("model");
                            var data = VehicleDataModule.Instance.GetDataById(modelId);
                            if (data == null) continue;
                            tax = tax + data.Tax;
                        }
                    }
                }
            }

            return tax;
        }

        public static async Task<int> GetTeamVehicleTaxes(uint teamId)
        {
            int tax = 0;

            String query = $"SELECT * FROM `fvehicles` WHERE `team` = '{teamId}' AND `registered` = 1 ;";

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = @query;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            var modelId = reader.GetUInt32("model");
                            var data = VehicleDataModule.Instance.GetDataById(modelId);
                            if (data == null) continue;
                            tax = tax + data.Tax;
                        }
                    }
                }
            }

            return tax;
        }

        public static void OnPlayerFirstSpawn(Player player, bool withoutguns = false)
        {
            if (player == null)
                return;

            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null) return;

            Modules.Instance.OnPlayerFirstSpawn(iPlayer);

            // Remove Mask
            iPlayer.SetClothes(1, 0, 0);

            // test
            iPlayer.Player.TriggerNewClient("startIntro");

            // Handynummer
            if (iPlayer.handy[0] == 0)
            {
                //Player is New
                player.CreateUserDialog(Dialogs.menu_info, "tutorial");
                iPlayer.SetWaypoint(-906.0185f, -2337.567f);
                Players.Instance.SendMessageToAuthorizedUsers("guidechat",
                    iPlayer.GetName() + " hat sich neu auf dem Server eingeloggt!");
            }

            //LoadPlayerHUD
            player.TriggerNewClient("loadHUD");
            player.TriggerNewClient("updateMoney", iPlayer.Money[0]);
            player.TriggerNewClient("updateBlackMoney", iPlayer.BlackMoney[0]);

            if (ptr)
            {
                iPlayer.SendNewNotification(
                    "ACHTUNG: Sie befinden sich auf dem Public-Test-Server (PTS)");
                iPlayer.SendNewNotification(
                    "Daten können verloren gehen und werden nicht auf die Live DB synchronisiert!");
            }
        }

        public void OnVehicleHealthChange(NetHandle netHandle, float oldValue)
        {
            // Getting SxVehicle
            var vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(netHandle);
            if (vehicle == null) return;

            SxVehicle sxVeh = vehicle.GetVehicle();
            if (sxVeh == null)
            {
                return;
            }

            //Should be Repairing
            if (oldValue < vehicle.Health)
            {
                if (sxVeh.RepairState) sxVeh.RepairState = false;
                else // ANTICHEAT MAY
                {
                    if (sxVeh.Occupants.GetUsedSeats() > 0)
                    {
                        sxVeh.Occupants.TriggerAntiCheatForEveryOccupant();
                    }
                }
            }

            return;
        }

        [ServerEvent(Event.PlayerWeaponSwitch)]
        public void OnPlayerWeaponSwitch(Player player, WeaponHash oldgun, WeaponHash newWeapon)
        {
            AsyncEventTasks.PlayerWeaponSwitchTask(player, oldgun, newWeapon);
        }

        public static void ScheduleRestart(uint p_Minuten, string p_Grund)
        {
            m_RestartScheduled = true;
            m_RestartMinuten = p_Minuten;
            m_RestartReason = p_Grund;
        }

        public static async Task OnMinHandler()
        {
            adList.RemoveAll(item => item.DateTime.AddMinutes(15) <= DateTime.Now);

            // Handle Server Restarts
            if (m_RestartScheduled)
            {
                if (m_RestartMinuten == 0)
                {
                    foreach (var itr in Players.Instance.players.Values)
                    {
                        if (itr == null || !itr.IsValid()) continue;
                        if (itr.Player != null)
                        {
                            itr.Save();
                            itr.Player.Kick();
                        }
                    }

                    foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetAllVehicles())
                    {
                        if (sxVehicle.IsValid())
                        {
                            VehicleSpawnerModule.Instance.SaveVehiclePosition(sxVehicle);
                        }
                    }

                    if (!Configuration.Instance.DevMode) Configuration.Instance.IsServerOpen = false;

                    return;
                }

                await Chats.SendGlobalMessage($"[SERVER-RESTART] Es findet ein geplanter Restart in {m_RestartMinuten} Minuten statt. Grund: {m_RestartReason}.", COLOR.ORANGE, ICON.GLOB, 15000);
                m_RestartMinuten--;
            }

            if (HourTimer == 60)
            {
                HourTimer = 0;
                try
                {
                    ShopsModule.Instance.ResetAllRobStatus();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            HourTimer++;
            try
            {
                int hour = DateTime.Now.Hour;
                int min = DateTime.Now.Minute;
                if (devmode != true)
                {
                    if (hour == 7 || hour == 15 || hour == 23)
                    {
                        if (min == 50)
                        {
                            Modules.Instance.OnServerBeforeRestart();

                            await Chats.SendGlobalMessage($"[AUTO-RESTART] Ein EMP steht kurz bevor... starte Verteidigungsmaßnahmen!", COLOR.ORANGE, ICON.GLOB);
                            WeatherModule.Instance.SetBlackout(true);
                        }
                        else if (min == 55)
                        {
                            await Chats.SendGlobalMessage($"[AUTO-RESTART] Der EMP konnte nicht aufgehalten werden!", COLOR.ORANGE, ICON.GLOB);
                        }
                        else if (min == 57)
                        {
                            await Chats.SendGlobalMessage($"[AUTO-RESTART] Durch den EMP sind sämtliche Stromquellen zerstört worden. Der Blackout steht kurz bevor!", COLOR.ORANGE, ICON.GLOB);

                            foreach (var itr in Players.Instance.players.Values)
                            {
                                if (itr == null || !itr.IsValid()) continue;
                                if (itr.Player != null)
                                {
                                    itr.Save();
                                    itr.Player.Kick();
                                }
                            }

                            foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetAllVehicles())
                            {
                                if (sxVehicle.IsValid())
                                {
                                    VehicleSpawnerModule.Instance.SaveVehiclePosition(sxVehicle);
                                }
                            }

                            if (!Configuration.Instance.DevMode) Configuration.Instance.IsServerOpen = false;
                        }
                        else if (min == 58)
                        {
                            // Clear Whitelist
                            MySQLHandler.ExecuteAsync("DELETE FROM whitelist.whitelisted_ips2");

                            Logger.Print(
                                "Alle Tasks wurden beendet, Server darf nun ausgeschaltet werden!");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static async Task OnUpdateHandler()
        {
            if (DateTime.Now.Subtract(SavePlayerCheck).TotalSeconds >= 5)
            {
                try
                {
                    SavePlayerCheck = DateTime.Now;
                    // RealCool Saving
                    int mininterval = mysqlSaveInterval;
                    int maxinterval = mysqlSaveInterval;
                    maxinterval = maxinterval + 30;
                    Logger.Debug("Saving Users... (" + mininterval + " - " + maxinterval + ")");

                    try
                    {
                        bool UsersSaved = false;

                        foreach (var itr in Players.Instance.players.Values)
                        {
                            if (!itr.IsValid()) continue;

                            if (itr.AccountStatus == AccountStatus.LoggedIn &&
                                itr.TemporaryPlayerId >= mininterval &&
                                itr.TemporaryPlayerId < maxinterval)
                            {
                                string updateQuery = itr.GetUpdateQuery();

                                if (updateQuery == "") continue;
                                if (!updateQuery.Contains("UPDATE")) continue;
                                MySQLHandler.ExecuteAsync(updateQuery);
                                UsersSaved = true;
                            }
                        }

                        if (UsersSaved == false)
                        {
                            failelessSaving++;
                        }
                        else
                        {
                            failelessSaving = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    if (failelessSaving > 2)
                    {
                        Logger.Debug("FaillessSaving resetting..");
                        mysqlSaveInterval = 0;
                        failelessSaving = 0;
                    }
                    else
                    {
                        mysqlSaveInterval = mysqlSaveInterval + 30;
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            timer.Start();
        }

        public static void lowerPlayerJobSkill(DbPlayer iPlayer)
        {
            string playerjob = iPlayer.job[0].ToString();
            string jobskills = iPlayer.job_skills[0];
            int actualskill = Convert.ToInt32(iPlayer.JobSkill[0]);
            string newskills = "";
            bool found = false;

            if (jobskills != "")
            {
                string[] Items = jobskills.Split(',');
                foreach (string item in Items)
                {
                    string[] parts = item.Split(':');
                    if (parts[0] == playerjob)
                    {
                        if (newskills == "")
                        {
                            newskills = playerjob + ":" + Convert.ToString(actualskill);
                        }
                        else
                        {
                            newskills =
                                newskills + "," + playerjob + ":" +
                                Convert.ToString(actualskill);
                        }

                        found = true;
                    }
                    else
                    {
                        int newskill = Convert.ToInt32(parts[1]) - 100;
                        if (newskill < 0) newskill = 0;
                        if (newskills == "")
                        {
                            newskills = parts[0] + ":" + Convert.ToString(newskill);
                        }
                        else
                        {
                            newskills =
                                newskills + "," + parts[0] + ":" + Convert.ToString(newskill);
                        }
                    }
                }

                if (!found)
                {
                    if (newskills == "")
                    {
                        newskills = playerjob + ":" + Convert.ToString(actualskill);
                    }
                    else
                    {
                        newskills =
                            newskills + "," + playerjob + ":" + Convert.ToString(actualskill);
                    }
                }
            }
            else
            {
                newskills = playerjob + ":" + Convert.ToString(actualskill);
            }

            iPlayer.job_skills[0] = newskills;
            return;
        }

        public static void DeletePlayerVehicle(DbPlayer iPlayer, SxVehicle sxVeh)
        {
            string query =
                $"DELETE FROM `vehicles` WHERE `owner` = '{iPlayer.Id}' AND `id` = '{sxVeh.databaseId}';";

            VehicleKeyHandler.Instance.DeleteAllVehicleKeys(sxVeh.databaseId);

            MySQLHandler.ExecuteAsync(query);
            VehicleHandler.Instance.DeleteVehicle(sxVeh, false);
            return;
        }

        public static void DeleteTeamVehicle(DbPlayer iPlayer, SxVehicle sxVeh)
        {
            string query = $"DELETE FROM `fvehicles` WHERE `team` = '{iPlayer.Team.Id}' AND `id` = '{sxVeh.databaseId}';";
            MySQLHandler.ExecuteAsync(query);
            VehicleHandler.Instance.DeleteVehicle(sxVeh, false);
            return;
        }


        public static void ChangePlayerVehicleOwner(DbPlayer iPlayer, DbPlayer NewPlayer,
            SxVehicle sxVeh)
        {
            string query =
                $"UPDATE `vehicles` SET owner = '{NewPlayer.Id}' WHERE `owner` = '{iPlayer.Id}' AND `id` = '{sxVeh.databaseId}';";

            VehicleKeyHandler.Instance.DeleteAllVehicleKeys(sxVeh.databaseId);

            MySQLHandler.ExecuteAsync(query);
            sxVeh.ownerId = NewPlayer.Id;
            return;
        }

        public void KickAll()
        {
            foreach (var itr in Players.Instance.players.Values)
            {
                if (itr == null || !itr.IsValid()) continue;
                itr.Player.Kick();
            }
        }

        public static int CToInt(dynamic str)
        {
            try
            {
                if (str is int) return str;
                StringBuilder sb = new StringBuilder();
                foreach (char c in str)
                {
                    if (c >= '0' && c <= '9')
                    {
                        sb.Append(c);
                    }
                }

                string str2 = sb.ToString();
                if (string.IsNullOrEmpty(str2)) return 0;
                int ret = Convert.ToInt32(str2);
                if (ret > 9999999) ret = 0;
                return ret;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static bool newsActivated(Player player)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return false;

            bool active = true;
            if (iPlayer.HasData("tognews"))
            {
                if (iPlayer.GetData("tognews") == 1)
                {
                    active = false;
                }
            }

            return active;
        }

        public static void setNewsActivated(Player player, bool duty)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            if (!duty) iPlayer.SetData("tognews", 1);
            else iPlayer.SetData("tognews", 0);
        }

        public static bool canPlayerFreed(DbPlayer iPlayer, DbPlayer iTarget)
        {
            if (iPlayer.job[0] == (int) jobs.JOB_ANWALT)
            {
                if (iTarget.JailTime[0] > 5)
                {
                    if (iTarget.JailTime[0] < 20) return true;
                    if (iTarget.JailTime[0] >= 20 && iTarget.JailTime[0] < 30 &&
                        iPlayer.JobSkill[0] >= 2000) return true;
                    else if (iTarget.JailTime[0] >= 30 && iTarget.JailTime[0] < 40 &&
                             iPlayer.JobSkill[0] >= 3000)
                        return true;
                    else if (iTarget.JailTime[0] >= 40 && iTarget.JailTime[0] < 50 &&
                             iPlayer.JobSkill[0] >= 4000)
                        return true;
                    else if (iTarget.JailTime[0] >= 50 && iTarget.JailTime[0] < 60 &&
                             iPlayer.JobSkill[0] >= 5000)
                        return true;
                }
            }

            return false;
        }

        public static int getFreePrice(DbPlayer iPlayer)
        {
            if (iPlayer.JailTime[0] > 5)
            {
                return 200 * iPlayer.JailTime[0];
            }

            return 0;
        }

        public static void freePlayer(DbPlayer iPlayer, DbPlayer iTarget, bool admin = false)
        {
            if (admin)
            {
                iTarget.JailTime[0] = 1;
                PlayerSpawn.OnPlayerSpawn(iTarget.Player);
                return;
            }

            if (iPlayer.job[0] == (int) jobs.JOB_ANWALT)
            {
                if (canPlayerFreed(iPlayer, iTarget))
                {
                    iTarget.JailTime[0] = 1;
                    PlayerSpawn.OnPlayerSpawn(iTarget.Player);
                    iTarget.SendNewNotification(
                        "Anwalt " + iPlayer.GetName() +
                        " hat Sie aus dem Gefaengnis geholt!");
                    iPlayer.SendNewNotification(
                        "Sie haben " + iTarget.GetName() +
                        " aus dem Gefaengnis geholt!");
                    return;
                }
            }

            return;
        }

        public static int rndColor()
        {
            int color = Random.Next(0, 157);
            return color;
        }

        public static async Task TriggerPlayer_L(DbPlayer iPlayer)
        {
            if (!iPlayer.CanInteract()) return;

            if (await Modules.Instance.OnKeyPressed(iPlayer, Key.L)) return;
            if (HouseModule.Instance.PlayerLockHouse(iPlayer)) return;

            return;
        }

        public static async Task TriggerPlayer_K(DbPlayer iPlayer)
        {
            if (!iPlayer.CanInteract()) return;

            if (await Modules.Instance.OnKeyPressed(iPlayer, Key.K)) return;

            return;
        }


        public static async Task TriggerPlayer_J(DbPlayer iPlayer)
        {
            if (!iPlayer.CanInteract()) return;

            if (await Modules.Instance.OnKeyPressed(iPlayer, Key.J)) return;

            return;
        }

        public static void sendNotificationToPlayersWhoCanReceive(String message, String title)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (var player in Players.Instance.GetValidPlayers())
                {
                    if (player.Container.GetItemAmount(174) > 0 && !player.phoneSetting.flugmodus)
                    {
                        player.SendNewNotification(message, title: title, notificationType: PlayerNotification.NotificationType.NEWS);
                    }
                }
            }));
        }

        public static async Task TriggerPlayerPoint(DbPlayer iPlayer)
        {
            if (iPlayer == null || !iPlayer.IsValid()) return;
            // PlayerObject
            var player = iPlayer.Player;

            if (iPlayer.IsInAnimation()) return;

            if ((iPlayer.IsInjured()) &&
                iPlayer.RankId == 0)
            {
                return;
            }

            if (await iPlayer.CanPressE() == false) return;

            if (await Modules.Instance.OnKeyPressed(iPlayer, Key.E)) return;

            if (iPlayer.RageExtension.IsInVehicle)
            {
                //Interactions f Fahrzeuge

                if (IsPlayerInRangeOfPoint(player, 15.0f,
                    new Vector3(-205.4892, -1325.464, 30.89039)))
                {
                    if (!iPlayer.RageExtension.IsInVehicle) return;
                    if (!iPlayer.IsOwner(player.Vehicle.GetVehicle())) return;
                    int price = (VehicleShopModule.Instance.GetVehiclePriceFromHash(
                        player.Vehicle.GetVehicle().Data) / 100) * 20;
                    DialogMigrator.CreateMenu(player, Dialogs.menu_shop_changecar, "Fahrzeug Aufruesten", "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_changecar, "Fahrzeug Aufruesten",
                        "Preis: $" + price);
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_changecar, "ACHTUNG TUNING WIRD RESETTET!",
                        "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_changecar, GlobalMessages.General.Close(), "");
                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_shop_changecar);
                    return;
                }
            }
            else
            {
                if (ArmoryModule.Instance.TriggerPoint(iPlayer)) return;

                // AD Punkt
                if (IsPlayerInRangeOfPoint(player, 5.0f,
                    new Vector3(-1051.296, -238.987, 45.02107)))
                {
                    if (adLastSend.AddSeconds(15) > DateTime.Now)
                    {
                        iPlayer.SendNewNotification("Es wurde bereits eine Werbung gesendet, bitte warte kurz!");
                        return;
                    }
                    else
                    {
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Schalte eine Werbung!", Callback = "LifeInvaderPurchaseAd", Message = "LifeInvader Werbung" });
                        return;
                    }
                }

                if (iPlayer.HasData("teamWardrobe"))
                {
                    MenuManager.Instance.Build(PlayerMenu.TeamWardrobe, iPlayer).Show(iPlayer);
                    return;
                }

                if (iPlayer.HasData("ammunationId"))
                {
                    Ammunation ammunation = Ammunations.Instance.Get(iPlayer.GetData("ammunationId"));
                    if (ammunation != null)
                    {
                        if (iPlayer.Level < 3)
                        {
                            iPlayer.SendNewNotification("Ammunations sind erst ab Level 3 verfuegbar!");
                            return;
                        }

                        //Waffenshop
                        DialogMigrator.CreateMenu(player, Dialogs.menu_shop_ammunation_main, "Ammunation", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_main, "Waffen", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_main, "Munition", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_main, "Componenten", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_main, "Fallschirm (1000$)", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_main, GlobalMessages.General.Close(), "");
                        DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_shop_ammunation_main);
                        return;
                    }
                }

                if (iPlayer.HasData("garageId"))
                {
                    Garage garage = GarageModule.Instance[iPlayer.GetData("garageId")];
                    if (garage != null && garage.HouseId <= 0)
                    {
                        uint teamId = iPlayer.TeamId;
                        if(garage.Teams.Contains((uint)teams.TEAM_IAA) && iPlayer.IsNSADuty) {
                            teamId = (uint)teams.TEAM_IAA;
                        }

                        if (garage.IsTeamGarage() && !garage.Teams.Contains(teamId)) return;

                        if (iPlayer.Player.Dimension != GangwarModule.Instance.DefaultDimension)
                        {
                            ComponentManager.Get<GarageWindow>().Show()(iPlayer, garage);
                        }

                        return;
                    }
                }

                if (iPlayer.HasData("houseId"))
                {
                    // Wenn in House nicht anzeigen
                    if (iPlayer.DimensionType[0] == DimensionType.World)
                    {
                        uint houseId = iPlayer.GetData("houseId");
                        var house = HouseModule.Instance.Get(houseId);
                        // Dialog für Keller us
                        DialogMigrator.CreateMenu(player, Dialogs.menu_house_main, "Hausverwaltung",
                            (house.Locked ? "Abgeschlossen" : "Aufgeschlossen"));
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_house_main, GlobalMessages.General.Close(), "Schließen");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_house_main, "Haus betreten", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_house_main, "Keller", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_house_main, "Haustiere", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_house_main, (house.ShowPhoneNumber.Length > 0 ? "Telefonnummer ausblenden" : "Telefonnummer einblenden"), "");
                        if (house.GarageId > 0)
                        {
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_main, "Garage öffnen", "");
                        }
                        else if (house.GarageId == 0 && iPlayer.Id == house.OwnerId)
                        {
                            var found = false;
                            foreach (var currGarage in GarageModule.Instance.GetAll())
                            {
                                if (currGarage.Value.HouseId == house.Id)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (found)
                            {
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_house_main, "Garage ausbauen",
                                    (house.Price / 4.0).ToString());
                            }
                        }
                        else
                        {
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_main, "Keine Garage vorhanden",
                                "");
                        }

                        DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_house_main, true);
                    }
                }

                if (iPlayer.HasData("bankId"))
                {
                    var bankId = iPlayer.GetData("bankId");
                    if (bankId == null)
                    {
                        return;
                    }

                    var parseBankId = uint.TryParse(bankId.ToString(), out uint bankIdNew);
                    if (!parseBankId)
                    {
                        return;
                    }

                    if (iPlayer.IsInRob)
                    {
                        iPlayer.SendNewNotification("Dieser Bankautomat wurde aufgrund eines Raubüberfalls vorübergehend gesperrt.");
                        return;
                    }

                    Bank bank = BankModule.Instance.Get(bankIdNew);
                    if (bank != null)
                    {
                        /*if (bank.Type == BankType.Atm)
                            player.PlaySoundFrontEnd("ATM_WINDOW",
                                "HUD_FRONTEND_DEFAULT_SOUNDSET");*/
                        ComponentManager.Get<BankWindow>().Show()(iPlayer, "Bank", iPlayer.GetName(), iPlayer.Money[0], iPlayer.BankMoney[0], bank.Type, iPlayer.BankHistory);
                        return;
                    }

                    return;
                }

                VehicleShop cShop = VehicleShopModule.Instance.GetThisShop(player.Position);
                if (cShop != null)
                {
                    DialogMigrator.CreateMenu(player, Dialogs.menu_carshop, "Fahrzeughandel",
                        "Waehle ein Fahrzeug");
                    List<ShopVehicle> VehSysList =
                        VehicleShopModule.Instance.GetVehsFromCarShop(cShop.Id);

                    foreach (ShopVehicle Vehicle in VehSysList)
                    {
                        var price = Vehicle.Price;

                        if (iPlayer.uni_business[0] > 0 && !Vehicle.IsSpecialCar)
                        {
                            var discount = 100 - (iPlayer.uni_business[0] * 2);
                            price = Convert.ToInt32((discount / 100.0) * price);
                        }

                        string add = "";
                        if (Vehicle.LimitedAmount > 0)
                        {
                            if (Vehicle.LimitedAmount - Vehicle.LimitedBuyed > 0)
                                add = $"[VERFÜGBAR - {Vehicle.LimitedAmount - Vehicle.LimitedBuyed}] ";
                            else
                                add = $"[NICHT VERFÜGBAR]";
                        }

                        DialogMigrator.AddMenuItem(player, Dialogs.menu_carshop, add + Vehicle.Name + " $" + price,
                            "");
                    }

                    DialogMigrator.AddMenuItem(player, Dialogs.menu_carshop, GlobalMessages.General.Close(), "");
                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_carshop);
                    return;
                }

                // Business Tower Wardrobe
                if (iPlayer.Player.Position.DistanceTo(new Vector3(-79.7095, -811.279, 243.386)) < 3.0f)
                {
                    Character character = iPlayer.Character;
                    var wardrobe = character.Wardrobe;

                    if (wardrobe.Count <= 0)
                    {
                        iPlayer.SendNewNotification("Der Kleiderschrank ist leer.");

                        return;
                    }

                    try
                    {
                        // Get EquippedProps
                        var eqProps = character.EquipedProps;

                        var eqFullProps = PropModule.Instance.GetAll()
                            .Where(p => eqProps.ContainsValue(p.Key))
                            .ToDictionary(pair => pair.Key, pair => pair.Value);

                        // Get EquippedClothes
                        var eqClothes = character.Clothes;

                        var eqFullClothes = ClothModule.Instance.GetAll()
                            .Where(c => eqClothes.ContainsValue(c.Key))
                            .ToDictionary(pair => pair.Key, pair => pair.Value);
                        
                        ComponentManager.Get<WardrobeWindow>().Show()(
                            iPlayer,
                            new[]
                                {
                                    ClothesShopModule.Instance.GetClothesSlots().ToDictionary(
                                        keyValuePair => keyValuePair.Key.ToString(),
                                        keyValuePair => keyValuePair.Value.Name
                                    ),
                                    ClothesShopModule.Instance.GetPropsSlots().ToDictionary(
                                        keyValuePair => $"p-{keyValuePair.Key}",
                                        keyValuePair => keyValuePair.Value.Name
                                    )
                                }
                                .SelectMany(dict => dict)
                                .ToDictionary(pair => pair.Key, pair => pair.Value)
                                .Select(kvp => new Slot(kvp.Key, kvp.Value))
                                .ToList(),
                            new[]
                                {
                                    eqFullProps
                                        .ToDictionary(
                                            kvp => "p-" + kvp.Value.Slot,
                                            kvp => new WardrobeWindow.JsonCloth(
                                                kvp.Key,
                                                kvp.Value.Name,
                                                kvp.Value.Slot,
                                                true
                                            )
                                        ),
                                    eqFullClothes
                                        .ToDictionary(
                                            kvp => kvp.Value.Slot.ToString(),
                                            kvp => new WardrobeWindow.JsonCloth(
                                                kvp.Key,
                                                kvp.Value.Name,
                                                kvp.Value.Slot
                                            )
                                        )
                                }
                                .SelectMany(dict => dict)
                                .ToDictionary(pair => pair.Key, pair => pair.Value)
                        );
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }
                }

                // House Wardrobe
                if (iPlayer.DimensionType[0] == DimensionType.House && iPlayer.HasData("inHouse"))
                {
                    House iHouse;

                    if ((iHouse = HouseModule.Instance.Get((uint) iPlayer.GetData("inHouse"))) == null) return;

                    if (iHouse.Interior.ClothesPosition.DistanceTo(iPlayer.Player.Position) > 3.0f) return;

                    Character character = iPlayer.Character;
                    var wardrobe = character.Wardrobe;

                    if (wardrobe.Count <= 0)
                    {
                        iPlayer.SendNewNotification("Der Kleiderschrank ist leer.");

                        return;
                    }

                    try
                    {
                        // Get EquippedProps
                        var eqProps = character.EquipedProps;

                        var eqFullProps = PropModule.Instance.GetAll()
                            .Where(p => eqProps.ContainsValue(p.Key))
                            .ToDictionary(pair => pair.Key, pair => pair.Value);

                        // Get EquippedClothes
                        var eqClothes = character.Clothes;

                        var eqFullClothes = ClothModule.Instance.GetAll()
                            .Where(c => eqClothes.ContainsValue(c.Key))
                            .ToDictionary(pair => pair.Key, pair => pair.Value);

                        ComponentManager.Get<WardrobeWindow>().Show()(
                            iPlayer,
                            new[]
                                {
                                    ClothesShopModule.Instance.GetClothesSlots().ToDictionary(
                                        keyValuePair => keyValuePair.Key.ToString(),
                                        keyValuePair => keyValuePair.Value.Name
                                    ),
                                    ClothesShopModule.Instance.GetPropsSlots().ToDictionary(
                                        keyValuePair => $"p-{keyValuePair.Key}",
                                        keyValuePair => keyValuePair.Value.Name
                                    )
                                }
                                .SelectMany(dict => dict)
                                .ToDictionary(pair => pair.Key, pair => pair.Value)
                                .Select(kvp => new Slot(kvp.Key, kvp.Value))
                                .ToList(),
                            new[]
                                {
                                    eqFullProps
                                        .ToDictionary(
                                            kvp => "p-" + kvp.Value.Slot,
                                            kvp => new WardrobeWindow.JsonCloth(
                                                kvp.Key,
                                                kvp.Value.Name,
                                                kvp.Value.Slot,
                                                true
                                            )
                                        ),
                                    eqFullClothes
                                        .ToDictionary(
                                            kvp => kvp.Value.Slot.ToString(),
                                            kvp => new WardrobeWindow.JsonCloth(
                                                kvp.Key,
                                                kvp.Value.Name,
                                                kvp.Value.Slot
                                            )
                                        )
                                }
                                .SelectMany(dict => dict)
                                .ToDictionary(pair => pair.Key, pair => pair.Value)
                        );
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }
                }



                if (IsPlayerInRangeOfPoint(player, 5.0f, new Vector3(441.3887, -981.2033, 30.68958)) ||
                         IsPlayerInRangeOfPoint(player, 5.0f, new Vector3(435.63, -977.03, 29.7177)) ||
                         IsPlayerInRangeOfPoint(player, 5.0f, new Vector3(1849.69, 3679.83, 34.2681)))
                {
                    DialogMigrator.CreateMenu(player, Dialogs.menu_pd_su, "Police Department", "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_pd_su, Content.License.Gun,
                        Price.License.Gun + "$");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_pd_su, Content.License.Hunting,
                        Price.License.Hunting + "$");

                    if (iPlayer.Crimes.Count > 0)
                    {
                        if (CrimeModule.Instance.CalcWantedStars(iPlayer.Crimes) == 0 && CrimeModule.Instance.CalcJailCosts(iPlayer.Crimes, iPlayer.EconomyIndex) > 0)
                        {
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_pd_su,
                            "Ticket ($" + CrimeModule.Instance.CalcJailCosts(iPlayer.Crimes, iPlayer.EconomyIndex) + ")",
                            "");
                        }
                    }

                    DialogMigrator.AddMenuItem(player, Dialogs.menu_pd_su, GlobalMessages.General.Close(), "");
                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_pd_su);
                    return;
                }
                else if (IsPlayerInRangeOfPoint(player, 2.5f,
                    new Vector3(483.179, -1309.349, 29.216)))
                {
                    if (iPlayer.job[0] != (int) jobs.JOB_MECH) return;
                    DialogMigrator.CreateMenu(player, Dialogs.menu_shop_mechanic, "Mechaniker Store", "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_mechanic, GlobalMessages.General.Close(), "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_mechanic, "Spraydose (300$)", "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_mechanic, "Schloss (1.000$)", "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_mechanic, "GPS-Tracker (4.000$)", "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_mechanic, "Leuchtstofflampe (2.000$)", "");

                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_shop_mechanic);
                    return;
                }
                else if (IsPlayerInRangeOfPoint(player, 20.0f,
                    new Vector3(895.7319, -178.6453, 74.70035)))
                {
                    DialogMigrator.CreateMenu(player, Dialogs.menu_taxi, "Taxi Zentrale", "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_taxi, "Taxilizenz", "Preis: $4300");
                    if (iPlayer.HasData("taxi"))
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_taxi, "Aus dem Dienst gehen", "");
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_taxi, GlobalMessages.General.Close(), "");
                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_taxi);
                }
                else if (IsPlayerInRangeOfPoint(player, 10.0f,
                    new Vector3(-1635.500, 181.086, 62.857)))
                {
                    // continue...
                    DialogMigrator.CreateMenu(player, Dialogs.menu_academic, "Universitaet",
                        "Waehlen Sie eine Weiterbildung");
                    if (iPlayer.uni_business[0] == 10)
                    {
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_academic, "Geschaeftsmann",
                            "Maximale Stufe erreicht");
                    }
                    else
                    {
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_academic, "Geschaeftsmann",
                            "Preisnachlass bei Fahrzeug&Hauskauf (naechste Stufe " +
                            ((iPlayer.uni_business[0] + 1) * 2) + "%)");
                    }

                    if (iPlayer.uni_economy[0] == 10)
                    {
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_academic, "Sparfuchs",
                            "Maximale Stufe erreicht");
                    }
                    else
                    {
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_academic, "Sparfuchs",
                            "Weniger Strom- & Wasserkosten (naechste Stufe " +
                            ((iPlayer.uni_economy[0] + 1) * 2) + "%)");
                    }

                    if (iPlayer.uni_workaholic[0] == 10)
                    {
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_academic, "Workaholic",
                            "Maximale Stufe erreicht");
                    }
                    else
                    {
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_academic, "Workaholic",
                            "Joberfahrung steigt um (naechste Stufe " +
                            ((iPlayer.uni_workaholic[0] + 1) * 2) + "%) schneller");
                    }

                    DialogMigrator.AddMenuItem(player, Dialogs.menu_academic, "Punkte resetten",
                        "Kosten: $" + (5000 * (iPlayer.Level - 1)));

                    DialogMigrator.AddMenuItem(player, Dialogs.menu_academic, GlobalMessages.General.Close(), "");
                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_academic);
                    return;
                }
            }

            // Mache das Interiorkaufmenu wieder auf
            if (iPlayer.DimensionType[0] == DimensionType.House_Shop_Interior)
            {
                House iHouse;
                if ((iHouse = HouseModule.Instance[iPlayer.OwnHouse[0]]) == null) return;
                DialogMigrator.CreateMenu(iPlayer.Player, Dialogs.menu_shop_interior, "Innenausstattung", "");
                DialogMigrator.AddMenuItem(iPlayer.Player, Dialogs.menu_shop_interior, GlobalMessages.General.Close(), "");
                DialogMigrator.AddMenuItem(iPlayer.Player, Dialogs.menu_shop_interior, "Interior kaufen",
                    "Kauft das aktuell ausgewaehlte Interior");
                foreach (var kvp in InteriorModule.Instance.GetAll())
                {
                    if (kvp.Value.Type == iHouse.Type)
                    {
                        DialogMigrator.AddMenuItem(iPlayer.Player, Dialogs.menu_shop_interior, kvp.Value.Comment + " $" + kvp.Value.Price,
                            "");
                    }
                }

                DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_shop_interior);
            }
        }
    }
}