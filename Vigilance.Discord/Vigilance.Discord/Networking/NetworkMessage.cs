namespace Vigilance.Discord.Networking
{
    public class NetworkMessage
    {
        public uint? NetworkChannel { get; set; }
        public ulong? Channel { get; set; }
        public string Data { get; set; }
        public DiscordEmbed Embed { get; set; }
        public NetworkPlayer Target { get; set; }
        public NetworkPlayer Sender { get; set; }
        public NetworkPlayerAction Action { get; set; }
        public NetworkDataType Type { get; set; }
        public object[] Additive { get; set; }

        public static NetworkMessage Create(string data = null, uint? channelId = null, ulong? dataId = null, DiscordEmbed embed = null, 
            NetworkPlayer target = null, NetworkPlayer sender = null, 
            NetworkDataType type = NetworkDataType.Heartbeat, NetworkPlayerAction act = null, object[] additive = null) => new NetworkMessage
        {
            NetworkChannel = channelId,
            Embed = embed,
            Channel = dataId,
            Target = target,
            Data = data,
            Sender = sender,
            Type = type,
            Action = act,
            Additive = additive
        };
    }
}