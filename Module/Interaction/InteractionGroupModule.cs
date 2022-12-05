using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMP_CNR.Module.Interaction
{
    public class InteractionGroupModule : SqlModule<InteractionGroupModule, InteractionGroupItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `interaction_groupanims`;";
        }

        public List<InteractionGroupItem> GetByGroup(int id)
        {
            return Instance.GetAll().Values.Where(c => c.InteractionGroupId == id).ToList();
        }
    }

    public class InteractionGroupItem : Loadable<uint>
    {
        public uint Id { get; set; }

        public int InteractionGroupId { get; set; }
        public string Anim1 { get; set; }
        public string Anim2 { get; set; }

        public InteractionGroupItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");

            InteractionGroupId = reader.GetInt32("interactiongroup_id");

            Anim1 = reader.GetString("anim1");
            Anim2 = reader.GetString("anim2");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
