using GTANetworkAPI;
using System;
using System.Text.RegularExpressions;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.BigDataSender;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.RemoteEvents;

namespace VMP_CNR.Module.Players.Phone
{
    public class PhoneApp : SimpleApp
    {
        public PhoneApp() : base("PhoneApp")
        {
        }

        [RemoteEvent]
        public void requestPhoneContacts(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            player.TriggerNewClientBig("responsePhoneContacts", dbPlayer.PhoneContacts.GetJson());
        }

        [RemoteEvent]
        public void updatePhoneContact(Player player, uint oldNumber, uint newNumber, string name, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.ContactUpdate)) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, name)) return;

            if (oldNumber <= 0 || oldNumber > 99999999) return;
            if (newNumber <= 0 || newNumber > 99999999) return;
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_#\s-]+$"))
            {
                dbPlayer.SendNewNotification("Kontakt konnte nicht aktualisiert werden!", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }
            dbPlayer.PhoneContacts.Update(oldNumber, newNumber, name);
        }

        [RemoteEvent]
        public void addPhoneContact(Player player, string name, string numberObj, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, name)) return;

            if (!Int32.TryParse(numberObj, out int number)) return;

            if (number <= 0 || number > 99999999) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.ContactAdd)) return;
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_#\s-]+$"))
            {
                dbPlayer.SendNewNotification("Kontakt konnte nicht eingespeichert werden.", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }

            dbPlayer.PhoneContacts.Add(name, (uint)number);
        }

        [RemoteEvent]
        public void delPhoneContact(Player player, string numberObj, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.ContactRemove)) return;

            if (!Int32.TryParse(numberObj, out int number)) return;
            if (number < 0 || number > 99999999) return;
            dbPlayer.PhoneContacts.Remove((uint)number);
        }
    }
}