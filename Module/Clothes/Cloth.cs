using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace VMP_CNR.Module.Clothes
{
    public class Cloth : Loadable<uint>
    {
        public uint Id { get; }

        public string Name { get; }

        public int Slot { get; }

        public int Variation { get; }

        public int Texture { get; }

        public HashSet<uint> Teams { get; }

        public int StoreId { get; }

        public int Gender { get; }

        public int Price { get; }

        public bool IsDefault { get; }

        public int SubCatId { get; }

        public Tuple<int, uint, int> Tuple { get; }

        public Cloth(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Slot = reader.GetInt32("slot");
            Variation = reader.GetInt32("variation");
            Texture = reader.GetInt32("texture");
            StoreId = reader.GetInt32("store_id");
            Gender = reader.GetInt32("gender");
            Price = reader.GetInt32("price");
            IsDefault = reader.GetInt32("default") == 1;
            SubCatId = reader.GetInt32("subcat_id");

            var teamString = reader.GetString("team");
            Teams = new HashSet<uint>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId)) continue;
                    Teams.Add(teamId);
                }
            }
        }

        public Cloth(
            uint id, string name, int slot, int var,
            int text, HashSet<uint> teams, int store, int gender,
            int price, bool def, int subCatId = -1)
        {
            Id = id;
            Name = name;
            Slot = slot;
            Variation = var;
            Texture = text;
            StoreId = store;
            Gender = gender;
            Price = price;
            IsDefault = def;
            Teams = teams;
            SubCatId = subCatId;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}