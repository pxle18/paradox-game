using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Camper;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> CampingSet(DbPlayer dbPlayer)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;

            await NAPI.Task.WaitForMainThread(0);

            // Check near...
            if (CampingModule.Instance.CampingPlaces.Where(cp => cp.Position.DistanceTo(dbPlayer.Player.Position) < 50.0f).Count() > 0)
            {
                dbPlayer.SendNewNotification("Ein Camp ist bereits zu nahe!");
                return false;
            }

            if (CampingModule.Instance.CampingPlaces.Where(cp => cp.PlayerId == dbPlayer.Id).Count() > 0)
            {
                dbPlayer.SendNewNotification("Sie haben bereits ein Camp!");
                return false;
            }

            if (dbPlayer.Player.Position.Z < 0 || dbPlayer.Player.Dimension != 0) return false;

            // Disable Build on Island
            if (dbPlayer.HasData("cayoPerico") || dbPlayer.HasData("cayoPerico2")) return false;

            Vector3 targetPos = dbPlayer.Player.Position.Add(new Vector3(-5.0f, 0, 0));

            dbPlayer.SetData("cp_building_step", 1);
            dbPlayer.SetData("cp_camppos", dbPlayer.Player.Position);
            dbPlayer.SetData("cp_markerpos", targetPos);
            dbPlayer.Player.TriggerNewClient("setCheckpoint", targetPos.X, targetPos.Y, targetPos.Z);
            dbPlayer.SendNewNotification("Bitte fang mit dem Aufbau an (Markierungen)");
            return true;
        }
    }
}