using System.Collections.Generic;

namespace VMP_CNR.Module.Houses
{
    public sealed class InteriorModule : SqlModule<InteriorModule, Interior, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `interiors` ORDER BY id;";
        }
    }
}