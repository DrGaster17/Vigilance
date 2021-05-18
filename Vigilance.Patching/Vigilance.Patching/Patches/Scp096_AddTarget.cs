using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using Mirror;
using PlayableScps;
using PlayableScps.Messages;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.AddTarget))]
    public static class Scp096_AddTarget
    {
        public static bool Prefix(Scp096 __instance, GameObject target)
        {
            try
            {
                Player myTarget = PlayersList.GetPlayer(target);
                Player scp = PlayersList.GetPlayer(__instance.Hub);

                if (myTarget == null || scp == null)
                    return true;

                if (!__instance.CanReceiveTargets || __instance._targets.Contains(myTarget.ReferenceHub))
                    return false;

                Scp096AddTargetEvent ev = new Scp096AddTargetEvent(scp, myTarget, true);
                EventManager.Trigger<IHandlerScp096AddTarget>(ev);

                if (!ev.Allow)
                    return false;

                if (!__instance._targets.IsEmpty() || __instance.Enraged)
                    __instance.AddReset();

                __instance._targets.Add(myTarget.ReferenceHub);

                NetworkServer.SendToClientOfPlayer(myTarget.ConnectionIdentity, new Scp096ToTargetMessage(myTarget.ReferenceHub));

                __instance.AdjustShield(PluginManager.Config.Scp096ShieldPerPlayer);

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp096_AddTarget), e);
                return true;
            }
        }
    }
}
