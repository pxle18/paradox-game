using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.Assets.Beard;
using VMP_CNR.Module.Assets.Hair;
using VMP_CNR.Module.Assets.HairColor;
using VMP_CNR.Module.Barber.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;

using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Shelter;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.VehicleTax
{
    public sealed class VehicleTaxModule : Module<VehicleTaxModule>
    {
        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.VehicleTaxSum = reader.GetInt32("tax_sum");
        }

        public override void OnFiveMinuteUpdate()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (SxVehicle sxVehicle in VehicleHandler.SxVehicles.Values.ToList().Where(x => x.IsValid() && x.Registered == true).ToList())
                {
                    if (sxVehicle.databaseId <= 0) continue;
                    if (sxVehicle.IsPlayerVehicle())
                    {
                        // Try to get Owner on server
                        DbPlayer dbPlayer = Players.Players.Instance.FindPlayerById(sxVehicle.ownerId);
                        if (dbPlayer != null && dbPlayer.IsValid())
                        {
                            dbPlayer.VehicleTaxSum += sxVehicle.Data.Tax / 12; // Steuern / 60
                        }
                    }
                }

                Task.Run(async () =>
                {
                    foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
                    {
                        dbPlayer.VehicleTaxSum += await GetPlayerVehicleTaxesForGarages(dbPlayer) / 24; // hälfte der Steuern wenn in garage
                    }
                });

                // Fraktionsfahrzeuge
                Task.Run(async () =>
                {
                    foreach (Team team in TeamModule.Instance.GetAll().Values.Where(x => x.IsGangsters()).ToList())
                    {
                        try
                        {
                            TeamShelter teamShelter = TeamShelterModule.Instance.Get(team.Id);
                            if (teamShelter == null) continue;
                            var cost = await Main.GetTeamVehicleTaxes(team.Id) / 12;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                });
            }));
        }

        public static async Task<int> GetPlayerVehicleTaxesForGarages(DbPlayer dbPlayer)
        {
            int tax = 0;

            string query = $"SELECT * FROM `vehicles` WHERE `owner` = '{dbPlayer.Id}' AND `inGarage` = '1' AND `registered` = '1';";

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = @query;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            var modelId = reader.GetUInt32("model");
                            var data = VehicleDataModule.Instance.GetDataById(modelId);
                            if (data == null) continue;
                            tax = tax + data.Tax;
                        }
                    }
                }
            }
            return tax;
        }
    }
}