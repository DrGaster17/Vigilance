using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using Mirror;
using Scp914;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUse914))]
    public static class PlayerInteract_CallCmdUse914
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (Scp914Machine.singleton.working)
                    return false;

                if (!__instance._playerInteractRateLimit.CanExecute(true) || 
                    ((__instance._hc.CufferId > 0 || __instance._hc.ForceCuff) && !PlayerInteract.CanDisarmedInteract)
                    || !__instance.ChckDis(Scp914Machine.singleton.button.position))
                    return false;

                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                SCP914ActivateEvent ev = new SCP914ActivateEvent(player, (float)NetworkTime.time, true);
                EventManager.Trigger<IHandlerScp914Activate>(ev);

                if (!ev.Allow)
                    return false;

                Scp914Machine.singleton.RpcActivate(ev.Time);

                __instance.OnInteract();

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdUse914), e);
                return true;
            }
        }
    }
}
