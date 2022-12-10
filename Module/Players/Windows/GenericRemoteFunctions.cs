using System;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Forum;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.LifeInvader.App;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Staatskasse;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Permission;

namespace VMP_CNR.Module.Players.Windows
{
    class GenericRemoteFunctions : Script
    {
        [RemoteEvent]
        public void kick(Player p_Player, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid())
                return;

            DatabaseLogging.Instance.LogAdminAction(p_Player, l_DbPlayer.GetName(), AdminLogTypes.kick, "AFK System", 0,
                    Configuration.Instance.DevMode);
            l_DbPlayer.SendNewNotification("Anti AFK");
            l_DbPlayer.Kick("AFK-Check nicht bestanden.");
        }

        [RemoteEvent]
        public void openAnimationMenu(Player p_Player, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null)
                return;
            if (l_DbPlayer.RageExtension.IsInVehicle || !l_DbPlayer.CanInteract()) return;

            MenuManager.Instance.Build(PlayerMenu.AnimationMenuOv, l_DbPlayer).Show(l_DbPlayer);
        }

        //Event: LifeInvader - Purchase of Ad
        [RemoteEvent]
        public void LifeInvaderPurchaseAd(Player player, string ad, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (Main.adLastSend.AddSeconds(15) > DateTime.Now)
            {
                dbPlayer.SendNewNotification(
                    
                    "Aktuell kann keine Werbung gesendet werden, bitte warte kurz!");
                return;
            }
            if (dbPlayer.IsHomeless())
            {
                dbPlayer.SendNewNotification("Ohne Wohnsitz können Sie keine Werbung schalten!");
                return;
            }
            int newsprice = 0;
            if (ad.Length < 10 || ad.Length > 96)
            {
                dbPlayer.SendNewNotification(
                    
                    "Werbungen muessen zwischen 10 und 96 Zeichen lang sein!");
                return;
            }

            newsprice = ad.Length * 5;

            if (!dbPlayer.TakeMoney(newsprice))
            {
                dbPlayer.SendNewNotification(
                     GlobalMessages.Money.NotEnoughMoney(newsprice));
                return;
            }

            //ToDo: AddToLifeInvaderAdListWithTimeStamp
            ad = ad.Replace("\"", "");
            Main.adList.Add(new LifeInvaderApp.AdsFound(dbPlayer.Id, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}", $"{ad}"));

            Main.adList.Sort(delegate (LifeInvaderApp.AdsFound x, LifeInvaderApp.AdsFound y)
            {
                return y.DateTime.CompareTo(x.DateTime);
            });

            Players.Instance.SendMessageToAuthorizedUsers("log",
                "AD gesendet von " + dbPlayer.GetName() + "(" + dbPlayer.ForumId + ")");
            dbPlayer.SendNewNotification(
                "Werbung abgesendet! Kosten: $5 / Buchstabe (insgesamt: $" +
                newsprice + ")");
            Main.sendNotificationToPlayersWhoCanReceive("Es gibt neue Werbung in der Lifeinvader App!", "Lifeinvader");
            Main.adLastSend = DateTime.Now;
            var adlog = ad.Replace("$", ":");
            Logging.Logger.LiveinvaderLog(dbPlayer.Id, dbPlayer.Player.Name, adlog);

        }

        //Event: TeamManageApp - addPlayerConfirmed after confirmation
        [RemoteEvent]
        public void addPlayerConfirmed(Player player, string invitingPersonName, string fraktion, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var editDbPlayer = Players.Instance.FindPlayer(invitingPersonName);
            if (editDbPlayer == null || !editDbPlayer.IsValid())
                return;

            var teamRankPermission = editDbPlayer.TeamRankPermission;
            if (teamRankPermission.Manage < 1)
            {
                dbPlayer.SendNewNotification("Die Person ist nicht berechtigt dich einzuladen!", title:"Fraktion", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }
            if (dbPlayer.IsHomeless())
            {
                dbPlayer.SendNewNotification("Ohne einen Wohnsitz kannst du keiner Fraktion beitreten!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
                editDbPlayer.SendNewNotification($"{ dbPlayer.GetName()} hat keinen Wohnsitz und kann daher nicht der Fraktion beitreten!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }
            if (dbPlayer.TeamId != (uint) TeamList.Zivilist)
            {
                dbPlayer.SendNewNotification("Du bist bereits in einer Fraktion!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
                editDbPlayer.SendNewNotification($"{dbPlayer.GetName()} ist bereits in einer Fraktion!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (!string.Equals(editDbPlayer.Team.Name.ToLower(), fraktion.ToLower()))
            {
                dbPlayer.SendNewNotification($"{editDbPlayer.GetName()} hat dich in die falsche Fraktion eingeladen! :(", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }
            
            dbPlayer.SetTeam(editDbPlayer.TeamId);
            dbPlayer.UpdateApps();
            dbPlayer.SynchronizeForum();

            // Rank Permissions & Rank
            dbPlayer.SetTeamRankPermission(false, 0, false, "");
            dbPlayer.TeamRank = 0;
            dbPlayer.fgehalt[0] = 0;

            dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} ist jetzt ein Mitglied - {fraktion}!");
            LogHandler.LogFactionAction(dbPlayer.Id, dbPlayer.GetName(), dbPlayer.Team.Id, true);
        }
        
    }
}
