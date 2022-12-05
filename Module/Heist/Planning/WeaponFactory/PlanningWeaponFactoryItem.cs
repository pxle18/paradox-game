using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Heist.Planning.WeaponFactory
{
    public class PlanningWeaponFactoryItem : Loadable<uint>
    {
        public uint Id { get; set; }
        public uint ResultItemId { get; set; }
        public int RequiredBlackMoney { get; set; }
        public Dictionary<uint, int> RequiredItems { get; set; }

        public PlanningWeaponFactoryItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            ResultItemId = reader.GetUInt32("result_item_id");
            RequiredBlackMoney = reader.GetInt32("required_blackmoney");

            RequiredItems = new Dictionary<uint, int>();

            string sourceItemString = reader.GetString("required_items");

            if (!string.IsNullOrEmpty(sourceItemString))
            {
                var splittedItemsSeperated = sourceItemString.Split(',');
                foreach (var splittedItemContainer in splittedItemsSeperated)
                {
                    if (splittedItemContainer == null || splittedItemContainer.Length <= 0 || !splittedItemContainer.Contains(":")) continue;
                    var splittedItemContainerParts = splittedItemContainer.Split(':');
                    if (splittedItemContainerParts.Length < 2) continue;
                    if (!UInt32.TryParse(splittedItemContainerParts[0], out uint splittedItemId)) continue;
                    if (!Int32.TryParse(splittedItemContainerParts[1], out int splittedItemAmount)) continue;

                    RequiredItems.Add(splittedItemId, splittedItemAmount);
                }
            }
        }
        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
