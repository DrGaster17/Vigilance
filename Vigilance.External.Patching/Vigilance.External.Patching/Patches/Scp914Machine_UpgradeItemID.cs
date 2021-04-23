using System;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Utilities;
using Harmony;
using Scp914;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradeItemID))]
    public static class Scp914Machine_UpgradeItemID
    {
        public static bool Prefix(Scp914Machine __instance, ItemType itemID, ref ItemType __result)
        {
            try
            {
                Scp914UpgradeItemEvent ev = new Scp914UpgradeItemEvent(itemID, true);

                ev.Output = Scp914Utilities.UpgradeItemId(itemID);

                EventManager.Trigger<IHandlerScp914UpgradeItem>(ev);

                __result = ev.Output;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp914Machine_UpgradeItemID), e);

                __result = Scp914Utilities.UpgradeItemId(itemID);

                return true;
            }
        }
    }
}
