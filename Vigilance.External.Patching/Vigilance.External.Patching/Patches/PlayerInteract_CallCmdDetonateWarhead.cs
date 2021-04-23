using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Utilities;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdDetonateWarhead))]
    public static class PlayerInteract_CallCmdDetonateWarhead
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || __instance._hc.CufferId > 0
                    || (__instance._hc.ForceCuff && !PlayerInteract.CanDisarmedInteract) || !__instance._playerInteractRateLimit.CanExecute(true)
                    || !__instance.ChckDis(MapCache.OutsitePanelScript.transform.position) || !AlphaWarheadOutsitePanel.nukeside.enabled 
                    || !MapCache.Outsite.NetworkkeycardEntered)
                    return false;

                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                WarheadStartEvent ev = new WarheadStartEvent(player, AlphaWarhead.RealDetonationTimer, true);
                EventManager.Trigger<IHandlerWarheadStart>(ev);

                if (!ev.Allow)
                    return false;

                AlphaWarhead.Start();

                ServerLogs.AddLog(ServerLogs.Modules.Warhead, __instance._hub.LoggedNameFromRefHub() + " started the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent, false);

                __instance.OnInteract();

                FlickerableLightController.WarheadEnabled = true;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdDetonateWarhead), e);
                return true;
            }
        }
    }
}
