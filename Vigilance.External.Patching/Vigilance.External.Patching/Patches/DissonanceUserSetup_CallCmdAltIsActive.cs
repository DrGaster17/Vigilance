using Assets._Scripts.Dissonance;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.CallCmdAltIsActive))]
    public static class DissonanceUserSetup_CallCmdAltIsActive
    {
        public static bool Prefix(DissonanceUserSetup __instance, bool value)
        {
            ReferenceHub hub = ReferenceHub.GetHub(__instance.gameObject);

            if (hub != null && hub.characterClassManager != null && PluginManager.Config.AltChatAllowedRoles.Contains(hub.characterClassManager.CurClass))
                __instance.MimicAs939 = value;

            return true;
        }
    }
}
