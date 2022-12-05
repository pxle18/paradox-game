using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMP_CNR.Module.Teams.AmmoArmory
{
    public class AmmoArmorie : Loadable<uint>
    {
        public UInt32 TeamId { get; set; }
        public int Packets { get; set; }
        public Vector3 Position { get; set; }
        public List<AmmoArmorieItem> ArmorieItems { get; set; }

        public int Powder { get; set; }

        public AmmoArmorie(MySqlDataReader reader) : base(reader)
        {
            TeamId = reader.GetUInt32("team_id");
            Position = new Vector3(reader.GetFloat("pos_x"),
                reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Packets = reader.GetInt32("packets");
            Powder = reader.GetInt32("powder");

            ArmorieItems = AmmoArmoryItemModule.Instance.GetAll().Values.Where(ai => ai.AmmoArmorieId == TeamId).ToList();
        }

        public override uint GetIdentifier()
        {
            return TeamId;
        }

        public void ChangePackets(int change)
        {
            Packets += change;
            SavePackets();
        }
        public void ChangePowder(int change)
        {
            Powder += change;
            SavePowder();
        }

        public void SavePackets()
        {
            MySQLHandler.ExecuteAsync($"UPDATE team_ammoarmories SET packets = '{Packets}' WHERE team_id = '{TeamId}'");
        }
        public void SavePowder()
        {
            MySQLHandler.ExecuteAsync($"UPDATE team_ammoarmories SET powder = '{Powder}' WHERE team_id = '{TeamId}'");
        }
    }
}
