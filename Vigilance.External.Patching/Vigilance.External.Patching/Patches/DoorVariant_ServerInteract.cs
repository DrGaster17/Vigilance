using System;

using Vigilance.API;

using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;

using Vigilance.External.Extensions;

using Harmony;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
    public static class DoorVariant_ServerInteract
    {
        public static bool Prefix(DoorVariant __instance, ReferenceHub ply, byte colliderId)
        {
			try
			{
				if (__instance.ActiveLocks > 0)
				{
					DoorLockMode mode = DoorLockUtils.GetMode((DoorLockReason)__instance.ActiveLocks);

					if ((!mode.HasFlagFast(DoorLockMode.CanClose) || !mode.HasFlagFast(DoorLockMode.CanOpen)) && (!mode.HasFlagFast(DoorLockMode.ScpOverride) || ply.characterClassManager.CurRole.team != Team.SCP) && (mode == DoorLockMode.FullLock || (__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanClose)) || (!__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanOpen))))
					{
						__instance.LockBypassDenied(ply, colliderId);
						DoorEvents.TriggerAction(__instance, DoorAction.AccessDenied, ply);
						return false;
					}

				}

				Player player = ply.GetPlayer();
				API.Door door = __instance.GetDoor();

				if (player == null || door == null)
					return true;

				if (door.CanUse(player, out bool cldwn))
				{
					DoorInteractEvent ev = new DoorInteractEvent(true, player, door);
					EventManager.Trigger<IHandlerDoorInteract>(ev);

					if (!ev.Allow)
						return false;

					__instance.NetworkTargetState = !__instance.TargetState;
					__instance._triggerPlayer = ply;

					return false;
				}

				if (!cldwn)
				{
					__instance.PermissionsDenied(ply, colliderId);
					DoorEvents.TriggerAction(__instance, DoorAction.AccessDenied, ply);
				}

				return false;
			}
			catch (Exception e)
			{
				Patcher.Log(typeof(DoorVariant_ServerInteract), e);
				return true;
			}
        }
    }
}