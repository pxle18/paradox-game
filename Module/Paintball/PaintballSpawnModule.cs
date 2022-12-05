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
    public class PaintballSpawnModule : SqlModule<PaintballSpawnModule, PaintballSpawn, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `paintball_spawns` WHERE active=1;";
        }

        public PaintballSpawn getSpawn(uint paintball_id)
        {
            var test = Instance.GetAll().Where(p => p.Value.paintball_id == paintball_id);
            return test.OrderBy(x => Guid.NewGuid()).FirstOrDefault().Value;
        }
    }

}
