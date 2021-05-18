using System;
using Vigilance.Custom.Items.Enums;
using Vigilance.Custom.Items.Extensions;
using YamlDotNet.Serialization;
using Vigilance.Serializable;

namespace Vigilance.Custom.Items.API
{
    public class DynamicSpawnPoint : SpawnPoint
    {
        public SpawnPosition Location { get; set; }
        public override float Chance { get; set; }

        [YamlIgnore]
        public override string Name
        {
            get => Location.ToString();
            set => throw new InvalidOperationException("You cannot change the name of a dynamic spawn location.");
        }

        [YamlIgnore]
        public override Vector Position
        {
            get => Location.GetPosition().ToVector();
            set => throw new InvalidOperationException("You cannot change the spawn vector of a dynamic spawn location.");
        }
    }
}
