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

namespace VMP_CNR.Module.LSCustoms.Window
{
    public class RimsItems
    {
        [JsonProperty(PropertyName = "id")] public int Id { get; set; }
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }
    }

    public class RimsWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            public ShowEvent(DbPlayer dbPlayer) : base(dbPlayer)
            {

            }
        }

        public RimsWindow() : base("Rims")
        {
        }

        public override Func<DbPlayer, bool> Show()
        {
            return (player) => OnShow(new ShowEvent(player));
        }

        [RemoteEvent]
        public void requestTuningCategoryRims(Player client)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;
            if (dbPlayer.TeamId != (int)TeamTypes.TEAM_LSC) return;

            List<RimsItems> rimsList = new List<RimsItems>();
            foreach (var rim in LSCustomsRimsCategoryModule.Instance.GetAll())
            {
                rimsList.Add(new RimsItems() { Id = (int)rim.Value.category_id, Name = rim.Value.category_name });
            }
            dbPlayer.Player.TriggerNewClient("componentServerEvent", "Rims", "responseTuningCategoryRims", NAPI.Util.ToJson(rimsList));
        }

        [RemoteEvent]
        public void requestTuningRims(Player client, int catid)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;
            if (dbPlayer.TeamId != (int)TeamTypes.TEAM_LSC) return;

            List<RimsItems> rimsList = new List<RimsItems>();
            foreach (var rim in LSCustomsRimsModule.Instance.GetAll().Where(x => x.Value.category_id==catid))
            {
                rimsList.Add(new RimsItems() { Id = rim.Value.tuning_id, Name = (rim.Value.chrome) ? $"{rim.Value.rim_name} - Chrome" : rim.Value.rim_name });
            }
            dbPlayer.Player.TriggerNewClient("componentServerEvent", "Rims", "responseTuningRims", NAPI.Util.ToJson(rimsList));
        }

        [RemoteEvent]
        public void requestResetRims(Player client)
        {
            try
            {
                var dbPlayer = client.GetPlayer();
                if (!dbPlayer.IsValid()) return;
                if (dbPlayer.TeamId != (int)TeamTypes.TEAM_LSC) return;

                SxVehicle sxVeh = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
                if (sxVeh == null || !sxVeh.IsValid()) return;

                if (sxVeh.Mods.ContainsKey(1337)) { sxVeh.SetMod(1337, sxVeh.Mods[1337]); } else { sxVeh.SetMod(1337, -1); }

                sxVeh.SetMod(23, (sxVeh.Mods.ContainsKey(23)) ? sxVeh.Mods[23] : 0);

            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void requestSetTuningRim(Player client, int catid, int tuning_id, bool save)
        {
            try
            {
                var dbPlayer = client.GetPlayer();
                if (!dbPlayer.IsValid()) return;
                if (dbPlayer.TeamId != (int)TeamTypes.TEAM_LSC) return;

                var wheeltype = LSCustomsRimsCategoryModule.Instance.GetAll().Where(x => x.Value.category_id == catid).FirstOrDefault();
                var rim = LSCustomsRimsModule.Instance.GetAll().Where(x => x.Value.tuning_id == tuning_id && x.Value.category_id == wheeltype.Value.category_id).FirstOrDefault();
                SxVehicle sxVeh = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
                if (sxVeh == null||!sxVeh.IsValid()) return;


                if (save)
                {
                    sxVeh.AddMod(1337, wheeltype.Value.category_id);
                    sxVeh.AddMod(23, rim.Value.tuning_id);
                    sxVeh.SaveMods();
                }

                    sxVeh.SetMod(1337, wheeltype.Value.category_id);
                    sxVeh.SetMod(23, rim.Value.tuning_id);

                


                dbPlayer.SendNewNotification($"Felge {wheeltype.Value.category_name} - {rim.Value.rim_name}{(rim.Value.chrome ? "- Chrome" : "")} {(save ? " angebracht" : "")}");
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
    }

}
