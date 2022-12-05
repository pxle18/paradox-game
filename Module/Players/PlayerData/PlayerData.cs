using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.PlayerDataCustom
{
    public class DbPlayerDataCustom : Loadable<uint>
    {
        public uint Id { get; }
        public uint PlayerId { get; }
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime Lastchanged { get; set; }

        public DbPlayerDataCustom(uint id, uint playerid, string key, string value, DateTime lastchanged)
        {
            Id = id;
            PlayerId = playerid;
            Key = key;
            Value = value;
            Lastchanged = lastchanged;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void UpdateValue(dynamic data)
        {
            Value = Convert.ToString(data);
            MySQLHandler.ExecuteAsync($"UPDATE player_data SET pvalue = '{Value}' WHERE pkey = '{Key}' AND player_id = '{PlayerId}';");
        }

        public void DeleteKey()
        {
            MySQLHandler.ExecuteAsync($"DELETE FROM player_data WHERE pkey = '{Key}' AND player_id = '{PlayerId}';");
        }

        public void CreateKey()
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO player_data (player_id,pkey,pvalue) VALUES ({PlayerId},'{Key}','{Value}')");
        }

        public int ParseInt()
        {
            return Int32.Parse(Value);
        }

    }

}