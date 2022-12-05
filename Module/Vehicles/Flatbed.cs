using GTANetworkAPI;
using System;
using System.Numerics;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles.Data;
using Vector3 = GTANetworkAPI.Vector3;

namespace VMP_CNR.Module.Vehicles
{

    public class Flatbed : Script
    {

        [RemoteEvent]
        public void fbSetState(Player player, Vehicle fb, int state, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            if (fb == null||player==null) return;
            fb.SetSharedData("fbState",state);
        }
        [RemoteEvent]
        public void fbSyncStreamIn(Player player, Vehicle fb, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (fb == null || player == null || !dbPlayer.IsValid()) return;
            
        }

        [RemoteEvent]
        public void fbAttachRope(Player player, Vehicle fb, Vehicle veh, bool attach, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (fb == null || player == null || veh == null || !dbPlayer.IsValid()) return;
           /* NAPI.Task.Run(() =>
            {
                var l_NearPlayers = NAPI.Player.GetPlayersInRadiusOfPosition(50.0f, fb.Position);
                foreach (var l_Player in l_NearPlayers)
                {
                    if (player == l_Player) continue;
                    DbPlayer iPlayer = l_Player.GetPlayer();
                    if (!iPlayer.IsValid()) continue;
                    if (attach)
                    {
                        l_Player.TriggerNewClient("sAttachRope", fb, veh);
                    }
                    else
                    {
                        l_Player.TriggerNewClient("sAttachRope", fb, false);
                    }
                }
            });
            */
            if (attach)
            {
                fb.SetSharedData("fbAttachRope", veh.Id);
            }
            else
            {
                fb.SetSharedData("fbAttachRope", false);
            }
        }

        [RemoteEvent]
        public void fbWindRope(Player player, Vehicle fb, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            if (fb == null || player == null) return;
            player.TriggerNewClient("windRope", fb);
        }

        [RemoteEvent]
        public void fbAttachVehicle(Player player, Player toplayer, Vehicle fb, Vehicle veh, bool attach, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            DbPlayer toPlayer = toplayer.GetPlayer();
            if (fb == null || player == null || veh == null || !dbPlayer.IsValid()|| !toPlayer.IsValid()) return;

            /*var l_NearPlayers = NAPI.Player.GetPlayersInRadiusOfPosition(250.0f, fb.Position);
            foreach (var l_Player in l_NearPlayers)
            {
                if (player == l_Player) continue;
                DbPlayer iPlayer = l_Player.GetPlayer();
                if (!iPlayer.IsValid()) continue;

                if (attach)
                {
                    l_Player.TriggerNewClient("sAttachToBed", fb, veh);
                }
                else
                {
                    l_Player.TriggerNewClient("sAttachToBed", fb, false);
                }

            }
            */

            if (attach)
            {
                toPlayer.Player.TriggerNewClient("sAttachToBed", fb, veh);
                fb.SetSharedData("fbAttachVehicle", veh.Id);
            }
            else
            {
                toPlayer.Player.TriggerNewClient("sAttachToBed", fb, false);
                fb.SetSharedData("fbAttachVehicle", false);
            }
        }

        [RemoteEvent]
        public void fbSyncPosition(Player player, Vehicle veh, string pos,string rot, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            if (veh == null || player == null || pos == null || rot == null) return;
            var pos_j = NAPI.Util.FromJson(pos);
            var rot_j = NAPI.Util.FromJson(rot);
            veh.Position = new Vector3((float) pos_j.x, (float)pos_j.y, (float)pos_j.z);
            veh.Rotation = new Vector3((float)rot_j.x, (float)rot_j.y, (float)rot_j.z);
        }




    }

}