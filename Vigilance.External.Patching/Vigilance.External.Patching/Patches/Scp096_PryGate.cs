using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using MEC;
using Interactables.Interobjects;
using PlayableScps;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.PryGate))]
    public static class Scp096_PryGate
    {
        public static bool Prefix(Scp096 __instance, PryableDoor gate)
        {
            try
            {
				if (!PluginManager.Config.Scp096PryGates)
					return false;

				if (!__instance.Charging || !__instance.Enraged || gate.TargetState || gate.GetExactState() > 0f || !gate.TryPryGate())
					return false;

				Player player = PlayersList.GetPlayer(__instance.Hub);

				if (player == null)
					return true;

				Scp096PryGateEvent ev = new Scp096PryGateEvent(player, true);
				EventManager.Trigger<IHandlerScp096PryGate>(ev);

				if (!ev.Allow)
					return false;

				__instance.Hub.fpc.NetworkforceStopInputs = true;
				__instance.PlayerState = Scp096PlayerState.PryGate;

				float num = float.PositiveInfinity;

				Transform transform = null;

				foreach (Transform transform2 in gate.PryPositions)
				{
					float num2 = Vector3.Distance(__instance.Hub.playerMovementSync.RealModelPosition, transform2.position);

					if (num2 < num)
					{
						num = num2;
						transform = transform2;
					}
				}

				if (transform != null)
				{
					float rot = transform.rotation.eulerAngles.y - __instance.Hub.PlayerCameraReference.rotation.eulerAngles.y;
					__instance.Hub.playerMovementSync.OverridePosition(transform.position, rot, true);
				}

				Timing.RunCoroutine(__instance.MoveThroughGate(gate));
				Timing.RunCoroutine(__instance.ResetGateAnim());

				return false;
			}
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp096_PryGate), e);
                return true;
            }
        }
    }
}
