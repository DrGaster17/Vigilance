using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using MEC;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdUsePortal))]
    public static class Scp106PlayerScript_CallCmdUsePortal
    {
        public static bool Prefix(Scp106PlayerScript __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true) || !__instance._hub.playerMovementSync.Grounded || !__instance.iAm106 
                    || __instance.portalPosition == Vector3.zero || __instance.goingViaThePortal)
                    return false;

                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                SCP106TeleportEvent ev = new SCP106TeleportEvent(player, player.Position, __instance.portalPosition, true);
                EventManager.Trigger<IHandlerScp106Teleport>(ev);

                if (!ev.Allow)
                    return false;

                if (ev.NewPosition != __instance.portalPosition)
                    __instance.NetworkportalPosition = ev.NewPosition;

                Timing.RunCoroutine(__instance._DoTeleportAnimation(), Segment.Update);
                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp106PlayerScript_CallCmdUsePortal), e);
                return true;
            }
        }
    }
}
