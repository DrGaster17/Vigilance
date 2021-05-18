using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(MuteHandler), nameof(MuteHandler.Reload))]
    public static class MuteHandler_Reload
    {
        // Making this a prefix void without the return means that this executes before the original method does
        // but then continues with executing the original method
        public static void Prefix()
        {
            if (MuteHandler.Mutes != null)
                MuteHandler.Mutes.Clear();
        }
    }
}
