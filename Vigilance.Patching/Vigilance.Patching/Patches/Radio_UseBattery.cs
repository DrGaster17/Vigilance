using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Radio), nameof(Radio.UseBattery))]
    public static class Radio_UseBattery
    {
        public static bool Prefix(Radio __instance) => PluginManager.Config.UnlimitedRadioBattery ? false : true;
    }
}
