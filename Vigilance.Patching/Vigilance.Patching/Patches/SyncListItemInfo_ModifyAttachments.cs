using System;
using Vigilance.API.Enums;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Inventory.SyncListItemInfo), nameof(Inventory.SyncListItemInfo.ModifyAttachments))]
    public static class SyncListItemInfo_ModifyAttachments
    {
        public static bool Prefix(Inventory.SyncListItemInfo __instance, int index, int s, int b, int o)
        {
            try
            {
                if (index < 0 || index >= __instance.Count)
                    return false;

                Inventory.SyncItemInfo value = __instance[index];

                ChangeItemAttachmentsEvent ev = new ChangeItemAttachmentsEvent(null, value, (SightType)value.modSight, (SightType)s, (BarrelType)value.modBarrel,
                    (BarrelType)b, (OtherType)value.modOther, (OtherType)o, true);
                EventManager.Trigger<IHandlerItemChangeAttachments>(ev);

                if (!ev.Allow)
                    return false;

                value.modSight = (int)ev.NewSight;
                value.modBarrel = (int)ev.NewBarrel;
                value.modOther = (int)ev.NewOther;

                __instance[index] = value;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(SyncListItemInfo_ModifyAttachments), e);
                return true;
            }
        }
    }
}
