
using GTANetworkAPI;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerFreeze
    {
        public static void Freeze(this DbPlayer dbPlayer, bool freeze)
        {
            dbPlayer.Player.TriggerNewClient("freezePlayer", freeze);
        }

        public static void Freeze(this DbPlayer dbPlayer, bool freeze, bool shapefreeze = false,
            bool interrupt = false)
        {
            if (!freeze)
            {
                if (dbPlayer.deadtime[0] > 0 || dbPlayer.isInjured() ||
                    dbPlayer.RageExtension.IsInVehicle
                    || dbPlayer.IsCuffed || dbPlayer.IsTied)
                {
                    return;
                }
            }
            dbPlayer.Player.TriggerNewClient("freezePlayer", freeze);
            //dbPlayer.Player.FreezePosition = freeze;
        }
        
        public static void Freeze(this Player player, bool freeze, bool shapefreeze = false,
            bool interrupt = false)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer != null)
            {
                dbPlayer.Freeze(freeze, shapefreeze, interrupt);
            }
            else
            {
                //player.TriggerNewClient("freezePlayer", freeze);
                //player.FreezePosition = freeze;
            }
        }
    }
}