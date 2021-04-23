using System;

namespace Vigilance.API.Discord.Networking
{
    [Serializable]
    public class NetworkQuaternion
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public static NetworkQuaternion FromQuaternion(float x, float y, float z, float w) => new NetworkQuaternion
        {
            X = x,
            Y = y,
            Z = z,
            W = w
        };
    }
}
