using Vigilance.API.Enums;

namespace Vigilance.Serializable
{
    public struct PickupModifiers
    {
        public PickupModifiers(BarrelType barrelType, SightType sightType, OtherType otherType)
        {
            BarrelType = barrelType;
            SightType = sightType;
            OtherType = otherType;
        }

        public BarrelType BarrelType { get; private set; }
        public SightType SightType { get; private set; }
        public OtherType OtherType { get; private set; }
    }
}
