using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Nutrition;

namespace VMP_CNR.Module.Sync
{
    public class MySqlSyncThread
    {
        public enum MysqlQueueTypes
        {
            Default = 0,
            Inventory = 1,
            Vehicles = 2,
            Logging = 3,
            Nutrition = 4,
            Damage = 5
        }

        public static MySqlSyncThread Instance { get; } = new MySqlSyncThread();

        public readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        public readonly ConcurrentQueue<string> queue2 = new ConcurrentQueue<string>();
        public readonly ConcurrentQueue<string> queue3 = new ConcurrentQueue<string>();
        public readonly ConcurrentQueue<string> InventoryQueue = new ConcurrentQueue<string>();
        public readonly ConcurrentQueue<string> VehiclesQueue = new ConcurrentQueue<string>();
        public readonly ConcurrentQueue<string> LoggingQueue = new ConcurrentQueue<string>();
        public readonly ConcurrentQueue<string> NutritionQueue = new ConcurrentQueue<string>();
        public readonly ConcurrentQueue<string> DamageQueue = new ConcurrentQueue<string>();


        private int index = 1;

        //public ConcurrentDictionary<DateTime, string> LastQueueQuerysAvoidSpam = new ConcurrentDictionary<DateTime, string>();

        private MySqlSyncThread()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (queue.IsEmpty)
                    {
                        await Task.Delay(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            await conn.OpenAsync();
                            while (!queue.IsEmpty)
                            {
                                if (!queue.TryDequeue(out var query)) continue;
                                try
                                {
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        Logger.Debug(@"async task: " + query);
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Print("##############################");
                                    Logger.Print("FAULTY MySQL Query executed!");
                                    Logger.Print($"Query: {query}");
                                    Logger.Print("Full Exception:");
                                    Logger.Crash(e);
                                    Logger.Print("##############################");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (queue2.IsEmpty)
                    {
                        await Task.Delay(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            await conn.OpenAsync();
                            while (!queue2.IsEmpty)
                            {
                                if (!queue2.TryDequeue(out var query)) continue;
                                try
                                {
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        Logger.Debug(@"async task: " + query);
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Print("##############################");
                                    Logger.Print("FAULTY MySQL Query executed!");
                                    Logger.Print($"Query: {query}");
                                    Logger.Print("Full Exception:");
                                    Logger.Crash(e);
                                    Logger.Print("##############################");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (queue3.IsEmpty)
                    {
                        await Task.Delay(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            await conn.OpenAsync();
                            while (!queue3.IsEmpty)
                            {
                                if (!queue3.TryDequeue(out var query)) continue;
                                try
                                {
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        Logger.Debug(@"async task: " + query);
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Print("##############################");
                                    Logger.Print("FAULTY MySQL Query executed!");
                                    Logger.Print($"Query: {query}");
                                    Logger.Print("Full Exception:");
                                    Logger.Crash(e);
                                    Logger.Print("##############################");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (InventoryQueue.IsEmpty)
                    {
                        await Task.Delay(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            await conn.OpenAsync();
                            while (!InventoryQueue.IsEmpty)
                            {
                                if (!InventoryQueue.TryDequeue(out var query)) continue;
                                using (var cmd = conn.CreateCommand())
                                {
                                    try
                                    {
                                        Logger.Debug(@"async task: " + query);
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                    catch(Exception e)
                                    {
                                        Logger.Print("##############################");
                                        Logger.Print("FAULTY MySQL Query executed!");
                                        Logger.Print($"Query: {query}");
                                        Logger.Print("Full Exception:");
                                        Logger.Crash(e);
                                        Logger.Print("##############################");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {

                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (VehiclesQueue.IsEmpty)
                    {
                        await Task.Delay(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            await conn.OpenAsync();
                            while (!VehiclesQueue.IsEmpty)
                            {
                                if (!VehiclesQueue.TryDequeue(out var query)) continue;
                                try
                                {
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        Logger.Debug(@"async task: " + query);
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Print("##############################");
                                    Logger.Print("FAULTY MySQL Query executed!");
                                    Logger.Print($"Query: {query}");
                                    Logger.Print("Full Exception:");
                                    Logger.Crash(e);
                                    Logger.Print("##############################");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            // MAKE MAKROS GREAT AGAIN!
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (LoggingQueue.IsEmpty)
                    {
                        await Task.Delay(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            await conn.OpenAsync();
                            while (!LoggingQueue.IsEmpty)
                            {
                                if (!LoggingQueue.TryDequeue(out var query)) continue;
                                try
                                {
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        Logger.Debug(@"async task: " + query);
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Print("##############################");
                                    Logger.Print("FAULTY MySQL Query executed!");
                                    Logger.Print($"Query: {query}");
                                    Logger.Print("Full Exception:");
                                    Logger.Crash(e);
                                    Logger.Print("##############################");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (NutritionQueue.IsEmpty)
                    {
                        await Task.Delay(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            await conn.OpenAsync();
                            while (!NutritionQueue.IsEmpty)
                            {
                                if (!NutritionQueue.TryDequeue(out var query)) continue;
                                try
                                {
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        Logger.Debug(@"async task: " + query);
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Print("##############################");
                                    Logger.Print("FAULTY MySQL Query executed!");
                                    Logger.Print($"Query: {query}");
                                    Logger.Print("Full Exception:");
                                    Logger.Crash(e);
                                    Logger.Print("##############################");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (DamageQueue.IsEmpty)
                    {
                        await Task.Delay(1500);
                    }

                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            await conn.OpenAsync();
                            while (!DamageQueue.IsEmpty)
                            {
                                if (!DamageQueue.TryDequeue(out var query)) continue;
                                try
                                {
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        Logger.Debug(@"async task: " + query);
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Print("##############################");
                                    Logger.Print("FAULTY MySQL Query executed!");
                                    Logger.Print($"Query: {query}");
                                    Logger.Print("Full Exception:");
                                    Logger.Crash(e);
                                    Logger.Print("##############################");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Add(string query, MysqlQueueTypes quetype)
        {
            /*if (LastQueueQuerysAvoidSpam.Values.Contains(query))
            {
                return; // protect 4 spamm
            }
            else
            {
                LastQueueQuerysAvoidSpam.TryAdd(DateTime.Now, query);
            }*/

            if (quetype == MysqlQueueTypes.Inventory)
            {
                InventoryQueue.Enqueue(query);
                return;
            }

            if (quetype == MysqlQueueTypes.Vehicles)
            {
                VehiclesQueue.Enqueue(query);
                return;
            }

            if (quetype == MysqlQueueTypes.Logging)
            {
                LoggingQueue.Enqueue(query);
                return;
            }

            if (quetype == MysqlQueueTypes.Nutrition)
            {
                NutritionQueue.Enqueue(query);
                return;
            }

            if (quetype == MysqlQueueTypes.Damage)
            {
                DamageQueue.Enqueue(query);
                return;
            }

            if (index > 3) index = 1;

            if (index == 1) queue.Enqueue(query);
            else if (index == 2) queue2.Enqueue(query);
            else queue3.Enqueue(query);

            index++;
        }
    }
}