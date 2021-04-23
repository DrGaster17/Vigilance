using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.SetCurItem))]
    public static class Inventory_SetCurItem
    {
        public static bool Prefix(Inventory __instance, ItemType ci)
        {
            try
            {
                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                AllowChangeItemCheckEvent ev = new AllowChangeItemCheckEvent(player, !__instance._ccmSet || (!__instance._amnesia.Enabled && (__instance._ccm.CurClass == RoleType.Spectator || __instance._cawi.cooldown <= 0f)));
                EventManager.Trigger<IHandlerAllowChangeItemCheck>(ev);

                if (!ev.Allow)
                    return false;

                ChangeItemEvent changeItemEvent = new ChangeItemEvent(player.CurrentItem, ci, player, true);
                EventManager.Trigger<IHandlerChangeItem>(changeItemEvent);

                __instance.curItem = changeItemEvent.NewItem;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Inventory_SetCurItem), e);
                return true;
            }
        }
    }
}
