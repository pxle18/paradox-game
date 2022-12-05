using GTANetworkAPI;
using GTANetworkMethods;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VMP_CNR.Module.Players
{
    public static class RawPlayerWrapper
    {
        public static void TriggerNewClient(this GTANetworkAPI.Player player, string eventName, params object[] args)
        {
            NAPI.Task.Run(() =>
            {
                player.TriggerEvent(eventName, args);
            });
        }

        public static async void TriggerNewClientAsync(this GTANetworkAPI.Player player, string eventName, params object[] args)
        {
            await NAPI.Task.WaitForMainThread();
            player.TriggerEvent(eventName, args);
            await System.Threading.Tasks.Task.Delay(5);
        }

        public static void SetIntoVehicleSave(this GTANetworkAPI.Player player, GTANetworkAPI.Vehicle vehicle, int seat)
        {
            if(seat < 0)
            {
                Logging.Logger.Debug($"!WARNING! SLOT USED UNDER 0 (Slot {seat})!");
                return;
            }

            System.Threading.Tasks.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread();
                player.SetIntoVehicle(vehicle, seat);

                await System.Threading.Tasks.Task.Delay(2000);
                await AsyncEventTasks.AsyncEventTasks.PlayerEnterVehicleTask(player, vehicle, (sbyte)seat);
            });
        }
    }

    public class PlayerRageExtension
    {
        public string Name { get;  }
        public string Serial { get;  }
        public string SocialClubName { get;  }
        public ulong SocialClubId { get;  }
        public bool IsCeFEnabled { get;  }
        public bool IsMediaStreamEnabled { get; }
        public string Address { get; }
        public bool IsInVehicle { get; set; }

        private Vector3 Position { get; set; }
        private DateTime LastPositionUpdate { get; set; }
        private GTANetworkAPI.Player Player { get; set; }


        public PlayerRageExtension(GTANetworkAPI.Player source)
        {
            this.Name = source.Name;
            this.Serial = source.Serial;
            this.SocialClubName = source.SocialClubName;
            this.SocialClubId = source.SocialClubId;
            this.IsCeFEnabled = source.IsCeFenabled;
            this.IsMediaStreamEnabled = source.IsMediaStreamEnabled;
            this.Address = source.Address;
            this.IsInVehicle = source.IsInVehicle;
            this.Position = source.Position;
            this.LastPositionUpdate = DateTime.Now;
            this.Player = source;
        }

        public async Task<Vector3> GetPositionAsync()
        {
            if (LastPositionUpdate.AddSeconds(1) > DateTime.Now)
                return Position;

            await NAPI.Task.WaitForMainThread();
            Position            = Player.Position;
            LastPositionUpdate  = DateTime.Now;

            await System.Threading.Tasks.Task.Delay(5);
            return Position;
        }
    }

    public class PlayerWrapper
    {
        private GTANetworkAPI.Player player { get; set; }

        public PlayerWrapper(GTANetworkAPI.Player player)
        {
            this.player = player;
        }

        public void TriggerNewClient(string eventName, params object[] args)
        {
            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(0);
                player.TriggerEvent(eventName, args);
            });
        }

        public async Task<dynamic> getProperty(string PropertyName)
        {
            await NAPI.Task.WaitForMainThread(0);

            Type lType = typeof(GTANetworkAPI.Player);

            dynamic prop = lType.GetProperty(PropertyName);
            dynamic value = prop.GetValue(player);

            // just in case of fail
            if (value == null) return null;

            return value;
        }
    }
}
