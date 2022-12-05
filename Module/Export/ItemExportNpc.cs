using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.NpcSpawner;

namespace VMP_CNR.Module.Export
{
    public class ItemExportNpc : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public Vector3 Position { get; set; }
        public uint Dimension { get; set; }
        public float Heading { get; set; }
        public PedHash Ped { get; set; }
        public List<ItemExport> ItemExportList { get; set; }

        public HashSet<uint> Teams { get; set; }
        public bool Illegal { get; set; }

        public int DealersCalculatingPriceSum { get; set; }

        public string LocationDescription { get; set; }

        public ItemExportNpc(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Dimension = reader.GetUInt32("dimension");
            Name = reader.GetString("name");
            Heading = reader.GetFloat("heading");
            Illegal = reader.GetInt32("illegal") == 1;
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Ped = Enum.TryParse(reader.GetString("pedhash"), true, out PedHash skin) ? skin : PedHash.ShopKeep01;

            LocationDescription = reader.GetString("location_desc");

            new Npc(Ped, Position, Heading, Dimension);

            DealersCalculatingPriceSum = 0;

            ItemExportList = new List<ItemExport>();
            foreach (ItemExport itemExport in ItemExportModule.Instance.GetAll().Values.Where(ie => ie.NpcId == Id))
            {
                ItemExportList.Add(itemExport);
                DealersCalculatingPriceSum += itemExport.MaxPrice;
            }

            var teamString = reader.GetString("team");

            Teams = new HashSet<uint>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId) || teamId == 0) continue;
                    Teams.Add(teamId);
                }
            }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void CalculateNewPrices(ItemExport itemExport)
        {
            if (itemExport.Price <= itemExport.MinPrice) return;

            int diff = Convert.ToInt32((double)itemExport.Price * 0.03); // -3% als Diff
            
            // Wenn Price - Diff unter MinPrice dann setze Diff auf (aktuell bis Minprice)
            if(itemExport.Price - diff < itemExport.MinPrice)
            {
                diff = itemExport.Price - itemExport.MinPrice;
            }

            // Ziehe die Diff ab
            itemExport.Price -= diff;

            // Setze die +1% auf alle anderen Items drauf
            foreach(ItemExport item in this.ItemExportList)
            {
                if (item.Item.Id == itemExport.Item.Id) continue;

                diff = Convert.ToInt32((double)item.Price * 0.01);
                // Wenn durch diff maxpreis überschritten, setze auf maxpreis
                if (item.Price + diff >= item.MaxPrice) item.Price = item.MaxPrice;
                else item.Price += diff;
            }
        }
    }
}