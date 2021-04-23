using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using Scp914;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdChange914Knob))]
    public static class PlayerInteract_CallCmdChange914Knob
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (Scp914Machine.singleton.working)
                    return false;

                if (!__instance._playerInteractRateLimit.CanExecute(true) 
                    || ((__instance._hc.CufferId > 0 || __instance._hc.ForceCuff) && !PlayerInteract.CanDisarmedInteract)
                    || !__instance.ChckDis(Scp914Machine.singleton.knob.position))
                    return false;

                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                Scp914Knob newState = API.Scp914.KnobStatus + 1 > Scp914Machine.knobStateMax ? Scp914Machine.knobStateMin : API.Scp914.KnobStatus + 1;

                SCP914ChangeKnobEvent ev = new SCP914ChangeKnobEvent(player, API.Scp914.Controller, API.Scp914.KnobStatus, newState, true);
                EventManager.Trigger<IHandlerScp914ChangeKnob>(ev);

                SetKnob(ev.NewKnob, API.Scp914.Controller);

                __instance.OnInteract();

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdChange914Knob), e);
                return true;
            }
        }

        public static void SetKnob(Scp914Knob knob, Scp914Machine instance)
        {
            if (Math.Abs(instance.curKnobCooldown) > 0.001f)
                return;

            instance.curKnobCooldown = instance.knobCooldown;
            instance.NetworkknobState = knob;
        }
    }
}
