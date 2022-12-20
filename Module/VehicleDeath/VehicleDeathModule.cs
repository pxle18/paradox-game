using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Meth;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Shelter;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.VehicleDeath
{

    public class VehicleDeathModule : Module<VehicleDeathModule>
    {
        private string VehicleBackupDB = "container_vehicle_backups";

        public int GetVehiclesRepairPrice(SxVehicle sxVehicle)
        {
            int price = sxVehicle.Data.Price / 1000;
            if (price <= 500) price = 500; // min
            if (price >= 50000) price = 50000; // max
            return price;
        }

        public void CreateVehicleBackupInventory(SxVehicle sxVehicle)
        {
            if (sxVehicle == null || (!sxVehicle.IsPlayerVehicle() && !sxVehicle.IsTeamVehicle()) || sxVehicle.databaseId == 0) return;

            string saveQuery = GetContainerInsertionQuery(sxVehicle.Container);
            if (saveQuery != "")
                MySQLHandler.ExecuteAsync(saveQuery, Sync.MySqlSyncThread.MysqlQueueTypes.Inventory);

            Logging.Logger.Debug(saveQuery);

            if (sxVehicle.Container != null)
                sxVehicle.Container.ClearInventory(); 

            if (sxVehicle.Container2 != null)
                sxVehicle.Container2.ClearInventory();
        }

        public override void OnTenSecUpdate()
        {
            // Deaktiviert lassen, Performance Killer
            // Todo: Fahrzeug bei Explosion entfernen
            if (!ServerFeatures.IsActive("vehicle-cleanup-exploded"))
                return;

            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(0);
                foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetAllVehicles())
                {
                    // Destroyed...
                    if (sxVehicle != null && sxVehicle.IsValid() && sxVehicle.Entity != null && sxVehicle.Entity.Health <= -300)
                    {
                        if (!sxVehicle.HasData("deathTrigger"))
                        {
                            sxVehicle.SetData("deathTrigger", true);
                            return;
                        }
                        else
                        {
                            sxVehicle.ResetData("deathTrigger");
                            // Inventory & Backup Stuff
                            VehicleDeathModule.Instance.CreateVehicleBackupInventory(sxVehicle);

                            VehicleDeathModule.Instance.RemoveOccupantsOnDeath(sxVehicle);
                            if (sxVehicle.IsPlayerVehicle() && sxVehicle.databaseId > 0)
                            {
                                sxVehicle.SetPrivateCarGarage(1, sxVehicle.Data.Classification.ScrapYard);
                            }
                            else if (sxVehicle.IsTeamVehicle())
                            {
                                if (TeamModule.Instance.GetById((int)sxVehicle.teamid).IsGangsters())
                                {
                                    // Abziehen von Fbank
                                    TeamShelter shelter = TeamShelterModule.Instance.GetByTeam(sxVehicle.teamid);
                                }

                                sxVehicle.SetTeamCarGarage(true);
                            }
                            else
                                VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                        }
                    }
                }
            });
        }

        public void RemoveOccupantsOnDeath(SxVehicle xVeh)
        {
            try
            {
                if (xVeh.Visitors.Count > 0)
                {
                    foreach (DbPlayer dbPlayer in xVeh.Visitors)
                    {
                        if (dbPlayer.DimensionType[0] == DimensionType.Camper && dbPlayer.Player.Dimension != 0)
                        {
                            try
                            {
                                if (xVeh.Visitors.Contains(dbPlayer)) xVeh.Visitors.Remove(dbPlayer);
                                dbPlayer.Player.SetPosition(new Vector3(xVeh.Entity.Position.X + 3.0f,
                                    xVeh.Entity.Position.Y,
                                    xVeh.Entity.Position.Z));
                            }
                            catch (Exception e)
                            {
                                Logging.Logger.Crash(e);
                            }
                            finally
                            {
                                // Reset Cooking on Exit
                                if (dbPlayer.HasData("cooking"))
                                {
                                    dbPlayer.ResetData("cooking");
                                }
                                if (HeroinModule.CookingPlayers.Contains(dbPlayer)) HeroinModule.CookingPlayers.Remove(dbPlayer);

                                dbPlayer.DimensionType[0] = DimensionType.World;
                                dbPlayer.Dimension[0] = 0;
                                dbPlayer.SetDimension(0);
                                dbPlayer.Player.SetPosition((Vector3)dbPlayer.GetData("CamperEnterPos"));
                                dbPlayer.ResetData("CamperEnterPos");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }

        private string GetContainerInsertionQuery(Container container)
        {
            string slotsValuesQuery = "";

            for (int i = 0; i < container.MaxSlots; i++)
            {
                slotsValuesQuery += $"'{NAPI.Util.ToJson(container.ConvertToSaving()[i])}',";
            }

            return $"INSERT INTO `{VehicleBackupDB}` VALUES ('', '{container.Id}', '', '{(int)container.Type}', '{container.MaxWeight}', '{container.MaxSlots}', {slotsValuesQuery.Substring(0, slotsValuesQuery.Length - 1)});";
        }
    }
}
