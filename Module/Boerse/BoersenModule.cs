using System;
using System.Collections.Concurrent;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Boerse
{
    public sealed class BoersenModule : Module<BoersenModule>
    {
        /// <summary>
        /// Beinhaltet die Aktien mit den gehandelt werden kann. ConcurrentDictionary um Thread-Safe zu bleiben.
        /// </summary>
        public static ConcurrentDictionary<uint, Aktie> Aktien = new ConcurrentDictionary<uint, Aktie>();

        public override Type[] RequiredModules()
        {
            return new[] { typeof(ConfigurationModule) };
        }

        /// <summary>
        /// Wenn der Spieler verbunden ist, sollen die gekauften Aktien geladen werden.
        /// </summary>
        /// <param name="dbPlayer"></param>
        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            //dbPlayer.LoadPlayerAktien();
        }
        
        /*protected override bool OnLoad()
        {
            LoadAktien();
            return base.OnLoad();
        }*/

        /// <summary>
        /// Jede Minute soll auf geänderte Aktien-Kurse geprüft werden
        /// </summary>
        public override void OnMinuteUpdate()
        {
            //LoadAktien();
        }

        /// <summary>
        /// DB-Abfrage für die handelbaren Aktien
        /// </summary>
        private void LoadAktien()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnectionBoerse()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT * FROM `aktien`;";
                
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            uint ID = reader.GetUInt32("id");
                            string Name = reader.GetString("name");
                            string Description = reader.GetString("description");
                            uint LastAmount = reader.GetUInt32("last_amount");
                            uint MinAmount = reader.GetUInt32("min_amount");
                            uint MaxAmount = reader.GetUInt32("max_amount");
                            uint ActualAmount = reader.GetUInt32("actual_amount");
                            
                            Aktie aktie = new Aktie(Name, Description, LastAmount, MinAmount, MaxAmount, ActualAmount);

                            if (Aktien.ContainsKey(ID))
                                Aktien[ID] = aktie;
                            else
                                Aktien.TryAdd(ID, aktie);
                        }
                    }
                }
            }
        }
    }
}