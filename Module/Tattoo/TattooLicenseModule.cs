using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Assets.Tattoo;

namespace VMP_CNR.Module.Tattoo
{
    public class TattooLicenseModule : SqlModule<TattooLicenseModule, TattooLicense, uint>
    {
        public class ClientLicense
        {
            public uint Id { get; }
            
            public int Price { get; }
            
            public string Name { get; }

            public ClientLicense(uint id, int price, string name)
            {
                Id = id;
                Price = price;
                Name = name;
            }
        }
        
        protected override string GetQuery()
        {
            return "SELECT " +
                   "ts.id AS tattooLicenseId, " +
                   "ts.price AS tattooLicensePrice, " +
                   "ts.assets_tattoo_id, " +
                   "at.* " +
                   "FROM tattoo_licenses ts " +
                   "RIGHT JOIN assets_tattoo at ON at.id = ts.assets_tattoo_id;";
        }

        public List<TattooLicense> GetLicensesForZone(int zoneId)
        {
            return GetAll().Values
                .Where(l => l.Tattoo != null && l.Tattoo.ZoneId == zoneId)
                .ToList();
        }
    }
}