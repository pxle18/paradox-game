using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Handler.Webhook
{
    /// <summary>
    /// Erweiterte Webhook-Client Klasse mit Rate Limiting, Retry-Logic und vollständigem Logging
    /// </summary>
    public class WebhookClient
    {
        private static readonly WebClient webClient = new WebClient();
        private static readonly ConcurrentQueue<WebhookRequest> requestQueue = new ConcurrentQueue<WebhookRequest>();
        private static readonly SemaphoreSlim rateLimitSemaphore = new SemaphoreSlim(WebhookConfig.RateLimits.RequestsPerMinute, WebhookConfig.RateLimits.RequestsPerMinute);
        private static readonly Timer rateLimitTimer;
        private static bool isProcessing = false;

        static WebhookClient()
        {
            webClient.Headers["Content-Type"] = "application/json";
            webClient.Headers["User-Agent"] = "VoidRoleplay-Webhook/1.0";
            // WebClient wird bei jeder Anfrage neu konfiguriert für bessere Stabilität
            
            // Rate Limit Reset Timer - alle 60 Sekunden zurücksetzen
            rateLimitTimer = new Timer(ResetRateLimit, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            
            // Starte den Request Processor
            Task.Run(ProcessRequestQueue);
        }

        /// <summary>
        /// Sendet eine Webhook-Nachricht asynchron
        /// </summary>
        public static async Task<bool> SendMessageAsync(DiscordWebhookMessage message, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            var request = new WebhookRequest
            {
                Message = message,
                Channel = channel,
                Timestamp = DateTime.UtcNow,
                AttemptCount = 0
            };

            requestQueue.Enqueue(request);
            WebhookLogger.LogInfo($"Webhook-Nachricht zur Queue hinzugefügt. Channel: {channel}, Queue-Größe: {requestQueue.Count}");
            
            return true;
        }

        /// <summary>
        /// Sendet eine einfache Nachricht (Synchron für Rückwärtskompatibilität)
        /// </summary>
        public static void SendMessage(string content, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            var message = new DiscordWebhookMessage().SetContent(content);
            _ = SendMessageAsync(message, channel);
        }

        /// <summary>
        /// Sendet eine Nachricht mit Embed
        /// </summary>
        public static void SendMessage(string content, DiscordEmbed embed, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            var message = new DiscordWebhookMessage()
                .SetContent(content)
                .AddEmbed(embed);
            _ = SendMessageAsync(message, channel);
        }

        /// <summary>
        /// Verarbeitet die Request-Queue kontinuierlich
        /// </summary>
        private static async Task ProcessRequestQueue()
        {
            while (true)
            {
                try
                {
                    if (!isProcessing && requestQueue.TryDequeue(out WebhookRequest request))
                    {
                        isProcessing = true;
                        await ProcessSingleRequest(request);
                        isProcessing = false;
                    }
                    
                    await Task.Delay(100); // Kurze Pause zwischen Requests
                }
                catch (Exception ex)
                {
                    WebhookLogger.LogError($"Fehler beim Verarbeiten der Request-Queue: {ex.Message}");
                    isProcessing = false;
                    await Task.Delay(5000); // Längere Pause bei Fehlern
                }
            }
        }

        /// <summary>
        /// Verarbeitet einen einzelnen Webhook-Request
        /// </summary>
        private static async Task ProcessSingleRequest(WebhookRequest request)
        {
            // Warte auf Rate Limit Slot
            await rateLimitSemaphore.WaitAsync();
            
            try
            {
                request.AttemptCount++;
                WebhookLogger.LogDebug($"Verarbeite Webhook-Request. Channel: {request.Channel}, Versuch: {request.AttemptCount}");

                string url = WebhookConfig.GetWebhookUrl(request.Channel);
                string json = JsonConvert.SerializeObject(request.Message, Formatting.None);

                WebhookLogger.LogDebug($"Sende Webhook-Daten: {json.Substring(0, Math.Min(json.Length, 200))}...");

                try
                {
                    // Test der URL vor dem Senden
                    if (string.IsNullOrEmpty(url) || !url.StartsWith("https://discord.com/api/webhooks/"))
                    {
                        WebhookLogger.LogError($"Ungültige Webhook-URL für Channel {request.Channel}: {url}");
                        return;
                    }
                    
                    WebhookLogger.LogDebug($"Sende Webhook an URL: {url.Substring(0, Math.Min(url.Length, 50))}...");
                    
                    // Erstelle neuen WebClient für jede Anfrage (um Threading-Probleme zu vermeiden)
                    using (var localWebClient = new WebClient())
                    {
                        localWebClient.Headers["Content-Type"] = "application/json; charset=utf-8";
                        localWebClient.Headers["User-Agent"] = "VoidRoleplay-Webhook/1.0";
                        
                        byte[] data = Encoding.UTF8.GetBytes(json);
                        
                        byte[] response = await Task.Run(() => {
                            try {
                                WebhookLogger.LogDebug($"Starte UploadData zu: {url}");
                                var result = localWebClient.UploadData(url, "POST", data);
                                WebhookLogger.LogDebug($"UploadData erfolgreich, {result.Length} bytes erhalten");
                                return result;
                            }
                            catch (WebException webEx) {
                                WebhookLogger.LogError($"WebClient UploadData WebException: {webEx.Message}, Status: {webEx.Status}");
                                if (webEx.Response != null && webEx.Response is HttpWebResponse httpResp) {
                                    WebhookLogger.LogError($"HTTP Status: {httpResp.StatusCode}, Description: {httpResp.StatusDescription}");
                                }
                                throw;
                            }
                            catch (Exception generalEx) {
                                WebhookLogger.LogError($"WebClient UploadData allgemeiner Fehler: {generalEx.Message}, Type: {generalEx.GetType().Name}");
                                WebhookLogger.LogError($"StackTrace: {generalEx.StackTrace}");
                                throw;
                            }
                        });
                        
                        string responseString = Encoding.UTF8.GetString(response);
                        WebhookLogger.LogSuccess($"Webhook erfolgreich gesendet. Channel: {request.Channel}, Response-Length: {responseString.Length}");
                    }
                }
                catch (WebException webEx)
                {
                    string responseContent = "Keine Response erhalten";
                    string statusCode = "Unbekannt";
                    
                    try
                    {
                        if (webEx.Response != null)
                        {
                            if (webEx.Response is HttpWebResponse httpResponse)
                            {
                                statusCode = httpResponse.StatusCode.ToString();
                            }
                            
                            using (var reader = new System.IO.StreamReader(webEx.Response.GetResponseStream()))
                            {
                                responseContent = reader.ReadToEnd();
                            }
                        }
                    }
                    catch (Exception readEx)
                    {
                        responseContent = $"Fehler beim Lesen der Response: {readEx.Message}";
                    }
                    
                    WebhookLogger.LogWarning($"Webhook-Request fehlgeschlagen. Channel: {request.Channel}, Status: {statusCode}, Message: {webEx.Message}, Content: {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}");

                    // Retry bei bestimmten Fehlern
                    if (ShouldRetryWebException(webEx, request.AttemptCount))
                    {
                        WebhookLogger.LogInfo($"Wiederhole Webhook-Request nach {WebhookConfig.RateLimits.RetryDelayMs}ms. Versuch: {request.AttemptCount + 1}");
                        await Task.Delay(WebhookConfig.RateLimits.RetryDelayMs);
                        requestQueue.Enqueue(request);
                    }
                    else
                    {
                        WebhookLogger.LogError($"Webhook-Request endgültig fehlgeschlagen nach {request.AttemptCount} Versuchen. Channel: {request.Channel}");
                    }
                }
            }
            catch (Exception ex)
            {
                WebhookLogger.LogError($"Unerwarteter Fehler beim Senden des Webhooks: {ex.Message}\nStackTrace: {ex.StackTrace}");
                
                if (request.AttemptCount < WebhookConfig.RateLimits.RetryAttempts)
                {
                    await Task.Delay(WebhookConfig.RateLimits.RetryDelayMs);
                    requestQueue.Enqueue(request);
                }
            }
        }

        /// <summary>
        /// Bestimmt ob ein Request bei WebException wiederholt werden soll
        /// </summary>
        private static bool ShouldRetryWebException(WebException webEx, int attemptCount)
        {
            if (attemptCount >= WebhookConfig.RateLimits.RetryAttempts)
                return false;

            // Bestimmte WebException Status Codes für Retry
            if (webEx.Response is HttpWebResponse httpResponse)
            {
                return httpResponse.StatusCode == HttpStatusCode.TooManyRequests ||
                       httpResponse.StatusCode == HttpStatusCode.InternalServerError ||
                       httpResponse.StatusCode == HttpStatusCode.BadGateway ||
                       httpResponse.StatusCode == HttpStatusCode.ServiceUnavailable ||
                       httpResponse.StatusCode == HttpStatusCode.GatewayTimeout;
            }
            
            // Bei Netzwerkfehlern auch retry versuchen
            return webEx.Status == WebExceptionStatus.Timeout ||
                   webEx.Status == WebExceptionStatus.ConnectFailure ||
                   webEx.Status == WebExceptionStatus.NameResolutionFailure;
        }

        /// <summary>
        /// Setzt das Rate Limit zurück
        /// </summary>
        private static void ResetRateLimit(object state)
        {
            int currentCount = rateLimitSemaphore.CurrentCount;
            int maxCount = WebhookConfig.RateLimits.RequestsPerMinute;
            int toRelease = maxCount - currentCount;
            
            if (toRelease > 0)
            {
                rateLimitSemaphore.Release(toRelease);
                WebhookLogger.LogDebug($"Rate Limit zurückgesetzt. Verfügbare Requests: {maxCount}");
            }
        }
        
        /// <summary>
        /// Testet die Webhook-Verbindung mit einer einfachen Nachricht
        /// </summary>
        public static async Task<bool> TestWebhookConnection()
        {
            try
            {
                WebhookLogger.LogInfo("Starte Webhook-Verbindungstest...");
                
                var testEmbed = new DiscordEmbed()
                    .SetTitle("🔧 Webhook-Test")
                    .SetDescription("Verbindungstest des Discord Webhook-Systems")
                    .SetColor(WebhookConfig.Colors.Info)
                    .AddField("Server", "Void Roleplay", true)
                    .AddField("Status", "Online", true)
                    .SetTimestamp(DateTime.UtcNow);
                
                var testMessage = new DiscordWebhookMessage()
                    .SetContent("**Webhook-System Test**")
                    .AddEmbed(testEmbed);
                
                // Teste nur den System-Channel
                var request = new WebhookRequest
                {
                    Message = testMessage,
                    Channel = WebhookConfig.WebhookChannel.System,
                    Timestamp = DateTime.UtcNow,
                    AttemptCount = 0
                };
                
                WebhookLogger.LogInfo("Sende Test-Webhook...");
                await ProcessSingleRequest(request);
                
                return true;
            }
            catch (Exception ex)
            {
                WebhookLogger.LogError($"Webhook-Verbindungstest fehlgeschlagen: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gibt Statistiken über die aktuelle Queue zurück
        /// </summary>
        public static WebhookStats GetStats()
        {
            return new WebhookStats
            {
                QueueSize = requestQueue.Count,
                AvailableRateLimit = rateLimitSemaphore.CurrentCount,
                IsProcessing = isProcessing
            };
        }
    }

    /// <summary>
    /// Webhook Request Container
    /// </summary>
    internal class WebhookRequest
    {
        public DiscordWebhookMessage Message { get; set; }
        public WebhookConfig.WebhookChannel Channel { get; set; }
        public DateTime Timestamp { get; set; }
        public int AttemptCount { get; set; }
    }

    /// <summary>
    /// Webhook Statistiken
    /// </summary>
    public class WebhookStats
    {
        public int QueueSize { get; set; }
        public int AvailableRateLimit { get; set; }
        public bool IsProcessing { get; set; }
    }
}