using System;

namespace Vigilance.API.Discord
{
    [Serializable]
    public class DiscordEmbed
    {
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
        public string AuthorIconUrl { get; set; }

        public string Title { get; set; }
        public string TitleUrl { get; set; }

        public string Description { get; set; }

        public string ThumbnailUrl { get; set; }

        public string ImageUrl { get; set; }

        public string Footer { get; set; }
        public string FooterIconUrl { get; set; }

        public string Color { get; set; }

        public bool? Timestamp { get; set; }

        public DiscordEmbedField[] Fields { get; set; }

        public static DiscordEmbed Create(string author = null, string authorUrl = null, string authorIconUrl = null, string title = null, string titleUrl = null, 
            string description = null, string thumbnailUrl = null, string imageUrl = null, string footer = null, string footerIconUrl = null, bool? timestamp = null, 
            string color = null, DiscordEmbedField[] fields = null)
        {
            return new DiscordEmbed
            {
                Author = author,
                AuthorUrl = authorUrl,
                AuthorIconUrl = authorIconUrl,
                Title = title,
                TitleUrl = titleUrl,
                Description = description,
                ThumbnailUrl = thumbnailUrl,
                ImageUrl = imageUrl,
                Footer = footer,
                FooterIconUrl = footerIconUrl,
                Timestamp = timestamp,
                Color = color,
                Fields = fields
            };
        }
    }
}
