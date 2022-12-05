using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Computer.Apps.KennzeichenUebersichtApp
{
    public class KennzeichenEvents : Script
    {
        [RemoteEvent]
        public void SetKennzeichen(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnString)) return;

            if (returnString.Length > 8 || returnString.Length < 1)
            {
                dbPlayer.SendNewNotification("Das Kennzeichen muss zwischen 1 und 8 Zeichen lang sein");
                return;
            }            

            if (!Regex.IsMatch(returnString, @"^[a-zA-Z0-9\s]+$"))
            {
                dbPlayer.SendNewNotification("Nur Buchstaben, Zahlen und Leerzeichen sind erlaubt");
                return;
            }

            dbPlayer.Container.RemoveItem(596, 1);
            dbPlayer.Container.AddItem(642, 1, new Dictionary<string, dynamic>() { { "Plate", returnString.Trim() } });
            dbPlayer.SendNewNotification("Sie haben das Kennzeichen bedruckt.");
        }

        [RemoteEvent]
        public void SetVehicleNote(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, returnString)) return;

            if (returnString.Length > 15 || returnString.Length < 1)
            {
                dbPlayer.SendNewNotification("Die Notiz muss zwischen 1 und 10 Zeichen lang sein");
                return;
            }

            if (!Regex.IsMatch(returnString, @"^[a-zA-Z0-9\s]+$"))
            {
                dbPlayer.SendNewNotification("Nur Buchstaben, Zahlen und Leerzeichen sind erlaubt");
                return;
            }

            if (player.Vehicle == null) return;

            SxVehicle sxVehicle = player.Vehicle.GetVehicle();
            if (sxVehicle == null || !sxVehicle.IsValid() || !dbPlayer.CanControl(sxVehicle)) return;

            if (sxVehicle.IsPlayerVehicle())
            {
                MySQLHandler.ExecuteAsync($"UPDATE vehicles SET note = '{returnString}' WHERE id = {sxVehicle.databaseId}");
            }
            else if (sxVehicle.IsTeamVehicle())
            {
                if (dbPlayer.TeamId == sxVehicle.teamid && dbPlayer.TeamRank >= 10)
                {
                    MySQLHandler.ExecuteAsync($"UPDATE fvehicles SET note = '{returnString}' WHERE id = {sxVehicle.databaseId}");
                }
            }


            dbPlayer.SendNewNotification("Sie haben die Notiz angebracht.");

            dbPlayer.Container.RemoveItem(659, 1); // Notizblock
        }



    }
}
