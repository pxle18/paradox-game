using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.RemoteEvents;

namespace VMP_CNR.Module.Players.Windows
{

    public enum TransferType
    {
        BAR=0,
        ÜBERWEISUNG=1
    }


    public class GiveMoneyWindow : Window<Func<DbPlayer, DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "name")] private string DestinationName { get; }

            public ShowEvent(DbPlayer dbPlayer, DbPlayer destinationPlayer) : base(dbPlayer)
            {
                DestinationName = destinationPlayer.GetName();
            }
        }

        public GiveMoneyWindow() : base("GiveMoney")
        {
        }

        public override Func<DbPlayer, DbPlayer, bool> Show()
        {
            return (player, destinationPlayer) => OnShow(new ShowEvent(player, destinationPlayer));
        }

        [RemoteEvent]
        public void GivePlayerMoney(Player player, string destinationPlayerName, int money, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null) return;
                if (!dbPlayer.CanAccessRemoteEvent()) return;

                if (money <= 0)
                {
                    dbPlayer.SendNewNotification("Der Betrag ist ungueltig!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var destinationDbPlayer = Players.Instance.FindPlayer(destinationPlayerName);
                if (destinationDbPlayer == null) return;
                if (!destinationDbPlayer.IsValid()) return;
                if (destinationDbPlayer.Id == dbPlayer.Id) return;
                if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 20f) return;

                if (dbPlayer.TakeMoney(money))
                {
                    if (!destinationDbPlayer.GiveMoney(money))
                    {
                        dbPlayer.GiveMoney(money);
                        dbPlayer.SendNewNotification($"Das Geld konnte nicht uebergeben werden! Du hast {money} zurueckbekommen.", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                    }
                    dbPlayer.SendNewNotification($"Du hast {MSG.Money.fnumber(money)}$ uebergeben!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    destinationDbPlayer.SendNewNotification($"Dir wurden {MSG.Money.fnumber(money)}$ zugesteckt!", notificationType: PlayerNotification.NotificationType.SUCCESS);


                    SaveToPayLog(dbPlayer.Id.ToString(), destinationDbPlayer.Id.ToString(), money, TransferType.BAR);

                    return;
                }
                dbPlayer.SendNewNotification($"Leider konnten wir dir das Geld nicht abziehen. Vorgang abgebrochen.", notificationType: PlayerNotification.NotificationType.ADMIN);
            }));
        }

        public static void SaveToPayLog(string u1, string u2, int value, TransferType transferType)
        {
            u1 = u1 ?? "undefined";
            u2 = u2 ?? "undefined";
            var query = $"INSERT INTO `paylog` (`s1`,`s2`, `amount`, `type`) VALUES ('{u1}', '{u2}','{value}','{(int)transferType}');";
            MySQLHandler.ExecuteAsync(query);
        }

    }
}