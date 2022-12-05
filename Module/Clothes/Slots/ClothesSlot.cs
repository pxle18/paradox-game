using System.Collections.Generic;

namespace VMP_CNR.Module.Clothes.Slots
{
    public class ClothesSlot
    {
        public int Id { get; }

        public string Name { get; }

        public List<SlotCategory> Categories { get; }

        public ClothesSlot(int id, string name, List<SlotCategory> categories)
        {
            Id = id;
            Name = name;
            Categories = categories ?? new List<SlotCategory>();
        }
    }
}