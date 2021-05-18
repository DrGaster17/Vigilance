namespace Vigilance.Discord.Networking
{
    public class NetworkPlayerAction
    {
        public NetworkPlayer Source { get; set; }
        public NetworkPlayer Target { get; set; }
        public NetworkPlayerActionType Action { get; set; }

        public object[] Parameters { get; set; }

        public static NetworkPlayerAction Create(NetworkPlayer source = null, NetworkPlayer target = null,
            NetworkPlayerActionType action = NetworkPlayerActionType.SyncAllValuesTo, object[] parameters = null) => new NetworkPlayerAction
            {
                Action = action,
                Source = source,
                Target = target,
                Parameters = parameters
            };

        public static NetworkPlayerAction Broadcast(NetworkPlayer target, string message, int duration, bool mono) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.Broadcast,
            Parameters = new object[] { message, duration, mono },
            Source = null,
            Target = target
        };

        public static NetworkPlayerAction Hint(NetworkPlayer target, string message, int duration) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.Hint,
            Parameters = new object[] { message, duration },
            Source = null,
            Target = target
        };

        public static NetworkPlayerAction RaMessage(NetworkPlayer target, string message) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.RaMessage,
            Parameters = new object[] { message },
            Source = null,
            Target = target
        };

        public static NetworkPlayerAction ConsoleMessage(NetworkPlayer target, string message, string color) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.ConsoleMessage,
            Parameters = new object[] { message, color },
            Source = null,
            Target = target
        };

        public static NetworkPlayerAction SyncValuesFrom(NetworkPlayer source, NetworkPlayer destination) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.SyncAllValuesFrom,
            Parameters = null,
            Source = source,
            Target = destination
        };

        public static NetworkPlayerAction SyncValuesTo(NetworkPlayer source, NetworkPlayer target) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.SyncAllValuesTo,
            Parameters = source.GetParameters(),
            Source = source,
            Target = target
        };

        public static NetworkPlayerAction SyncInventory(NetworkPlayer source, NetworkPlayer target) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.SyncInventory,
            Parameters = source.GetParameters(),
            Source = source,
            Target = target
        };

        public static NetworkPlayerAction SyncPosition(NetworkPlayer source, NetworkPlayer target) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.SyncPosition,
            Parameters = source.GetParameters(),
            Source = source,
            Target = target
        };

        public static NetworkPlayerAction SyncRotation(NetworkPlayer source, NetworkPlayer target) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.SyncRotation,
            Parameters = source.GetParameters(),
            Source = source,
            Target = target
        };

        public static NetworkPlayerAction SyncHealth(NetworkPlayer source, NetworkPlayer target) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.SyncHealth,
            Parameters = source.GetParameters(),
            Source = source,
            Target = target
        };

        public static NetworkPlayerAction SyncGroup(NetworkPlayer source, NetworkPlayer target) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.SyncGroup,
            Parameters = source.GetParameters(),
            Source = source,
            Target = target
        };

        public static NetworkPlayerAction SyncItem(NetworkPlayer source, NetworkPlayer target) => new NetworkPlayerAction
        {
            Action = NetworkPlayerActionType.SyncItem,
            Parameters = source.GetParameters(),
            Source = source,
            Target = target
        };
    }
}
