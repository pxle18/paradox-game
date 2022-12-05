using GTANetworkAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Vehicles
{
    public class VehicleOccupants
    {
        private ConcurrentDictionary<int, DbPlayer> Occupants   = new ConcurrentDictionary<int, DbPlayer>();
        private SxVehicle Vehicle                               = null;

        public VehicleOccupants(SxVehicle pVehicle)
        {
            Vehicle = pVehicle;
        }

        private bool ValidVehicle()
        {
            return (Vehicle != null && Vehicle.IsValid());
        }

        public bool HasPlayerOccupant(DbPlayer pDbPlayer)
        {
            if (pDbPlayer == null || !pDbPlayer.IsValid() || !ValidVehicle())
                return false;

            foreach (var pair in Occupants)
            {
                if (pair.Value == pDbPlayer)
                    return true;
            }

            return false;
        }

        public int GetPlayerSeat(DbPlayer pDbPlayer)
        {
            if (!ValidVehicle() || pDbPlayer == null || !pDbPlayer.IsValid() || !HasPlayerOccupant(pDbPlayer))
                return -1;

            foreach (var lPair in Occupants)
            {
                if (lPair.Value != null && lPair.Value.IsValid() && lPair.Value == pDbPlayer)
                    return lPair.Key;
            }

            return -1;
        }

        public bool AddPlayer(DbPlayer pDbPlayer, int pSeat)
        {
            if (!ValidVehicle() || pDbPlayer == null || !pDbPlayer.IsValid())
                return false;

            int lPlayerSeat = GetPlayerSeat(pDbPlayer);
            if (lPlayerSeat != -1)
                Occupants.TryRemove(lPlayerSeat, out DbPlayer lNothing);

            if (Occupants.ContainsKey(pSeat))
                Occupants.TryRemove(pSeat, out DbPlayer lNothing);

            return Occupants.TryAdd(pSeat, pDbPlayer);
        }

        public bool RemovePlayer(DbPlayer pDbPlayer)
        {
            if (!ValidVehicle() || pDbPlayer == null || !pDbPlayer.IsValid())
                return false;

            int lPlayerSeat = GetPlayerSeat(pDbPlayer);
            if (lPlayerSeat == -1)
            {
                if (!Occupants.ContainsKey(lPlayerSeat) || Occupants[lPlayerSeat] != pDbPlayer)
                    return false;
            }

            return Occupants.TryRemove(lPlayerSeat, out DbPlayer lNothing);
        }

        public bool HasFreeSeat(DbPlayer pDbPlayer)
        {
            if (!ValidVehicle() || pDbPlayer == null || !pDbPlayer.IsValid())
                return false;

            return (Occupants.Count < Vehicle.Data.Slots);
        }

        public bool IsSeatFree(int pSeat)
        {
            return !Occupants.ContainsKey(pSeat);
        }

        public int GetUsedSeats()
        {
            return Occupants.Count;
        }

        public void TriggerAntiCheatForEveryOccupant()
        {
            foreach (var lPair in Occupants)
            {
                if (lPair.Value == null || !lPair.Value.IsValid())
                    continue;

                Players.Players.Instance.SendMessageToAuthorizedUsers("anticheat", $"ANTICHEAT (Vehicle REPAIR Hack) {lPair.Value.GetName()}");
            }
        }

        public DbPlayer GetDriver()
        {
            if (!Occupants.ContainsKey(0))
                return null;

            return Occupants[0];
        }

        public DbPlayer GetPlayerFromSeat(int pSeat)
        {
            if (!ValidVehicle() || !Occupants.ContainsKey(pSeat))
                return null;

            return Occupants[pSeat];
        }

        public bool IsEmpty()
        {
            return Occupants.IsEmpty;
        }

        public Dictionary<int, DbPlayer> GetLegacyDictionary()
        {
            Dictionary<int, DbPlayer> lDictionary = new Dictionary<int, DbPlayer>();
            foreach (var lPair in Occupants)
            {
                lDictionary.Add(lPair.Key, lPair.Value);
            }

            return lDictionary;
        }

        public void TriggerEventForOccupants(string eventName, params object[] args)
        {
            NAPI.Task.Run(() =>
            {
                foreach (var lPair in Occupants)
                {
                    if (lPair.Value == null || !lPair.Value.IsValid())
                        continue;

                    lPair.Value.Player.TriggerEvent(eventName, args);
                }
            });
        }
    }
}
