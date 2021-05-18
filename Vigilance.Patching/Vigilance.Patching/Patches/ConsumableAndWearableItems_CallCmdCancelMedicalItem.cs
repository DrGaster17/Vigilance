using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdCancelMedicalItem))]
    public static class ConsumableAndWeableItems_CallCmdCancelMedicalItem
    {
        public static bool Prefix(ConsumableAndWearableItems __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;

                Player player = __instance._hub.GetPlayer();

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                foreach (ConsumableAndWearableItems.UsableItem usableItem in __instance.usableItems)
                {
                    if (usableItem.inventoryID == __instance._hub.inventory.curItem && usableItem.cancelableTime > 0f)
                    {
                        CancelMedicalItemEvent ev = new CancelMedicalItemEvent(__instance.cooldown, player, usableItem.inventoryID, true);
                        EventManager.Trigger<IHandlerCancelMedicalItem>(ev);

                        if (!ev.Allow)
                            return false;

                        __instance.cooldown = ev.Cooldown;
                        __instance._cancel = true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Add("ConsumableAndWeableItems.CallCmdCancelMedicalItem", e);
                return true;
            }
        }
    }
}
