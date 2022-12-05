using MySql.Data.MySqlClient;

namespace VMP_CNR.Module.Animal
{
    public class AnimalDataModule : SqlModule<AnimalDataModule, AnimalData, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `animaldata`;";
        }
    }

    public class AnimalData : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint UIntHash { get; set; }
        public int Textures { get; set; }
        public bool CanAttack { get; set; }

        public uint HuntingItemId { get; set; }

        public AnimalData(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            UIntHash = reader.GetUInt32("uint");
            Textures = reader.GetInt32("textures");
            CanAttack = reader.GetInt32("can_attack") == 1;
            HuntingItemId = reader.GetUInt32("hunting_item_id");
        }
        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
