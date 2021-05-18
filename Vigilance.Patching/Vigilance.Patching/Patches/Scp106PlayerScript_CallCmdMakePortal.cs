using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMakePortal))]
    public static class Scp106PlayerScript_CallCmdMakePortal
    {
        public static bool Prefix(Scp106PlayerScript __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true) || !__instance._hub.playerMovementSync.Grounded)
                    return false;

                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                Debug.DrawRay(__instance.transform.position, -__instance.transform.up, Color.red, 10f);

                if (__instance.iAm106 && !__instance.goingViaThePortal && Physics.Raycast(new Ray(__instance.transform.position, -__instance.transform.up), out RaycastHit raycastHit, 10f, __instance.teleportPlacementMask))
                {
                    SCP106CreatePortalEvent ev = new SCP106CreatePortalEvent(player, raycastHit.point - Vector3.up, true);
                    EventManager.Trigger<IHandlerScp106CreatePortal>(ev);

                    if (!ev.Allow)
                        return false;

                    __instance.SetPortalPosition(ev.Position);
                }

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp106PlayerScript_CallCmdMakePortal), e);
                return true;
            }
        }
    }
}
