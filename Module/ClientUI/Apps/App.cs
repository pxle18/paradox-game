using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Players;

namespace VMP_CNR.Module.ClientUI.Apps
{
    public abstract class App<T> : Window<T>
    {
        private string Component { get; }

        public App(string name, string component) : base(name)
        {
            Component = component;
        }

        public override void Open(Player player, string json)
        {
            player.TriggerNewClient("openApp", Component, Name, json);
        }
    }
}