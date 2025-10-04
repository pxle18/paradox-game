using GTANetworkAPI;
using System;
using System.Collections.Generic;
using VMP_CNR.Handler.Webhook;

namespace VMP_CNR.Handler
{
    /// <summary>
    /// Vorgefertigte Message-Templates für verschiedene Event-Typen
    /// </summary>
    public static class MessageTemplates
    {
        /// <summary>
        /// Server-Start Nachricht
        /// </summary>
        public static DiscordWebhookMessage ServerStartup()
        {
            return new DiscordWebhookMessage()
                .SetContent("🟢 **Void Roleplay Server gestartet!**")
                .AddEmbed(new DiscordEmbed()
                    .SetTitle("🚀 Server Online")
                    .SetColor(WebhookConfig.Colors.Success)
                    .AddField("Status", "Online und bereit für Spieler", false)
                    .AddField("Startzeit", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), true)
                    .AddField("Version", "Latest", true)
                    .SetFooter("Void Roleplay | Server Management")
                    .SetThumbnail("https://cdn.discordapp.com/emojis/123456789.png")); // Hier kannst du ein Logo einfügen
        }

        /// <summary>
        /// Server-Shutdown Nachricht
        /// </summary>
        public static DiscordWebhookMessage ServerShutdown(string reason = "Geplanter Neustart")
        {
            return new DiscordWebhookMessage()
                .SetContent("🔴 **Void Roleplay Server wird heruntergefahren...**")
                .AddEmbed(new DiscordEmbed()
                    .SetTitle("🛑 Server Shutdown")
                    .SetColor(WebhookConfig.Colors.Warning)
                    .AddField("Grund", reason, false)
                    .AddField("Shutdown-Zeit", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), true)
                    .AddField("Erwartete Downtime", "5-10 Minuten", true)
                    .SetFooter("Void Roleplay | Server Management"));
        }

        /// <summary>
        /// Milestone-Achievement Template (z.B. 100. Spieler, etc.)
        /// </summary>
        public static DiscordWebhookMessage Milestone(string title, string description, int playerCount = 0)
        {
            return new DiscordWebhookMessage()
                .SetContent($"🎉 **{title}**")
                .AddEmbed(new DiscordEmbed()
                    .SetTitle("🏆 Milestone Erreicht!")
                    .SetColor(WebhookConfig.Colors.VoidTurquoise)
                    .AddField("Achievement", title, false)
                    .AddField("Beschreibung", description, false)
                    .AddField("Aktuelle Spieler", playerCount > 0 ? playerCount.ToString() : "N/A", true)
                    .AddField("Zeit", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), true)
                    .SetFooter("Void Roleplay | Achievements"));
        }

        /// <summary>
        /// Hacker/Cheater Bericht Template
        /// </summary>
        public static DiscordWebhookMessage HackerReport(Player player, string cheatType, string evidence, int confidenceLevel)
        {
            var color = confidenceLevel >= 90 ? WebhookConfig.Colors.Error :
                       confidenceLevel >= 70 ? WebhookConfig.Colors.Warning :
                       WebhookConfig.Colors.Info;

            return new DiscordWebhookMessage()
                .SetContent($"🚨 **ANTI-CHEAT ALERT**")
                .AddEmbed(new DiscordEmbed()
                    .SetTitle("⚠️ Verdächtige Aktivität Erkannt")
                    .SetColor(color)
                    .AddField("Spieler", $"{player.Name} ({player.SocialClubName})", true)
                    .AddField("Social Club ID", player.SocialClubId.ToString(), true)
                    .AddField("Cheat-Typ", cheatType, true)
                    .AddField("Vertrauen", $"{confidenceLevel}%", true)
                    .AddField("Position", $"{player.Position.X:F1}, {player.Position.Y:F1}, {player.Position.Z:F1}", true)
                    .AddField("Ping", player.Ping.ToString(), true)
                    .AddField("Evidence", evidence, false)
                    .SetFooter($"Player ID: {player.Id} | Anti-Cheat System"));
        }

        /// <summary>
        /// Großer Money-Transfer Template (für verdächtige Transaktionen)
        /// </summary>
        public static DiscordWebhookMessage LargeMoneyTransfer(Player sender, Player receiver, decimal amount, string method)
        {
            return new DiscordWebhookMessage()
                .SetContent($"💸 **Große Geldüberweisung erkannt!**")
                .AddEmbed(new DiscordEmbed()
                    .SetTitle("💰 Große Transaktion")
                    .SetColor(WebhookConfig.Colors.Money)
                    .AddField("Sender", sender?.Name ?? "System", true)
                    .AddField("Empfänger", receiver?.Name ?? "System", true)
                    .AddField("Betrag", $"${amount:N2}", true)
                    .AddField("Methode", method, true)
                    .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                    .AddField("Status", amount > 100000 ? "🔴 Verdächtig" : "🟡 Überwachung", true)
                    .SetFooter($"Money Monitoring | Void Roleplay"));
        }

        /// <summary>
        /// VIP-Spieler Willkommensnachricht
        /// </summary>
        public static DiscordWebhookMessage VipWelcome(Player player, string vipLevel)
        {
            return new DiscordWebhookMessage()
                .SetContent($"👑 **VIP-Spieler online!**")
                .AddEmbed(new DiscordEmbed()
                    .SetTitle("💎 VIP Connected")
                    .SetColor(WebhookConfig.Colors.Money)
                    .AddField("VIP-Spieler", player.Name, true)
                    .AddField("VIP-Level", vipLevel, true)
                    .AddField("Social Club", player.SocialClubName, true)
                    .AddField("Verbindungszeit", DateTime.Now.ToString("HH:mm:ss"), true)
                    .SetFooter($"Player ID: {player.Id} | VIP System"));
        }
    }

    /// <summary>
    /// Flexibler Message Builder für Custom Messages
    /// </summary>
    public class VoidMessageBuilder
    {
        private DiscordWebhookMessage _message;
        private DiscordEmbed _embed;

        public VoidMessageBuilder()
        {
            _message = new DiscordWebhookMessage();
            _embed = new DiscordEmbed();
        }

        /// <summary>
        /// Startet einen neuen Message Builder
        /// </summary>
        public static VoidMessageBuilder Create()
        {
            return new VoidMessageBuilder();
        }

        /// <summary>
        /// Setzt den Hauptinhalt der Nachricht
        /// </summary>
        public VoidMessageBuilder SetContent(string content)
        {
            _message.SetContent(content);
            return this;
        }

        /// <summary>
        /// Setzt den Titel des Embeds
        /// </summary>
        public VoidMessageBuilder SetTitle(string title)
        {
            _embed.SetTitle(title);
            return this;
        }

        /// <summary>
        /// Setzt die Beschreibung des Embeds
        /// </summary>
        public VoidMessageBuilder SetDescription(string description)
        {
            _embed.SetDescription(description);
            return this;
        }

        /// <summary>
        /// Setzt die Farbe basierend auf Event-Typ
        /// </summary>
        public VoidMessageBuilder SetEventColor(EventType eventType)
        {
            int color;
            switch (eventType)
            {
                case EventType.Success:
                    color = WebhookConfig.Colors.Success;
                    break;
                case EventType.Error:
                    color = WebhookConfig.Colors.Error;
                    break;
                case EventType.Warning:
                    color = WebhookConfig.Colors.Warning;
                    break;
                case EventType.Info:
                    color = WebhookConfig.Colors.Info;
                    break;
                case EventType.Admin:
                    color = WebhookConfig.Colors.Admin;
                    break;
                case EventType.Player:
                    color = WebhookConfig.Colors.Player;
                    break;
                case EventType.System:
                    color = WebhookConfig.Colors.System;
                    break;
                case EventType.Money:
                    color = WebhookConfig.Colors.Money;
                    break;
                default:
                    color = WebhookConfig.Colors.VoidTurquoise;
                    break;
            }

            _embed.SetColor(color);
            return this;
        }

        /// <summary>
        /// Setzt eine Custom-Farbe
        /// </summary>
        public VoidMessageBuilder SetColor(int color)
        {
            _embed.SetColor(color);
            return this;
        }

        /// <summary>
        /// Fügt ein Feld hinzu
        /// </summary>
        public VoidMessageBuilder AddField(string name, string value, bool inline = false)
        {
            _embed.AddField(name, value, inline);
            return this;
        }

        /// <summary>
        /// Fügt Player-Informationen als Felder hinzu
        /// </summary>
        public VoidMessageBuilder AddPlayerInfo(Player player, bool includePosition = true)
        {
            AddField("Spieler", player.Name, true);
            AddField("Social Club", player.SocialClubName, true);
            AddField("Player ID", player.Id.ToString(), true);
            
            if (includePosition)
            {
                AddField("Position", $"{player.Position.X:F1}, {player.Position.Y:F1}, {player.Position.Z:F1}", true);
            }
            
            return this;
        }

        /// <summary>
        /// Fügt Zeitstempel hinzu
        /// </summary>
        public VoidMessageBuilder AddTimestamp()
        {
            AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true);
            return this;
        }

        /// <summary>
        /// Setzt den Footer
        /// </summary>
        public VoidMessageBuilder SetFooter(string text)
        {
            _embed.SetFooter($"{text} | Void Roleplay");
            return this;
        }

        /// <summary>
        /// Setzt ein Thumbnail
        /// </summary>
        public VoidMessageBuilder SetThumbnail(string url)
        {
            _embed.SetThumbnail(url);
            return this;
        }

        /// <summary>
        /// Setzt ein Image
        /// </summary>
        public VoidMessageBuilder SetImage(string url)
        {
            _embed.SetImage(url);
            return this;
        }

        /// <summary>
        /// Aktiviert Text-to-Speech
        /// </summary>
        public VoidMessageBuilder EnableTTS()
        {
            _message.EnableTextToSpeech(true);
            return this;
        }

        /// <summary>
        /// Baut die finale Nachricht
        /// </summary>
        public DiscordWebhookMessage Build()
        {
            _message.AddEmbed(_embed);
            return _message;
        }

        /// <summary>
        /// Baut und sendet die Nachricht direkt
        /// </summary>
        public void SendTo(WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            var message = Build();
            WebhookClient.SendMessageAsync(message, channel);
        }
    }

    /// <summary>
    /// Event-Typen für die Farbzuordnung
    /// </summary>
    public enum EventType
    {
        Success,
        Error,
        Warning,
        Info,
        Admin,
        Player,
        System,
        Money
    }

    /// <summary>
    /// Vorgefertigte Quick-Templates für häufige Events
    /// </summary>
    public static class QuickTemplates
    {
        /// <summary>
        /// Schnelle Player Connect Nachricht
        /// </summary>
        public static void PlayerConnect(Player player)
        {
            VoidMessageBuilder.Create()
                .SetContent($"🟢 **{player.Name} ist dem Server beigetreten!**")
                .SetTitle("Player Connected")
                .SetEventColor(EventType.Success)
                .AddPlayerInfo(player, false)
                .AddField("IP", player.Address, true)
                .AddField("Ping", player.Ping.ToString(), true)
                .AddTimestamp()
                .SetFooter("Player Management")
                .SendTo(WebhookConfig.WebhookChannel.PlayerActions);
        }

        /// <summary>
        /// Schnelle Admin-Aktion
        /// </summary>
        public static void AdminAction(Player admin, string action, Player target = null, string reason = "")
        {
            var builder = VoidMessageBuilder.Create()
                .SetContent($"🛡️ **Admin-Aktion ausgeführt**")
                .SetTitle("Admin Action")
                .SetEventColor(EventType.Admin)
                .AddField("Admin", admin.Name, true)
                .AddField("Aktion", action, true);

            if (target != null)
                builder.AddField("Target", target.Name, true);

            if (!string.IsNullOrEmpty(reason))
                builder.AddField("Grund", reason, false);

            builder.AddTimestamp()
                   .SetFooter("Admin Management")
                   .SendTo(WebhookConfig.WebhookChannel.AdminActions);
        }

        /// <summary>
        /// Schnelle Fehler-Meldung
        /// </summary>
        public static void Error(string title, string description, string details = "")
        {
            var builder = VoidMessageBuilder.Create()
                .SetContent($"❌ **Fehler aufgetreten**")
                .SetTitle(title)
                .SetEventColor(EventType.Error)
                .SetDescription(description);

            if (!string.IsNullOrEmpty(details))
                builder.AddField("Details", details, false);

            builder.AddTimestamp()
                   .SetFooter("Error System")
                   .SendTo(WebhookConfig.WebhookChannel.System);
        }
    }
}