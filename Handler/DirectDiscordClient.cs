using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Handler.Webhook
{
    /// <summary>
    /// DIREKTER Discord API Client - Einfach und funktional!
    /// </summary>
    public static class DirectDiscordClient
    {
        private static readonly object lockObj = new object();
        
        /// <summary>
        /// Sendet direkt an Discord API
        /// </summary>
        public static bool SendToDiscord(string webhookUrl, string content, DiscordEmbed embed = null)
        {
            lock (lockObj)
            {
                try
                {
                    // Erstelle Discord Payload
                    var payload = new
                    {
                        content = content,
                        username = "Void Roleplay",
                        embeds = embed != null ? new[] { embed } : null
                    };
                    
                    string json = JsonConvert.SerializeObject(payload, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    
                    WebhookLogger.LogInfo($"Sende an Discord: {content}");
                    
                    // HTTP Request direkt
                    var request = (HttpWebRequest)WebRequest.Create(webhookUrl);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.UserAgent = "VoidRP-Bot/1.0";
                    
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    request.ContentLength = data.Length;
                    
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        WebhookLogger.LogSuccess($"Discord API Response: {response.StatusCode}");
                        return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
                    }
                }
                catch (WebException webEx)
                {
                    if (webEx.Response is HttpWebResponse errorResponse)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            string error = reader.ReadToEnd();
                            WebhookLogger.LogError($"Discord API Error {errorResponse.StatusCode}: {error}");
                        }
                    }
                    else
                    {
                        WebhookLogger.LogError($"Web Error: {webEx.Message}");
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    WebhookLogger.LogError($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Einfache Message senden
        /// </summary>
        public static void SendMessage(string message, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            string url = WebhookConfig.GetWebhookUrl(channel);
            
            // Async senden um nicht zu blockieren
            ThreadPool.QueueUserWorkItem(_ => {
                try
                {
                    SendToDiscord(url, message);
                }
                catch (Exception ex)
                {
                    WebhookLogger.LogError($"Async send error: {ex.Message}");
                }
            });
        }
        
        /// <summary>
        /// Message mit Embed senden
        /// </summary>
        public static void SendMessage(string message, DiscordEmbed embed, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            string url = WebhookConfig.GetWebhookUrl(channel);
            
            ThreadPool.QueueUserWorkItem(_ => {
                try
                {
                    SendToDiscord(url, message, embed);
                }
                catch (Exception ex)
                {
                    WebhookLogger.LogError($"Async send with embed error: {ex.Message}");
                }
            });
        }
        
        /// <summary>
        /// Test senden
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                var embed = new DiscordEmbed()
                    .SetTitle("🚀 Test Erfolgreich")
                    .SetDescription("Direkter Discord API Client funktioniert!")
                    .SetColor(0x00FF00)
                    .SetTimestamp(DateTime.UtcNow);
                
                string url = WebhookConfig.GetWebhookUrl(WebhookConfig.WebhookChannel.System);
                return SendToDiscord(url, "**VOID ROLEPLAY SERVER GESTARTET**", embed);
            }
            catch (Exception ex)
            {
                WebhookLogger.LogError($"Test failed: {ex.Message}");
                return false;
            }
        }
    }
}