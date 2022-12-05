using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.LSCustoms
{
    public class LSCustomsRimsCategoryModule : SqlModule<LSCustomsRimsCategoryModule, LSCustomsRimsCategory, uint>
    {
        
        protected override string GetQuery()
        {
            return "SELECT * FROM lsc_rims_category ORDER BY category_name ASC";
        }

        protected override void OnLoaded()
        {
        }

    }
}
