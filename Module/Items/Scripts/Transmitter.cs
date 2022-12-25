using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using Task = System.Threading.Tasks.Task;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async System.Threading.Tasks.Task<bool> Transmitter(DbPlayer dbPlayer, Item item)
        {
            await GTANetworkAPI.NAPI.Task.WaitForMainThread(1);

            GTANetworkAPI.NAPI.Task.Run(() => ComponentManager.Get<TextInputBoxWindow>().Show()(
                dbPlayer, new TextInputBoxWindowObject() { Title = "Transmitter", Callback = "SendTransmitter", Message = "Gebe eine Nachricht an, die Global als LSPD gesendet wird." }));

            return false;
        }
    }
}