using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Laboratories;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Gaspberry(DbPlayer dbPlayer, ItemModel itemModel)
        {
            if (dbPlayer.DimensionType[0] == DimensionTypes.Heroinlaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryLaptopPosition) > 2.0f) return false;
                if (!dbPlayer.IsAGangster() && !dbPlayer.IsACop()) return false;
                await HeroinlaboratoryModule.Instance.HackHeroinlaboratory(dbPlayer);
            }

            if (dbPlayer.DimensionType[0] == DimensionTypes.Methlaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryLaptopPosition) > 2.0f) return false;
                if (!dbPlayer.IsAGangster() && !dbPlayer.IsACop()) return false;
                await MethlaboratoryModule.Instance.HackMethlaboratory(dbPlayer);
            }

            if (dbPlayer.DimensionType[0] == DimensionTypes.Weaponlaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.WeaponlaboratoryComputerPosition) > 2.0f) return false;
                if (!dbPlayer.Team.IsWeaponTeam() && !dbPlayer.IsACop()) return false;
                await WeaponlaboratoryModule.Instance.HackWeaponlaboratory(dbPlayer);
            }

            if (dbPlayer.DimensionType[0] == DimensionTypes.Cannabislaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryComputerPosition) > 2.0f) return false;
                if (!dbPlayer.IsAGangster() && !dbPlayer.IsACop()) return false;
                await CannabislaboratoryModule.Instance.HackCannabislaboratory(dbPlayer);
            }
            return true;
        }
    }
}