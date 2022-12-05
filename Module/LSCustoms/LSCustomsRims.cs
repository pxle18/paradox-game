using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Configurations;

namespace VMP_CNR.Module.LSCustoms
{
    public class LSCustomsRims : Loadable<uint>
    {
        public uint id { get; set; }
        public string rim_name { get; set; }
        public int tuning_id { get; set; }
        public Boolean chrome { get; set; }
        public int category_id { get; set; }

        public LSCustomsRims(MySqlDataReader reader) : base(reader)
        {
            id = reader.GetUInt32("id");
            rim_name = reader.GetString("rim_name");
            tuning_id = reader.GetInt32("tuning_id");
            chrome = reader.GetBoolean("chrome");
            category_id = reader.GetInt32("rim_category_id");
        }

        public override uint GetIdentifier()
        {
            return id;
        }

    }

   
}
