using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using UnityEngine;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseWorkStationPlace))]
    public static class PlayerInteract_CallCmdUseWorkStationPlace
    {
        public static bool Prefix(PlayerInteract __instance, GameObject station)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || __instance._hc.CufferId > 0 
                    || (__instance._hc.ForceCuff && !PlayerInteract.CanDisarmedInteract)
                    || station == null || !__instance.ChckDis(station.transform.position)
                    || !station.TryGetComponent(out WorkStation workStation))
                    return false;

                Workstation apiStation = workStation.GetWorkstation();
                Player player = PlayersList.GetPlayer(__instance._hub);

                if (apiStation == null || player == null)
                    return true;

                if (!apiStation.CanUse(player))
                    return false;

                WorkStationActivateEvent ev = new WorkStationActivateEvent(player, apiStation, true);
                EventManager.Trigger<IHandlerActivateWorkStation>(ev);

                if (!ev.Allow)
                    return false;

                apiStation.ConnectTablet(player);

				__instance.OnInteract();

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdUseWorkStationPlace), e);
                return true;
            }
        }
    }
}
