using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Nutrition
{
    public class NutritionItem : Loadable<uint>
    {
        public uint Id { get; }
        public uint Items_gd_id;
        public int food;
        public int drink;
        public string Effect;


        public NutritionItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Items_gd_id = reader.GetUInt32("items_gd_id");
            food = reader.GetInt32("food");
            drink = reader.GetInt32("drink");
            Effect = reader.GetString("effect");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
