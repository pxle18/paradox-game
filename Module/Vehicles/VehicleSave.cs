using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VMP_CNR.Handler;

namespace VMP_CNR.Module.Vehicles
{
    public static class VehicleSave
    {
        //public static string GetFVehicleSaveQuery(this SxVehicle sxVeh, bool status)
        //{
        //    String statusString = (status == true) ? "1" : "0";
        //    return $"UPDATE `fvehicles` SET `plate` = '{sxVeh.plate}', `registered` = {statusString}";
        //}


        public static async Task<string> GetVehicleSaveQuery(this SxVehicle sxVeh, bool position = true, bool statsonly = false,
            uint? garage = null, uint? garageId = null)
        {
            if (!sxVeh.IsPlayerVehicle()) return "";

            String fuel = sxVeh.fuel.ToString().Replace(",", ".");
            uint vehOwnerId = sxVeh.databaseId;
            
            List<string> ups = new List<string>();
            
            // Preparing the Update Query
            string update = "UPDATE `vehicles` SET ";

            if (!statsonly)
            {
                string px = "";
                string py = "";
                string pz = "";
                string heading = "";
                uint dimension = 0;

                // Main Thread, um entity Details abzufragen
                await NAPI.Task.WaitForMainThread(0);
                var vehPos = sxVeh.entity.Position;
                var vehHeading = sxVeh.entity.Rotation.Z;
                var vehDimension = sxVeh.entity.Dimension;

                // Wieder in einen Background Thread wechseln
                await Task.Delay(1);
                if(garage == 0) // nicht in garage
                {
                    // Aktuelle Position des Fahrzeuges
                    px = vehPos.X.ToString().Replace(",", ".");
                    py = vehPos.Y.ToString().Replace(",", ".");
                    pz = vehPos.Z.ToString().Replace(",", ".");
                    heading = vehHeading.ToString().Replace(",", ".");
                    dimension = vehDimension;
                }
                else
                {
                    // Position 0 wegen inGarage lul
                    px = "0";
                    py = "0";
                    pz = "0";
                    heading = "0";
                }

                if (px != "") ups.Add("`pos_x` = '" + px + "'");
                if (py != "") ups.Add("`pos_y` = '" + py + "'");
                if (pz != "") ups.Add("`pos_z` = '" + pz + "'");
                if (heading != "") ups.Add("`heading` = '" + heading + "'");
            }
            string xstr = "`fuel` = '" + fuel + "'";
            ups.Add(xstr);
            
            // Main Thread, da Entity Property
            await NAPI.Task.WaitForMainThread();
            var zustand = Convert.ToInt32(sxVeh.entity.Health);

            // Zurück in einen Worker Thread
            await Task.Delay(1);
            //zustand
            ups.Add("`zustand` = '" + zustand + "'");

            // Kilometer
            ups.Add("`km` = '" + sxVeh.Distance + "'");

            // Nummernschild
            //ups.Add("`plate` = '" + sxVeh.plate + "'");

            //garage
            if (garage != null) ups.Add("`inGarage` = '" + garage + "'");

            if (garageId != null) ups.Add("`garage_id` = '" + garageId + "'");

            string updateX = "";

            updateX = string.Join(", ", ups);

            updateX = update + updateX + " WHERE id = '" + vehOwnerId + "';";

            return updateX;
        }

        public static void Save(this SxVehicle sxVeh, bool statsonly = false, uint? garage = null, uint? garageId = null)
        {
            if (sxVeh == null || sxVeh.entity == null) return;
            if (!sxVeh.IsPlayerVehicle() || sxVeh.databaseId == 0) return;

            Task.Run(async () =>
            {
                string updateX = await GetVehicleSaveQuery(sxVeh, true, statsonly, garage, garageId);

                if (updateX != "")
                {
                    sxVeh.saveQuery = "";
                    MySQLHandler.ExecuteAsync(updateX, Sync.MySqlSyncThread.MysqlQueueTypes.Vehicles);
                }
            });
        }
    }
}