using MySql.Data.MySqlClient;

namespace VMP_CNR.Module.Vehicles.Data
{
    public class VehicleCarsellCategory : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int Limit { get; }

        public VehicleCarsellCategory(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("category");
            Limit = reader.GetInt32("category_limit");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}