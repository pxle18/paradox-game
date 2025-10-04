using System;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Handler.Webhook
{
    public static class DiscordWebhookService
    {
        static DiscordWebhookService()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;
            }
            catch { /* fallback silently */ }
        }

        private static string GetUrl(WebhookConfig.WebhookChannel channel)
            => WebhookConfig.GetWebhookUrl(channel);

        public static async Task<bool> SendAsync(string content, DiscordEmbed embed = null, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            try
            {
                var url = GetUrl(channel);
                using (var client = new DiscordWebhookClient(url))
                {
                    Embed discordEmbed = null;
                    if (embed != null)
                    {
                        var builder = new EmbedBuilder();
                        if (!string.IsNullOrEmpty(embed.Title)) builder.WithTitle(embed.Title);
                        if (!string.IsNullOrEmpty(embed.Description)) builder.WithDescription(embed.Description);
                        if (embed.Color.HasValue) builder.WithColor(new Color((uint)embed.Color.Value));
                        if (embed.Timestamp.HasValue) builder.WithTimestamp(new DateTimeOffset(embed.Timestamp.Value));
                        if (embed.Fields != null)
                        {
                            foreach (var f in embed.Fields)
                            {
                                builder.AddField(f.Name, f.Value, f.Inline);
                            }
                        }
                        discordEmbed = builder.Build();
                    }

                    await client.SendMessageAsync(text: content, embeds: discordEmbed != null ? new[] { discordEmbed } : null, username: "Void Roleplay");
                    WebhookLogger.LogSuccess($"Discord.Net webhook sent (channel={channel})");
                    return true;
                }
            }
            catch (Discord.Net.HttpException httpEx)
            {
                WebhookLogger.LogError($"Discord.Net HTTP error: {httpEx.HttpCode} {httpEx.Message}");
                if (httpEx.DiscordCode.HasValue) WebhookLogger.LogError($"DiscordCode: {httpEx.DiscordCode}");
                return false;
            }
            catch (Exception ex)
            {
                WebhookLogger.LogError($"Discord.Net error: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public static bool Send(string content, DiscordEmbed embed = null, WebhookConfig.WebhookChannel channel = WebhookConfig.WebhookChannel.General)
        {
            try
            {
                return SendAsync(content, embed, channel).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                WebhookLogger.LogError($"Discord.Net sync send error: {ex.Message}");
                return false;
            }
        }

        public static bool Test()
        {
            var e = new DiscordEmbed()
                .SetTitle("🚀 Test mit Discord.Net")
                .SetDescription("Webhook-Client funktioniert!")
                .SetColor(WebhookConfig.Colors.VoidTurquoise)
                .SetTimestamp(DateTime.UtcNow);
            return Send("**VOID ROLEPLAY SERVER GESTARTET**", e, WebhookConfig.WebhookChannel.System);
        }
    }
}
