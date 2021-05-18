using Harmony;
using Targeting;
using Mirror;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096Target), nameof(Scp096Target.Start))]
    public static class Scp096Target_Start
    {
        public static bool Prefix(Scp096Target __instance)
        {
            __instance.IsTarget = false;
            
            if (!PluginManager.Config.Scp096VisionParticles)
            {
                NetworkServer.Destroy(__instance._targetParticles);
                __instance._targetParticles = null;
            }

            return false;
        }
    }
}
