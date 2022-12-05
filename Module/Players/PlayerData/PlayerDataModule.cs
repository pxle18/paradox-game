using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.PlayerDataCustom
{
    /***
     * 
      string CustomKey = "enten";
                    if (!dbPlayer.PlayerDataCustom.ContainsKey(CustomKey))
                    {
                        dbPlayer.PlayerDataCustom.Add(CustomKey, new DbPlayerDataCustom(0,dbPlayer.Id,"enten","0",DateTime.Now));
                        dbPlayer.PlayerDataCustom[CustomKey].CreateKey();
                    }

                    if(dbPlayer.PlayerDataCustom.TryGetValue(CustomKey, out var value))
                    {
                        if(value.ParseInt() >= MaxEnten || value.Lastchanged.AddDays(1) <= DateTime.Now)
                        {
                            dbPlayer.SendNewNotification("Genug Enten für heute");
                            return true;
                        }
                        else
                        {
                            value.UpdateValue(value.ParseInt() + 1);
                        }
                    }
     * 
     * 
     * 
     * 
     * 
     * */
    public class DbPlayerDataCustomModule : Module<DbPlayerDataCustomModule>
    {
        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.PlayerDataCustom = new Dictionary<string, DbPlayerDataCustom>();

            string query = $"SELECT * FROM `player_data` WHERE `player_id` = '{dbPlayer.Id}'";
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var CustomReader = cmd.ExecuteReader())
                {
                    if (CustomReader.HasRows)
                    {
                        while (CustomReader.Read())
                        {
                            //Console.WriteLine(CustomReader.GetString("pkey"));
                            dbPlayer.PlayerDataCustom.Add(CustomReader.GetString("pkey"), 
                            new DbPlayerDataCustom(
                            CustomReader.GetUInt32("id"),
                            CustomReader.GetUInt32("player_id"),
                            CustomReader.GetString("pkey"),
                            CustomReader.GetString("pvalue"),
                            CustomReader.GetDateTime("lastchanged"))
                            );
                        }
                    }
                }
            }
        }



    }

}
