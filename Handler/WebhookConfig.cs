using System.Collections.Generic;

namespace VMP_CNR.Handler.Webhook
{
    /// <summary>
    /// Konfiguration für alle Discord Webhooks
    /// </summary>
    public static class WebhookConfig
    {
        /// <summary>
        /// Verschiedene Webhook-Kanäle für unterschiedliche Zwecke
        /// </summary>
        public enum WebhookChannel
        {
            General,        // Captain Hook - Allgemeine Events
            PlayerActions,  // Spidey Bot - Player-spezifische Aktionen
            AdminActions,   // Spidey Bot - Admin-Aktionen  
            System         // Captain Hook - System-Events
        }

        /// <summary>
        /// Webhook-URLs mapping
        /// </summary>
        private static readonly Dictionary<WebhookChannel, string> WebhookUrls = new Dictionary<WebhookChannel, string>
        {
            { WebhookChannel.General, "https://discord.com/api/webhooks/1423324613844140032/3hi5Yeqwmtu6Umy97Wd83YbJ9-MUvIrYv3nMEGNFyN4JbswUdnbi4boK3FfiWXOxNpF1" },
            { WebhookChannel.PlayerActions, "https://discord.com/api/webhooks/1423980284495396965/XCsk0ELUvTHXnZqiTaEsY7cQoZC9-79mZ-9rnsnZmlLWezMN9pxFESO4uItQDo1ujb7N" },
            { WebhookChannel.AdminActions, "https://discord.com/api/webhooks/1423980285304897536/D81xYLuJ61wWgMpLLyHypHWgE84yXiuUuI6pAQPyQKXogH1lJT6DNC4aSqcZxH9jCvDk" },
            { WebhookChannel.System, "https://discord.com/api/webhooks/1423980285522870326/DBj-bU8EXNZMALqfPE4Y23ZFGPJ9xuu6gXGTHzbp96RK8XhbKKsQUo7jCa5DxRCX5-X5" }
        };

        /// <summary>
        /// Webhook-Namen mapping
        /// </summary>
        private static readonly Dictionary<WebhookChannel, string> WebhookNames = new Dictionary<WebhookChannel, string>
        {
            { WebhookChannel.General, "Void Roleplay" },
            { WebhookChannel.PlayerActions, "Void Roleplay" },
            { WebhookChannel.AdminActions, "Void Roleplay" },
            { WebhookChannel.System, "Void Roleplay" }
        };

        /// <summary>
        /// Standard-Farben für verschiedene Event-Typen
        /// </summary>
        public static class Colors
        {
            public const int VoidTurquoise = 0x40E0D0; // Void Roleplay Türkis (Standard)
            public const int Success = 0x00FF00;       // Grün
            public const int Info = 0x0099FF;          // Blau  
            public const int Warning = 0xFFAA00;       // Orange
            public const int Error = 0xFF0000;         // Rot
            public const int Admin = 0x9932CC;         // Lila
            public const int Player = 0x00FFFF;        // Cyan
            public const int System = 0x808080;        // Grau
            public const int Money = 0xFFD700;         // Gold
        }

        /// <summary>
        /// Hole die URL für einen bestimmten Webhook-Kanal
        /// </summary>
        public static string GetWebhookUrl(WebhookChannel channel)
        {
            return WebhookUrls.TryGetValue(channel, out string url) ? url : WebhookUrls[WebhookChannel.General];
        }

        /// <summary>
        /// Hole den Namen für einen bestimmten Webhook-Kanal
        /// </summary>
        public static string GetWebhookName(WebhookChannel channel)
        {
            return WebhookNames.TryGetValue(channel, out string name) ? name : WebhookNames[WebhookChannel.General];
        }

        /// <summary>
        /// Rate Limiting Konfiguration
        /// </summary>
        public static class RateLimits
        {
            public const int RequestsPerMinute = 30;
            public const int RetryAttempts = 3;
            public const int RetryDelayMs = 1000;
        }

        /// <summary>
        /// Timeout-Konfiguration
        /// </summary>
        public static class Timeouts
        {
            public const int RequestTimeoutMs = 10000; // 10 Sekunden
        }
    }
}