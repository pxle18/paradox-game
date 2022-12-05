using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.PlayerTask
{
    public static class PlayerTaskExtension
    {
        public static void LoadTasks(this DbPlayer dbPlayer)
        {
            NAPI.Task.Run(async () =>
            {
                dbPlayer.PlayerTasks = new Dictionary<uint, PlayerTask>();

                var query = $"SELECT * FROM `tasks` WHERE `owner_id` = '{dbPlayer.Id}'";

                dbPlayer.PlayerTasks.Clear();
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
                                // Verarbeitung DONE... Load PlayerTask into Players Tasks
                                var pTask = new PlayerTask(
                                    reader.GetUInt32("id"),
                                    reader.GetUInt32("type"),
                                    dbPlayer,
                                    reader.GetString("data"),
                                    reader.GetDateTime("finish")
                                );

                                dbPlayer.PlayerTasks.Add(pTask.Id, pTask);
                            }
                        }
                    }
                }
            });
        }

        public static void AddTask(this DbPlayer dbPlayer, PlayerTaskTypeId type, string data = "")
        {
            NAPI.Task.Run(async () =>
            {
                var pTaskType = PlayerTaskTypeModule.Instance.Get((uint)type);
                if (pTaskType == null) return;

                var finishedTime = DateTime.Now.AddMinutes(pTaskType.TaskTime);

                using (var connection = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var command = connection.CreateCommand())
                {
                    await connection.OpenAsync();
                    command.CommandText =
                        $"INSERT INTO `tasks` (type, owner_id, data, finish) VALUES('{(int)pTaskType.Id}', '{(int)dbPlayer.Id}', '{data}', '{finishedTime:yyyy-MM-dd H:mm:ss}'); select last_insert_id();";
                    var taskId = Convert.ToUInt32(await command.ExecuteScalarAsync());
                    dbPlayer.PlayerTasks.Add(taskId, new PlayerTask(taskId, (uint)type, dbPlayer, data, finishedTime));
                    await connection.CloseAsync();
                }
            });
        }

        public static void RemoveTask(this DbPlayer dbPlayer, uint taskId)
        {
            MySQLHandler.ExecuteAsync($"DELETE FROM `tasks` WHERE `id` = '{taskId}'");
            dbPlayer.PlayerTasks?.Remove(taskId);
        }

        public static void CheckTasks(this DbPlayer dbPlayer)
        {
            if (dbPlayer.PlayerTasks == null) return;
            var now = DateTime.Now;
            
            foreach(KeyValuePair<uint, PlayerTask> kvp in dbPlayer.PlayerTasks)
            {
                if (kvp.Value.Finish < now) kvp.Value.OnTaskFinish();
            }

            List<uint> toRemove = new List<uint>();
            foreach (KeyValuePair<uint, PlayerTask> pair in dbPlayer.PlayerTasks)
            {
                if (pair.Value.Finish < now)
                {
                    toRemove.Add(pair.Key);
                }
            }

            foreach (var key in toRemove)
            {
                dbPlayer.RemoveTask(dbPlayer.PlayerTasks[key].Id);
            }
            
        }

        public static bool CheckTaskExists(this DbPlayer dbPlayer, PlayerTaskTypeId type)
        {
            return dbPlayer.PlayerTasks?.FirstOrDefault(task => task.Value.Type.Id == type).Value != null;
        }
    }
}