using System;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using LightContainmentZoneDecontamination;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.FinishDecontamination))]
    public static class DecontaminationController_FinishDecontamination
    {
        public static bool Prefix(DecontaminationController __instance)
        {
            try
            {
				DecontaminationEvent ev = new DecontaminationEvent(true);
				EventManager.Trigger<IHandlerLczDecontamination>(ev);

				if (!ev.Allow)
					return false;

				foreach (Lift lift in Lift.Instances)
				{
					if (lift != null)
						lift.Lock();
				}

				foreach (GameObject gameObject in __instance.LczGenerator.doors)
				{
					if (gameObject != null && gameObject.gameObject.activeSelf)
					{
						DoorVariant door = gameObject.GetComponent<DoorVariant>();

						if (door != null)
						{
							door.NetworkTargetState = false;
							door.ServerChangeLock(DoorLockReason.DecontLockdown, true);
						}
					}
				}

				DoorEventOpenerExtension.TriggerAction(DoorEventOpenerExtension.OpenerEventType.DeconFinish);

				if (DecontaminationController.AutoDeconBroadcastEnabled && !__instance._decontaminationBegun && __instance._broadcaster != null)
					__instance._broadcaster.RpcAddElement(DecontaminationController.DeconBroadcastDeconMessage, DecontaminationController.DeconBroadcastDeconMessageTime, Broadcast.BroadcastFlags.Normal);

				__instance._decontaminationBegun = true;

				DecontaminationController.KillPlayers();

				return false;
            }
            catch (Exception e)
            {
                Log.Add("LightContainmentZoneDecontamination.DecontaminationController.FinishDecontamination", e);
                return true;
            }
        }
    }
}
