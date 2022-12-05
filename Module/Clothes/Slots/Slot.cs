namespace VMP_CNR.Module.Clothes.Slots
{
    public class Slot
    {
        public string Id { get; }
        public string Name { get; }

        public Slot(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}