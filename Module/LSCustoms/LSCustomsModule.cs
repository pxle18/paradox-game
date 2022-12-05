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
    public class LSCustomsModule : SqlModule<LSCustomsModule, LSCustoms, uint>
    {
        ColShape LSCInteriorColshape;
        
        protected override string GetQuery()
        {
            return "SELECT * FROM `ls_customs`;";
        }

        protected override void OnLoaded()
        {
            MenuManager.Instance.AddBuilder(new Menu.LSCTuningSelectionBuilder());
            MenuManager.Instance.AddBuilder(new Menu.LSCVehicleSelectionMenuBuilder());
            NAPI.Task.Run(() => LSCInteriorColshape = NAPI.ColShape.CreateSphereColShape(new Vector3(-1267.12, -3013.25, -48.4902), 100.0f, 0));
        }

        // Colshape events
        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;
            if (LSCInteriorColshape == null) return false;
            if (colShapeState == ColShapeState.Enter)
            {
                if (LSCInteriorColshape == colShape)
                {
                    dbPlayer.Player.TriggerNewClient("loadlschangar");
                    return true;
                }
            }
            else if (colShapeState == ColShapeState.Exit)
            {
                if (LSCInteriorColshape == colShape)
                {
                    dbPlayer.Player.TriggerNewClient("unloadlschangar");
                    return true;
                }
            }

            return false;
        }
    }
}
