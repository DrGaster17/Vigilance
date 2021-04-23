using System;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Inventory.SyncListItemInfo), nameof(Inventory.SyncListItemInfo.ModifyDuration))]
    public static class SyncListItemInfo_ModifyDuration
    {
        public static bool Prefix(Inventory.SyncListItemInfo __instance, int index, float value)
        {
            try
            {
                if (index < 0 || index >= __instance.Count)
                    return false;

                Inventory.SyncItemInfo item = __instance[index];

                ChangeItemDurabilityEvent ev = new ChangeItemDurabilityEvent(null, item, value, true);
                EventManager.Trigger<IHandlerItemChangeDurability>(ev);

                if (!ev.Allow)
                    return false;

                item.durability = ev.Durability;
                __instance[index] = item;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(SyncListItemInfo_ModifyDuration), e);
                return true;
            }
        }
    }
}
