using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using LightContainmentZoneDecontamination;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonate))]
    public static class AlphaWarheadController_Detonate
    {
        public static bool Prefix(AlphaWarheadController __instance)
        {
            try
            {
				if (__instance.detonated)
					return false;

				EventManager.Trigger<IHandlerWarheadDetonate>(new DetonationEvent());

				if (AlphaWarheadController.AutoWarheadBroadcastEnabled && __instance._broadcaster != null)
					__instance._broadcaster.RpcAddElement(AlphaWarheadController.WarheadExplodedBroadcastMessage, AlphaWarheadController.WarheadExplodedBroadcastMessageTime, Broadcast.BroadcastFlags.Normal);

				ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Warhead detonated.", ServerLogs.ServerLogType.GameEvent, false);

				if (!DecontaminationController.Singleton.disableDecontamination)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, "LCZ decontamination has been disabled by detonation of the Alpha Warhead.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
					DecontaminationController.Singleton.disableDecontamination = true;
				}

				__instance.detonated = true;
				__instance.RpcShake(true);

				GameObject[] array = GameObject.FindGameObjectsWithTag("LiftTarget");

				foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
				{
					scp079PlayerScript.lockedDoors.Clear();
				}

				foreach (ReferenceHub hub in ReferenceHub.Hubs.Values)
                {
					foreach (GameObject liftTarget in array)
                    {
						if (hub.playerStats.Explode(Vector3.Distance(liftTarget.transform.position, hub.playerMovementSync.RealModelPosition) <= 3.5f))
							__instance.warheadKills++;
                    }
                }

				if (DoorNametagExtension.NamedDoors.TryGetValue("SURFACE_NUKE", out DoorNametagExtension doorNametagExtension))
				{
					doorNametagExtension.TargetDoor.ServerChangeLock(DoorLockReason.SpecialDoorFeature, true);
					doorNametagExtension.TargetDoor.NetworkTargetState = true;
				}

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(AlphaWarheadController_Detonate), e);
                return true;
            }
        }
    }
}
