using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Items;

namespace VMP_CNR.Module.Christmas.Models
{
    public class ChristmasPresentModel : Loadable<uint>
    {
        public uint Id { get; set; }
        public uint PlayerId { get; set; }
        public ItemModel Item { get; set; }
        public int Amount { get; set; }
        public string Code { get; set; }

        public ChristmasPresentModel(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            PlayerId = reader.GetUInt32("player_id");
            
            Item = ItemModelModule.Instance.Get(
                reader.GetUInt32("item_id")
            );

            Amount = reader.GetInt32("item_amount");
            Code = reader.GetString("christmas_code");
        }

        public override uint GetIdentifier() => Id;

        public void Delete()
        {
            MySQLHandler.Execute($"DELETE FROM log_present_reward WHERE Id = {Id}");
        }
    }
}
