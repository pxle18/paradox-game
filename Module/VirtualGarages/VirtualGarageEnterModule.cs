using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.VirtualGarages
{
    public class VirtualGarageEnterModule : SqlModule<VirtualGarageEnterModule, VirtualGarageEnter, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(VirtualGarageModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `virtual_garages_enters`;";
        }
    }
}
