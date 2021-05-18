using Harmony;
using PlayableScps;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.MaxShield), MethodType.Getter)]
    public static class Scp096_MaxShield
    {
        public static bool Prefix(Scp096 __instance, ref float __result)
        {
            __result = PluginManager.Config.Scp096MaxShield;
            return false;
        }
    }
}
