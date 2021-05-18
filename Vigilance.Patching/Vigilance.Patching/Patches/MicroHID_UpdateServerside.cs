using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(MicroHID), nameof(MicroHID.UpdateServerside))]
    public static class MicroHID_UpdateServerside
    {
        public static void Prefix(MicroHID __instance)
        {
            if (PluginManager.Config.UnlimitedMicroEnergy && __instance.refHub.inventory.curItem == ItemType.MicroHID)
            {
                __instance.ChangeEnergy(1f);
                __instance.NetworkEnergy = 1f;
            }
        }
    }
}
