using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.PoliceAktenSearchApp
{
    public class PoliceAktenSearchApp : SimpleApp
    {
        public PoliceAktenSearchApp() : base("PoliceAktenSearchApp")
        {
        }
        
        [RemoteEvent]
        public void requestPlayerResults(Player player, string searchQuery, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            if (!MySQLHandler.IsValidNoSQLi(player, searchQuery)) return;

            HandlePoliceAktenSearch(player, searchQuery);
        }

        private void HandlePoliceAktenSearch(Player p_Client, string searchQuery)
        {
            DbPlayer p_DbPlayer = p_Client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            var l_Overview = GetSearchResults(searchQuery);
            TriggerNewClient(p_Client, "responsePlayerResults", NAPI.Util.ToJson(l_Overview));
        }

        private List<string> GetSearchResults(string searchQuery)
        {
            List<string> results = new List<string>();

            List<uint> resultPlayerIds = new List<uint>();
            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                keyConn.Open();
                keyCmd.CommandText =
                    $"SELECT player_id FROM player_customdata WHERE address LIKE '%{searchQuery}%' OR membership LIKE '%{searchQuery}%' OR phone LIKE '%{searchQuery}%' OR info LIKE '%{searchQuery}%' LIMIT 10;";
                using (var keyReader = keyCmd.ExecuteReader())
                {
                    if (keyReader.HasRows)
                    {
                        while (keyReader.Read())
                        {
                            resultPlayerIds.Add(keyReader.GetUInt32("player_id"));
                        }
                    }
                }
                keyConn.Close();
            }

            // by Player Name
            foreach (PlayerName.PlayerName pn in PlayerNameModule.Instance.GetAll().Values.Where(pn => pn.Name.ToLower().Contains(searchQuery.ToLower()) || resultPlayerIds.Contains(pn.Id)).Take(30).ToList())
            {
                results.Add(pn.Name);
            }

            return results;
        }
    }
}
