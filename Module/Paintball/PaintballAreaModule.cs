using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Paintball
{
    public class PaintballAreaModule : SqlModule<PaintballAreaModule, PaintballArea, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `paintball` WHERE active=1;";
        }
    }
}
