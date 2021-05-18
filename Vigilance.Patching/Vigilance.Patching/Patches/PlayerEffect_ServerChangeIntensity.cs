using System;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using CustomPlayerEffects;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerEffect), nameof(PlayerEffect.ServerChangeIntensity))]
    public static class PlayerEffect_ServerChangeIntensity
    {
        public static bool Prefix(PlayerEffect __instance, byte newState)
        {
            try
            {
                if (__instance.Intensity == newState)
                    return false;

                PlayerReceiveEffect ev = new PlayerReceiveEffect(PlayersList.GetPlayer(__instance.Hub), __instance, EffectType.Amnesia, newState, true);
                EventManager.Trigger<IHandlerPlayerReceiveEffect>(ev);

                if (!ev.Allow)
                    return false;

                byte cur = __instance.Intensity;

                __instance.Intensity = ev.Intensity;
                __instance.Hub.playerEffectsController.Resync();
                __instance.ServerOnIntensityChange(cur, ev.Intensity);

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerEffect_ServerChangeIntensity), e);
                return true;
            }
        }
    }
}
