using System;
using VMP_CNR.Module.Players.Db;
using Newtonsoft.Json;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Events;
using System.Threading.Tasks;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Time;
using System.Linq;
using VMP_CNR.Module.Anticheat;

namespace VMP_CNR.Module.Players.Windows
{
    public class LoginWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "name")] private string Name { get; }
            [JsonProperty(PropertyName = "rank")] private uint Rank { get; }

            public ShowEvent(DbPlayer dbPlayer, string name, uint rank) : base(dbPlayer)
            {
                Name = name;
                Rank = rank;
            }
        }

        public LoginWindow() : base("Login")
        {
        }

        public override Func<DbPlayer, bool> Show()
        {
            return player => OnShow(new ShowEvent(player, player.GetName(), player.RankId));
        }
        
        [RemoteEvent]
        public void PlayerLogin(Player player, string password, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(0);

                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null) return;


                if (dbPlayer.AccountStatus != AccountStatus.Registered)
                {
                    dbPlayer.SendNewNotification("Sie sind bereits eingeloggt!");
                    TriggerNewClient(player, "status", "successfully");
                    return;
                }

                var pass = HashThis.GetSha256Hash(dbPlayer.Salt + password);
                var pass2 = dbPlayer.Password;
                if (pass == pass2)
                {
                    Logger.SaveLoginAttempt(dbPlayer.Id, dbPlayer.Player.SocialClubName, dbPlayer.Player.Address, 1);

                    try
                    {

                        // Set Data that Player is Connected
                        dbPlayer.Player.SetData("Connected", true);

                        dbPlayer.AccountStatus = AccountStatus.LoggedIn;

                        dbPlayer.SetACLogin();

                        //Set online
                        var query =
                            $"UPDATE `player` SET `Online` = '{1}', LastLogin = '{DateTime.Now.GetTimestamp()}' WHERE `id` = '{dbPlayer.Id}';";
                        MySQLHandler.ExecuteAsync(query);

                        dbPlayer.Player.ResetData("loginStatusCheck");

                        TriggerNewClient(player, "status", "successfully");

                        //player.SetSharedData("AC_Status", true);

                        // send phone data
                        var data = new { Credit = dbPlayer.guthaben[0], Number = dbPlayer.handy[0] };
                        dbPlayer.Player.TriggerNewClient("RESPONSE_PHONE_SETTINGS", JsonConvert.SerializeObject(data));
        
                        var duplicates = NAPI.Pools.GetAllPlayers().ToList().FindAll(p => p.Name == player.Name && p != player);

                        if (duplicates.Count > 0)
                        {
                            try
                            {
                                foreach (var duplicate in duplicates)
                                {
                                    Logger.Debug($"Duplicated Player {duplicate.Name} deleted");

                                    duplicate.Delete();

                                    duplicate.SendNotification("Duplicated Player");
                                    duplicate.Kick();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Crash(ex);
                                // ignored
                            }
                        }

                        dbPlayer.IsFirstSpawn = true;
                        // Character Sync
                        NAPI.Task.Run(() =>
                        {
                            dbPlayer.ApplyCharacter(true);
                            dbPlayer.ApplyPlayerHealth();
                            dbPlayer.Player.TriggerEvent("setPlayerHealthRechargeMultiplier");
                        }, 3000);

                        PlayerSpawn.OnPlayerSpawn(player);

                        dbPlayer.SetData("login_time", DateTime.Now);
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }
                }
                else
                {
                    Logger.SaveLoginAttempt(dbPlayer.Id, dbPlayer.Player.SocialClubName, dbPlayer.Player.Address, 0);
                    dbPlayer.PassAttempts += 1;

                    if (dbPlayer.PassAttempts >= 3)
                    {
                        //dbPlayer.SendNewNotification("Sie haben ein falsches Passwort 3x eingegeben, Sicherheitskick.", title:"SERVER", notificationType:PlayerNotification.NotificationType.SERVER);
                        TriggerNewClient(player, "status", "Passwort wurde 3x falsch eingegeben. Sicherheitskick");
                        player.Kick("Falsches Passwort (3x)");
                        return;
                    }

                    string message = string.Format(

                        "Falsches Passwort ({0}/3)",
                        dbPlayer.PassAttempts);
                    //dbPlayer.SendNewNotification(message, title:"SERVER", notificationType:PlayerNotification.NotificationType.SERVER);
                    TriggerNewClient(player, "status", message);
                }
            });
        }
    }
}