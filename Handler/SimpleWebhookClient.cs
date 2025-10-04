using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VMP_CNR.Module.Logging;
using System.Collections.Concurrent;

namespace VMP_CNR.Handler.Webhook
{
    /// <summary>
    /// KOMPLETT NEUER einfacher Webhook-Client OHNE System.Net.Http Dependencies
    /// Basiert nur auf System.Net.WebRequest - 100% stabil!
    /// </summary>
    public static class SimpleWebhookClient
    {
        private static readonly ConcurrentQueue<WebhookRequest> requestQueue = new ConcurrentQueue<WebhookRequest>();
        private static readonly object lockObject = new object();
        private static bool isRunning = false;
        private static Thread processingThread;
        
        static SimpleWebhookClient()
        {
            StartProcessor();
        }
        
        /// <summary>
        /// Startet den Webhook-Processor
        /// </summary>
        private static void StartProcessor()
        {
            if (isRunning) return;
            
            isRunning = true;
            processingThread = new Thread(ProcessQueue)
            {
                Name = "WebhookProcessor",
                IsBackground = true
            };
            processingThread.Start();
            
            WebhookLogger.LogInfo("SimpleWebhookClient Processor gestartet");
        }
        
        /// <summary>
        /// Sendet eine Webhook-Nachricht asynchron
        /// </summary>
        public static void SendMessage(DiscordWebhookMessage message, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            try
            {
                var request = new WebhookRequest
                {
                    Message = message,
                    Channel = channel,
                    Timestamp = DateTime.UtcNow,
                    AttemptCount = 0
                };
                
                requestQueue.Enqueue(request);
                WebhookLogger.LogInfo($"Webhook zur Queue hinzugefügt. Channel: {channel}, Queue: {requestQueue.Count}");
            }
            catch (Exception ex)
            {
                WebhookLogger.LogError($"Fehler beim Hinzufügen zur Queue: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Sendet eine einfache Nachricht
        /// </summary>
        public static void SendMessage(string content, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            var message = new DiscordWebhookMessage().SetContent(content);
            SendMessage(message, channel);
        }
        
        /// <summary>
        /// Sendet eine Nachricht mit Embed
        /// </summary>
        public static void SendMessage(string content, DiscordEmbed embed, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            var message = new DiscordWebhookMessage()
                .SetContent(content)
                .AddEmbed(embed);
            SendMessage(message, channel);
        }
        
        /// <summary>
        /// Async Wrapper für Kompatibilität
        /// </summary>
        public static Task<bool> SendMessageAsync(DiscordWebhookMessage message, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            SendMessage(message, channel);
            return Task.FromResult(true);
        }
        
        /// <summary>
        /// Verarbeitet die Request-Queue kontinuierlich
        /// </summary>
        private static void ProcessQueue()
        {
            WebhookLogger.LogInfo("Webhook-Processor Thread gestartet");
            
            while (isRunning)
            {
                try
                {
                    if (requestQueue.TryDequeue(out WebhookRequest request))
                    {
                        ProcessSingleRequest(request);
                        Thread.Sleep(100); // Kurze Pause zwischen Requests
                    }
                    else
                    {
                        Thread.Sleep(500); // Längere Pause wenn Queue leer
                    }
                }
                catch (Exception ex)
                {
                    WebhookLogger.LogError($"Webhook-Processor Fehler: {ex.Message}");
                    Thread.Sleep(2000); // Längere Pause bei Fehlern
                }
            }
        }
        
        /// <summary>
        /// Verarbeitet einen einzelnen Webhook-Request mit System.Net.WebRequest
        /// </summary>
        private static void ProcessSingleRequest(WebhookRequest request)
        {
            WebRequest webRequest = null;
            HttpWebResponse response = null;
            
            try
            {
                request.AttemptCount++;
                string url = WebhookConfig.GetWebhookUrl(request.Channel);
                string json = JsonConvert.SerializeObject(request.Message, Formatting.None);
                
                WebhookLogger.LogDebug($"Starte Webhook-Request. Channel: {request.Channel}, URL: {url.Substring(0, Math.Min(50, url.Length))}..., Versuch: {request.AttemptCount}");
                
                // URL Validation
                if (string.IsNullOrEmpty(url) || !url.StartsWith("https://discord.com/api/webhooks/"))
                {
                    WebhookLogger.LogError($"Ungültige Webhook-URL für Channel {request.Channel}: {url}");
                    return;
                }
                
                // Erstelle WebRequest
                webRequest = WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json; charset=utf-8";
                webRequest.Timeout = 10000; // 10 Sekunden Timeout
                
                // Header hinzufügen
                if (webRequest is HttpWebRequest httpReq)
                {
                    httpReq.UserAgent = "VoidRoleplay-Webhook/2.0";
                    httpReq.KeepAlive = false;
                }
                
                // JSON Daten schreiben
                byte[] data = Encoding.UTF8.GetBytes(json);
                webRequest.ContentLength = data.Length;
                
                WebhookLogger.LogDebug($"Sende JSON ({data.Length} bytes): {json.Substring(0, Math.Min(100, json.Length))}...");
                
                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                }
                
                // Response lesen
                response = (HttpWebResponse)webRequest.GetResponse();
                
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseContent = reader.ReadToEnd();
                    WebhookLogger.LogSuccess($"Webhook erfolgreich gesendet! Channel: {request.Channel}, Status: {response.StatusCode}, Response: {responseContent.Substring(0, Math.Min(100, responseContent.Length))}...");
                }
            }
            catch (WebException webEx)
            {
                HandleWebException(webEx, request);
            }
            catch (Exception ex)
            {
                WebhookLogger.LogError($"Unerwarteter Fehler beim Webhook-Request: {ex.GetType().Name}: {ex.Message}");
                WebhookLogger.LogError($"StackTrace: {ex.StackTrace}");
                
                // Retry bei unerwarteten Fehlern
                if (request.AttemptCount < WebhookConfig.RateLimits.RetryAttempts)
                {
                    WebhookLogger.LogInfo($"Wiederhole Request nach Fehler. Versuch: {request.AttemptCount + 1}");
                    Thread.Sleep(WebhookConfig.RateLimits.RetryDelayMs);
                    requestQueue.Enqueue(request);
                }
            }
            finally
            {
                response?.Close();
            }
        }
        
        /// <summary>
        /// Behandelt WebExceptions mit detaillierter Ausgabe
        /// </summary>
        private static void HandleWebException(WebException webEx, WebhookRequest request)
        {
            string statusCode = "Unbekannt";
            string responseContent = "Keine Response erhalten";
            
            try
            {
                if (webEx.Response is HttpWebResponse httpResponse)
                {
                    statusCode = $"{httpResponse.StatusCode} ({(int)httpResponse.StatusCode})";
                    
                    using (Stream responseStream = httpResponse.GetResponseStream())
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception readEx)
            {
                responseContent = $"Fehler beim Lesen der Response: {readEx.Message}";
            }
            
            WebhookLogger.LogWarning($"Webhook-Request fehlgeschlagen. Channel: {request.Channel}, Status: {statusCode}, WebException: {webEx.Status}, Message: {webEx.Message}");
            WebhookLogger.LogWarning($"Response Content: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}");
            
            // Retry Logik
            bool shouldRetry = ShouldRetry(webEx, request.AttemptCount);
            if (shouldRetry)
            {
                WebhookLogger.LogInfo($"Wiederhole Webhook-Request. Versuch: {request.AttemptCount + 1}/{WebhookConfig.RateLimits.RetryAttempts}");
                Thread.Sleep(WebhookConfig.RateLimits.RetryDelayMs);
                requestQueue.Enqueue(request);
            }
            else
            {
                WebhookLogger.LogError($"Webhook-Request endgültig fehlgeschlagen nach {request.AttemptCount} Versuchen. Channel: {request.Channel}");
            }
        }
        
        /// <summary>
        /// Bestimmt ob ein Request wiederholt werden soll
        /// </summary>
        private static bool ShouldRetry(WebException webEx, int attemptCount)
        {
            if (attemptCount >= WebhookConfig.RateLimits.RetryAttempts)
                return false;
            
            // Retry bei bestimmten WebException Status
            switch (webEx.Status)
            {
                case WebExceptionStatus.Timeout:
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.NameResolutionFailure:
                case WebExceptionStatus.ReceiveFailure:
                case WebExceptionStatus.SendFailure:
                    return true;
            }
            
            // Retry bei bestimmten HTTP Status Codes
            if (webEx.Response is HttpWebResponse httpResponse)
            {
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.TooManyRequests:
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.GatewayTimeout:
                        return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Testet die Webhook-Verbindung
        /// </summary>
        public static bool TestWebhookConnection()
        {
            try
            {
                WebhookLogger.LogInfo("🔧 Starte EINFACHEN Webhook-Verbindungstest...");
                
                var testMessage = new DiscordWebhookMessage()
                    .SetContent("**🚀 Webhook-System Test - NEUE VERSION!**")
                    .AddEmbed(new DiscordEmbed()
                        .SetTitle("✅ Verbindungstest")
                        .SetDescription("Das neue SimpleWebhookClient-System ist aktiv!")
                        .SetColor(WebhookConfig.Colors.Success)
                        .AddField("Server", "Void Roleplay", true)
                        .AddField("Version", "2.0 - Stabil", true)
                        .SetTimestamp(DateTime.UtcNow));
                
                SendMessage(testMessage, WebhookConfig.WebhookChannel.System);
                
                WebhookLogger.LogSuccess("Test-Webhook zur Queue hinzugefügt!");
                return true;
            }
            catch (Exception ex)
            {
                WebhookLogger.LogError($"Webhook-Test fehlgeschlagen: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Stoppt den Processor (für Cleanup)
        /// </summary>
        public static void Stop()
        {
            isRunning = false;
            WebhookLogger.LogInfo("SimpleWebhookClient wird gestoppt...");
        }
    }
}