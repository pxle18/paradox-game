using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Events.Jahrmarkt.Scooter;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Scooter(DbPlayer dbPlayer, ItemModel itemModel)
        {
            if (!dbPlayer.RageExtension.IsInVehicle) return false;

            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();

            if (sxVehicle == null || !sxVehicle.IsValid()) return false;

            Scooter scooter = ScooterModule.Instance.Scooters.Values.ToList().Where(s => s.sxVehicle == sxVehicle).FirstOrDefault();

            if(scooter != null)
            {
                if(scooter.CoinInserted)
                {
                    dbPlayer.SendNewNotification("Dieses Fahrzeug hat bereits einen Autoscooter Coin!");
                    return false;
                }
                else
                {
                    dbPlayer.SendNewNotification("Coin eingeschmissen, viel Spaß! Start ist ab dem nächsten Startsignal!");
                    scooter.CoinInserted = true;
                }
            }
            return true;
        }
    }
}