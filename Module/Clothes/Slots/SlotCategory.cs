namespace VMP_CNR.Module.Clothes.Slots
{
    public class SlotCategory
    {
        public int Id { get; }

        public string Name { get; }

        public SlotCategory(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}