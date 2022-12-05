using GTANetworkAPI;
using MySql.Data.MySqlClient;


namespace VMP_CNR.Module.Houses
{
    public class InteriorIplDisable : Loadable<uint>
    {
        public uint Id { get; set; }
        public uint InteriorID { get; set; }
        public string IPL { get; set; }

        public InteriorIplDisable(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            InteriorID = reader.GetUInt32("interior_id");
            IPL = reader.GetString("ipl");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
