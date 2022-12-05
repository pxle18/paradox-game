using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Crime
{
    public sealed class CrimeModule : Module<CrimeModule>
    {
        public List<Vector3> ComputerPositions = new List<Vector3>();
     
        public override bool Load(bool reload = false)
        {
            ComputerPositions = new List<Vector3>();
            ComputerPositions.Add(new Vector3(440.971, -978.654, 31.690));
            ComputerPositions.Add(new Vector3(461.575, -988.992, 24.9149));

            MenuManager.Instance.AddBuilder(new CrimeJailMenuBuilder());
            return base.Load(reload);
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.J || !dbPlayer.RageExtension.IsInVehicle) return false;
            SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (!sxVeh.IsValid() || sxVeh.teamid != dbPlayer.TeamId || dbPlayer.TeamId != (int)teams.TEAM_POLICE) return false;
            try
            {
                if (sxVeh.Data.ClassificationId == 3) return false; // Disable Boote

                if ((dbPlayer.TeamRank < 6 && sxVeh.GetSpeed() > 0) || (dbPlayer.Player.VehicleSeat == 0 && dbPlayer.TeamRank < 6))
                {
                    dbPlayer.SendNewNotification("Messungen während der Fahrt sind erst ab rang 6 erlaubt.");
                    return false;
                }

                if(dbPlayer.HasData("lastblitzed"))
                {
                    if(dbPlayer.GetData("lastblitzed").AddSeconds(15) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Du kannst nur alle 15 Sekunden blitzen!");
                        return false;
                    }
                }

                //Get Closest Player kleiner als 6 und speed größer als 0 messungen wärend der fahrt 
                List<DbPlayer> targetPlayers = Players.Players.Instance.GetPlayersListInRange(dbPlayer.Player.Position, 40);

                bool targetFound = false;
                bool msgsend = false;
                foreach (DbPlayer tpl in targetPlayers)
                {
                    if (!tpl.RageExtension.IsInVehicle) continue;
                    if (dbPlayer.Player.Vehicle == tpl.Player.Vehicle || tpl.Player.VehicleSeat != 0) continue;
                    SxVehicle targetVeh = tpl.Player.Vehicle.GetVehicle();
                    if (!targetVeh.IsValid() || targetVeh.GetSpeed() <= 0) continue;
                    if (!msgsend)
                    {
                        dbPlayer.SendNewNotification("Folgende Fahrzeuge wurden geblitzt:");
                        msgsend = true;
                    }

                    tpl.Player.TriggerNewClient("startScreenEffect", "MP_SmugglerCheckpoint", 3000, false);
                    tpl.Player.TriggerNewClient("startsoundplay", "Camera_Shoot", "Phone_Soundset_Franklin");

                    dbPlayer.SendNewNotification($"{targetVeh.GetSpeed()} KM/H - [{targetVeh.GetName()}]");
                    targetFound = true;
                }
                if(targetFound)
                {
                    dbPlayer.SetData("lastblitzed", DateTime.Now);
                }
                return true;
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
            return false;
        }

        public bool IsInRangeOfPDComputers(Vector3 pos)
        {
            return (ComputerPositions.FindAll(p => p.DistanceTo(pos) < 3.0f).Count() > 0);
        }
        
        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            if(Instance.TicketHighIsJail(dbPlayer.Crimes, dbPlayer.EconomyIndex))
            {
                // Gebe Wanted wenn Ticketkosten größer als 30k
                dbPlayer.AddCrime("Justiz", CrimeReasonModule.Instance.Get(208), "");
                return;
            }
        }

        public int CalcJailTime(IEnumerable<CrimePlayerReason> wantedList)
        {
            return wantedList.ToList().Sum(wanted => wanted.Jailtime);
        }

        public bool TicketHighIsJail(IEnumerable<CrimePlayerReason> wantedList, EconomyIndex economyIndex)
        {
            return (wantedList.ToList().Sum(wanted => wanted.Jailtime) == 0 && CalcJailCosts(wantedList, economyIndex) > 60000);
        }

        public int GetCrimeCosts(CrimePlayerReason reason, EconomyIndex economyIndex)
        {
            return CalcJailCosts(new List<CrimePlayerReason> { reason }, economyIndex);
        }

        public int CalcJailCosts(IEnumerable<CrimePlayerReason> wantedList, EconomyIndex economyIndex)
        {
            if (wantedList == null) return 0;

            int costs = wantedList.ToList().Sum(wanted => (wanted != null) ? wanted.Costs : 0);

            switch(economyIndex)
            {
                case EconomyIndex.Superrich:
                    costs -= (costs / 10); // -10%
                    break;
                case EconomyIndex.Rich:
                    costs -= (costs / 5); // -20%
                    break;
                case EconomyIndex.Mid:
                    costs -= (costs / 4); // -25%
                    break;
                case EconomyIndex.Low:
                    costs -= (costs / 2); // -50%
                    break;
            }

            return costs;
        }

        public int CalcWantedStars(IEnumerable<CrimePlayerReason> wantedList)
        {
            return CalcJailTime(wantedList) / 10;
        }
    }
}