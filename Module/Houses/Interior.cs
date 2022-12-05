using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace VMP_CNR.Module.Houses
{
    public class Interior : Loadable<uint>
    {
        public uint Id { get; set; }
        public int Type { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public int Price { get; set; }
        public string Comment { get; set; }

        public Vector3 ClothesPosition { get; set; }
        public Vector3 InventoryPosition { get; set; }

        public Interior(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("posX"),
                reader.GetFloat("posY"), reader.GetFloat("posZ"));
            Heading = reader.GetFloat("Heading");
            Type = reader.GetInt32("type");
            Price = reader.GetInt32("price");
            Comment = reader.GetString("comment");

            ClothesPosition = new Vector3(reader.GetFloat("clothes_pos_x"),
                reader.GetFloat("clothes_pos_y"), reader.GetFloat("clothes_pos_z"));

            InventoryPosition = new Vector3(reader.GetFloat("inventory_pos_x"),
                reader.GetFloat("inventory_pos_y"), reader.GetFloat("inventory_pos_z"));
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}