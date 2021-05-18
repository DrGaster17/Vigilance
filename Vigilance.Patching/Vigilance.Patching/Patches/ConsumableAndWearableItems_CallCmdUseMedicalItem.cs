using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using MEC;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdUseMedicalItem))]
    public static class ConsumableAndWearableItems_CallCmdUseMedicalItem
    {
        public static bool Prefix(ConsumableAndWearableItems __instance)
        {
            try
            {
                if (!__instance._medicalItemRateLimit.CanExecute(true) || __instance.cooldown > 0f)
                    return false;

                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                __instance._cancel = false;

                for (int i = 0; i < __instance.usableItems.Length; i++)
                {
                    if (__instance.usableItems[i].inventoryID == __instance._hub.inventory.curItem && __instance.usableCooldowns[i] <= 0f)
                    {
                        UseItemEvent ev = new UseItemEvent(player, __instance.usableItems[i], true);
                        EventManager.Trigger<IHandlerUseItem>(ev);

                        if (!ev.Allow)
                            return false;

                        __instance.usableItems[i] = ev.Item;

                        Timing.RunCoroutine(__instance.UseMedicalItem(i), Segment.FixedUpdate);
                        return false;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(ConsumableAndWearableItems_CallCmdUseMedicalItem), e);
                return true;
            }
        }
    }
}
