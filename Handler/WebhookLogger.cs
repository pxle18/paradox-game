using System;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Handler.Webhook
{
    /// <summary>
    /// Spezieller Logger für das Webhook-System
    /// </summary>
    public static class WebhookLogger
    {
        private const string PREFIX = "[WEBHOOK]";

        /// <summary>
        /// Loggt Debug-Informationen (nur in Debug-Mode)
        /// </summary>
        public static void LogDebug(string message)
        {
            #if DEBUG
            Logger.Print($"{PREFIX} [DEBUG] {message}");
            #endif
        }

        /// <summary>
        /// Loggt allgemeine Informationen
        /// </summary>
        public static void LogInfo(string message)
        {
            Logger.Print($"{PREFIX} [INFO] {message}");
        }

        /// <summary>
        /// Loggt Erfolgs-Meldungen
        /// </summary>
        public static void LogSuccess(string message)
        {
            Logger.Print($"{PREFIX} [SUCCESS] {message}");
        }

        /// <summary>
        /// Loggt Warnungen
        /// </summary>
        public static void LogWarning(string message)
        {
            Logger.Print($"{PREFIX} [WARNING] {message}");
        }

        /// <summary>
        /// Loggt Fehler
        /// </summary>
        public static void LogError(string message)
        {
            Logger.Print($"{PREFIX} [ERROR] {message}");
        }

        /// <summary>
        /// Loggt kritische Fehler
        /// </summary>
        public static void LogCritical(string message)
        {
            Logger.Print($"{PREFIX} [CRITICAL] {message}");
        }

        /// <summary>
        /// Loggt Webhook-Aktivitäten mit Zeitstempel
        /// </summary>
        public static void LogActivity(string activity, string details = "")
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logMessage = $"{PREFIX} [ACTIVITY] [{timestamp}] {activity}";
            
            if (!string.IsNullOrEmpty(details))
            {
                logMessage += $" - {details}";
            }
            
            Logger.Print(logMessage);
        }
    }
}