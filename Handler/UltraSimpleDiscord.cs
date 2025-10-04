using System;
using System.IO;
using System.Net;
using System.Text;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Handler.Webhook
{
    /// <summary>
    /// ULTRA-EINFACHES Discord System - GAR KEINE Dependencies!
    /// </summary>
    public static class UltraSimpleDiscord
    {
        // Webhook URLs direkt hier
        private const string GENERAL_WEBHOOK = "https://discord.com/api/webhooks/1423324613844140032/3hi5Yeqwmtu6Umy97Wd83YbJ9-MUvIrYv3nMEGNFyN4JbswUdnbi4boK3FfiWXOxNpF1";
        private const string SYSTEM_WEBHOOK = "https://discord.com/api/webhooks/1423980285522870326/DBj-bU8EXNZMALqfPE4Y23ZFGPJ9xuu6gXGTHzbp96RK8XhbKKsQUo7jCa5DxRCX5-X5";
        private const string ADMIN_WEBHOOK = "https://discord.com/api/webhooks/1423980285304897536/D81xYLuJ61wWgMpLLyHypHWgE84yXiuUuI6pAQPyQKXogH1lJT6DNC4aSqcZxH9jCvDk";
        
        /// <summary>
        /// Sendet einfache Message an Discord
        /// </summary>
        public static bool SendMessage(string message, string webhookUrl = GENERAL_WEBHOOK)
        {
            try
            {
                WebhookLogger.LogInfo($"Sende Message: {message}");
                
                // Erstelle einfaches JSON manually
                string json = "{\"content\":\"" + message.Replace("\"", "\\\"") + "\",\"username\":\"Void Roleplay\"}";
                
                // HTTP Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webhookUrl);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.UserAgent = "VoidRP/1.0";
                
                byte[] data = Encoding.UTF8.GetBytes(json);
                request.ContentLength = data.Length;
                
                // Schreibe Daten
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                
                // Lese Response
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    WebhookLogger.LogSuccess($"Discord Response: {response.StatusCode}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                WebhookLogger.LogError($"Discord Fehler: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Test Discord Verbindung
        /// </summary>
        public static bool Test()
        {
            return SendMessage("🚀 **VOID ROLEPLAY SERVER ONLINE** - Direktes System funktioniert!", SYSTEM_WEBHOOK);
        }
        
        /// <summary>
        /// Sende an General Channel
        /// </summary>
        public static void ToGeneral(string message)
        {
            SendMessage(message, GENERAL_WEBHOOK);
        }
        
        /// <summary>
        /// Sende an System Channel
        /// </summary>
        public static void ToSystem(string message)
        {
            SendMessage(message, SYSTEM_WEBHOOK);
        }
        
        /// <summary>
        /// Sende an Admin Channel
        /// </summary>
        public static void ToAdmin(string message)
        {
            SendMessage(message, ADMIN_WEBHOOK);
        }
    }
}