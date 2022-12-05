using System.Collections.Generic;

namespace VMP_CNR.Module.Clothes.Slots
{
    public class PropsSlot
    {
        public int Id { get; }

        public string Name { get; }

        public List<SlotCategory> Categories { get; }

        public PropsSlot(int id, string name, List<SlotCategory> categories = null)
        {
            Id = id;
            Name = name;
            Categories = categories ?? new List<SlotCategory>();
        }
    }
}