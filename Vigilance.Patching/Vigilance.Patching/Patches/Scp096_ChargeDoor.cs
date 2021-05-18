using PlayableScps;
using Harmony;
using Interactables.Interobjects.DoorUtils;
using Interactables.Interobjects;
using UnityEngine;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargeDoor))]
    public static class Scp096_ChargeDoor
    {
        public static bool Prefix(Scp096 __instance, DoorVariant door)
        {
			if (door.GetExactState() >= 1f)
				return false;

			if (door != null)
			{
				IDamageableDoor damagable = door as IDamageableDoor;

				if (damagable == null)
				{
					if (door is PryableDoor pryable && pryable != null)
					{
						if (door.GetExactState() == 0f && !door.TargetState)
						{
							__instance.Hub.fpc.NetworkmovementOverride = Vector2.zero;
							__instance._chargeCooldown = 0f;
							__instance.PryGate(pryable);
						}
					}
				}
				else
				{
					if (!damagable.IsDestroyed && door.GetExactState() < 1f && __instance._lastChargedDamageableDoor != damagable && PluginManager.Config.Scp096DestroyDoors)
					{
						damagable.ServerDamage(750f, DoorDamageType.Scp096);
						__instance._lastChargedDamageableDoor = damagable;
						return false;
					}
				}
			}

			return false;
		}
    }
}
