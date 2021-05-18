using UnityEngine;

namespace Vigilance.Utilities
{
    public static class Cacher
    {
        private static RaycastHit[] _rayCache;
        private static System.Random _random;
        public static float Infinity => -4.6566467E+11f;

        public static RaycastHit[] RayCache => _rayCache == null ? _rayCache = new RaycastHit[1] : _rayCache;
        public static System.Random RandomGen => _random == null ? _random = new System.Random() : _random;
    }
}