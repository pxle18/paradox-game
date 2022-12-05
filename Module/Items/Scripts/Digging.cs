using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Digging(DbPlayer dbPlayer)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            return false;

            //ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Gold-Digger", Callback = "DigTo", Message = "Gib einen Namen zudem du drich graben willst ein" });
            //return true;
        }
    }
}