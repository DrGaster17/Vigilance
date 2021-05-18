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
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseElevator))]
    public static class PlayerInteract_CallCmdUseElevator
    {
        public static bool Prefix(PlayerInteract __instance, GameObject elevator)
        {
            try
            {
				if (!__instance._playerInteractRateLimit.CanExecute(true) || __instance._hc.CufferId > 0 || (__instance._hc.ForceCuff && !PlayerInteract.CanDisarmedInteract) || elevator == null)
					return false;

				Lift component = elevator.GetComponent<Lift>();

                if (component == null)
                    return false;

                Elevator apiElevator = component.GetElevator();
                Player player = PlayersList.GetPlayer(__instance._hub);

                if (apiElevator == null || player == null)
                    return true;

                if (player.PlayerLock || apiElevator.IsLocked || apiElevator.DisallowedPlayers.Contains(player))
                    return false;

				foreach (Lift.Elevator elevator2 in component.elevators)
				{
					if (__instance.ChckDis(elevator2.door.transform.position))
					{
                        ElevatorInteractEvent ev = new ElevatorInteractEvent(component, player, true);
                        EventManager.Trigger<IHandlerElevatorInteract>(ev);

                        if (!ev.Allow)
                            return false;

                        apiElevator.Use();
						__instance.OnInteract();
					}
				}

                return false;
			}
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdUseElevator), e);
                return true;
            }
        }
    }
}
