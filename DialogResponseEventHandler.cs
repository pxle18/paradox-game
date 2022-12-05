using System;
using System.IO;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;

using VMP_CNR.Module.Players.Db;

using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Teams.Shelter;
using VMP_CNR.Module.Players.Events;

namespace VMP_CNR
{
    public class DialogResponseEventHandler : Script
    {
        [RemoteEvent]
        public void DialogResponse(Player player, params object[] args)
        {
            if (!player.CheckRemoteEventKey("")) return;
            if (args.Length == 0) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;
            // General Close Dialog
            if (Convert.ToString(args[0]) == "false")
            {
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                //player.Freeze(false);
                dbPlayer.watchDialog = 0;
                player.TriggerNewClient("deleteDialog");
                return;
            }

            var dialogid = dbPlayer.watchDialog;
            var input = args[0];
            var input2 = "";
            if (args.Length > 1 && Convert.ToString(args[1]) != "")
            {
                input2 = Convert.ToString(args[1]);
            }

            if (dialogid == Dialogs.menu_login)
            {
                string response = Convert.ToString(input);

                if (response != "false")
                {
                    if (string.IsNullOrEmpty(response) || response == null)
                    {
                        dbPlayer.SendNewNotification("Geben Sie ein Passwort ein!", title:"SERVER", notificationType:PlayerNotification.NotificationType.SERVER);
                        return;
                    }

                    if (dbPlayer.AccountStatus != AccountStatus.Registered)
                    {
                        dbPlayer.SendNewNotification("Sie sind bereits eingeloggt!", title: "SERVER", notificationType: PlayerNotification.NotificationType.SERVER);
                        dbPlayer.CloseUserDialog(Dialogs.menu_login);
                        return;
                    }

                    var pass = HashThis.GetSha256Hash(dbPlayer.Salt + response);
                    var pass2 = dbPlayer.Password;
                    if (pass == pass2)
                    {
                        dbPlayer.AccountStatus = AccountStatus.LoggedIn;

                        dbPlayer.Player.ResetData("loginStatusCheck");

                        dbPlayer.CloseUserDialog(Dialogs.menu_login);

                        //player.SetSharedData("AC_Status", true);

                        dbPlayer.Firstspawn = true;
                        PlayerSpawn.OnPlayerSpawn(player);

                        // send phone data
                        Phone.SetPlayerPhoneData(dbPlayer);
                        return;
                    }
                    else
                    {
                        dbPlayer.PassAttempts += 1;

                        if (dbPlayer.PassAttempts >= 3)
                        {
                            dbPlayer.Player.SendNotification("Sie haben ein falsches Passwort 3x eingegeben, Sicherheitskick.");
                            player.Kick("Falsches Passwort (3x)");
                            return;
                        }

                        string message = string.Format("Sie haben ein falsches Passwort eingegeben. Warnung [{0}/3]",
                            dbPlayer.PassAttempts);
                        dbPlayer.SendNewNotification(message, title: "SERVER", notificationType: PlayerNotification.NotificationType.SERVER);
                        return;
                    }
                }
            }
            else if (dialogid == Dialogs.menu_weapondealer_input)
            {
                if (!dbPlayer.HasData("sWeaponBuild")) return;
                if (!int.TryParse(input.ToString(), out var amount)) return;
                if (amount > 0 && amount < 9999)
                {
                    uint itemid = dbPlayer.GetData("sWeaponBuild");
                    //ItemData item = itemid);

                    int price = 500;//item.JobMats * 500;
                    if (!dbPlayer.TakeMoney(price))
                    {
                        dbPlayer.SendNewNotification(
                             MSG.Money.NotEnoughMoney(price));
                        return;
                    }
                    
                    dbPlayer.SendNewNotification(
                  "Sie haben sich " + amount + " ");
                    dbPlayer.PlayAnimation(AnimationScenarioType.Animation,
                        "amb@prop_human_movie_studio_light@base", "base");
                    dbPlayer.CloseUserDialog(Dialogs.menu_weapondealer_input);
                }

                return;
            }
            else if (dialogid == Dialogs.menu_ad_input)
            {
                //Moved to Window
            }
            else if (dialogid == Dialogs.menu_givemoney_input)
            {
                if (!dbPlayer.HasData("sInteraction")) return;
                if (!int.TryParse(input.ToString(), out var amount)) return;
                DbPlayer desPlayer = dbPlayer.GetData("sInteraction");
                if (!desPlayer.IsValid()) return;
                if (desPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 5.0f) return;
                dbPlayer.GiveMoneyToPlayer(desPlayer, amount);
                dbPlayer.ResetData("sInteraction");
                dbPlayer.CloseUserDialog(Dialogs.menu_givemoney_input);
                return;
            }
            else if (dialogid == Dialogs.menu_shop_input)
            {
                if (!int.TryParse(input.ToString(), out var amount)) return;
                if (!dbPlayer.TryData("sBuyItem", out uint itemid)) return;
                if (amount <= 0) return;
                dbPlayer.CloseUserDialog(Dialogs.menu_shop_input);
                return;
            }
        }
        
        public static void SendChatMessageToAll(string command, bool sponly = false)
        {
            var players = Players.Instance.GetValidPlayers();
            foreach (var player in players)
            {
                if (!player.IsValid()) continue;
                player.SendNewNotification(command);
            }
        }
    }
}