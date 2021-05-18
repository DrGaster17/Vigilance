using System;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using Harmony;
using Scp914;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradeItem))]
    public static class Scp914Machine_UpgradeItem
    {
        public static bool Prefix(Scp914Machine __instance, Pickup item, ref bool __result)
        {
            try
            {
                Scp914UpgradePickupEvent ev = new Scp914UpgradePickupEvent(item, Scp914Utilities.UpgradeItemId(item.ItemId), true);
                EventManager.Trigger<IHandlerScp914UpgradeItem>(ev);

                if (!ev.Allow)
                {
                    __result = false;
                    return false;
                }

                if (ev.Output < ItemType.KeycardJanitor)
                {
                    item.Delete();

                    return false;
                }

                item.SetIDFull(ev.Output);

                __result = true;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp914Machine_UpgradeItem), e);

                __result = false;

                return true;
            }
        }
    }
}
