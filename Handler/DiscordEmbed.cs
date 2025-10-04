using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VMP_CNR.Handler.Webhook
{
    /// <summary>
    /// Vollständige Discord Embed Implementation
    /// </summary>
    public class DiscordEmbed
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; }

        [JsonProperty("color")]
        public int? Color { get; set; }

        [JsonProperty("footer")]
        public EmbedFooter Footer { get; set; }

        [JsonProperty("image")]
        public EmbedImage Image { get; set; }

        [JsonProperty("thumbnail")]
        public EmbedThumbnail Thumbnail { get; set; }

        [JsonProperty("author")]
        public EmbedAuthor Author { get; set; }

        [JsonProperty("fields")]
        public List<EmbedField> Fields { get; set; }

        public DiscordEmbed()
        {
            Fields = new List<EmbedField>();
            Timestamp = DateTime.UtcNow;
            Color = WebhookConfig.Colors.VoidTurquoise; // Standard Void Roleplay Farbe
        }

        /// <summary>
        /// Setzt den Titel des Embeds
        /// </summary>
        public DiscordEmbed SetTitle(string title)
        {
            Title = title?.Length > 256 ? title.Substring(0, 256) : title;
            return this;
        }

        /// <summary>
        /// Setzt die Beschreibung des Embeds
        /// </summary>
        public DiscordEmbed SetDescription(string description)
        {
            Description = description?.Length > 4096 ? description.Substring(0, 4096) : description;
            return this;
        }

        /// <summary>
        /// Setzt die Farbe des Embeds
        /// </summary>
        public DiscordEmbed SetColor(int color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Setzt den Zeitstempel des Embeds
        /// </summary>
        public DiscordEmbed SetTimestamp(DateTime timestamp)
        {
            Timestamp = timestamp;
            return this;
        }

        /// <summary>
        /// Fügt ein Feld zum Embed hinzu
        /// </summary>
        public DiscordEmbed AddField(string name, string value, bool inline = false)
        {
            if (Fields.Count >= 25) return this; // Discord Limit

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            {
                Fields.Add(new EmbedField
                {
                    Name = name.Length > 256 ? name.Substring(0, 256) : name,
                    Value = value.Length > 1024 ? value.Substring(0, 1024) : value,
                    Inline = inline
                });
            }
            return this;
        }

        /// <summary>
        /// Setzt den Footer des Embeds
        /// </summary>
        public DiscordEmbed SetFooter(string text, string iconUrl = null)
        {
            Footer = new EmbedFooter
            {
                Text = text?.Length > 2048 ? text.Substring(0, 2048) : text,
                IconUrl = iconUrl
            };
            return this;
        }

        /// <summary>
        /// Setzt den Author des Embeds
        /// </summary>
        public DiscordEmbed SetAuthor(string name, string url = null, string iconUrl = null)
        {
            Author = new EmbedAuthor
            {
                Name = name?.Length > 256 ? name.Substring(0, 256) : name,
                Url = url,
                IconUrl = iconUrl
            };
            return this;
        }

        /// <summary>
        /// Setzt das Thumbnail des Embeds
        /// </summary>
        public DiscordEmbed SetThumbnail(string url)
        {
            Thumbnail = new EmbedThumbnail { Url = url };
            return this;
        }

        /// <summary>
        /// Setzt das Bild des Embeds
        /// </summary>
        public DiscordEmbed SetImage(string url)
        {
            Image = new EmbedImage { Url = url };
            return this;
        }
    }

    /// <summary>
    /// Discord Embed Field
    /// </summary>
    public class EmbedField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("inline")]
        public bool Inline { get; set; }
    }

    /// <summary>
    /// Discord Embed Footer
    /// </summary>
    public class EmbedFooter
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
    }

    /// <summary>
    /// Discord Embed Image
    /// </summary>
    public class EmbedImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// Discord Embed Thumbnail
    /// </summary>
    public class EmbedThumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// Discord Embed Author
    /// </summary>
    public class EmbedAuthor
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
    }

    /// <summary>
    /// Discord Webhook Message mit vollständiger Embed-Unterstützung
    /// </summary>
    public class DiscordWebhookMessage
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("tts")]
        public bool TextToSpeech { get; set; }

        [JsonProperty("embeds")]
        public List<DiscordEmbed> Embeds { get; set; }

        public DiscordWebhookMessage()
        {
            Embeds = new List<DiscordEmbed>();
            Username = "Void Roleplay";
            TextToSpeech = false;
        }

        /// <summary>
        /// Setzt den Content der Nachricht
        /// </summary>
        public DiscordWebhookMessage SetContent(string content)
        {
            Content = content?.Length > 2000 ? content.Substring(0, 2000) : content;
            return this;
        }

        /// <summary>
        /// Setzt den Username für den Webhook
        /// </summary>
        public DiscordWebhookMessage SetUsername(string username)
        {
            Username = username;
            return this;
        }

        /// <summary>
        /// Setzt die Avatar URL für den Webhook
        /// </summary>
        public DiscordWebhookMessage SetAvatarUrl(string avatarUrl)
        {
            AvatarUrl = avatarUrl;
            return this;
        }

        /// <summary>
        /// Fügt ein Embed zur Nachricht hinzu
        /// </summary>
        public DiscordWebhookMessage AddEmbed(DiscordEmbed embed)
        {
            if (Embeds.Count < 10 && embed != null) // Discord Limit
            {
                Embeds.Add(embed);
            }
            return this;
        }

        /// <summary>
        /// Aktiviert Text-to-Speech
        /// </summary>
        public DiscordWebhookMessage EnableTextToSpeech(bool enable = true)
        {
            TextToSpeech = enable;
            return this;
        }
    }
}