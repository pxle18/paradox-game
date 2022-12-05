using System;

namespace VMP_CNR.Module.Boerse
{
    public class Aktie
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public uint LastAmount { get; private set; }
        public uint MinAmount { get; private set; }
        public uint MaxAmount { get; private set; }
        public uint ActualAmount { get; private set; }
        
        public Aktie(string name, string description, uint lastAmount, uint minAmount, uint maxAmount,
            uint actualAmount)
        {
            Name = name;
            Description = description;
            LastAmount = lastAmount;
            MinAmount = minAmount;
            MaxAmount = maxAmount;
            ActualAmount = actualAmount;
        }

        public void UpdateAktie(uint lastAmount, uint minAmount, uint maxAmount, uint actualAmount)
        {
            LastAmount = lastAmount;
            MinAmount = minAmount;
            MaxAmount = maxAmount;
            ActualAmount = actualAmount;
        }
    }
}