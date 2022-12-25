using System.Linq;

namespace VMP_CNR.Module.Gangwar
{
    public class GangwarGarageModule : SqlModule<GangwarGarageModule, GangwarGarage, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `gangwar_garages`;";
        }

        public GangwarGarage GetGarageByID(uint id)
        {
            return Instance.GetAll().FirstOrDefault(x => x.Value.Id == id).Value;
        }
    }
}