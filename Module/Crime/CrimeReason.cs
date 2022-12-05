using MySql.Data.MySqlClient;
using System;

namespace VMP_CNR.Module.Crime
{
    public class CrimeReason : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Jailtime { get; }
        public int Costs { get; }
        public bool TakeGunLic { get; }
        public bool TakeDriversLic { get; }
        public CrimeCategory Category { get; }

        public CrimeReason(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Description = reader.GetString("description");
            Jailtime = reader.GetInt32("jailtime");
            Costs = reader.GetInt32("costs");
            TakeGunLic = reader.GetInt32("take_gunlic") == 1;
            TakeDriversLic = reader.GetInt32("take_driverlic") == 1;
            Category = CrimeCategoryModule.Instance.Get(reader.GetUInt32("crime_category_id"));
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class CrimePlayerReason
    {
        public uint Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Jailtime { get; }
        public int Costs { get; }
        public bool TakeGunLic { get; }
        public bool TakeDriversLic { get; }
        public CrimeCategory Category { get; }
        public string Notice { get; set; }

        public CrimePlayerReason(CrimeReason reason, string notice)
        {
            Id = reason.Id;
            Name = reason.Name;
            Description = reason.Description;
            Jailtime = reason.Jailtime;
            Costs = reason.Costs;
            TakeGunLic = reason.TakeGunLic;
            TakeDriversLic = reason.TakeGunLic;
            Category = reason.Category;
            Notice = notice;
        }
    }

    public class CrimePlayerHistory
    {
        public string Crimes { get; set; }
        public DateTime Date { get; set; }

        public CrimePlayerHistory(string crimes, DateTime date)
        {
            Crimes = crimes;
            Date = date;
        }
    }
}