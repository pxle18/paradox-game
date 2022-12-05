using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.Buffs
{
    public class CustomDrugModule : Module<CustomDrugModule>
    {
        public string NewDrugEffect = "DrugsMichaelAliensFight";

        public void SetCustomDrugEffect(DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerNewClient("startScreenEffect", NewDrugEffect, 60000, true);
        }
        
        public void RemoveEffect(DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerNewClient("stopScreenEffect", NewDrugEffect);
        }
        
    }
}
