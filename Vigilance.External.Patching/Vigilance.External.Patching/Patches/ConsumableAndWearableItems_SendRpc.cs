using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.SendRpc))]
    public static class ConsumableAndWearableItems_SendRpc
    {
        public static bool Prefix(ConsumableAndWearableItems __instance, ConsumableAndWearableItems.HealAnimation healAnimation, int mid)
        {
            try
            {
                if (healAnimation == ConsumableAndWearableItems.HealAnimation.DequipMedicalItem)
                {
                    DequipMedicalItemEvent ev = new DequipMedicalItemEvent(PlayersList.GetPlayer(__instance._hub), __instance.usableItems[mid], true);
                    EventManager.Trigger<IHandlerMedicalDequip>(ev);

                    if (!ev.Allow)
                        return false;
                }

                __instance.RpcDoAnimations((int)healAnimation, mid);
                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(ConsumableAndWearableItems_SendRpc), e);
                return true;
            }
        }
    }
}
