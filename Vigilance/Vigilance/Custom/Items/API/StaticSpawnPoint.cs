using Vigilance.Serializable;

namespace Vigilance.Custom.Items.API
{
    public class StaticSpawnPoint : SpawnPoint
    {
        public override string Name { get; set; }
        public override float Chance { get; set; }
        public override Vector Position { get; set; }
    }
}
