using System.Collections.Generic;

namespace VMP_CNR.Module.Configurations
{
    public class DefaultConfiguration
    {
        public bool DevLog { get; }
        public bool Ptr { get; }
        public bool DevMode { get; }
        public string VoiceChannel { get; }
        public string VoiceChannelPassword { get; }
        public bool IsServerOpen { get; set; }
        public bool InventoryActivated { get; set; }
        public bool EKeyActivated { get; set; }
        public bool BlackMoneyEnabled { get; set; }
        public bool MeertraeubelEnabled { get; set; }
        public bool JailescapeEnabled { get; set; }
        public bool MethLabEnabled { get; set; }
        public bool JumpPointsEnabled { get; set; }
        public string mysql_pw { get; }
        public string mysql_user { get; }
        public string mysql_user_forum { get; }
        public string mysql_pw_forum { get; }
        public bool disableAPILogin { get; set; }
        public bool LipsyncActive { get; set; }
        public bool TuningActive { get; set; }

        public bool CanBridgeUsed { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsUpdateModeOn { get; set; }

        public bool PlayerSync { get; set; } = true;
        public bool VehicleSync { get; set; } = true;

        public float WeaponDamageMultipier { get; set; }
        public float MeleeDamageMultipier { get; set; }
        public bool DamageLog { get; set; }
        public string RESET_API_KEY { get; set; }
        public string CLEAR_API_KEY { get; set; }
        public string MAINTENACE_API_KEY { get; set; }
        public bool ShowAllJumppoints { get; set; }
        public bool EventActive { get; set; }
        
        public string MapsPath { get; set; }
        public string DevServerName { get; set; }

        public bool IsLinux { get; set; }

        public bool DisableLauncher { get; set; }
        public LogLevel LoggingLevel { get; set; }
        public uint DamageThreads { get; set; }
        public string LinuxPath { get; set; }


        public DefaultConfiguration(IReadOnlyDictionary<string, dynamic> data)
        {
            DevLog = false;
            Ptr = false;
            DevMode = true;
            VoiceChannel = "";
            VoiceChannelPassword = "";
            IsServerOpen = false;
            InventoryActivated = true;
            EKeyActivated = true;
            BlackMoneyEnabled = true;  //set to true later
            MethLabEnabled = true;  //set to true later
            MeertraeubelEnabled = true; //set to true later
            JailescapeEnabled = false;
            JumpPointsEnabled = true;
            mysql_user = "vultradmin";
            mysql_pw = "AVNS_jKBQhHfTpVcr1AJ27JW";
            mysql_user_forum = "forum_live";
            mysql_pw_forum = "AVNS_jKBQhHfTpVcr1AJ27JW";

            DisableLauncher = false;
            if(data.ContainsKey("disable_launcher"))
            {
                DisableLauncher = bool.Parse(data["disable_launcher"]);
            }

            MapsPath = "C:\\MapsTest";
            if(data.ContainsKey("maps_path"))
            {
                MapsPath = data["maps_path"];
            }

            IsLinux = false;
            if (data.ContainsKey("linux"))
            {
                IsLinux = bool.Parse(data["linux"]);
            }

            DevServerName = "ragemp_server.exe";

            CanBridgeUsed = false;
            MaxPlayers = data.ContainsKey("max_players") ? int.Parse(data["max_players"]) : 1000;

            // Damage Multipliers
            WeaponDamageMultipier = 0.35f;
            MeleeDamageMultipier = 0.25f;

            DamageLog = data.ContainsKey("damagelog") ? bool.Parse(data["damagelog"]) : false;

            disableAPILogin = false;
            LipsyncActive = data.ContainsKey("lipsync") ? bool.Parse(data["lipsync"]) : false;
            TuningActive = true;
            IsUpdateModeOn = false;
            RESET_API_KEY = data.ContainsKey("reset_api_key") ? data["reset_api_key"] : "";
            CLEAR_API_KEY = data.ContainsKey("clear_api_key") ? data["clear_api_key"] : "";
            MAINTENACE_API_KEY = data.ContainsKey("maintenance_api_key") ? data["maintenance_api_key"] : "";
            ShowAllJumppoints = false;
            EventActive = false;
            LoggingLevel = data.ContainsKey("logging_level") ? (LogLevel)uint.Parse(data["logging_level"]) : LogLevel.Everything;
            DamageThreads = data.ContainsKey("damage_threads") ? uint.Parse(data["damage_threads"]) : (uint)4;
            LinuxPath = data.ContainsKey("linux_server_path") ? data["linux_server_path"] : "/home/ragemp-srv/";
        }
        
        public string GetMySqlConnection()
        {
            return Ptr
                ? "server='45.135.201.56'; uid='gvmp'; pwd='YLuOibDV75DglViZ'; database='dev_gvmp_ptr_1.1';max pool size=999;SslMode=none;convert zero datetime=True;"
                : "server='db.prdx.to'; uid='" + mysql_user + "'; pwd='" + mysql_pw + "'; port=16751; database='ingame_live';max pool size=999;SslMode=none;convert zero datetime=True;";
        }
        
        public string GetMySqlConnectionBoerse()
        {
            return Ptr
                ? "server='45.135.201.56'; uid='gvmp'; pwd='YLuOibDV75DglViZ'; database='boerse';max pool size=999;SslMode=none;convert zero datetime=True;"
                : "server='db.prdx.to'; uid='" + mysql_user + "'; pwd='" + mysql_pw + "'; database='boerse';max pool size=999;SslMode=none;convert zero datetime=True;";
        }

        public string GetMySqlConnectionForum()
        {
            return
                "server='db.prdx.to'; uid='" + mysql_user_forum + "'; pwd='" + mysql_pw_forum + "'; database='forum_live';max pool size=999;SslMode=none;";
        }

        public override string ToString()
        {
            return $"Devlog: {DevLog}\n" +
                   $"Ptr: {Ptr}\n" +
                   $"DevMode: {DevMode}\n" +
                   $"VoiceChannel: {VoiceChannel}\n" +
                   $"VoiceChannelPassword: {VoiceChannelPassword}\n";
        }
    }

    public enum LogLevel : uint
    {
        None        = 0,
        Minimum     = 1,
        Medium      = 2,
        Everything  = 3
    }
}