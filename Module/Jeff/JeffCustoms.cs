using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using GTANetworkAPI;
using GTANetworkMethods;

namespace GTANetworkAPI
{
    // todo import gtan vehiclehashes
    public enum VehicleNewHash : uint
    {
        Test = 11111111,
    }
}

namespace VMP_CNR.Module.Jeff
{
    /*[SecurityCritical]
    internal class RageCustom
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport("bridge2", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnablePlayerVoiceTo(ushort id, ushort targetId);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bridge2", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DisablePlayerVoiceTo(ushort id, ushort targetId);
    }

    public static class NAPIExtension
    {
        public static void EnablePlayerVoiceTo(Player player, Client target)
        {
            if (!((GTANetworkAPI.Entity)target != (GTANetworkAPI.Entity)null))
                return;
            NetHandle handle = player.Handle;
            int num1 = (int)handle.Value;
            handle = target.Handle;
            int num2 = (int)handle.Value;
            RageCustom.EnablePlayerVoiceTo((ushort)num1, (ushort)num2);
        }

        public static void DisablePlayerVoiceTo(Player player, Client target)
        {
            if (!((GTANetworkAPI.Entity)target != (GTANetworkAPI.Entity)null))
                return;
            NetHandle handle = player.Handle;
            int num1 = (int)handle.Value;
            handle = target.Handle;
            int num2 = (int)handle.Value;
            RageCustom.DisablePlayerVoiceTo((ushort)num1, (ushort)num2);
        }
    }*/
}
