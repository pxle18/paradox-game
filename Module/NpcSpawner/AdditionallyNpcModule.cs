using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.NpcSpawner
{
    public class AdditionallyNpcModule : SqlModule<AdditionallyNpcModule, AdditionallyNpc, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `additionally_npcs` WHERE deactivated = 0;";
        }
    }
}
