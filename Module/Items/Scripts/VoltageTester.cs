using System;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Laboratories;
using VMP_CNR.Module.Meth;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool VoltageTest(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle || dbPlayer.TeamId != (int)teams.TEAM_FIB) return false;

            HousesVoltage housesVoltage = HousesVoltageModule.Instance.GetAll().Values.ToList().Where(hv => hv.Position.DistanceTo(dbPlayer.Player.Position) < 5.0f).FirstOrDefault();

            if (housesVoltage == null) return false;
            
            Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.VoltageMenu, dbPlayer).Show(dbPlayer);
            return true;
        }
        
    }
}