using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Utilities;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdSwitchAWButton))]
    public static class PlayerInteract_CallCmdSwitchAWButton
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || __instance._hc.CufferId > 0 || (__instance._hc.ForceCuff && !PlayerInteract.CanDisarmedInteract) 
                    || !__instance.ChckDis(MapCache.OutsitePanelScript.transform.position))
                    return false;

                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                WarheadKeycardAccessEvent ev = new WarheadKeycardAccessEvent(player, player.CurrentItem, AlphaWarhead.CanOperatePanel(player));
                EventManager.Trigger<IHandlerWarheadKeycardAccess>(ev);

                if (!ev.Allow)
                    return false;

                MapCache.Outsite.NetworkkeycardEntered = !MapCache.Outsite.NetworkkeycardEntered;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdSwitchAWButton), e);
                return true;
            }
        }
    }
}
