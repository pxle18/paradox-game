using GTANetworkAPI;
using System;
using System.Collections.Generic;
using VMP_CNR.Handler.Webhook;

namespace VMP_CNR.Handler
{
    /// <summary>
    /// INTEGRATION GUIDE - Wie du das neue Webhook-System in ALLEN deinen Modulen verwendest
    /// BEISPIELE FÜR JEDE SITUATION!
    /// </summary>
    public static class VoidWebhookIntegration
    {
        /// <summary>
        /// PLAYER EVENTS - Alle Player-Interaktionen loggen
        /// </summary>
        public static class PlayerEvents
        {
            // Player Connect/Disconnect
            public static void OnPlayerConnect(Player player)
            {
                VoidEventLogger.LogPlayerConnect(player);
                
                // Extra: VIP Check
                if (IsVipPlayer(player))
                {
                    var vipMessage = MessageTemplates.VipWelcome(player, GetVipLevel(player));
                    WebhookClient.SendMessageAsync(vipMessage, WebhookConfig.WebhookChannel.PlayerActions);
                }
            }

            public static void OnPlayerDisconnect(Player player, DisconnectionType type, string reason)
            {
                VoidEventLogger.LogPlayerDisconnect(player, type, reason);
            }

            // Player Death
            public static void OnPlayerDeath(Player player, Player killer, uint weapon)
            {
                VoidEventLogger.LogPlayerDeath(player, killer, weapon);
            }

            // Chat Messages
            public static void OnPlayerChat(Player player, string message)
            {
                VoidEventLogger.LogChatMessage(player, message, "Global");
            }

            // Private Messages
            public static void OnPrivateMessage(Player sender, Player receiver, string message)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"📩 **Private Nachricht**")
                    .SetTitle("Private Message")
                    .SetEventColor(EventType.Player)
                    .AddField("Sender", sender.Name, true)
                    .AddField("Empfänger", receiver.Name, true)
                    .AddField("Nachricht", message, false)
                    .AddTimestamp()
                    .SetFooter("Chat System")
                    .SendTo(WebhookConfig.WebhookChannel.General);
            }

            // Player Level Up
            public static void OnPlayerLevelUp(Player player, int oldLevel, int newLevel)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"🎉 **Level Up!**")
                    .SetTitle("Player Level Up")
                    .SetEventColor(EventType.Success)
                    .AddPlayerInfo(player, false)
                    .AddField("Altes Level", oldLevel.ToString(), true)
                    .AddField("Neues Level", newLevel.ToString(), true)
                    .AddTimestamp()
                    .SetFooter("Character System")
                    .SendTo(WebhookConfig.WebhookChannel.PlayerActions);
            }
        }

        /// <summary>
        /// COMMAND EVENTS - Alle Commands loggen
        /// </summary>
        public static class CommandEvents
        {
            public static void OnCommandExecuted(Player player, string command, string[] args)
            {
                VoidEventLogger.LogCommand(player, command, args);
            }

            // Admin Commands speziell
            public static void OnAdminCommand(Player admin, string command, string[] args, Player target = null)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"👑 **Admin Command**")
                    .SetTitle("Admin Command Executed")
                    .SetEventColor(EventType.Admin)
                    .AddField("Admin", admin.Name, true)
                    .AddField("Command", $"/{command}", true)
                    .AddField("Parameter", args.Length > 0 ? string.Join(" ", args) : "Keine", true)
                    .AddField("Target", target?.Name ?? "Keiner", true)
                    .AddTimestamp()
                    .SetFooter("Admin System")
                    .SendTo(WebhookConfig.WebhookChannel.AdminActions);
            }
        }

        /// <summary>
        /// MONEY EVENTS - Alle Geld-Transaktionen
        /// </summary>
        public static class MoneyEvents
        {
            public static void OnMoneyGiven(Player player, decimal amount, string reason)
            {
                VoidEventLogger.LogMoneyTransaction(player, amount, reason, "Erhalten");
                
                // Bei großen Beträgen extra Alert
                if (amount > 50000)
                {
                    VoidMessageBuilder.Create()
                        .SetContent($"💸 **Große Geld-Transaktion!**")
                        .SetTitle("Large Money Transaction")
                        .SetColor(WebhookConfig.Colors.Money)
                        .AddPlayerInfo(player, false)
                        .AddField("Betrag", $"${amount:N2}", true)
                        .AddField("Grund", reason, false)
                        .AddField("Status", amount > 100000 ? "🔴 Verdächtig" : "🟡 Überwachung", true)
                        .AddTimestamp()
                        .SetFooter("Money Monitoring")
                        .SendTo(WebhookConfig.WebhookChannel.AdminActions);
                }
            }

            public static void OnMoneyTaken(Player player, decimal amount, string reason)
            {
                VoidEventLogger.LogMoneyTransaction(player, -amount, reason, "Entfernt");
            }

            public static void OnPlayerTrade(Player seller, Player buyer, string item, decimal price)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"🤝 **Player Trade**")
                    .SetTitle("Trade Transaction")
                    .SetEventColor(EventType.Money)
                    .AddField("Verkäufer", seller.Name, true)
                    .AddField("Käufer", buyer.Name, true)
                    .AddField("Item", item, true)
                    .AddField("Preis", $"${price:N2}", true)
                    .AddTimestamp()
                    .SetFooter("Trade System")
                    .SendTo(WebhookConfig.WebhookChannel.General);
            }
        }

        /// <summary>
        /// VEHICLE EVENTS - Alle Fahrzeug-Interaktionen
        /// </summary>
        public static class VehicleEvents
        {
            public static void OnPlayerEnterVehicle(Player player, Vehicle vehicle)
            {
                VoidEventLogger.LogVehicleEvent(player, vehicle, "Eingestiegen");
            }

            public static void OnPlayerExitVehicle(Player player, Vehicle vehicle)
            {
                VoidEventLogger.LogVehicleEvent(player, vehicle, "Ausgestiegen");
            }

            public static void OnVehiclePurchase(Player player, string vehicleName, decimal price)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"🚗 **Fahrzeug gekauft!**")
                    .SetTitle("Vehicle Purchase")
                    .SetEventColor(EventType.Money)
                    .AddPlayerInfo(player, false)
                    .AddField("Fahrzeug", vehicleName, true)
                    .AddField("Preis", $"${price:N2}", true)
                    .AddTimestamp()
                    .SetFooter("Vehicle System")
                    .SendTo(WebhookConfig.WebhookChannel.General);
            }

            public static void OnVehicleDestroyed(Player player, Vehicle vehicle, string reason)
            {
                VoidEventLogger.LogVehicleEvent(player, vehicle, "Zerstört", reason);
            }
        }

        /// <summary>
        /// PROPERTY EVENTS - Immobilien-System
        /// </summary>
        public static class PropertyEvents
        {
            public static void OnPropertyPurchase(Player player, string propertyName, decimal price)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"🏠 **Immobilie gekauft!**")
                    .SetTitle("Property Purchase")
                    .SetEventColor(EventType.Money)
                    .AddPlayerInfo(player, false)
                    .AddField("Immobilie", propertyName, true)
                    .AddField("Preis", $"${price:N2}", true)
                    .AddTimestamp()
                    .SetFooter("Property System")
                    .SendTo(WebhookConfig.WebhookChannel.General);
            }

            public static void OnPropertySold(Player player, string propertyName, decimal price)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"🏠 **Immobilie verkauft!**")
                    .SetTitle("Property Sale")
                    .SetEventColor(EventType.Info)
                    .AddPlayerInfo(player, false)
                    .AddField("Immobilie", propertyName, true)
                    .AddField("Verkaufspreis", $"${price:N2}", true)
                    .AddTimestamp()
                    .SetFooter("Property System")
                    .SendTo(WebhookConfig.WebhookChannel.General);
            }
        }

        /// <summary>
        /// JOB EVENTS - Job-System
        /// </summary>
        public static class JobEvents
        {
            public static void OnJobStarted(Player player, string jobName)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"💼 **Job gestartet**")
                    .SetTitle("Job Started")
                    .SetEventColor(EventType.Info)
                    .AddPlayerInfo(player, false)
                    .AddField("Job", jobName, true)
                    .AddTimestamp()
                    .SetFooter("Job System")
                    .SendTo(WebhookConfig.WebhookChannel.General);
            }

            public static void OnJobCompleted(Player player, string jobName, decimal payment)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"✅ **Job abgeschlossen**")
                    .SetTitle("Job Completed")
                    .SetEventColor(EventType.Success)
                    .AddPlayerInfo(player, false)
                    .AddField("Job", jobName, true)
                    .AddField("Bezahlung", $"${payment:N2}", true)
                    .AddTimestamp()
                    .SetFooter("Job System")
                    .SendTo(WebhookConfig.WebhookChannel.General);
            }
        }

        /// <summary>
        /// CRIME EVENTS - Verbrechen/Polizei-System
        /// </summary>
        public static class CrimeEvents
        {
            public static void OnCrimeCommitted(Player player, string crimeType, int wantedLevel)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"🚨 **Verbrechen begangen!**")
                    .SetTitle("Crime Committed")
                    .SetEventColor(EventType.Warning)
                    .AddPlayerInfo(player)
                    .AddField("Verbrechen", crimeType, true)
                    .AddField("Wanted Level", wantedLevel.ToString(), true)
                    .AddTimestamp()
                    .SetFooter("Crime System")
                    .SendTo(WebhookConfig.WebhookChannel.General);
            }

            public static void OnPlayerArrested(Player criminal, Player cop, string reason)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"👮 **Verhaftung!**")
                    .SetTitle("Player Arrested")
                    .SetEventColor(EventType.Admin)
                    .AddField("Verbrecher", criminal.Name, true)
                    .AddField("Polizist", cop.Name, true)
                    .AddField("Grund", reason, false)
                    .AddTimestamp()
                    .SetFooter("Police System")
                    .SendTo(WebhookConfig.WebhookChannel.General);
            }
        }

        /// <summary>
        /// ANTI-CHEAT EVENTS - Sicherheitssystem
        /// </summary>
        public static class AntiCheatEvents
        {
            public static void OnSuspiciousActivity(Player player, string cheatType, int confidence, string details)
            {
                VoidEventLogger.LogSecurityEvent(player, cheatType, details, confidence);
                
                // Bei hoher Sicherheit sofort an Admins
                if (confidence >= 80)
                {
                    var hackerReport = MessageTemplates.HackerReport(player, cheatType, details, confidence);
                    WebhookClient.SendMessageAsync(hackerReport, WebhookConfig.WebhookChannel.AdminActions);
                }
            }

            public static void OnPlayerBanned(Player player, Player admin, string reason, int duration)
            {
                VoidMessageBuilder.Create()
                    .SetContent($"🔨 **Player gebannt!**")
                    .SetTitle("Player Banned")
                    .SetEventColor(EventType.Error)
                    .AddField("Spieler", $"{player.Name} ({player.SocialClubName})", true)
                    .AddField("Admin", admin?.Name ?? "Anti-Cheat", true)
                    .AddField("Grund", reason, false)
                    .AddField("Dauer", duration == 0 ? "Permanent" : $"{duration} Tage", true)
                    .AddTimestamp()
                    .SetFooter("Ban System")
                    .SendTo(WebhookConfig.WebhookChannel.AdminActions);
            }
        }

        /// <summary>
        /// DATABASE EVENTS - Datenbank-Aktivitäten
        /// </summary>
        public static class DatabaseEvents
        {
            public static void OnDatabaseError(string query, string error)
            {
                VoidEventLogger.LogSystemEvent("Database Error", $"Query: {query}, Error: {error}", VoidEventLogger.EventCategory.DatabaseEvent);
            }

            public static void OnSlowQuery(string query, double executionTime)
            {
                if (executionTime > 5.0) // Über 5 Sekunden
                {
                    VoidMessageBuilder.Create()
                        .SetContent($"⚠️ **Langsame Datenbank-Query!**")
                        .SetTitle("Slow Database Query")
                        .SetEventColor(EventType.Warning)
                        .AddField("Ausführungszeit", $"{executionTime:F2}s", true)
                        .AddField("Query", query.Length > 500 ? query.Substring(0, 500) + "..." : query, false)
                        .AddTimestamp()
                        .SetFooter("Database Monitor")
                        .SendTo(WebhookConfig.WebhookChannel.System);
                }
            }
        }

        /// <summary>
        /// SERVER EVENTS - Server-Management
        /// </summary>
        public static class ServerEvents
        {
            public static void OnServerRestart(string reason)
            {
                DiscordHandler.SendShutdownMessage(reason);
            }

            public static void OnHighPlayerCount(int playerCount, int maxPlayers)
            {
                if (playerCount >= maxPlayers * 0.9) // 90% Auslastung
                {
                    VoidMessageBuilder.Create()
                        .SetContent($"📈 **Hohe Serverauslastung!**")
                        .SetTitle("High Server Load")
                        .SetEventColor(EventType.Warning)
                        .AddField("Online Spieler", playerCount.ToString(), true)
                        .AddField("Max Spieler", maxPlayers.ToString(), true)
                        .AddField("Auslastung", $"{(playerCount / (double)maxPlayers * 100):F1}%", true)
                        .AddTimestamp()
                        .SetFooter("Server Monitor")
                        .SendTo(WebhookConfig.WebhookChannel.System);
                }
            }

            // Milestone Events
            public static void OnPlayerMilestone(int playerCount)
            {
                if (playerCount % 50 == 0 && playerCount > 0) // Jeder 50. Spieler
                {
                    var milestone = MessageTemplates.Milestone(
                        $"{playerCount} Spieler Online!",
                        $"Void Roleplay hat einen neuen Rekord erreicht!",
                        playerCount
                    );
                    WebhookClient.SendMessageAsync(milestone, WebhookConfig.WebhookChannel.System);
                }
            }
        }

        // Hilfsmethoden (Beispiele)
        private static bool IsVipPlayer(Player player)
        {
            // Hier würdest du deinen VIP-Check implementieren
            return false;
        }

        private static string GetVipLevel(Player player)
        {
            // Hier würdest du das VIP-Level holen
            return "Gold";
        }
    }

    /// <summary>
    /// QUICK-INTEGRATION für bestehende Module
    /// Einfach diese Methoden in deine bestehenden Events einbauen!
    /// </summary>
    public static class QuickIntegration
    {
        /// <summary>
        /// Füge diese Zeile zu deinem PlayerConnect Event hinzu:
        /// VoidWebhookIntegration.PlayerEvents.OnPlayerConnect(player);
        /// </summary>
        
        /// <summary>
        /// Füge diese Zeile zu deinem PlayerDisconnect Event hinzu:
        /// VoidWebhookIntegration.PlayerEvents.OnPlayerDisconnect(player, type, reason);
        /// </summary>
        
        /// <summary>
        /// Für Admin-Aktionen:
        /// VoidWebhookIntegration.CommandEvents.OnAdminCommand(admin, "ban", new[] { target.Name, reason }, target);
        /// </summary>
        
        /// <summary>
        /// Für Geld-Transaktionen:
        /// VoidWebhookIntegration.MoneyEvents.OnMoneyGiven(player, amount, "Payday");
        /// </summary>

        /// <summary>
        /// Custom Events - für spezielle Situationen:
        /// </summary>
        public static void LogCustomEvent(string title, Dictionary<string, string> data, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            VoidEventLogger.LogCustomEvent(title, data, -1, channel);
        }
    }
}