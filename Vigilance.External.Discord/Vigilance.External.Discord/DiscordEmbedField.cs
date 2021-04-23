using System;

namespace Vigilance.API.Discord
{
    [Serializable]
    public class DiscordEmbedField
    {
        public bool Inline { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
    }
}
