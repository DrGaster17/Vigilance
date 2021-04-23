using System;
using Harmony;

using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.EventSystem;
using Vigilance.API;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.CallCmdDropItem))]
    public static class Inventory_CallCmdDropItem
    {
        public static bool Prefix(Inventory __instance, int itemInventoryIndex)
        {
            try
            {
                if (__instance.isLocalPlayer)
                    return true;

                if (!__instance._iawRateLimit.CanExecute(true) || itemInventoryIndex < 0 || itemInventoryIndex >= __instance.items.Count || __instance._amnesia.Enabled)
                    return false;

                Inventory.SyncItemInfo syncItemInfo = __instance.items[itemInventoryIndex];

                // whatever nw
                if (__instance.items[itemInventoryIndex].id != syncItemInfo.id)
                    return false;

                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                DropItemEvent ev = new DropItemEvent(__instance.items[itemInventoryIndex], player, true);
                EventManager.Trigger<IHandlerDropItem>(ev);
                Custom.Items.API.CustomItem.EventHandling.OnDropItem(ev);

                if (!ev.Allow)
                    return false;

                Pickup pickup = __instance.SetPickup(ev.Item.id, ev.Item.durability, __instance.transform.position, __instance.camera.transform.rotation, ev.Item.modSight, ev.Item.modBarrel, ev.Item.modOther, true);
                __instance.items.RemoveAt(itemInventoryIndex);

                DroppedItemEvent dropped = new DroppedItemEvent(pickup, player);
                EventManager.Trigger<IHandlerDroppedItem>(dropped);

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Inventory_CallCmdDropItem), e);
                return true;
            }
        }
    }
}
