using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using PlayableScps;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.Enrage))]
    public static class Scp096_Enrage
    {
        public static bool Prefix(Scp096 __instance)
        {
            try
            {
                SCP096EnrageEvent ev = new SCP096EnrageEvent(PlayersList.GetPlayer(__instance.Hub), true);
                EventManager.Trigger<IHandlerScp096Enrage>(ev);

                if (!ev.Allow)
                    return false;

                if (__instance.Enraged)
                {
                    __instance.AddReset();
                    return false;
                }

                __instance.SetMovementSpeed(12f);
                __instance.SetJumpHeight(10f);

                __instance.PlayerState = Scp096PlayerState.Enraged;
                __instance.EnrageTimeLeft += __instance.EnrageTime;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp096_Enrage), e);
                return true;
            }
        }
    }
}
