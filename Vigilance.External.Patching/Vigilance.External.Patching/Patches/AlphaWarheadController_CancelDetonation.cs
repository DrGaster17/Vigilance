using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Utilities;
using UnityEngine;
using Harmony;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.External.Patching.Patches
{
	[HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation), new Type[] { typeof(GameObject) })]
	public static class AlphaWarheadController_CancelDetonation
    {
        public static bool Prefix(AlphaWarheadController __instance, GameObject disabler)
        {
            try
            {
				if (!__instance.inProgress || __instance.timeToDetonation <= 10f || __instance._isLocked)
					return false;

				Player player = PlayersList.GetPlayer(disabler);

				if (player == null)
					player = LocalComponents.Player;

				if (player.PlayerLock)
					return false;

				WarheadCancelEvent ev = new WarheadCancelEvent(player, __instance.timeToDetonation, true);
				EventManager.Trigger<IHandlerWarheadCancel>(ev);

				if (!ev.Allow)
					return false;

				if (__instance.timeToDetonation != ev.TimeLeft)
					__instance.NetworktimeToDetonation = ev.TimeLeft;

				if (__instance.timeToDetonation <= 15f && !player.ReferenceHub.isDedicatedServer && !player.ReferenceHub.isLocalPlayer && !player.IsHost)
					LocalComponents.PlayerStats.TargetAchieve(player.Connection, "thatwasclose");

				sbyte b = 0;

				while (b < __instance.scenarios_resume.Length)
				{

					if (__instance.scenarios_resume[b].SumTime() > __instance.timeToDetonation && __instance.scenarios_resume[b].SumTime() < __instance.scenarios_start[AlphaWarheadController._startScenario].SumTime())
						__instance.NetworksyncResumeScenario = b;

					b += 1;
				}

				__instance.NetworktimeToDetonation = ((AlphaWarheadController._resumeScenario < 0) 
					? __instance.scenarios_start[AlphaWarheadController._startScenario].SumTime() 
					: __instance.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime()) + __instance.cooldown;
				__instance.NetworkinProgress = false;

				DoorEventOpenerExtension.TriggerAction(DoorEventOpenerExtension.OpenerEventType.WarheadCancel);
				__instance._autoDetonate = false;

				ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Detonation cancelled.", ServerLogs.ServerLogType.GameEvent, false);

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(AlphaWarheadController_CancelDetonation), e);
                return true;
            }
        }
    }
}
