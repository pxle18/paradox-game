using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Utilities;

/// <summary>
/// This function checks for any sleeping connections beyond a reasonable time and kills them.
/// Since .NET appears to have a bug with how pooling MySQL connections are handled and leaves
/// too many sleeping connections without closing them, we will kill them here.
/// </summary>
/// iMinSecondsToExpire - all connections sleeping more than this amount in seconds will be killed.
/// <returns>integer - number of connections killed</returns>
/// 
namespace VMP_CNR
{
    /**
     * Really trash lol
     */
    public class DatabaseLogging : Singleton<DatabaseLogging>
    {
        public void LogAdminAction(Player admin, string user, AdminLogTypes type, string reason, int optime = 0, bool devMode = false)
        {
            DbPlayer dbPlayer = admin.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;
            
            string xtype = "undefined";

            // Getting Type
            switch (type)
            {
                case AdminLogTypes.perm:
                    xtype = "permanent Ban";
                    break;
                case AdminLogTypes.timeban:
                    xtype = "terminated Ban (" + optime + ")";
                    break;
                case AdminLogTypes.kick:
                    xtype = "kick";
                    break;
                case AdminLogTypes.warn:
                    xtype = "Verwarnung";
                    break;
                case AdminLogTypes.log:
                    xtype = "Logging";
                    break;
                case AdminLogTypes.whisper:
                    xtype = "Whisper";
                    break;
                case AdminLogTypes.setitem:
                    xtype = "Setitem";
                    break;
                case AdminLogTypes.coord:
                    xtype = "Coord";
                    break;
                case AdminLogTypes.veh:
                    xtype = "Veh";
                    break;
                case AdminLogTypes.arev:
                    xtype = "Arev";
                    break;
                case AdminLogTypes.setdpos:
                    xtype = "Setdpos";
                    break;
                case AdminLogTypes.setgarage:
                    xtype = "setgarage";
                    break;
            }
            
            string query = "";

            // Special Whisperlog
            if (type == AdminLogTypes.whisper)
            {
                query = string.Format(
                    "INSERT INTO `log_whisper` (`sender`, `player`, `message`) VALUES ('{0}', '{1}', '{2}')",
                    dbPlayer.GetName(), MySqlHelper.EscapeString(user), MySqlHelper.EscapeString(reason));
            }
            else
            {
                query = string.Format(
                    "INSERT INTO `log_admin` (`admin`, `user`, `type`, `reason`) VALUES ('{0}', '{1}', '{2}', '{3}')",
                    dbPlayer.GetName(), MySqlHelper.EscapeString(user), (int)type, MySqlHelper.EscapeString(xtype + ":: " + reason));
            }
            MySQLHandler.ExecuteAsync(query);
        }

        public void LogAcpAdminAction(PlayerNameModel admin, string user, AdminLogTypes type, string reason)
        {
            string xtype = "undefined";

            // Getting Type
            switch (type)
            {
                case AdminLogTypes.kick:
                    xtype = "acp-kick";
                    break;
                case AdminLogTypes.whisper:
                    xtype = "acp-whisper";
                    break;
                case AdminLogTypes.setmoney:
                    xtype = "acp-setmoney";
                    break;
            }

            string query = "";

            // Special Whisperlog
            query = string.Format(
                    "INSERT INTO `log_admin` (`admin`, `user`, `type`, `reason`) VALUES ('{0}', '{1}', '{2}', '{3}')",
                    admin.Name, MySqlHelper.EscapeString(user), (int) type, MySqlHelper.EscapeString(xtype + ":: " + reason));
            MySQLHandler.ExecuteAsync(query);
        }

        public void LogAcpAdminAction(string admin, string user, AdminLogTypes type, string reason)
        {
            string xtype = "undefined";

            // Getting Type
            switch (type)
            {
                case AdminLogTypes.kick:
                    xtype = "acp-kick";
                    break;
                case AdminLogTypes.whisper:
                    xtype = "acp-whisper";
                    break;
                case AdminLogTypes.setmoney:
                    xtype = "acp-setmoney";
                    break;
            }

            string query = "";

            // Special Whisperlog
            query = string.Format(
                    "INSERT INTO `log_admin` (`admin`, `user`, `type`, `reason`) VALUES ('{0}', '{1}', '{2}', '{3}')",
                    admin, MySqlHelper.EscapeString(user), (int)type, MySqlHelper.EscapeString(xtype + ":: " + reason));
            MySQLHandler.ExecuteAsync(query);
        }
    }
}