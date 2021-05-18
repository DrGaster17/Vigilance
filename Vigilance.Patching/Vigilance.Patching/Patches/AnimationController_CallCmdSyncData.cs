using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.CallCmdSyncData))]
    public static class AnimationController_CallCmdSyncData
    {
        public static bool Prefix(AnimationController __instance, byte state, Vector2 v2)
        {
            try
            {
                if (!__instance._mSyncRateLimit.CanExecute(true))
                    return false;

                Player player = PlayersList.GetPlayer(__instance.ccm._hub);

                if (player == null)
                    return true;

                SyncDataEvent ev = new SyncDataEvent(player, v2, state, true);
                EventManager.Trigger<IHandlerSyncData>(ev);

                if (!ev.Allow)
                    return false;

                __instance.NetworkcurAnim = ev.Animation;
                __instance.Networkspeed = ev.Speed;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(AnimationController_CallCmdSyncData), e);
                return true;
            }
        }
    }
}
