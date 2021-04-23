using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.CallCmdFreeTeammate))]
    public static class Handcuffs_CallCmdFreeTeammate
    {
        public static bool Prefix(Handcuffs __instance, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true) || target == null
                    || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f
                    || __instance.MyReferenceHub.characterClassManager.CurRole.team == Team.SCP)
                    return false;

                Player teammate = PlayersList.GetPlayer(target);
                Player cuffer = PlayersList.GetPlayer(__instance.gameObject);

                if (cuffer.PlayerLock)
                    return false;

                if (teammate == null || cuffer == null)
                    return true;

                UncuffEvent ev = new UncuffEvent(teammate, cuffer, true);
                EventManager.Trigger<IHandlerUncuff>(ev);

                if (!ev.Allow)
                    return false;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Handcuffs_CallCmdFreeTeammate), e);
                return true;
            }
        }
    }
}
