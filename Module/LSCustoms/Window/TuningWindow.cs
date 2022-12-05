using System;
using VMP_CNR.Module.Players.Db;
using Newtonsoft.Json;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Vehicles.Garages;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Windows;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Tuning;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.LSCustoms.Window
{

    public class TuningItems
    {
        [JsonProperty(PropertyName = "id")] public int Id { get; set; }
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }
        [JsonProperty(PropertyName = "index")] public int Index { get; set; }
        [JsonProperty(PropertyName = "maxindex")] public int MaxIndex { get; set; }
    }


    public class TuningWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            public ShowEvent(DbPlayer dbPlayer) : base(dbPlayer)
            {

            }
        }

        public TuningWindow() : base("Tuning")
        {
        }

        public override Func<DbPlayer, bool> Show()
        {
            return (player) => OnShow(new ShowEvent(player));
        }

        [RemoteEvent]
        public void requestResetTuningMod(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            try
            {
                var dbPlayer = client.GetPlayer();
                if (!dbPlayer.IsValid()) return;
                if (dbPlayer.TeamId != (int)teams.TEAM_LSC) return;
                if (!dbPlayer.HasData("tuneVeh")) return;
                SxVehicle veh = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.GetData("tuneVeh"));
                if (veh == null) return;

                //DO NOTTIN
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }


        [RemoteEvent]
        public void requestSaveTuningMod(Player client, int type, int mod, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            try
            {
                var dbPlayer = client.GetPlayer();
                if (!dbPlayer.IsValid()) return;
                if (dbPlayer.TeamId != (int)teams.TEAM_LSC) return;
                if (!dbPlayer.HasData("tuneVeh")) return;
                SxVehicle veh = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.GetData("tuneVeh"));
                if (veh == null) return;

                veh.AddSavedMod(type, mod, false);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void requestTuningMod(Player client, int type,int mod, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            try
            {
                var dbPlayer = client.GetPlayer();
                if (!dbPlayer.IsValid()) return;
                if (dbPlayer.TeamId != (int)teams.TEAM_LSC) return;
                if (!dbPlayer.HasData("tuneVeh")) return;
                SxVehicle veh = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.GetData("tuneVeh"));
                if (veh == null) return;

                veh.SetMod(type, mod);

            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void requestTuningModlist(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            try
            {
                var dbPlayer = client.GetPlayer();
                if (!dbPlayer.IsValid()) return;
                if (dbPlayer.TeamId != (int)teams.TEAM_LSC) return;
                if (!dbPlayer.HasData("tuneVeh")) return;
                SxVehicle veh = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.GetData("tuneVeh"));
                if (veh == null) return;

                var l_Tunings = Helper.Helper.m_Mods;
                List<TuningItems> tuninglist = new List<TuningItems>();

                foreach (var l_Tuning in l_Tunings)
                {
                    if (l_Tuning.Value.ID >= 90) continue;
                    tuninglist.Add(new TuningItems() { 
                        Id = (int)l_Tuning.Value.ID, 
                        Name = l_Tuning.Value.Name, 
                        Index = veh.Mods.ContainsKey((int)l_Tuning.Value.ID) ? veh.Mods[(int)l_Tuning.Value.ID] : -1,
                        MaxIndex =((int)l_Tuning.Value.MaxIndex > 0) ? (int)l_Tuning.Value.MaxIndex : 100  });
                }


                dbPlayer.Player.TriggerNewClient("componentServerEvent", "Tuning", "responseTuningModlist", NAPI.Util.ToJson(tuninglist),veh.entity.Id);

            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
    }

}
