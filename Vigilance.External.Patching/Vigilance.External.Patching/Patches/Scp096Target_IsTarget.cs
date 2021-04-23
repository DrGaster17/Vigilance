using Targeting;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096Target), nameof(Scp096Target.IsTarget), MethodType.Setter)]
    public static class Scp096Target_IsTarget
    {
        public static bool Prefix(Scp096Target __instance, bool value)
        {
            __instance._isTarget = value;

            if (!PluginManager.Config.Scp096VisionParticles && __instance._targetParticles != null)
            {
                __instance._targetParticles.SetActive(value);
                return false;
            }

            if (__instance._targetParticles != null)
                __instance._targetParticles.SetActive(value);

            return false;
        }
    }
}
