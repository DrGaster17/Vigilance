using System.Collections.Generic;

namespace Vigilance.Custom.Items.API
{
    public class SpawnProperties
    {
        public uint Limit { get; set; }
        public List<DynamicSpawnPoint> DynamicSpawnPoints { get; set; } = new List<DynamicSpawnPoint>();
        public List<StaticSpawnPoint> StaticSpawnPoints { get; set; } = new List<StaticSpawnPoint>();
    }
}
