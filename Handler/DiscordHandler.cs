using GTANetworkAPI;
using System;
using VMP_CNR.Handler.Webhook;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Handler
{
    /// <summary>
    /// NEUER VOID ROLEPLAY DISCORD HANDLER - Komplett modernisiert!
    /// Ersetzt das alte, schlechte System durch ein modernes Webhook-System
    /// </summary>
    public class DiscordHandler
    {
        /// <summary>
        /// Sendet eine einfache Nachricht (Rückwärtskompatibilität)
        /// </summary>
        public static void SendMessage(string message, string description = "")
        {
            try
            {
                // Verwende das DIREKTE System
                if (!string.IsNullOrEmpty(description))
                {
                    // Mit Embed für Beschreibung
                    var embed = new DiscordEmbed()
                        .SetTitle("Void Roleplay")
                        .SetDescription(description)
                        .SetColor(WebhookConfig.Colors.VoidTurquoise);
                    
                    DiscordWebhookService.Send(message, embed);
                }
                else
                {
                    // Nur Content
                    DiscordWebhookService.Send(message);
                }
                
                WebhookLogger.LogActivity("Legacy Message Sent", $"Message: {message}, Description: {description}");
            }
            catch (Exception e)
            {
                Logger.Print($"[DISCORD] Fehler beim Senden der Nachricht: {e.Message}");
                WebhookLogger.LogError($"Fehler beim Senden einer Legacy-Nachricht: {e.Message}");
            }
        }

        /// <summary>
        /// NEUE METHODEN - Erweiterte Funktionalität
        /// </summary>
        
        /// <summary>
        /// Sendet eine Nachricht an einen spezifischen Kanal
        /// </summary>
        public static void SendMessage(string message, WebhookConfig.WebhookChannel channel)
        {
            DiscordWebhookService.Send(message, null, channel);
            WebhookLogger.LogActivity("Channel Message Sent", $"Channel: {channel}, Message: {message}");
        }

        /// <summary>
        /// Sendet eine Nachricht mit einem vollständigen Embed
        /// </summary>
        public static void SendEmbedMessage(string title, string description, int color = -1, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            var embed = new DiscordEmbed()
                .SetTitle(title)
                .SetDescription(description)
                .SetColor(color == -1 ? WebhookConfig.Colors.VoidTurquoise : color);
            
            DiscordWebhookService.Send("", embed, channel);
            WebhookLogger.LogActivity("Embed Message Sent", $"Title: {title}, Channel: {channel}");
        }

        /// <summary>
        /// Sendet eine Admin-Benachrichtigung
        /// </summary>
        public static void SendAdminNotification(string title, string message, Player admin = null)
        {
            var embed = new DiscordEmbed()
                .SetTitle($"🛡️ {title}")
                .SetDescription(message)
                .SetColor(WebhookConfig.Colors.Admin)
                .AddField("Zeit", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), true);

            if (admin != null)
            {
                embed.AddField("Admin", admin.Name, true);
            }

            DiscordWebhookService.Send("", embed, WebhookConfig.WebhookChannel.AdminActions);
            WebhookLogger.LogActivity("Admin Notification Sent", $"Title: {title}, Admin: {admin?.Name ?? "System"}");
        }

        /// <summary>
        /// Sendet eine Fehler-Benachrichtigung
        /// </summary>
        public static void SendErrorNotification(string title, string error, string details = "")
        {
            var embed = new DiscordEmbed()
                .SetTitle($"❌ {title}")
                .SetDescription(error)
                .SetColor(WebhookConfig.Colors.Error)
                .AddField("Zeit", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), true);

            if (!string.IsNullOrEmpty(details))
            {
                embed.AddField("Details", details, false);
            }

            DiscordWebhookService.Send("", embed, WebhookConfig.WebhookChannel.System);
            WebhookLogger.LogActivity("Error Notification Sent", $"Title: {title}, Error: {error}");
        }

        /// <summary>
        /// Sendet Server-Statistiken
        /// </summary>
        public static void SendServerStats()
        {
            VoidEventLogger.LogServerStats();
        }

        /// <summary>
        /// Initialisiert das Webhook-System (Startup-Message)
        /// </summary>
        public static void Initialize()
        {
            try
            {
                WebhookLogger.LogInfo("Void Roleplay Discord Webhook System initialisiert!");
                WebhookLogger.LogInfo($"Verfügbare Kanäle: {string.Join(", ", Enum.GetNames(typeof(WebhookConfig.WebhookChannel)))}");
                WebhookLogger.LogInfo($"Rate Limit: {WebhookConfig.RateLimits.RequestsPerMinute} Requests/Minute");
                
                Logger.Print("[DISCORD] Neues Webhook-System erfolgreich initialisiert!");
                
                // Starte DIREKTEN Discord API Test
                _ = System.Threading.Tasks.Task.Run(async () => {
                    await System.Threading.Tasks.Task.Delay(2000); // 2 Sekunden warten
                    WebhookLogger.LogInfo("🚀 === DIREKTER DISCORD API TEST ===");
                    
                    bool testResult = DirectDiscordClient.TestConnection();
                    
                    if (DiscordWebhookService.Test())
                    {
                        WebhookLogger.LogSuccess("✅ Discord.Net Webhook funktioniert!");
                        await System.Threading.Tasks.Task.Delay(1000);
                        DiscordWebhookService.Send("Server erfolgreich gestartet! 🎉", null, WebhookConfig.WebhookChannel.General);
                    }
                    else
                    {
                        WebhookLogger.LogError("❌ Discord.Net Webhook Test fehlgeschlagen!");
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Print($"[DISCORD] Fehler bei der Initialisierung: {e.Message}");
                WebhookLogger.LogError($"Initialisierung fehlgeschlagen: {e.Message}");
            }
        }

        /// <summary>
        /// Sendet Shutdown-Nachricht
        /// </summary>
        public static void SendShutdownMessage(string reason = "Geplanter Neustart")
        {
            var shutdownMessage = MessageTemplates.ServerShutdown(reason);
            DiscordEmbed embed = (shutdownMessage.Embeds != null && shutdownMessage.Embeds.Count > 0) ? shutdownMessage.Embeds[0] : null;
            DiscordWebhookService.Send(shutdownMessage.Content, embed, WebhookConfig.WebhookChannel.System);
            WebhookLogger.LogActivity("Server Shutdown Message Sent", reason);
        }
    }

    // Alte Klassen werden für Kompatibilität beibehalten, aber als deprecated markiert
    [Obsolete("Diese Klasse ist veraltet. Verwende das neue DiscordEmbed System.")]
    public class DiscordMessage
    {
        public string content { get; private set; }
        public bool tts { get; private set; }
        public EmbedObject[] embeds { get; private set; }

        public DiscordMessage(string p_Message, string p_EmbedContent)
        {
            content = p_Message;
            tts = false; // TTS standardmäßig deaktiviert

            EmbedObject l_Embed = new EmbedObject(p_EmbedContent);
            embeds = new EmbedObject[] { l_Embed };
        }
    }

    [Obsolete("Diese Klasse ist veraltet. Verwende das neue DiscordEmbed System.")]
    public class EmbedObject
    {
        public string title { get; private set; }
        public string description { get; private set; }

        public EmbedObject(string p_Desc)
        {
            title = "Void Roleplay";
            description = p_Desc;
        }
    }
}
