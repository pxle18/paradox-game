using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.PlayerName
{
    public class PlayerNameModule : SqlModule<PlayerNameModule, PlayerName, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `player`;";
        }
    }
}
