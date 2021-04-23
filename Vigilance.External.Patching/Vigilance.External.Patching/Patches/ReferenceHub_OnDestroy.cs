using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
    public static class ReferenceHub_OnDestroy
    {
        public static bool Prefix(ReferenceHub __instance)
        {
            try
            {
                Player player = PlayersList.GetPlayer(__instance);

                if (player == null)
                    return true;

                PlayerLeaveEvent ev = new PlayerLeaveEvent(player);
                EventManager.Trigger<IHandlerPlayerLeave>(ev);

                PatchData.OnPlayerLeave(player);

                PlayersList.Remove(__instance);

                if (ReferenceHub.Hubs.ContainsKey(__instance.gameObject))
                    ReferenceHub.Hubs.Remove(__instance.gameObject);

                if (ReferenceHub.HubIds.ContainsKey(__instance.queryProcessor.PlayerId))
                    ReferenceHub.HubIds.Remove(__instance.queryProcessor.PlayerId);

                if (ReferenceHub._hostHub == __instance)
                    ReferenceHub._hostHub = null;

                if (ReferenceHub._localHub == __instance)
                    ReferenceHub._localHub = null;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(ReferenceHub_OnDestroy), e);
                return true;
            }
        }
    }
}
