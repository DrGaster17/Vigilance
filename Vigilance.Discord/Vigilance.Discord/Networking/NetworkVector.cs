namespace Vigilance.Discord.Networking
{
    public class NetworkVector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static NetworkVector FromVector(float x, float y, float z) => new NetworkVector
        {
            X = x,
            Y = y,
            Z = z
        };
    }
}
