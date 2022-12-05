using VMP_CNR.Handler;
using VMP_CNR.Module.Items;

namespace VMP_CNR.Module.Armory
{
    public class ArmoryItem
    {
        public int ItemId { get; set; }
        public ItemModel Item { get; set; }
        public int RestrictedRang { get; set; }
        public int Packets { get; set; }

        public int Price { get; set; }
    }
}