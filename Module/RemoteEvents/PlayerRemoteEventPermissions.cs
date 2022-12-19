using System;
using System.Runtime.CompilerServices;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.RemoteEvents
{
    public static class PlayerRemoteEventPermissions
    {
        public static bool CanAccessRemoteEvent(this DbPlayer dbPlayer, [CallerMemberName] string callerName = "")
        {
            if (!dbPlayer.IsValid()) return false;
            if (Configurations.Configuration.Instance.DevMode) return true;
            var methodName = callerName.ToLower();
            var remoteEventPermission = RemoteEventPermissions.Instance[methodName];
            if (remoteEventPermission == null) return true;
            if (remoteEventPermission.TeamId != null && dbPlayer.TeamId != remoteEventPermission.TeamId) return false;
            if (remoteEventPermission.PlayerRankPermission && !dbPlayer.Rank.CanAccessEvent(methodName))
                return false;
            if (!remoteEventPermission.AllowedDeath && dbPlayer.IsInjured()) return false;
            if (!remoteEventPermission.AllowedOnCuff && dbPlayer.IsCuffed) return false;
            if (!remoteEventPermission.AllowedOnTied && dbPlayer.IsTied) return false;
            return true;
        }

        public static bool CheckForSpam(this DbPlayer dbPlayer, DbPlayer.OperationType operationType)
        {
            if (!dbPlayer.IsValid())
                return false;

            if (!dbPlayer.SpamProtection.ContainsKey(operationType))
            {
                dbPlayer.SpamProtection.TryAdd(operationType, DateTime.Now);
                return true;
            }

            float antiSpamDuration = GetAntiSpamDuration(operationType);

            if (dbPlayer.SpamProtection[operationType].AddSeconds(antiSpamDuration) > DateTime.Now)
                return false;

            dbPlayer.SpamProtection[operationType] = DateTime.Now;
            return true;
        }

        private static float GetAntiSpamDuration(DbPlayer.OperationType operationType)
        {
            float duration = 2;

            switch (operationType)
            {
                case DbPlayer.OperationType.PressedJ:
                case DbPlayer.OperationType.PressedH:
                case DbPlayer.OperationType.Smartphone:
                    duration = 0.5f;
                    break;
                case DbPlayer.OperationType.BusinessCreate:
                    duration = 30;
                    break;
                case DbPlayer.OperationType.ClothesPacked:
                case DbPlayer.OperationType.ContactAdd:
                case DbPlayer.OperationType.ContactRemove:
                case DbPlayer.OperationType.ContactUpdate:
                    duration = 10;
                    break;
                case DbPlayer.OperationType.WeaponAmmoSync:
                case DbPlayer.OperationType.InventoryOpened:
                    duration = 3;
                    break;
            }

            return duration;
        }
    }
}