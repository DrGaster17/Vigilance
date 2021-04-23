using Harmony;
using PlayableScps;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.OnEnable))]
    public static class Scp096_OnEnable
    {
        public static void Postfix(Scp096 __instance)
        {
            __instance.CurMaxShield = PluginManager.Config.Scp096MaxShield;
            __instance.ShieldAmount = PluginManager.Config.Scp096MaxShield;
            __instance.ShieldRechargeRate = PluginManager.Config.Scp096RechargeRate;
            __instance._canRegen = PluginManager.Config.Scp096CanRegen;

            if (!__instance._canRegen)
                __instance.TimeUntilShieldRecharge = float.PositiveInfinity;
        }
    }
}
