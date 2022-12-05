using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Animal
{
    public class AnimalToPlayerModule : Module<AnimalToPlayerModule>
    {
        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            NAPI.Task.Run(async () =>
            {
                using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var keyCmd = keyConn.CreateCommand())
                {
                    await keyConn.OpenAsync();
                    keyCmd.CommandText =
                        $"SELECT * FROM animal_to_player WHERE player_id = '{dbPlayer.Id}';";
                    using (var keyReader = keyCmd.ExecuteReader())
                    {
                        if (keyReader.HasRows)
                        {
                            while (keyReader.Read())
                            {
                                uint keyId = keyReader.GetUInt32("animal_id");
                                if (!dbPlayer.AssignedAnimals.Contains(keyId))
                                {
                                    dbPlayer.AssignedAnimals.Add(keyId);
                                }
                            }
                        }
                    }
                    await keyConn.CloseAsync();
                }
            });
        }
    }
}
