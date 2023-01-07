using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Helper;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Time;

namespace VMP_CNR.Module.Tasks
{
    public class PlayerLiveReloadTask : AsyncSqlResultTask
    {
        private readonly DbPlayer _player;
        private readonly Action<MySqlDataReader> _reloadAction;

        public PlayerLiveReloadTask(DbPlayer player, Action<MySqlDataReader> reloadAction)
        {
            _player = player;
            _reloadAction = reloadAction;
        }

        public override string GetQuery()
        {
            return $"SELECT * FROM `player` WHERE `id` = '{_player.Id}' LIMIT 1;";
        }

        public override async Task OnFinished(MySqlDataReader reader)
        {
            if (_player == null && !_player.IsValid()) return;

            if (!reader.HasRows)
                return;

            while (await reader.ReadAsync())
            {
                await NAPI.Task.WaitForMainThread(0);

                try
                {
                    await Task.Run(() => _reloadAction(reader));
                }
                catch (Exception e) { Logger.Print(e.ToString()); }
            }
        }
    }
}