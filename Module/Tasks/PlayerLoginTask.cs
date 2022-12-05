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
        private readonly string PlayerName;
        private readonly Player player;

        public PlayerLoginTask(Player player)
        {
            this.player = player;
            this.PlayerName = player.Name;
        }

        public override string GetQuery()
        {
            return $"SELECT * FROM `player` WHERE `Name` = '{MySqlHelper.EscapeString(PlayerName)}' LIMIT 1;";
        }

        public override async Task OnFinished(MySqlDataReader reader)
        {
            // Check to Avoid Double Login
            DbPlayer checkPlayer = player.GetPlayer();
            if(checkPlayer != null && checkPlayer.IsValid())
            {
                return;
            }

            if (reader.HasRows)
            {
                DbPlayer dbPlayer = null;
                while (await reader.ReadAsync())
                {
                    await NAPI.Task.WaitForMainThread(0);

                    if (player == null) return;
                    //Bei Warn hau wech
                    if (reader.GetInt32("warns") >= 3)
                    {
                        player.TriggerNewClient("freezePlayer", true);
                        //player.Freeze(true);
                        player.CreateUserDialog(Dialogs.menu_register, "banwindow");

                        PlayerLoginDataValidationModule.SyncUserBanToForum(reader.GetInt32("forumid"));

                        player.SendNotification($"Dein GVMP (IC-)Account wurde gesperrt. Melde dich im Teamspeak!");
                        player.Kick();
                        return;
                    }

                    if (!PlayerLoginDataValidationModule.HasValidForumAccount(reader.GetInt32("forumid")))
                    {
                        player.TriggerNewClient("freezePlayer", true);

                        player.CreateUserDialog(Dialogs.menu_register, "banwindow");

                        player.Kick("Dein Forumaccount ist nicht für das Spiel freigeschaltet!");
                        return;
                    }

                    // TODO: Edit before release Check Timeban
                    //if (reader.GetInt32("timeban") != 0 && reader.GetInt32("timeban") > DateTime.Now.GetTimestamp())
                    //{
                    //    player.SendNotification("Ban aktiv");
                    //    player.Kick("Ban aktiv");
                    //    return;
                    //}

                    dbPlayer = await Players.Players.Instance.Load(reader, player);

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
                    dbPlayer.Freezed = false;
                    dbPlayer.watchDialog = 0;
                    dbPlayer.Firstspawn = false;
                    dbPlayer.PassAttempts = 0;
                    dbPlayer.TempWanteds = 0;


                    dbPlayer.adminObject = null;
                    dbPlayer.adminObjectSpeed = 0.5f;

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
                            dbPlayer.Player.SendNotification($"Server voll! ({Configuration.Instance.MaxPlayers.ToString()})");
                            dbPlayer.Player.Kick("Server voll");
                            return;
                        }
                    }

                    if (dbPlayer == null) return;
                    player.TriggerEvent("setPlayerHealthRechargeMultiplier");

                    if (dbPlayer.Player.HasData("auth_key"))
                        dbPlayer.Player.ResetData("auth_key");

                    if (dbPlayer.AccountStatus == AccountStatus.LoggedIn) return;

                    ComponentManager.Get<LoginWindow>().Show()(dbPlayer);

                    if (Configuration.Instance.IsUpdateModeOn)
                    {
                        new LoginWindow().TriggerNewClient(dbPlayer.Player, "status", "Der Server befindet sich derzeit im Update Modus!");
                        if (dbPlayer.Rank.Id < 1) dbPlayer.Kick();
                    }
                }
            }
            else
            {
                player.SendNotification("Sie benoetigen einen Account (www.gvmp.de)! Name richtig gesetzt? Vorname_Nachname");
                player.Kick(
                    "Sie benoetigen einen Account (www.gvmp.de)! Name richtig gesetzt? Vorname_Nachname");
                Logger.Debug($"Player was kicked, no Account found for {player.Name}");
            }
        }
    }
}