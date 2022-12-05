using System;
using MySql.Data.MySqlClient;

namespace VMP_CNR.Module.Clothes.Props
{
    public class Prop : Loadable<uint>
    {
        public uint Id { get; set; }
        
        public string Name { get; set; }
        
        public int Slot { get; set; }
        
        public int Variation { get; set; }
        
        public int Texture { get; set; }
        
        public int Price { get; set; }
        
        public uint StoreId { get; set; }
        
        public int Gender { get; set; }
        
        public uint TeamId { get; set; }
        
        public bool IsDefault { get; set; }

        public int SubCatId { get; }

        public Tuple<int, uint, int> Tuple;

        public Prop(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            Name = reader.GetString(1);
            Slot = reader.GetInt32(2);
            Variation = reader.GetInt32(3);
            Texture = reader.GetInt32(4);
            Price = reader.GetInt32(5);
            StoreId = reader.GetUInt32(6);
            Gender = reader.GetInt32(7);
            TeamId = reader.GetUInt32(8);
            IsDefault = reader.GetInt32(9) == 1;
            SubCatId = reader.GetInt32("subcat_id");
        }

        public Prop(uint id, string name, int slot, int var, int text, int price, uint store, int gender, uint team,
            bool def, int subCatId = -1)
        {
            Id = id;
            Name = name;
            Slot = slot;
            Variation = var;
            Texture = text;
            Price = price;
            StoreId = store;
            Gender = gender;
            TeamId = team;
            IsDefault = def;
            SubCatId = subCatId;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}