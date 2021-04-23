using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using Mirror;
using PlayableScps;
using PlayableScps.Messages;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.EndEnrage))]
    public static class Scp096_EndEnrage
    {
        public static bool Prefix(Scp096 __instance)
        {
            try
            {
                SCP096CalmEvent ev = new SCP096CalmEvent(PlayersList.GetPlayer(__instance.Hub), true);
                EventManager.Trigger<IHandlerScp096Calm>(ev);

                if (!ev.Allow)
                    return false;

                __instance.EndCharge();
                __instance.SetMovementSpeed(0f);
                __instance.SetJumpHeight(4f);
                __instance.ResetShield();

                __instance.PlayerState = Scp096PlayerState.Calming;
                __instance.AddedTimeThisRage = 0f;
                __instance._calmingTime = 6f;
                __instance.EnrageTimeLeft = 0f;
                __instance._targets.Clear();

                NetworkServer.SendToClientOfPlayer(__instance.Hub.characterClassManager.netIdentity, new Scp096ToSelfMessage(__instance.EnrageTimeLeft, __instance._chargeCooldown));

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp096_EndEnrage), e);
                return true;
            }
        }
    }
}
