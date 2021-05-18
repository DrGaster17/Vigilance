using System;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Lift), nameof(Lift.SetStatus))]
    public static class Lift_SetStatus
    {
        public static bool Prefix(Lift __instance, byte i)
        {
            try
            {
                LiftChangeStatusEvent ev = new LiftChangeStatusEvent(__instance.GetElevator(), __instance.status, (Lift.Status)i);
                EventManager.Trigger<IHandlerLiftChangeStatus>(ev);

                __instance.NetworkstatusID = (byte)ev.New;
                __instance.status = ev.New;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Lift_SetStatus), e);
                return true;
            }
        }
    }
}
