using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Handler
{
    public class DiscordHandler
    {
        private static string m_LiveWebhookURL = "https://discord.com/api/webhooks/1059284662381453362/mXuGT2pCQvT0xkvYmYMKQbZm0PcPaHXm_2hK46BVesIt1s_ty5oa1wEsOc2LFZnllgUF";

        public DiscordHandler() { }

        public static void SendMessage(string p_Message, string p_Description = "")
        {
            try
            {
                DiscordMessage l_Message = new DiscordMessage($"{p_Message}", p_Description);

                using (WebClient l_WC = new WebClient())
                {
                    l_WC.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    l_WC.Encoding = Encoding.UTF8;

                    string l_Upload = JsonConvert.SerializeObject(l_Message);
                    l_WC.UploadString(m_LiveWebhookURL, l_Upload);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
    }

    public class DiscordMessage
    {
        public string content { get; private set; }
        public bool tts { get; private set; }
        public EmbedObject[] embeds { get; private set; }

        public DiscordMessage(string p_Message, string p_EmbedContent)
        {
            content = p_Message;
            tts = true;

            EmbedObject l_Embed = new EmbedObject(p_EmbedContent);
            embeds = new EmbedObject[] { l_Embed };
        }
    }

    public class EmbedObject
    {
        public string title { get; private set; }
        public string description { get; private set; }

        public EmbedObject(string p_Desc)
        {
            title = DateTime.Now.ToString();
            description = p_Desc;
        }
    }
}
