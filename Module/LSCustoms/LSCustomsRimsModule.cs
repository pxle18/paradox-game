using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.LSCustoms.Window;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.LSCustoms
{
    public class LSCustomsRimsModule : SqlModule<LSCustomsRimsModule, LSCustomsRims, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT lsc_rims.id,lsc_rims.rim_name, lsc_rims.tuning_id, lsc_rims.chrome, lsc_rims.rim_category_id FROM `lsc_rims` ORDER BY lsc_rims.rim_name, chrome ASC";
        }

        protected override void OnLoaded()
        {

        }

        [CommandPermission]
        [Command]
        public void Commandlscrims(Player client)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;
            bool check = false;


            LSCustoms l_LSC = LSCustomsModule.Instance.GetAll().Where(x => dbPlayer.Player.Position.DistanceTo(x.Value.position) <= 6.0f).FirstOrDefault().Value;
            if (l_LSC == null)
            {
                dbPlayer.SendNewNotification("Du bist nicht in einer Tuningwerkstatt!");
                return;
            }

            SxVehicle sxVeh = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
            if (sxVeh == null || !sxVeh.IsValid()) return;

            if (dbPlayer.TeamId == (int)teams.TEAM_LSC && dbPlayer.IsInDuty())
            {
                if (dbPlayer.TeamRank >= 10)
                {
                    check = true;
                }
                if (dbPlayer.TeamRank < 10 && sxVeh.InTuningProcess)
                {
                    check = true;
                }
            }


            if (check)
            {
                dbPlayer.SendNewNotification($"Fahrzeug: {sxVeh.Data.Model}");
                ComponentManager.Get<RimsWindow>().Show()(dbPlayer);
                return;
            }

            dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
            return;
        }

    }
}
