using System;
using System.Collections.Generic;
using GTANetworkAPI;
using GTANetworkMethods;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.RegistrationOffice;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool VehicleUnregister(DbPlayer dbPlayer, ItemModel ItemData)
        {
            //check if worker is from DPOS
            if (!dbPlayer.Team.CanRegisterVehicles())
            {
                dbPlayer.SendNewNotification("Dieser Vorgang ist nur fuer geschultes Personal vom DPOS und DMV!");
                return false;
            }

            if (!dbPlayer.IsInDuty())
            {
                dbPlayer.SendNewNotification("Sie müssen im Dienst sein um Fahrzeuge anzumelden.");
                return false;
            }

            if (dbPlayer.TeamRank < 3)
            {
                dbPlayer.SendNewNotification("Sie müssen mindestens Rang 3 sein um Fahrzeuge abmelden zu können.");
                return false;
            }

            bool canUseEverywhere = bool.Parse(ItemData.Script.Split("_")[1]);
            if (!canUseEverywhere && dbPlayer.Player.Position.DistanceTo(new GTANetworkAPI.Vector3(386.223, -1621.51, 29.292)) > RegistrationOfficeFunctions.RegistrationRadius)
            {
                dbPlayer.SendNewNotification("Sie müssen am Zulassungsplatz sein.");
                return false;
            }

            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
            if (sxVehicle == null)
            {
                dbPlayer.SendNewNotification("Kein Fahrzeug in der Nähe!");
                return false;
            }

            if (sxVehicle.databaseId == 0) return false;

            if (!RegistrationOfficeFunctions.IsVehicleRegistered(sxVehicle.databaseId))
            {
                dbPlayer.SendNewNotification("Dieses Fahrzeug ist nicht mit einem Kennzeichen angemeldet.");
                return false;
            }

            //check if vehicle has driver
            if (sxVehicle.GetOccupants().GetDriver() != null)
            {
                //driver is available
                DbPlayer driver = sxVehicle.GetOccupants().GetDriver();
                if (driver == null || !driver.IsValid()) return false;

                //check if driver is owner
                if (sxVehicle.ownerId == driver.Id || (sxVehicle.IsTeamVehicle() && sxVehicle.teamid == driver.TeamId))
                {
                    //yees driver is owner

                    if (sxVehicle.Team.IsStaatsfraktion())
                    {
                        if (driver.TeamRank < 9)
                        {
                            dbPlayer.SendNewNotification("Der Bürger muss mindestens Rang 9 seiner Organisation zu sein um das Fahrzeug abzumelden.");
                            return false;
                        }
                    }

                    if (driver.TakeBankMoney(10000, "Fahrzeug abgemeldet " + sxVehicle.databaseId))
                    {
                        driver.SendNewNotification("Ihr Fahrzeug wurde erfolgreich abgemeldet");
                        dbPlayer.SendNewNotification("Sie haben das Fahrzeug erfolgreich abgemeldet.");

                        sxVehicle.Registered = false;
                        RegistrationOfficeFunctions.UpdateVehicleRegistrationToDb(sxVehicle, driver, dbPlayer, sxVehicle.plate, false);
                        sxVehicle.plate = "";

                        NAPI.Task.Run(() => { sxVehicle.entity.NumberPlate = ""; });
                        return true;
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Ihr Konto ist nicht gedeckt 10.000$");
                        return false;
                    }
                }
            }

            dbPlayer.SendNewNotification("Der Besitzer des Fahrzeugs muss auf dem Fahrersitz sein");
            return false;
        }
    }
}