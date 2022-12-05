using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Business.Raffinery
{
    public class RaffineryAusbaustufeModule : SqlModule<RaffineryAusbaustufeModule, RaffineryAusbaustufe, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `business_raffinery_ausbaustufe`;";
        }
    }
}
