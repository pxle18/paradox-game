using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Staatsgefaengnis.Menu;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Staatsgefaengnis
{
    public enum SGJobs
    {
        WASHING = 1,
        WORKBENCH = 2,
    }


    public class StaatsgefaengnisModule : Module<StaatsgefaengnisModule>
    {
        public static Vector3 klingelPosition = new Vector3(1842.2, 2583.09, 45.891);
        public static Vector3 sgBellPosition = new Vector3(1690.99, 2533.63, 61.3783);

        public static Vector3 JobMenuPosition = new Vector3(1761.11, 2574.75, 45.9177);

        public Dictionary<DbPlayer, SGJobs> SGJobPlayers = new Dictionary<DbPlayer, SGJobs>();

        public DateTime lastKlingelUsed = DateTime.Now;

        public List<uint> removeSGItemsOnNormalUnjail = new List<uint>();

        public static int SGWashingJobMax = 5;
        public static int SGWorkbenchJobMax = 5;

        public static Vector3 sgEmergencyPosition = new Vector3(1775.75, 2552.02, 45.565);
        public DateTime lastsgEmergencyUsed = DateTime.Now;
        protected override bool OnLoad()
        {
            MenuManager.Instance.AddBuilder(new SGJobChooseMenu());

            lastKlingelUsed = DateTime.Now;
            lastsgEmergencyUsed = DateTime.Now;

            removeSGItemsOnNormalUnjail = new List<uint>();
            removeSGItemsOnNormalUnjail.Add(LaundryModule.LaundryBagItemId);  // wäschekorb
            removeSGItemsOnNormalUnjail.Add(LaundryModule.LaundryKeyItemId); // schlüssel

            SGJobPlayers = new Dictionary<DbPlayer, SGJobs>();

            PlayerNotifications.Instance.Add(klingelPosition,
                "Staatsgefängnis",
                "Benutze 'E' um einen Wärter zu verständigen!");

            PlayerNotifications.Instance.Add(sgEmergencyPosition,
                "Staatsgefängnis Notruf",
                "Benutze 'E' um einen Notruf zu senden!");

            NAPI.Marker.CreateMarker(25, (JobMenuPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, 0);

            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            if(key == Key.E && !dbPlayer.RageExtension.IsInVehicle)
            {
                if (dbPlayer.Player.Position.DistanceTo(sgEmergencyPosition) < 2.0f)
                {
                    // Klingel
                    if (lastsgEmergencyUsed.AddMinutes(15) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Es wurde vor kurzem bereits ein Notruf gesendet, bitte warten Sie einen Augenblick!");
                        return false;
                    }
                    else
                    {
                        lastKlingelUsed = DateTime.Now;

                        var requestSuccess = false;

                        if (dbPlayer.HasData("service") && dbPlayer.GetData("service") > 0)
                        {
                            dbPlayer.SendNewNotification("Sie haben bereits einen Notruf/Service offen!");
                            return false;
                        }

                        var telnr = dbPlayer.handy[0].ToString();

                        string message = $"SG-Notrufsystem: Notfall im Staatsgefängnis von {dbPlayer.GetName()}";
                        
                        if (TeamModule.Instance[(int)teams.TEAM_ARMY].GetTeamMembers().Where(c => c.Duty).Count()
                            - TeamModule.Instance[(int)teams.TEAM_ARMY].GetTeamMembers().Where(c => c.IsInAdminDuty() || c.IsInGuideDuty() || c.IsInGameDesignDuty()).Count()
                            > 0)
                        {
                            TeamModule.Instance[(int)teams.TEAM_ARMY].SendNotification($"Ein Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                            requestSuccess = true;
                        }

                        if (requestSuccess)
                        {
                            VMP_CNR.Module.Service.Service service = new VMP_CNR.Module.Service.Service(dbPlayer.Player.Position, message, (uint)teams.TEAM_ARMY, dbPlayer, "", telnr);
                            bool status = VMP_CNR.Module.Service.ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_ARMY, service);

                            dbPlayer.SetData("service", 13);

                            if (status)
                            {
                                dbPlayer.SendNewNotification("Sie haben die US Army informiert!");
                            }
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("Derzeit ist die US Army nicht verfuegbar!");
                        }

                        return true;
                    }
                }
                if (dbPlayer.Player.Position.DistanceTo(klingelPosition) < 1.0f)
                {
                    // Klingel
                    if(lastKlingelUsed.AddMinutes(5) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Die Klingel wurde vor kurzem genutzt, bitte warten Sie einen Augenblick!");
                        return false;
                    }
                    else
                    {
                        lastKlingelUsed = DateTime.Now;

                        dbPlayer.SendNewNotification("Sie haben die Klingel betätigt, bitte warten Sie hier!");

                        foreach (DbPlayer dbPlayer1 in TeamModule.Instance.Get((uint)teams.TEAM_ARMY).Members.Values.Where(a => a != null && a.IsValid() && a.IsInDuty()).ToList())
                        {
                            if(dbPlayer1.Player.Position.DistanceTo(sgBellPosition) < 200.0f)
                            {
                                dbPlayer1.SendNewNotification($"1337Allahuakbar$sgbell", duration: 5000);
                                dbPlayer1.SendNewNotification("Die Klingel am Eingangsbereich des Staatsgefängnis wurde betätigt!");
                            }
                        }
                        return true;
                    }
                }

                if (dbPlayer.Player.Position.DistanceTo(JobMenuPosition) < 1.0f)
                {
                    if(dbPlayer.JailTime[0] > 5)
                    {
                        Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.SGJobChooseMenu, dbPlayer).Show(dbPlayer);
                        return false;
                    }
                    
                }
            }

            return false;
        }

        public int GetJobAmounts(SGJobs sGJob)
        {
            return SGJobPlayers.ToList().Where(p => p.Key != null && p.Key.IsValid() && p.Value == sGJob).Count();
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (SGJobPlayers.ContainsKey(dbPlayer)) SGJobPlayers.Remove(dbPlayer);
        }
    }

    public static class SgPlayerExtension
    {
        public static void RemoveItemsOnUnjail(this DbPlayer dbPlayer)
        {
            if(dbPlayer != null && dbPlayer.IsValid())
            {
                foreach(uint itemId in StaatsgefaengnisModule.Instance.removeSGItemsOnNormalUnjail)
                {
                    if(dbPlayer.Container.GetItemAmount(itemId) > 0)
                    {
                        dbPlayer.Container.RemoveItemAll(itemId);
                    }
                }

                if(dbPlayer.HasPlayerSGJob())
                {
                    StaatsgefaengnisModule.Instance.SGJobPlayers.Remove(dbPlayer);
                }
            }
        }

        public static bool HasPlayerSGJob(this DbPlayer dbPlayer)
        {
            return StaatsgefaengnisModule.Instance.SGJobPlayers.ContainsKey(dbPlayer);
        }

        public static bool IsSGJobActive(this DbPlayer dbPlayer, SGJobs type)
        {
            return StaatsgefaengnisModule.Instance.SGJobPlayers.ContainsKey(dbPlayer) && StaatsgefaengnisModule.Instance.SGJobPlayers[dbPlayer] == type;
        }
    }
}