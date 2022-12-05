using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Weapons.Component;

namespace VMP_CNR.Module.Helper
{
    public static class FlagsHelper
    {
        public static bool IsSet<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void Set<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        public static void Unset<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }
    }

    public class Tuning
    {
        public uint ID { get; }
        public string Name { get; }
        public int MaxIndex { get; }
        public int StartIndex { get; }

        public Tuning(uint p_ID, string p_Name, int p_StartIndex = -1, int p_MaxIndex = 20)
        {
            ID = p_ID;
            Name = p_Name;
            MaxIndex = p_MaxIndex;
            StartIndex = p_StartIndex;
        }
    }
    public static class Helper
    {
        /*
         * https://wiki.rage.mp/index.php?title=Vehicle_Mods#Plate_Types
         */
        public static Dictionary<int, Tuning> m_Mods = new Dictionary<int, Tuning>
        {
            {1, new Tuning(0, "Spoiler", p_MaxIndex: 100)},
            {2, new Tuning(1, "Front Bumper", p_MaxIndex: 100)},
            {3, new Tuning(2, "Rear Bumper", p_MaxIndex: 100)},
            {4, new Tuning(3, "Side Skirt", p_MaxIndex: 100)},
            {5, new Tuning(4, "Exhaust", p_MaxIndex: 100)},
            {6, new Tuning(5, "Frame")},
            {7, new Tuning(6, "Grille")},
            {8, new Tuning(7, "Hood", p_MaxIndex: 100)},
            {9, new Tuning(8, "Fender")},
            {10, new Tuning(9, "Right Fender")},
            {11, new Tuning(10, "Roof")},
            {12, new Tuning(11, "Engine", p_MaxIndex: 3)},
            {13, new Tuning(12, "Brakes", p_MaxIndex: 2)},
            {14, new Tuning(13, "Transmission", p_MaxIndex: 2)},
            {15, new Tuning(14, "Horn", p_MaxIndex: 52)},
            {16, new Tuning(15, "Suspension", p_MaxIndex: 3)},
            {17, new Tuning(16, "Armor", p_MaxIndex: 4)},
            {18, new Tuning(18, "Turbo", p_MaxIndex: 0)},
            {19, new Tuning(22, "Xenon", p_MaxIndex: 0)},
            //{20, new Tuning(23, "Front Wheels", p_MaxIndex: 250)},
            //{21, new Tuning(24, "Back Wheels", p_MaxIndex: 250)},
            {22, new Tuning(27, "Trim Design")},
            {23, new Tuning(30, "Dials")},
            {24, new Tuning(33, "Steering Wheel")},
            {25, new Tuning(34, "Shift Lever")},
            {26, new Tuning(38, "Hydraulics")},
            {27, new Tuning(48, "Livery", p_MaxIndex: 100)},
            {28, new Tuning(55, "Window Tint", p_StartIndex: 0, p_MaxIndex: 5)},
            {29, new Tuning(80, "Headlight Color", p_MaxIndex: 12)},
            {30, new Tuning(53, "Numberplate")},
            {31, new Tuning(95, "Tire SmokeR")},
            {32, new Tuning(96, "Tire SmokeG")},
            {33, new Tuning(97, "Tire SmokeB")},
            {34, new Tuning(98, "Pearllack")},
            {35, new Tuning(99, "Felgenfarbe")},
            {36, new Tuning(25,"Plateholders")},
            {37, new Tuning(26,"Vanity Plates")},
            {39, new Tuning(28,"Ornaments")},
            {40, new Tuning(29,"Dashboard")},
            {42, new Tuning(31,"Door Speaker")},
            {43, new Tuning(32,"Seats")},
            {46, new Tuning(35,"Plaques")},
            {47, new Tuning(37,"Trunk")},
            {48, new Tuning(39,"Engine Block")},
            {49, new Tuning(40,"Air filter")},
            {50, new Tuning(41,"Struts")},
            {51, new Tuning(42,"Arch Cover")},
            {52, new Tuning(43,"Aerials")},
            {53, new Tuning(44,"Trim")},
            {54, new Tuning(46,"Windows")},
        };

        public static string GenerateAuthKey()
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes($"{DateTime.Now}1337ajklödfjlöadjklöfkalödf"));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
        
        
        public static int GetTimestamp(DateTime date)
        {
            return (int) date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string ComplainPlayerDataInt(int[] playerVar, string dbField)
        {
            string str = "";
            if (playerVar[0] != playerVar[1])
            {
                str = "`" + dbField + "` = '" + Convert.ToString(playerVar[0]) + "'";
            }

            return str;
        }

        public static string GetWeapons(DbPlayer p_Player)
        {
            string l_String = "";
            var l_JsonOutput = GTANetworkAPI.NAPI.Util.ToJson(p_Player.Weapons);

            l_String = "`weapons` = '" + l_JsonOutput + "'";

            return l_String;
        }


        public static string GetWeaponComponents(DbPlayer p_Player)
        {
            string l_String = "";

            Dictionary<uint, List<uint>> components = new Dictionary<uint, List<uint>>();

            foreach(KeyValuePair<uint, List<WeaponComponent>> kvp in p_Player.WeaponComponents)
            {
                if(!components.ContainsKey(kvp.Key))
                {
                    components.Add(kvp.Key, new List<uint>());
                }

                foreach(WeaponComponent comp in kvp.Value.ToList())
                {
                    if(!components[kvp.Key].Contains((uint)comp.Id))
                    {
                        components[kvp.Key].Add((uint)comp.Id);
                    }
                }
            }

            var l_JsonOutput = GTANetworkAPI.NAPI.Util.ToJson(components);

            l_String = "`weapon_components` = '" + l_JsonOutput + "'";

            return l_String;
        }

        public static bool CheckPlayerData(DbPlayer dbPlayer, dynamic playerData, DbPlayer.Value value,
            out string query)
        {
            var valueUInt = (uint) value;
            if (dbPlayer.DbValues[valueUInt] != playerData)
            {
                string stringData;
                switch (playerData)
                {
                    case bool _:
                        stringData = playerData ? "1" : "0";
                        break;
                    case Enum _:
                        stringData = Convert.ToString(Convert.ChangeType(playerData,
                            Enum.GetUnderlyingType(playerData.GetType())));
                        break;
                    default:
                        stringData = Convert.ToString(playerData);
                        break;
                }

                query = "`" + DbPlayer.DbColumns[valueUInt] + "` = '" + stringData + "'";
                dbPlayer.DbValues[valueUInt] = playerData;
                return true;
            }

            query = null;
            return false;
        }

        public static string ComplainPlayerDataInt(uint[] playerVar, string dbField)
        {
            string str = "";
            if (playerVar[0] != playerVar[1])
            {
                str = "`" + dbField + "` = '" + Convert.ToString(playerVar[0]) + "'";
            }

            return str;
        }

        public static string ComplainPlayerData(DimensionType[] playerVar, string dbField)
        {
            return (int) playerVar[0] != (int) playerVar[1] ? $"`{dbField}` = '{(int) playerVar[0]}'" : null;
        }
        
        public static string ComplainPlayerDataFloat(float[] playerVar, string dbField)
        {
            string str = "";
            if (playerVar[0] != playerVar[1])
            {
                str = "`" + dbField + "` = '" + Convert.ToString(playerVar[0]) + "'";
            }

            return str;
        }

        public static string ComplainPlayerDataString(string[] playerVar, string dbField)
        {
            if (playerVar[0] == null) return "";
            string str = "";
            if (playerVar[0] != playerVar[1])
            {
                str = "`" + dbField + "` = '" + Convert.ToString(playerVar[0]) + "'";
            }

            return str;
        }

        public static string GetLetter(Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var num = random.Next(0, chars.Length - 1);
            return "" + chars[num];
        }
    }

    public class ResponseData
    {
        public int m_Code { get; set; }
        public string m_Type { get; set; }
        public string m_Reason { get; set; }
    }

    public class WhitelistData
    {
        public string m_Key { get; set; }
        public string m_AccountData { get; set; }
        public string m_Hash { get; set; }
    }

    public class ClearWhitelistData
    {
        public string api_token { get; private set; }

        public ClearWhitelistData()
        {
            api_token = Configurations.Configuration.Instance.CLEAR_API_KEY;
        }
    }

    public class ClearWhitelistDataAnswer
    {
        public bool success { get; set; }
    }

    /// <summary>
    /// Von Gameserver ausgehendes Paket für das Löschen eines Spielers aus der Launcher-Whitelist
    /// </summary>
    public class ResetWhitelistData
    {
        public string auth_token { get; private set; }
        public string forum_id { get; private set; }

        public ResetWhitelistData(string p_ForumID)
        {
            auth_token = Configurations.Configuration.Instance.RESET_API_KEY;
            forum_id = p_ForumID;
        }
    }

    /// <summary>
    /// Von API Ausgehendes Paket nach Löschen eines Spielers aus der Launcher Whitelist
    /// </summary>
    public class ResetWhitelistDataAnswer
    {
        public bool success { get; set; }
    }

    /// <summary>
    /// Von API Ausgehendes Paket nach Whitelist Statis Abfrage
    /// </summary>
    public class WhitelistDataAnswer
    {
        public bool success { get; set; }
        public string reason { get; set; }
    }

    public class VerifyData
    {
        public uint forum_id { get; private set; }
        public string ip { get; private set; }

        public VerifyData(uint p_ForumID, string p_IP)
        {
            forum_id = p_ForumID;
            ip = p_IP;
        }
    }

    public class VerifyDataAnswer
    {
        public bool success { get; set; }
    }

    /// <summary>
    /// CommType sind die verschiedenen Möglichkeiten der Nachrichtenübermittlung
    /// </summary>
    public enum CommType : uint
    {
        /// <summary>
        /// Rote Nachricht im Launcher
        /// </summary>
        BreakingNews = 0,

        /// <summary>
        /// Launcher-Wartungsarbeiten Begründung
        /// </summary>
        Maintenance = 1,

        /// <summary>
        /// NYI
        /// </summary>
        Development = 2
    }

    /// <summary>
    /// Objekt, was eine Comm-Nachricht beinhaltet und dessen Status (aktiv oder inaktiv wie Euka himself)
    /// </summary>
    public class Communication
    {
        public string message { get; private set; }
        public bool active { get; private set; }

        /// <summary>
        /// Constructor für das Comm-Objekt
        /// </summary>
        /// <param name="p_Message">Die Comm-Nachricht</param>
        /// <param name="p_Active">Status der Nachricht (wird angezeigt im Launcher oder nicht)</param>
        public Communication(string p_Message, bool p_Active)
        {
            message = p_Message;
            active = p_Active;
        }
    }

    /// <summary>
    /// Eingehendes POST-Paket zum ändern des Statuses und Nachricht einer Comm-Nachricht mit angegebenen Typ
    /// </summary>
    public class CommunicationsData
    {
        public string api_key { get; set; }
        public CommType type { get; set; }
        public bool active { get; set; }
        public string message { get; set; }

        public CommunicationsData(string p_Key, CommType p_Type, bool p_Active, string p_Message)
        {
            api_key = p_Key;
            type    = p_Type;
            active  = p_Active;
            message = p_Message;
        }
    }

    /// <summary>
    /// Ausgehendes Paket nach Anfrage zum ändern einer Comm-Nachricht
    /// </summary>
    public class CommunicationsDataAnswer
    {
        public bool active { get; private set; }
        public string message { get; private set; }

        /// <summary>
        /// Constructor für ausgehendes Comm-Nachricht Paket
        /// </summary>
        /// <param name="p_Success">Gibt an, ob die Anfrage erfolgreich war</param>
        /// <param name="p_Message">Die geänderte Nachricht hier angeben</param>
        public CommunicationsDataAnswer(bool p_Success, string p_Message)
        {
            active = p_Success;
            message = p_Message;
        }
    }
}