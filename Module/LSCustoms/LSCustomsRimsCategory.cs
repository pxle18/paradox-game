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
    public class LSCustomsRimsCategory : Loadable<uint>
    {
        public uint id { get; set; }
        public string category_name { get; set; }
        public int category_id { get; set; }

        public LSCustomsRimsCategory(MySqlDataReader reader) : base(reader)
        {
            id = reader.GetUInt32("id");
            category_name = reader.GetString("category_name");
            category_id = reader.GetInt32("category_id");
        }

        public override uint GetIdentifier()
        {
            return id;
        }

    }

   
}
