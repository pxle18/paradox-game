using MySql.Data.MySqlClient;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Weapons.Component;

namespace VMP_CNR.Module.Players.Duty
{
    public class PlayerDutyModule : Module<PlayerDutyModule>
    {
        private const string DutyEventName = "setDuty";
        
        //Todo: this was previously in onSpawn check if its fine to do just one time as well
        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            // handle player duty states
            if (!dbPlayer.Duty) return;
            if (dbPlayer.Team.HasDuty && dbPlayer.Team != null)
            {
                dbPlayer.Player.TriggerNewClient(DutyEventName, true);
            }
            else
            {
                dbPlayer.Duty = false;
                dbPlayer.RemoveWeapons();
                dbPlayer.ResetAllWeaponComponents();
            }
        }
    }
}