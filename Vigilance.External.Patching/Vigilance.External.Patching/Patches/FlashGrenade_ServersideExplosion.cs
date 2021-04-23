using System;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Custom.Items.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using Grenades;
using CustomPlayerEffects;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(FlashGrenade), nameof(FlashGrenade.ServersideExplosion))]
    public static class FlashGrenade_ServersideExplosion
    {
        public static bool Prefix(FlashGrenade __instance, ref bool __result)
        {
            try
            {
				GrenadeExplodeEvent ev = new GrenadeExplodeEvent(PlayersList.GetPlayer(__instance.thrower.hub), __instance, GrenadeType.Flash, true);
				CustomGrenade.Handler.OnExplode(ev);
				EventManager.Trigger<IHandlerGrenadeExplode>(ev);

				if (!ev.Allow)
					return false;

				foreach (ReferenceHub hub in ReferenceHub.Hubs.Values)
				{
					Vector3 position = __instance.transform.position;

					Flashed effect = hub.playerEffectsController.GetEffect<Flashed>();

					Deafened effect2 = hub.playerEffectsController.GetEffect<Deafened>();

					if (effect != null && !(__instance.thrower == null) && (__instance._friendlyFlash || effect.Flashable(__instance.thrower.hub, position, __instance._ignoredLayers)))
					{
						float num = __instance.powerOverDistance.Evaluate(Vector3.Distance(hub.transform.position, position) / ((position.y > 900f) ? __instance.distanceMultiplierSurface : __instance.distanceMultiplierFacility)) * __instance.powerOverDot.Evaluate(Vector3.Dot(hub.PlayerCameraReference.forward, (hub.PlayerCameraReference.position - position).normalized));

						byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(num * 10f * __instance.maximumDuration), 1, 255);

						if (b >= effect.Intensity && num > 0f)
						{
							if (PluginManager.Config.FlashGrenadeFlashedEffect)
								hub.playerEffectsController.ChangeEffectIntensity<Flashed>(b);

							if (effect2 != null)
							{
								if (PluginManager.Config.FlashGrenadeDeafenedEffect)
									hub.playerEffectsController.EnableEffect(effect2, num * __instance.maximumDuration, true);
							}
						}
					}
				}


				__result = PatchData.ServersideExplosion(__instance);
				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(FlashGrenade_ServersideExplosion), e);
                return true;
            }
        }
    }
}
