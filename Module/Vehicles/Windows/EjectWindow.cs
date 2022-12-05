using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Commands;

namespace VMP_CNR.Module.Vehicles.Windows
{
    
    public class SeatClientItem
    {
        [JsonProperty(PropertyName = "seatid")] public uint SeatId { get; set; }
        [JsonProperty(PropertyName = "used")] public bool Used { get; set; }
    }

    public class EjectWindow : Window<Func<DbPlayer, SxVehicle, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "seats")] private List<SeatClientItem> seats { get; }
            public ShowEvent(DbPlayer dbPlayer, SxVehicle sxVehicle) : base(dbPlayer)
            {
                List<SeatClientItem> seatsList = new List<SeatClientItem>();

                for (int i = 0; i < sxVehicle.Data.Slots; i++)
                {
                    bool used = false;

                    Dictionary<int, DbPlayer> occu = sxVehicle.GetOccupants().GetLegacyDictionary();
                    if(occu.ContainsKey(i))
                    {
                        if(occu[i] != null && occu[i].IsValid() && !occu[i].IsInAdminDuty())
                        {
                            used = true;
                        }
                    }

                    seatsList.Add(new SeatClientItem() { SeatId = (uint)i, Used = used });
                }

                seats = seatsList;
            }
        }

        public EjectWindow() : base("EjectWindow")
        {
        }

        public override Func<DbPlayer, SxVehicle, bool> Show()
        {
            return (player, veh) => OnShow(new ShowEvent(player, veh));
        }

        [RemoteEvent]
        public void ejectSeat(Player player, int seatId, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (!dbPlayer.CanAccessMethod()) return;

                if (dbPlayer.Player.VehicleSeat != 0)
                {
                    dbPlayer.SendNewNotification(
                        "Sie muessen Fahrer des Fahrzeuges sein!");
                    return;
                }

                if (seatId == 0) return;

                var sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh == null || !sxVeh.IsValid() || sxVeh.GetOccupants() == null || sxVeh.GetOccupants().IsEmpty()) return;

                if (sxVeh.GetOccupants().IsSeatFree(seatId))
                {
                    dbPlayer.SendNewNotification("Auf diesem Platz sitzt keiner.");
                    return;
                }

                DbPlayer findPlayer = sxVeh.GetOccupants().GetPlayerFromSeat(seatId);
                if (findPlayer == null || !findPlayer.IsValid()
                    || !findPlayer.RageExtension.IsInVehicle
                    || findPlayer.Player.Vehicle != dbPlayer.Player.Vehicle)
                {
                    return;
                }


                if (sxVeh != null && sxVeh.GetOccupants().HasPlayerOccupant(findPlayer))
                    sxVeh.Occupants.RemovePlayer(findPlayer);


                findPlayer.StopAnimation();
                if (findPlayer.IsCuffed)
                {
                    findPlayer.ActionsBlocked = true;
                    findPlayer.SetCuffed(false);
                    findPlayer.WarpOutOfVehicle();

                    NAPI.Task.Run(() =>
                    {
                        findPlayer.Player.SetPosition(new Vector3(dbPlayer.Player.Vehicle.Position.X + 1,
                            dbPlayer.Player.Vehicle.Position.Y + 1, dbPlayer.Player.Vehicle.Position.Z));
                    });

                    // muss warten bis er cuffed setzt wegen animationbug
                    NAPI.Task.Run(() =>
                    {
                        findPlayer.SetTied(true);
                        findPlayer.ActionsBlocked = false;
                    }, 1000);
                }
                else if (findPlayer.IsTied)
                {
                    findPlayer.ActionsBlocked = true;
                    findPlayer.SetTied(false);
                    findPlayer.WarpOutOfVehicle();

                    NAPI.Task.Run(() =>
                    {
                        findPlayer.Player.SetPosition(new Vector3(dbPlayer.Player.Vehicle.Position.X + 1,
                        dbPlayer.Player.Vehicle.Position.Y + 1, dbPlayer.Player.Vehicle.Position.Z));
                    });

                    // muss warten bis er cuffed setzt wegen animationbug
                    NAPI.Task.Run(() =>
                    {
                        findPlayer.SetTied(true);
                        findPlayer.ActionsBlocked = false;
                    }, 1000);
                }
                else
                {
                    findPlayer.WarpOutOfVehicle();
                }


                dbPlayer.SendNewNotification("Sie haben jemanden aus dem Fahrzeug geschmissen!");
                findPlayer.SendNewNotification("Jemand hat Sie aus dem Fahrzeug geschmissen!");
            }));

        }
    }
}
