using Vigilance.Serializable;

namespace Vigilance.Custom.Items.API
{
    public abstract class SpawnPoint
    {
        public abstract string Name { get; set; }
        public abstract float Chance { get; set; }
        public abstract Vector Position { get; set; }
    }
}
