using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.FIB.Menu;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.InteriorVehicles;


namespace VMP_CNR.Module.FIB
{
    [Flags]
    public enum FindFlags : int
    {
        None            = 0, // Keiner Sonder-Ortungsrechte
        Beamte          = 1, // Kann jederzeit Beamte orten (z.B. Korruptionsermittlung)
        WithoutWarrant  = 2, // Kann jederzeit jeden orten (ohne aktive Ortung)
        Continuous      = 4, // Aktive Ortung
        Phonehistory    = 8 // Telefonverlauf
    }

    public class FIBModule : Module<FIBModule>
    {
        public static Vector3 UCPoint = new Vector3(152.084, -735.972, 242.152);
        public static Vector3 PermitPoint = new Vector3(153.232, -767.607, 258.152);
        public static Vector3 PermitPoint2 = new Vector3(150.331, -766.021, 258.152);

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.UndercoverName = reader.GetString("ucname");

            if(dbPlayer.IsUndercover())
            {
                string[] nameParts = dbPlayer.UndercoverName.Split("_");
                if (nameParts.Length < 2 || nameParts[0].Length < 3 || nameParts[1].Length < 3) return;

                dbPlayer.SetUndercover(nameParts[0], nameParts[1]);
            }
        }


        protected override bool OnLoad()
        {
            MenuManager.Instance.AddBuilder(new FIBPhoneHistoryMenu());
            MenuManager.Instance.AddBuilder(new FIBPermitMenu());
            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        { 
            if (key != Key.E)
                return false;
                
            if (dbPlayer.TeamId != (int)TeamTypes.TEAM_FIB || dbPlayer.RageExtension.IsInVehicle) return false;
            if (dbPlayer.TeamRank == 0) return false; // @Jeff: Durch den Zivildienst brauchen wir es für unsere normalen Personalausweise - war schon abgesegnet als Loxa es angefragt hat aber nie umgesetzt worden

            if(dbPlayer.Player.Position.DistanceTo(UCPoint) < 2.0f)
            {

                if(dbPlayer.IsUndercover())
                {
                    dbPlayer.ResetUndercover();
                    dbPlayer.SendNewNotification("Sie haben den Undercoverdienst beendet!");
                    dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat den Undercover Dienst beendet!", 5000, 10);
                }
                else
                {
                    // Set Undercover
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Undercover Dienst", Callback = "FIBSetUnderCoverName", Message = "Bitte geben Sie einen Decknamen ein (Max_Mustermann):" });
                }
                return true;
            }

            if (dbPlayer.TeamRank >= 11 && dbPlayer.IsInDuty())
            {
                if (dbPlayer.Player.Position.DistanceTo(PermitPoint) < 2.0f || dbPlayer.Player.Position.DistanceTo(PermitPoint2) < 2.0f)
                {
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "FIB Ortungsverwaltung", Callback = "FIBEditMemberCallback", Message = "Bitte geben Sie den Namen des Agenten ein (Max_Mustermann):" });
                    return true;
                }
            }

            return false;
        }
    }

    public static class FIBPlayerExtension
    {
        public static void SetUndercover(this DbPlayer dbPlayer, string fakename, string surname)
        {
            dbPlayer.fakePerso = true;
            dbPlayer.fakeName = fakename;
            dbPlayer.fakeSurname = surname;

            dbPlayer.UndercoverName = fakename + "_" + surname;
            dbPlayer.UpdateUndercoverName();
            return;
        }
        public static void ResetUndercover(this DbPlayer dbPlayer)
        {
            dbPlayer.fakePerso = false;
            dbPlayer.fakeName = "";
            dbPlayer.fakeSurname = "";

            dbPlayer.UndercoverName = "";
            dbPlayer.UpdateUndercoverName();
            return;
        }

        public static bool IsUndercover(this DbPlayer dbPlayer)
        {
            return dbPlayer.UndercoverName != null && dbPlayer.UndercoverName.Length >= 3;
        }

        public static void UpdateUndercoverName(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET `ucname` = '{dbPlayer.UndercoverName}' WHERE `id` = '{dbPlayer.Id}'");
        }

        public static void SaveFindFlags(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET `fib_find_flags` = '{(int)dbPlayer.FindFlags}' WHERE `id` = '{dbPlayer.Id}'");
        }
    }
}
