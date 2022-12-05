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
    public class PlayerLoginTask : AsyncSqlResultTask
    {
        private readonly string _playerName;
        private readonly Player _player;

        public PlayerLoginTask(Player player)
        {
            _player = player;
            _playerName = player.Name;
        }

        public override string GetQuery()
        {
            return $"SELECT * FROM `player` WHERE `Name` = '{MySqlHelper.EscapeString(_playerName)}' LIMIT 1;";
        }

        public override async Task OnFinished(MySqlDataReader reader)
        {
            DbPlayer checkPlayer = _player.GetPlayer();
            if (checkPlayer != null && checkPlayer.IsValid()) return;

            if (!reader.HasRows)
            {
                _player.SendNotification("Sie benoetigen einen Account (www.gvmp.de)! Name richtig gesetzt? Vorname_Nachname");
                _player.Kick(
                    "Sie benoetigen einen Account (www.gvmp.de)! Name richtig gesetzt? Vorname_Nachname");
                Logger.Debug($"Player was kicked, no Account found for {_player.Name}");

                return;
            }

            while (await reader.ReadAsync())
            {
                await NAPI.Task.WaitForMainThread(0);

                if (_player == null) return;

                if (reader.GetInt32("warns") >= 3)
                {
                    _player.TriggerNewClient("freezePlayer", true);
                    //player.Freeze(true);
                    _player.CreateUserDialog(Dialogs.menu_register, "banwindow");

                    PlayerLoginDataValidationModule.SyncUserBanToForum(reader.GetInt32("forumid"));

                    _player.SendNotification($"Dein GVMP (IC-)Account wurde gesperrt. Melde dich im Teamspeak!");
                    _player.Kick();
                    return;
                }

                if (!PlayerLoginDataValidationModule.HasValidForumAccount(reader.GetInt32("forumid")))
                {
                    _player.TriggerNewClient("freezePlayer", true);

                    _player.CreateUserDialog(Dialogs.menu_register, "banwindow");

                    _player.Kick("Dein Forumaccount ist nicht für das Spiel freigeschaltet!");
                    return;
                }

                // TODO: Edit before release Check Timeban
                //if (reader.GetInt32("timeban") != 0 && reader.GetInt32("timeban") > DateTime.Now.GetTimestamp())
                //{
                //    player.SendNotification("Ban aktiv");
                //    player.Kick("Ban aktiv");
                //    return;
                //}
                
                DbPlayer dbPlayer = await Players.Players.Instance.Load(reader, _player);

                // TODO: Edit before release
                //if (!SocialBanHandler.Instance.IsPlayerWhitelisted(dbPlayer))
                //{
                //    player.SendNotification("Bitte whitelisten Sie sich im Forum (GVMP-Shield)!");
                //    player.Kick();
                //    return;
                //}

                await NAPI.Task.WaitForMainThread(0);
                dbPlayer.Player.TriggerEvent("sendAuthKey", dbPlayer.AuthKey);

                dbPlayer.WatchMenu = 0;
                dbPlayer.WatchDialog = 0;
                dbPlayer.IsFreezed = false;
                dbPlayer.IsFirstSpawn = false;
                dbPlayer.PassAttempts = 0;
                dbPlayer.TempWanteds = 0;


                dbPlayer.AdminObject = null;
                dbPlayer.AdminObjectSpeed = 0.5f;

                dbPlayer.AccountStatus = AccountStatus.Registered;

                dbPlayer.Character = ClothModule.Instance.LoadCharacter(dbPlayer);

                await VehicleKeyHandler.Instance.LoadPlayerVehicleKeys(dbPlayer);

                dbPlayer.SetPlayerCurrentJobSkill();
                //dbPlayer.ClearChat();

                await NAPI.Task.WaitForMainThread();
                string l_SocialClub = dbPlayer.Player.SocialClubName;

                if (dbPlayer.Rank == null || !dbPlayer.Rank.CanAccessFeature("ignore_maxplayers"))
                {
                    if (Players.Players.Instance.players.ToList().Count >= Configuration.Instance.MaxPlayers)
                    {
                        dbPlayer.Player.SendNotification($"Server voll! ({Configuration.Instance.MaxPlayers})");
                        dbPlayer.Player.Kick("Server voll");
                        return;
                    }
                }

                if (dbPlayer == null) return;
                _player.TriggerEvent("setPlayerHealthRechargeMultiplier");

                if (dbPlayer.Player.HasData("auth_key"))
                    dbPlayer.Player.ResetData("auth_key");

                if (dbPlayer.AccountStatus == AccountStatus.LoggedIn) return;

                ComponentManager.Get<LoginWindow>().Show()(dbPlayer);

                if (Configuration.Instance.IsUpdateModeOn)
                {
                    ComponentManager.Get<LoginWindow>().TriggerNewClient(dbPlayer.Player, "status", "Der Server befindet sich derzeit im Update Modus!");
                    if (dbPlayer.Rank.Id < 1) dbPlayer.Kick();
                }
            }
        }
    }
}