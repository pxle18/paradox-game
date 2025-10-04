using GTANetworkAPI;
using System;
using System.Collections.Generic;
using VMP_CNR.Handler.Webhook;

namespace VMP_CNR.Handler
{
    /// <summary>
    /// MEGA EVENT LOGGER - Loggt WIRKLICH ALLES was auf dem Server passiert!
    /// </summary>
    public static class VoidEventLogger
    {
        // Event-Kategorien für bessere Organisation
        public enum EventCategory
        {
            PlayerAction,
            AdminAction,
            SystemEvent,
            VehicleEvent,
            ChatEvent,
            MoneyTransaction,
            ItemEvent,
            PropertyEvent,
            JobEvent,
            CrimeEvent,
            DeathEvent,
            ConnectionEvent,
            CommandEvent,
            DatabaseEvent,
            SecurityEvent
        }

        /// <summary>
        /// Loggt Player-Verbindungen
        /// </summary>
        public static void LogPlayerConnect(Player player)
        {
            var embed = new DiscordEmbed()
                .SetTitle("🟢 Player Connected")
                .SetColor(WebhookConfig.Colors.Success)
                .AddField("Spieler", $"{player.Name} ({player.SocialClubName})", true)
                .AddField("Social Club ID", player.SocialClubId.ToString(), true)
                .AddField("IP Adresse", player.Address, true)
                .AddField("Ping", player.Ping.ToString(), true)
                .AddField("Verbindungszeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter($"Player ID: {player.Id} | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.PlayerActions);
            WebhookLogger.LogActivity("Player Connect", $"{player.Name} ({player.SocialClubName}) connected");
        }

        /// <summary>
        /// Loggt Player-Disconnections
        /// </summary>
        public static void LogPlayerDisconnect(Player player, DisconnectionType type, string reason)
        {
            var embed = new DiscordEmbed()
                .SetTitle("🔴 Player Disconnected")
                .SetColor(WebhookConfig.Colors.Error)
                .AddField("Spieler", $"{player.Name} ({player.SocialClubName})", true)
                .AddField("Disconnect-Typ", type.ToString(), true)
                .AddField("Grund", reason ?? "Unbekannt", true)
                .AddField("Verbindungsdauer", CalculatePlaytime(player), true)
                .AddField("Disconnect-Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter($"Player ID: {player.Id} | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.PlayerActions);
            WebhookLogger.LogActivity("Player Disconnect", $"{player.Name} disconnected: {type} - {reason}");
        }

        /// <summary>
        /// Loggt Chat-Nachrichten
        /// </summary>
        public static void LogChatMessage(Player player, string message, string chatType = "Global")
        {
            var embed = new DiscordEmbed()
                .SetTitle("💬 Chat Message")
                .SetColor(WebhookConfig.Colors.Info)
                .AddField("Spieler", player.Name, true)
                .AddField("Chat-Typ", chatType, true)
                .AddField("Nachricht", message.Length > 1000 ? message.Substring(0, 1000) + "..." : message, false)
                .AddField("Position", $"{player.Position.X:F1}, {player.Position.Y:F1}, {player.Position.Z:F1}", true)
                .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter($"Player ID: {player.Id} | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.General);
            WebhookLogger.LogActivity("Chat Message", $"{player.Name}: {message}");
        }

        /// <summary>
        /// Loggt Command-Ausführungen
        /// </summary>
        public static void LogCommand(Player player, string command, string[] args)
        {
            var embed = new DiscordEmbed()
                .SetTitle("⚡ Command Executed")
                .SetColor(WebhookConfig.Colors.Admin)
                .AddField("Spieler", player.Name, true)
                .AddField("Command", $"/{command}", true)
                .AddField("Parameter", args.Length > 0 ? string.Join(" ", args) : "Keine", true)
                .AddField("Position", $"{player.Position.X:F1}, {player.Position.Y:F1}, {player.Position.Z:F1}", true)
                .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter($"Player ID: {player.Id} | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.AdminActions);
            WebhookLogger.LogActivity("Command", $"{player.Name} executed: /{command} {string.Join(" ", args)}");
        }

        /// <summary>
        /// Loggt Geld-Transaktionen
        /// </summary>
        public static void LogMoneyTransaction(Player player, decimal amount, string reason, string transactionType)
        {
            var embed = new DiscordEmbed()
                .SetTitle("💰 Money Transaction")
                .SetColor(WebhookConfig.Colors.Money)
                .AddField("Spieler", player.Name, true)
                .AddField("Betrag", $"${amount:N2}", true)
                .AddField("Typ", transactionType, true)
                .AddField("Grund", reason, false)
                .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter($"Player ID: {player.Id} | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.General);
            WebhookLogger.LogActivity("Money Transaction", $"{player.Name}: {transactionType} ${amount:N2} - {reason}");
        }

        /// <summary>
        /// Loggt Vehicle-Events
        /// </summary>
        public static void LogVehicleEvent(Player player, Vehicle vehicle, string eventType, string details = "")
        {
            var embed = new DiscordEmbed()
                .SetTitle("🚗 Vehicle Event")
                .SetColor(WebhookConfig.Colors.Player)
                .AddField("Spieler", player.Name, true)
                .AddField("Event-Typ", eventType, true)
                .AddField("Fahrzeug", $"{vehicle.DisplayName} ({vehicle.NumberPlate})", true)
                .AddField("Vehicle ID", vehicle.Id.ToString(), true)
                .AddField("Position", $"{vehicle.Position.X:F1}, {vehicle.Position.Y:F1}, {vehicle.Position.Z:F1}", true)
                .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true);

            if (!string.IsNullOrEmpty(details))
            {
                embed.AddField("Details", details, false);
            }

            embed.SetFooter($"Player ID: {player.Id} | Vehicle ID: {vehicle.Id} | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.General);
            WebhookLogger.LogActivity("Vehicle Event", $"{player.Name}: {eventType} - {vehicle.DisplayName} - {details}");
        }

        /// <summary>
        /// Loggt Tod-Events
        /// </summary>
        public static void LogPlayerDeath(Player player, Player killer, uint weapon)
        {
            var embed = new DiscordEmbed()
                .SetTitle("💀 Player Death")
                .SetColor(WebhookConfig.Colors.Error)
                .AddField("Verstorbener", player.Name, true)
                .AddField("Killer", killer?.Name ?? "Unbekannt", true)
                .AddField("Waffe", weapon.ToString(), true)
                .AddField("Position", $"{player.Position.X:F1}, {player.Position.Y:F1}, {player.Position.Z:F1}", true)
                .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter($"Player ID: {player.Id} | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.General);
            WebhookLogger.LogActivity("Player Death", $"{player.Name} killed by {killer?.Name ?? "Unknown"} with weapon {weapon}");
        }

        /// <summary>
        /// Loggt Admin-Aktionen
        /// </summary>
        public static void LogAdminAction(Player admin, Player target, string action, string reason = "")
        {
            var embed = new DiscordEmbed()
                .SetTitle("🛡️ Admin Action")
                .SetColor(WebhookConfig.Colors.Admin)
                .AddField("Admin", admin.Name, true)
                .AddField("Target", target?.Name ?? "System", true)
                .AddField("Aktion", action, true)
                .AddField("Grund", !string.IsNullOrEmpty(reason) ? reason : "Kein Grund angegeben", false)
                .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter($"Admin ID: {admin.Id} | Target ID: {target?.Id} | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.AdminActions);
            WebhookLogger.LogActivity("Admin Action", $"{admin.Name} -> {target?.Name}: {action} - {reason}");
        }

        /// <summary>
        /// Loggt System-Events
        /// </summary>
        public static void LogSystemEvent(string eventName, string details, EventCategory category = EventCategory.SystemEvent)
        {
            var embed = new DiscordEmbed()
                .SetTitle("⚙️ System Event")
                .SetColor(WebhookConfig.Colors.System)
                .AddField("Event", eventName, true)
                .AddField("Kategorie", category.ToString(), true)
                .AddField("Details", details, false)
                .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter("System | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.System);
            WebhookLogger.LogActivity("System Event", $"{eventName}: {details}");
        }

        /// <summary>
        /// Loggt Sicherheits-Events (Anti-Cheat, etc.)
        /// </summary>
        public static void LogSecurityEvent(Player player, string threatType, string details, int threatLevel)
        {
            var color = threatLevel >= 8 ? WebhookConfig.Colors.Error : 
                       threatLevel >= 5 ? WebhookConfig.Colors.Warning : 
                       WebhookConfig.Colors.Info;

            var embed = new DiscordEmbed()
                .SetTitle("🚨 Security Alert")
                .SetColor(color)
                .AddField("Spieler", player.Name, true)
                .AddField("Threat-Typ", threatType, true)
                .AddField("Threat-Level", $"{threatLevel}/10", true)
                .AddField("Details", details, false)
                .AddField("Position", $"{player.Position.X:F1}, {player.Position.Y:F1}, {player.Position.Z:F1}", true)
                .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter($"Player ID: {player.Id} | Threat Level: {threatLevel} | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.AdminActions);
            WebhookLogger.LogActivity("Security Alert", $"{player.Name}: {threatType} (Level {threatLevel}) - {details}");
        }

        /// <summary>
        /// Loggt Custom-Events für spezielle Module
        /// </summary>
        public static void LogCustomEvent(string title, Dictionary<string, string> fields, int color = -1, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            var embed = new DiscordEmbed()
                .SetTitle(title)
                .SetColor(color == -1 ? WebhookConfig.Colors.VoidTurquoise : color);

            foreach (var field in fields)
            {
                embed.AddField(field.Key, field.Value, true);
            }

            embed.AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                 .SetFooter("Custom Event | Void Roleplay");

            WebhookClient.SendMessage("", embed, channel);
            WebhookLogger.LogActivity("Custom Event", $"{title}: {string.Join(", ", fields.Values)}");
        }

        /// <summary>
        /// Loggt Server-Statistiken
        /// </summary>
        public static void LogServerStats()
        {
            var players = NAPI.Pools.GetAllPlayers();
            var vehicles = NAPI.Pools.GetAllVehicles();
            
            var embed = new DiscordEmbed()
                .SetTitle("📊 Server Statistics")
                .SetColor(WebhookConfig.Colors.Info)
                .AddField("Online Players", players.Count.ToString(), true)
                .AddField("Active Vehicles", vehicles.Count.ToString(), true)
                .AddField("Uptime", CalculateUptime(), true)
                .AddField("Memory Usage", GC.GetTotalMemory(false) / 1024 / 1024 + " MB", true)
                .AddField("Webhook Queue", WebhookClient.GetStats().QueueSize.ToString(), true)
                .AddField("Zeit", DateTime.Now.ToString("HH:mm:ss"), true)
                .SetFooter("Server Stats | Void Roleplay");

            WebhookClient.SendMessage("", embed, WebhookConfig.WebhookChannel.System);
            WebhookLogger.LogActivity("Server Stats", $"Players: {players.Count}, Vehicles: {vehicles.Count}");
        }

        // Hilfsmethoden
        private static string CalculatePlaytime(Player player)
        {
            // Hier würdest du die tatsächliche Spielzeit berechnen
            // Für jetzt ein Platzhalter
            return "Unbekannt";
        }

        private static string CalculateUptime()
        {
            // Hier würdest du die Server-Uptime berechnen
            // Für jetzt ein Platzhalter
            return "Unbekannt";
        }
    }
}